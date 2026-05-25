using MudBlazor;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;

namespace OIL.Shared.Services
{
    public class DailyDieselPriceService
    {
        private readonly HttpClient _http;
        private readonly Supabase.Client _supabase;

        public DailyDieselPriceService(HttpClient http, Supabase.Client supabase)
        {
            _http = http;
            _supabase = supabase;

            if (!_http.DefaultRequestHeaders.Contains("User-Agent"))
            {
                _http.DefaultRequestHeaders.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
            }
        }

        public async Task GetLatestDieselPrice()
        {
            try
            {
                // 1. Pre-Check: Prevent unnecessary API calls if today's data already exists
                var today = DateTime.Today;

                var existingData = await _supabase
                    .From<DieselPrice>()
                    .Select("id") // Select only the ID to minimize payload size
                    .Filter("city", Supabase.Postgrest.Constants.Operator.Equals, "Guwahati")
                    .Filter("price_date", Supabase.Postgrest.Constants.Operator.Equals, today.ToString("yyyy-MM-dd"))
                    .Get();

                if (existingData.Models.Any())
                {
                    // Today's record already exists. Circuit breaker triggered.
                    return;
                }

                // 2. Define the target URL and proxy
                string targetUrl = "https://service.upstox.com/commodity/open/v1/diesel-price/charts?city=guwahati&days=30";
                string proxyUrl = $"https://corsproxy.io/?url={Uri.EscapeDataString(targetUrl)}";

                // 3. Fetch and deserialize
                var response = await _http.GetFromJsonAsync<FuelResponse>(proxyUrl, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var latest = response?.Data?.PriceHistory?.FirstOrDefault();

                if (latest == null)
                {
                    return;
                }

                var price = latest.Price;
                var dateStr = latest.Date;

                // 4. Update global runtime monitoring contexts
                GlobalVariables.GlobalLatestDieselPrice = price.ToString(CultureInfo.InvariantCulture) ?? "0.00";
                GlobalVariables.GlobalLatestDieselPriceDate = dateStr ?? "N/A";

                // 5. Map model structure
                var item = new DieselPrice
                {
                    City = "Guwahati",
                    Price = latest.Price,
                    PricePercChg = latest.PricePercChg,
                    PriceDate = DateTime.Parse(latest.Date, CultureInfo.InvariantCulture),
                    UpdatedBy = GlobalVariables.GlobalCurrentUserName
                };

                // 6. Insert the new record
                await _supabase
                    .From<DieselPrice>()
                    .Insert(item);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Method Error inside DailyDieselPriceService: {ex.Message}");
            }
        }
    }

    public class FuelResponse
    {
        public bool Success { get; set; }
        public FuelData Data { get; set; }
        public string? Message { get; set; }
    }

    public class FuelData
    {
        public List<PriceHistory> PriceHistory { get; set; } = new();
    }

    public class PriceHistory
    {
        public string Date { get; set; } = "";
        public decimal Price { get; set; }
        public decimal PricePercChg { get; set; }
    }

    [Table("diesel_prices")]
    public class DieselPrice : BaseModel
    {
        [PrimaryKey("id", false)]
        public long Id { get; set; }

        [Column("city")]
        public string City { get; set; } = "";

        [Column("price")]
        public decimal Price { get; set; }

        [Column("price_perc_chg")]
        public decimal? PricePercChg { get; set; }

        [Column("price_date")]
        public DateTime PriceDate { get; set; }

        [Column("updated_by")]
        public string? UpdatedBy { get; set; }
    }
}