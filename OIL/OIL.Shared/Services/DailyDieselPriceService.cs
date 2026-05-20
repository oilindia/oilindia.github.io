using MudBlazor;
using Supabase.Gotrue;
using Supabase.Postgrest.Attributes;
using Supabase.Postgrest.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;

namespace OIL.Shared.Services
{
    public class DailyDieselPriceService
    {
        private readonly HttpClient _http = new HttpClient();
        private readonly Supabase.Client _supabase;

        // Constructor ensures Supabase client is passed in and initialized safely
        public DailyDieselPriceService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }

        public async Task GetLatestDieselPrice()
        {
            try
            {
                // 1. URL encode the target Upstox endpoint
                string targetUrl = "https://service.upstox.com/commodity/open/v1/diesel-price/charts?city=guwahati&days=30";
                string proxyUrl = $"https://api.allorigins.win/get?url={HttpUtility.UrlEncode(targetUrl)}";

                // 2. Fetch the wrapper payload from AllOrigins
                var proxyResponse = await _http.GetFromJsonAsync<AllOriginsResponse>(proxyUrl);

                // 3. Extract the nested string data payload returned by Upstox
                string rawUpstoxJson = proxyResponse?.Contents ?? "";

                if (string.IsNullOrWhiteSpace(rawUpstoxJson))
                {
                    Console.WriteLine("Warning: Received empty data payload from proxy backend.");
                    return;
                }

                Console.WriteLine("Raw Upstox JSON successfully retrieved via proxy:");
                Console.WriteLine(rawUpstoxJson);

                // 4. Deserialize the nested text content directly into your FuelResponse objects
                var response = JsonSerializer.Deserialize<FuelResponse>(rawUpstoxJson, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                var latest = response?.Data?.PriceHistory?.FirstOrDefault();

                if (latest == null)
                {
                    Console.WriteLine("Notice: No diesel price history records found inside the response.");
                    return;
                }

                var price = latest.Price;
                var dateStr = latest.Date;

                // 5. Update global runtime monitoring contexts
                GlobalVariables.GlobalLatestDieselPrice = price.ToString() ?? "0.00";
                GlobalVariables.GlobalLatestDieselPriceDate = dateStr ?? "N/A";

                // 6. Map model structure to save into your Supabase database state table
                var item = new DieselPrice
                {
                    City = "Guwahati",
                    Price = latest.Price,
                    PricePercChg = latest.PricePercChg,
                    PriceDate = DateTime.Parse(latest.Date),
                    UpdatedBy = GlobalVariables.GlobalCurrentUserName
                };

                await _supabase
                    .From<DieselPrice>()
                    .Insert(item);

                //Console.WriteLine($"Database state updated successfully. Guwahati Diesel Price: {price}");
            }
            catch (Exception ex)
            {
                //Console.WriteLine($"Method Error inside DailyDieselPriceService: {ex.Message}");
            }
        }
    }

    // Helper class to map the AllOrigins wrapper structure
    public class AllOriginsResponse
    {
        public string Contents { get; set; } = "";
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