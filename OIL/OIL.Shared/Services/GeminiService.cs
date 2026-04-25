using Microsoft.Extensions.Configuration;
using MudBlazor;
using Supabase;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static Supabase.Functions.Client;


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
            

            List<ChatMessage> updatedHistory = new List<ChatMessage>(history);


            //updatedHistory.Add(new ChatMessage("system", systemPrompt)); // Add system prompt to the history

            String jsontoAI = JsonSerializer.Serialize(updatedHistory);

            //Console.WriteLine(jsontoAI);

            try
            {
                var payload = new
                {
                    history = jsontoAI // Add system prompt to the history
                };

                var json = JsonSerializer.Serialize(payload);

                var response = await _supabase.Functions.Invoke("gemini-chat", json);

                var result = JsonSerializer.Deserialize<AiResponse>(
                    response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                return result?.Text ?? "No reply";
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return "Error";
            }
        }
    }

    public class AiResponse
    {
        public string Text { get; set; }
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
