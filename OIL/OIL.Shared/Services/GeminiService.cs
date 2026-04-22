using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http.Json;
using Microsoft.Extensions.Configuration;

namespace OIL.Shared.Services
{
    // GeminiService.cs in your Shared RCL
    public class GeminiService
    {
        private readonly string gkey;

        // 2. This is the Constructor. It "injects" the config into the service.
        public GeminiService(IConfiguration config)
        {
            gkey = config["Gemini:ApiKey"] ?? "";
        }

        // In your GeminiService.cs
        private string _modelName = "gemini-3-flash-preview"; // Try changing this to gemini-2.5-flash if it fails

        private readonly HttpClient _http;
        //private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-1.5-flash:generateContent";

        private string BaseUrl => $"https://generativelanguage.googleapis.com/v1beta/models/{_modelName}:generateContent";

        

        //private readonly string gkey = config["Gemini:ApiKey"] ?? "";


        public GeminiService(HttpClient http) => _http = http;

        public async Task<string> Chat(List<ChatMessage> history, string locationCode, string[] assets)
        {
            try
            {
                var systemPrompt = $@"You are an Expert Field Engineer at a {locationCode} facility. 
        Current equipment context: {string.Join(", ", assets)}.
        1. Ask for error codes or physical symptoms.
        2. Suggest 3 diagnostics (e.g., check lube oil levels, verify PT-101 reading).
        3. If unresolved, say: 'SIGNAL_TICKET_LOG'.";

                var requestBody = new
                {
                    contents = history.Select(m => new { role = m.Role, parts = new[] { new { text = m.Text } } }),
                    system_instruction = new { parts = new[] { new { text = systemPrompt } } }
                };

                var response = await _http.PostAsJsonAsync($"{BaseUrl}?key={gkey}", requestBody);
                var result = await response.Content.ReadFromJsonAsync<GeminiResponse>();
                return result?.Candidates?[0].Content.Parts[0].Text ?? "Connection Error";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error calling Gemini API: {ex.Message}");
                return "Connection Error";
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
