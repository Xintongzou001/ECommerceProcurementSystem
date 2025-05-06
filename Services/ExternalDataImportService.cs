// In ExternalDataImportService.cs (Relevant Part)
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerceProcurementSystem.Data;
using ECommerceProcurementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerceProcurementSystem.Services
{
    public class ExternalDataImportService
    {
        private readonly HttpClient _httpClient;
        // Inject IServiceProvider to resolve DbContext within a scope if needed,
        // Or ensure this service itself is scoped if ProcurementContext is scoped.
        // Direct injection of DbContext into a Singleton service can cause issues.
        // Assuming this service is Scoped or Transient for now.
        private readonly ProcurementContext _context;

        public ExternalDataImportService(HttpClient httpClient, ProcurementContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task ImportDataFromApiAsync()
        {
            // Consider making the URL configurable
            string url = "https://data.austintexas.gov/api/views/3ebq-e9iz/rows.json?accessType=DOWNLOAD";

            var response = await _httpClient.GetAsync(url);
            response.EnsureSuccessStatusCode(); // Throws exception on failure

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };

            // This endpoint seems to return data in a specific format, not a simple array of records.
            // Adjust deserialization based on actual JSON structure.
            // Assuming the actual data is nested, e.g., within a "data" property.
            // You might need a wrapper class. For now, assuming it deserializes correctly to RawRecord list.
            var apiResponse = JsonSerializer.Deserialize<SocrataApiResponse>(json, options); // Adjust based on actual structure

            if (apiResponse?.Data == null)
            {
                // Handle case where data is not found or structure is wrong
                Console.WriteLine("Failed to deserialize or find data in API response."); // Use proper logging
                return;
            }


            // Check context availability
            if (_context.Cities == null || _context.Vendors == null || _context.AnnualReports == null)
            {
                Console.WriteLine("Database context or required DbSets are null."); // Use proper logging
                return;
            }

            // It's generally more efficient to fetch existing entities into memory first
            // if you expect many duplicates, rather than querying one by one.
            var existingCities = await _context.Cities.ToDictionaryAsync(c => c.CityName, c => c);
            var existingVendors = await _context.Vendors.ToDictionaryAsync(v => v.Vendor_Code, v => v);


            foreach (var recordArray in apiResponse.Data) // Assuming data is an array of arrays/objects
            {
                // --- Adapt index based on actual data structure from rows.json ---
                // Example indices (MUST BE VERIFIED AGAINST ACTUAL rows.json output):
                const int cityIndex = 12;        // Example index for CITY
                const int vendorCodeIndex = 8;   // Example index for VENDOR_CODE
                const int vendorNameIndex = 9;   // Example index for LGL_NM (Vendor Name)
                const int dateIndex = 2;         // Example index for DATA_BUILD_DATE or similar date field
                const int amountIndex = 21;      // Example index for ITM_TOT_AM (Sale Amount)
                                                 // --- End of indices to verify ---

                // Basic validation based on assumed indices
                if (recordArray == null || recordArray.Length <= Math.Max(Math.Max(Math.Max(cityIndex, vendorCodeIndex), vendorNameIndex), Math.Max(dateIndex, amountIndex)) ||
                   recordArray[cityIndex] == null || recordArray[vendorCodeIndex] == null || recordArray[vendorNameIndex] == null ||
                   recordArray[dateIndex] == null || recordArray[amountIndex] == null)
                {
                    Console.WriteLine("Skipping record due to missing data at expected indices."); // Use logging
                    continue;
                }


                string cityName = recordArray[cityIndex]?.ToString() ?? string.Empty;
                string vendorCodeStr = recordArray[vendorCodeIndex]?.ToString() ?? string.Empty;
                string vendorName = recordArray[vendorNameIndex]?.ToString() ?? string.Empty;
                string dateStr = recordArray[dateIndex]?.ToString() ?? string.Empty;
                string saleAmountStr = recordArray[amountIndex]?.ToString() ?? string.Empty;


                if (string.IsNullOrEmpty(cityName) || string.IsNullOrEmpty(vendorCodeStr) || string.IsNullOrEmpty(vendorName) || string.IsNullOrEmpty(dateStr) || string.IsNullOrEmpty(saleAmountStr))
                    continue; // Skip if essential data is missing

                // Find or Create City
                if (!existingCities.TryGetValue(cityName, out City? city))
                {
                    city = new City { CityName = cityName };
                    _context.Cities.Add(city);
                    // Save immediately to get CityID if needed, or save all at the end
                    // await _context.SaveChangesAsync(); // Might be inefficient in a loop
                    existingCities.Add(cityName, city); // Add to in-memory dictionary
                }


                // Find or Create Vendor
                // Vendor_Code in AnnualReport model is int, but in Vendor model it's string. Needs consistency.
                // Assuming Vendor_Code should be string based on Vendor model.
                string vendorCode = vendorCodeStr; // Use string code directly
                if (!existingVendors.TryGetValue(vendorCode, out Vendor? vendor))
                {
                    vendor = new Vendor
                    {
                        Vendor_Code = vendorCode, // Use string
                        VendorName = vendorName
                        // Populate other Vendor fields if available/needed
                    };
                    _context.Vendors.Add(vendor);
                    // await _context.SaveChangesAsync(); // Inefficient
                    existingVendors.Add(vendorCode, vendor);
                }

                // Parse Date and Amount
                // Adjust date format "MM/dd/yyyy" if needed based on actual API data
                if (!DateTime.TryParseExact(dateStr, "yyyy-MM-dd'T'HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date)) // Example ISO 8601 format
                {
                    Console.WriteLine($"Skipping record due to invalid date format: {dateStr}"); // Use logging
                    continue;
                }


                int year = date.Year;


                if (!decimal.TryParse(saleAmountStr, NumberStyles.Currency, CultureInfo.InvariantCulture, out decimal saleAmount)) // Allow currency symbols if present
                {
                    Console.WriteLine($"Skipping record due to invalid sale amount format: {saleAmountStr}"); // Use logging
                    continue;
                }


                // <<< --- MODIFIED LINES START --- >>>
                // Create an AnnualReport object
                var report = new AnnualReport
                {
                    // CityID = city.CityID, // EF Core can handle relationship via navigation property
                    City = city, // Assign navigation property
                                 // Vendor_Code = vendor.Vendor_Code, // EF Core can handle relationship
                    Vendor = vendor, // Assign navigation property
                    Year = year,
                    SaleAmount = saleAmount
                };


                // Add to the AnnualReports DbSet
                _context.AnnualReports.Add(report);
                // <<< --- MODIFIED LINES END --- >>>
            }


            // Save all changes outside the loop for efficiency
            try
            {
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving changes: {ex.Message}"); // Use proper logging
                // Handle potential exceptions (e.g., constraint violations)
            }
        }

        // Helper class for Socrata API response structure (adjust based on actual JSON)
        private class SocrataApiResponse
        {
            // Example: Assuming data is under a "data" property which is an array of object arrays
            [System.Text.Json.Serialization.JsonPropertyName("data")]
            public List<object?[]>? Data { get; set; }
            // Add other properties like "meta" if needed
        }


        // Original RawRecord class - might not be needed if parsing directly from object?[]
        // private class RawRecord { ... }
    }
}