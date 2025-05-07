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
using System.Text.Json.Serialization;

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
            string uri = $"/resource/{_datasetId}.json?$limit={limit}&$offset={offset}";
            List<RawRow> rawRows = await _http.GetFromJsonAsync<List<RawRow>>(uri) ?? new();
            // Only map navigation properties, do not flatten fields
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
        public async Task<IReadOnlyList<AnnualReport>> GetAnnualReportsFromSocrataAsync(int limit = 40)
        {
            // Socrata API: https://data.austintexas.gov/resource/3ebq-e9iz.json
            // Ensure _datasetId is correctly set to "3ebq-e9iz" for this specific report.
            string uri = $"/resource/{_datasetId}.json?$limit={limit}"; // Make sure _datasetId is "3ebq-e9iz"
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
                    SaleAmount = g.Sum(x =>
                        x.line_item_total_amount != null && decimal.TryParse(x.line_item_total_amount, out var totalAmount) ? totalAmount :
                        (x.unit_price != null && x.quantity != null && decimal.TryParse(x.unit_price, out var unitPrice) && decimal.TryParse(x.quantity, out var quantity) ?
                            unitPrice * quantity : 0m) // Calculate if possible, else 0. Explicitly use '0m' for decimal.
                    ),
                    City = new City { CityName = g.Key.city.Trim() }
                })
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
                    VendorName = first.vendor_name,
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
                    Agreement = agreement,
                    Vendor_Code = first.vendor_code
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
                        Quantity_Ordered = row.quantity != null && decimal.TryParse(row.quantity, out var quantityOrdered) ? quantityOrdered : null,
                        Unit_Of_Measure_Code = row.unit_of_measure_code,
                        Unit_Of_Measure_Description = row.unit_of_measure_description,
                        Unit_Price = row.unit_price != null && decimal.TryParse(row.unit_price, out var unitPrice) ? unitPrice : null,
                        Line_Item_Total_Amount = row.line_item_total_amount != null && decimal.TryParse(row.line_item_total_amount, out var totalAmount) ? totalAmount : null,
                        Extended_Description = row.extended_description
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
            [property: JsonPropertyName("purchase_order")] string? purchase_order,
            [property: JsonPropertyName("vendor_code")] string? vendor_code,
            [property: JsonPropertyName("lgl_nm")] string? vendor_name,
            [property: JsonPropertyName("ad_ln_1")] string? address,
            [property: JsonPropertyName("city")] string? city,
            [property: JsonPropertyName("zip")] string? zip,
            [property: JsonPropertyName("ctry")] string? country,
            [property: JsonPropertyName("master_agreement")] string? master_agreement,
            [property: JsonPropertyName("contract_name")] string? contract_name,
            [property: JsonPropertyName("award_date")] DateTime? award_date,
            [property: JsonPropertyName("commodity")] string? commodity_id,
            [property: JsonPropertyName("commodity_description")] string? commodity_description,
            [property: JsonPropertyName("line_item_description")] string? line_item_description,
            [property: JsonPropertyName("quantity")] string? quantity,
            [property: JsonPropertyName("unit_of_measure")] string? unit_of_measure_code,
            [property: JsonPropertyName("unit_of_meas_desc")] string? unit_of_measure_description,
            [property: JsonPropertyName("unit_price")] string? unit_price,
            [property: JsonPropertyName("itm_tot_am")] string? line_item_total_amount,
            [property: JsonPropertyName("extended_description")] string? extended_description
        );
    }
}