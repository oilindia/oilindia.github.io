using Microsoft.Extensions.Configuration;
using MudBlazor;
using Supabase;
using System;
using System.Collections.Generic;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using static Supabase.Functions.Client;
using Supabase.Functions;
using static OIL.Shared.Pages.AI.ChatBotComponent;
using static OIL.Shared.Pages.AI.AIInsights;

namespace OIL.Shared.Services
{
    // GeminiService.cs in your Shared RCL
    public class GeminiService
    {
        private readonly Supabase.Client _supabase;

        public GeminiService(Supabase.Client supabase)
        {
            _supabase = supabase;
        }


        public async Task<string> GenerateMaintenanceReport(string period, List<Job> jobs)
        {
            // 1. Convert Job List into a compact text summary for the AI
            var summaryData = string.Join("\n", jobs.Select(j =>
                $"[{j.CreatedAt:yyyy-MM-dd}] {j.JobName} | Type: {j.JobType} | Priority: {j.Priority} | Status: {j.Status} | Cost: {j.PartsCost} | Downtime: {j.DowntimeHours}hrs"));

            // 2. Create the specialized Prompt
            var prompt = $@"
        Act as an Industrial Maintenance Analyst. Analyze the following job logs for the {period} period:
        
        {summaryData}

        Please provide a report in the following format:
        1. **Executive Summary**: A high-level overview of department activity.
        2. **Cost & Efficiency**: Analysis of parts cost vs downtime.
        3. **Key Issues**: Identify recurring problems or problematic locations.
        4. **Recommendations**: 3 actionable points for the next period.
        
        Use professional engineering terminology.";

            // 3. Call your existing AI logic (using the same structure as your chat)
            // We create a temporary message list to reuse your existing chat logic
            var tempMessages = new List<ChatMessage> { new ChatMessage("user", prompt) };
            return await ProcessMaintenanceChat(tempMessages);
        }



        public async Task<string> ProcessMaintenanceChat(List<ChatMessage> history)
        {
            try
            {
                // 1. Create the history string
                string historyString = string.Join("\n", history.Select(m => $"{m.Role}: {m.Text}"));

                // 2. We use a HARDCODED ID "current_session" so we only ever have ONE row
                // This keeps storage at near zero and makes it easy to find.
                var session = new ChatSession { Id = "current_session", History = historyString };

                // 3. FORCE the update
                await _supabase.From<ChatSession>().Upsert(session);

                // 4. Call function with NO payload
                var response = await _supabase.Functions.Invoke("insights", "{}");

                var result = JsonSerializer.Deserialize<AiResponse>(response,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                return result?.Text ?? "Error";
            }
            catch (Exception ex)
            {
                return $"C# Error: {ex.Message}";
            }
        }

        public async Task<string> Chat(List<ChatMessage> history)
        {
            // 1. Extract the text
            string mssagefromlist = history.LastOrDefault(x => x.Role == "user")?.Text ?? "Hi";

            try
            {
                // 2. Create a PLAIN object (Do NOT serialize it to a string here)
                var payload = new { message = mssagefromlist };


                // 3. Define the options for the Invoke call
                var options = new InvokeFunctionOptions
                {
                    Headers = new Dictionary<string, string> { { "Content-Type", "application/json" } }
                };

                string jsonPayload = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                Console.WriteLine($"Invoking function with payload: {jsonPayload}");

                // 4. Pass 'payload' as an object. 
                // Supabase.Functions will serialize this correctly for the Edge Function.
                var response = await _supabase.Functions.Invoke("gemini-chat", jsonPayload, options);

                var result = JsonSerializer.Deserialize<AiResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Text ?? "No reply";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Function Error: {ex.Message}");
                return "Error";
            }
        }
        public async Task<string> Chatold(List<ChatMessage> history)
        {
            try
            {
                // Extract the latest message from history
                var latestInput = history.LastOrDefault(x => x.Role == "user")?.Text ?? "";

                // Create a serializable object with lowercase names to match Edge Function logic
                var payload = new
                {
                    latest = latestInput,
                    history = history.Select(h => new { role = h.Role, text = h.Text }).ToList()
                };

                // Serialize with camelCase just in case
                var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                // Invoke the Supabase Edge Function
                var response = await _supabase.Functions.Invoke("gemini-chat", json);

                if (string.IsNullOrEmpty(response)) return "Error: No response from assistant.";

                var result = JsonSerializer.Deserialize<AiResponse>(response, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                return result?.Text ?? "Error parsing response.";
            }
            catch (Exception ex)
            {
                return $"Error: {ex.Message}";
            }
        }
    }

    // Support Classes
    public class AiResponse { public string Text { get; set; } }

    public class ChatMessage
    {
        public string Role { get; set; } // "user" or "model"
        public string Text { get; set; }
        public DateTime Time { get; set; } = DateTime.UtcNow;

        public ChatMessage(string role, string text) { Role = role; Text = text; }
    }

}
