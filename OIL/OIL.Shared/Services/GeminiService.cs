using Microsoft.Extensions.Configuration;
using MudBlazor;
using Supabase;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;


namespace OIL.Shared.Services
{
    // GeminiService.cs in your Shared RCL
    public class GeminiService
    {
        private readonly string gkey;

        private readonly HttpClient _http;
        private readonly Supabase.Client _supabase;

        // 2. This is the Constructor. It "injects" the config into the service.
        // COMBINE BOTH HERE:

        public GeminiService(Supabase.Client supabase) // Ensure this matches Program.cs registration
        {
            _supabase = supabase ?? throw new ArgumentNullException(nameof(supabase));
        }


        //public GeminiService(HttpClient http, IConfiguration config)
        //{
        //    _http = http;
        //    //gkey = config["Gemini:ApiKey"] ?? "";
        //}

        // In your GeminiService.cs
        private string _modelName = "gemini-3-flash-preview"; // Try changing this to gemini-2.5-flash if it fails

        //private readonly HttpClient _http;
        //private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        private string BaseUrl => $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent";

        

        





        public async Task<string> Chat(List<ChatMessage> history, string locationCode, string[] assets)
        {
            try
            {
                // Safety: If the UI sends an empty list, create a default one
                // Using the constructor: ChatMessage(role, text)
                var safeHistory = (history != null && history.Any())
                    ? history
                    : new List<ChatMessage> { new ChatMessage("user", "Initial engineering query.") };

                var payloadObj = new
                {
                    // Map to anonymous object to ensure lowercase keys for the Edge Function
                    history = safeHistory.Select(m => new {
                        role = m.Role?.ToLower() ?? "user",
                        text = m.Text ?? ""
                    }).ToList(),
                    locationCode = locationCode ?? "Unknown",
                    assets = assets ?? new string[] { "None" }
                };

                var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
                string jsonPayload = JsonSerializer.Serialize(payloadObj, options);

                // Clear headers to avoid 'Invalid Value' JS error
                var invokeOptions = new Supabase.Functions.Client.InvokeFunctionOptions
                {
                    Headers = new Dictionary<string, string>()
                };

                // LINE 114: The call that was failing
                var response = await _supabase.Functions.Invoke("gemini-chat", jsonPayload, invokeOptions);

                var result = JsonSerializer.Deserialize<GeminiResponse>(response, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Candidates?.FirstOrDefault()?.Content?.Parts?.FirstOrDefault()?.Text ?? "No response content.";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[OIL.Shared] Critical Error at Line 114: {ex.Message}");
                throw; // Allow the UI to catch this and show the Snackbar
            }
        }


    }

    public class ChatMessage
    {
        public string Role { get; set; } // "user" or "model"
        public string Text { get; set; }

        public ChatMessage(string role, string text)
        {
            Role = role;
            Text = text;
        }
    }

    // These classes map the JSON response from Google Gemini
    public class GeminiResponse
    {
        public List<Candidate> Candidates { get; set; }
    }

    public class Candidate
    {
        public Content Content { get; set; }
    }

    public class Content
    {
        public List<Part> Parts { get; set; }
    }

    public class Part
    {
        public string Text { get; set; }
    }



}
