using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using ECommerceProcurementSystem.Data;
using ECommerceProcurementSystem.Models;

namespace ECommerceProcurementSystem.Services
{
    public class ExternalDataImportService
    {
        private readonly HttpClient _httpClient;
        private readonly ProcurementContext _context;

        public ExternalDataImportService(HttpClient httpClient, ProcurementContext context)
        {
            _httpClient = httpClient;
            _context = context;
        }

        public async Task ImportDataFromApiAsync()
        {
            string url = "https://data.austintexas.gov/api/views/3ebq-e9iz/rows.json?accessType=DOWNLOAD"; 

            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                throw new Exception("Failed to fetch data");

            var json = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var records = JsonSerializer.Deserialize<List<RawRecord>>(json, options);

            foreach (var record in records)
            {
                if (string.IsNullOrEmpty(record.CITY) || string.IsNullOrEmpty(record.VENDOR_CODE) ||
                    string.IsNullOrEmpty(record.LGL_NM) || string.IsNullOrEmpty(record.DATA_BUILD_DATE) ||
                    string.IsNullOrEmpty(record.ITM_TOT_AM))
                    continue; 

             
                var city = _context.Cities.FirstOrDefault(c => c.CityName == record.CITY);
                if (city == null)
                {
                    city = new City { CityName = record.CITY };
                    _context.Cities.Add(city);
                    await _context.SaveChangesAsync(); 
                }

                
                int vendorCode = int.Parse(record.VENDOR_CODE);
                var vendor = _context.Vendors.FirstOrDefault(v => v.Vendor_Code == vendorCode);
                if (vendor == null)
                {
                    vendor = new Vendor
                    {
                        Vendor_Code = vendorCode,
                        VendorName = record.LGL_NM
                    };
                    _context.Vendors.Add(vendor);
                    await _context.SaveChangesAsync(); 
                }

             
                if (!DateTime.TryParseExact(record.DATA_BUILD_DATE, "MM/dd/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime date))
                    continue;

                int year = date.Year;

                
                if (!decimal.TryParse(record.ITM_TOT_AM, out decimal saleAmount))
                    continue;

                var sale = new AnnualSaleAmount
                {
                    CityID = city.CityID,
                    Vendor_Code = vendor.Vendor_Code,
                    Year = year,
                    SaleAmount = saleAmount
                };

                _context.AnnualSaleAmounts.Add(sale);
            }

            await _context.SaveChangesAsync();
        }

        
        private class RawRecord
        {
            public string CITY { get; set; }
            public string VENDOR_CODE { get; set; }
            public string LGL_NM { get; set; }
            public string DATA_BUILD_DATE { get; set; }
            public string ITM_TOT_AM { get; set; }
        }
    }
}
