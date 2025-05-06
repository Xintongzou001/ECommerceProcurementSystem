// In SocrataService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using ECommerceProcurementSystem.Models;
using System.Net; // Required for HttpUtility or Uri

namespace ECommerceProcurementSystem.Services
{
    public class SocrataService : ISocrataService
    {
        private readonly HttpClient _http;
        private readonly string _datasetId;

        public SocrataService(HttpClient http, IConfiguration cfg)
        {
            _http = http;
            // Ensure DatasetId is read correctly - Null check is good practice
            _datasetId = cfg["Socrata:DatasetId"] ?? throw new InvalidOperationException("Socrata:DatasetId not configured.");
        }

        public async Task<IReadOnlyList<PurchaseOrder>> GetPurchaseOrdersAsync(
            int limit = 1000,
            int offset = 0)
        {
            // Use SoQL for limit and offset
            string uri = $"/resource/{_datasetId}.json?$limit={limit}&$offset={offset}";
            List<RawRow> rawRows = await _http.GetFromJsonAsync<List<RawRow>>(uri) ?? new();
            return MapRawRowsToPurchaseOrders(rawRows);
        }

        public async Task<PurchaseOrder?> GetPurchaseOrderByIdAsync(string id)
        {
            // Use SoQL $where parameter to filter by purchase_order ID.
            // Use Uri.EscapeDataString to safely encode the ID for the URL.
            string encodedId = Uri.EscapeDataString(id);
            string uri = $"/resource/{_datasetId}.json?purchase_order={encodedId}";

            List<RawRow> rawRows = await _http.GetFromJsonAsync<List<RawRow>>(uri) ?? new();

            // If the API returns an empty array, the PO wasn't found
            if (!rawRows.Any())
            {
                return null;
            }

            // Since we filtered by ID, all returned rows belong to the same PO.
            // Map them using the existing logic (extracted to a helper method below).
            // We expect only one PurchaseOrder object from this mapping.
            var purchaseOrders = MapRawRowsToPurchaseOrders(rawRows);
            return purchaseOrders.FirstOrDefault(); // Return the single mapped order, or null if mapping somehow failed
        }

        /// <summary>
        /// Fetches annual report data from the Socrata API (Austin Open Data portal).
        /// Maps the data to the AnnualReport model for offline use.
        /// </summary>
        public async Task<IReadOnlyList<AnnualReport>> GetAnnualReportsFromSocrataAsync(int limit = 20)
        {
            // Socrata API: https://data.austintexas.gov/resource/3ebq-e9iz.json
            string uri = $"/resource/{_datasetId}.json?$limit={limit}";
            var rawRows = await _http.GetFromJsonAsync<List<RawRow>>(uri) ?? new();

            if (!rawRows.Any())
            {
                return new List<AnnualReport>();
            }

            // Map to AnnualReport (group by vendor_code, city, and year, sum SaleAmount)
            var reports = rawRows
                .Where(r => !string.IsNullOrEmpty(r.vendor_code) && !string.IsNullOrEmpty(r.city) && r.award_date.HasValue)
                .GroupBy(r => new { r.vendor_code, r.city, Year = r.award_date.Value.Year })
                .Select(g => new AnnualReport
                {
                    Vendor_Code = g.Key.vendor_code,
                    CityID = 0, // Will be populated in controller
                    Year = g.Key.Year,
                    SaleAmount = g.Sum(x => x.line_item_total_amount ?? 0),
                    // Create a new City object to help with mapping
                    City = new City { CityName = g.Key.city.Trim() }
                })
                .Take(limit)
                .ToList();
            
            return reports;
        }

        private IReadOnlyList<PurchaseOrder> MapRawRowsToPurchaseOrders(List<RawRow> rawRows)
        {
            // Group by Purchase_Order so each group represents one PO header
            var grouped = rawRows.GroupBy(r => r.purchase_order);
            var orders = new List<PurchaseOrder>();

            foreach (var g in grouped)
            {
                var first = g.First(); // first row contains header info

                var vendor = new Vendor
                {
                    Vendor_Code = first.vendor_code,
                    // Corrected: Use VendorName property in Vendor model
                    VendorName = first.vendor,
                    Address = first.address,
                    City = first.city,
                    Zip = first.zip,
                    Country = first.country
                };

                var agreement = new MasterAgreement
                {
                    Master_Agreement = first.master_agreement,
                    Contract_Name = first.contract_name,
                    Award_Date = first.award_date
                };

                var po = new PurchaseOrder
                {
                    Purchase_Order = first.purchase_order,
                    Vendor = vendor,
                    Agreement = agreement
                    // Lines collection initialized by default
                };

                foreach (var row in g) // Iterate through all rows for this PO to get lines
                {
                    // Avoid adding line if CommodityID is missing (essential for composite key)
                    if (string.IsNullOrEmpty(row.commodity_id)) continue;

                    po.Lines.Add(new PurchaseOrderLine
                    {
                        Purchase_Order = row.purchase_order, // Redundant but okay
                        CommodityID = row.commodity_id,
                        Commodity = new Commodity
                        {
                            CommodityID = row.commodity_id,
                            Commodity_Description = row.commodity_description
                        },
                        Line_Item_Description = row.line_item_description,
                        Quantity_Ordered = row.quantity_ordered,
                        Unit_Of_Measure_Code = row.unit_of_measure_code,
                        Unit_Of_Measure_Description = row.unit_of_measure_description,
                        Unit_Price = row.unit_price,
                        Line_Item_Total_Amount = row.line_item_total_amount
                    });
                }
                orders.Add(po);
            }
            return orders;
        }

        /* -------------------------------------------------------------
         * Internal DTO that matches the Socrata JSON columns exactly.
         * Keep this private—only used for deserializing raw API rows.
         * -----------------------------------------------------------*/
        private record RawRow(
            string purchase_order,
            string vendor_code,
            string vendor, // Matches JSON field name
            string address,
            string city,
            string zip,
            string country,
            string master_agreement,
            string contract_name,
            DateTime? award_date,
            string commodity_id,
            string commodity_description,
            string line_item_description,
            decimal? quantity_ordered,
            string unit_of_measure_code,
            string unit_of_measure_description,
            decimal? unit_price,
            decimal? line_item_total_amount);
    }
}