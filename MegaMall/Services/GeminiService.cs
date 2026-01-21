using System.Net.Http.Headers;
using System.Text.Json;
using System.Text;
using MegaMall.Interfaces;
using MegaMall.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace MegaMall.Services
{
    public class GeminiService : IAIService
    {
        private readonly HttpClient _http;
        private readonly ILogger<GeminiService> _logger;
        private readonly string _apiKey;
        private readonly string _model;

        public GeminiService(HttpClient http, IConfiguration config, ILogger<GeminiService> logger)
        {
            _http = http;
            _logger = logger;
            _apiKey = config["Gemini:ApiKey"] ?? string.Empty;
            _model = config["Gemini:Model"] ?? "gemini-2.5-flash";

            if (!string.IsNullOrEmpty(_apiKey))
            {
                // Use Bearer header for Google Gen AI if applicable
                _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _apiKey);
            }
        }

        public async Task<string?> GenerateTextAsync(string prompt, int maxTokens = 512, double temperature = 0.2)
        {
            if (string.IsNullOrEmpty(_apiKey))
            {
                _logger.LogWarning("Gemini API key not configured");
                return null;
            }

            try
            {
                var endpoint = $"https://generativelanguage.googleapis.com/v1beta2/models/{_model}:generateText";
                var requestObj = new
                {
                    prompt = new { text = prompt },
                    maxOutputTokens = maxTokens,
                    temperature = temperature
                };

                var json = JsonSerializer.Serialize(requestObj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var res = await _http.PostAsync(endpoint, content);
                var resText = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini API call failed: {Status} {Body}", res.StatusCode, resText);
                    return null;
                }

                using var doc = JsonDocument.Parse(resText);
                // Try to read candidates/outputs per API shape
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var first = candidates[0];
                    if (first.TryGetProperty("output", out var output))
                    {
                        return output.GetString();
                    }
                    if (first.TryGetProperty("content", out var contentProp))
                    {
                        // join text pieces
                        if (contentProp.ValueKind == JsonValueKind.Array && contentProp.GetArrayLength() > 0)
                        {
                            var sb = new StringBuilder();
                            foreach (var item in contentProp.EnumerateArray())
                            {
                                if (item.TryGetProperty("text", out var t)) sb.Append(t.GetString());
                            }
                            return sb.ToString();
                        }
                        return contentProp.GetString();
                    }
                }

                // Fallback: try "candidates[0].output" or top-level "output"
                if (doc.RootElement.TryGetProperty("result", out var resultEl))
                {
                    if (resultEl.TryGetProperty("output", out var out2)) return out2.GetString();
                }

                return null;
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error calling Gemini API");
                return null;
            }
        }

        public async Task<AiEmailResult> GenerateEmailAsync(string templateKey, object variables)
        {
            var prompt = $"Create a short email subject and an HTML email body for the template '{templateKey}'. Use this data: {JsonSerializer.Serialize(variables)}. Keep subject concise and body friendly and actionable. Return subject and body separated by a line with '---BODY---' marker.";
            var text = await GenerateTextAsync(prompt, maxTokens: 600);
            if (string.IsNullOrEmpty(text)) return new AiEmailResult { Subject = templateKey, Body = string.Empty };

            // If marker present, split
            var marker = "---BODY---";
            if (text.Contains(marker))
            {
                var parts = text.Split(new string[] { marker }, System.StringSplitOptions.None);
                return new AiEmailResult { Subject = parts[0].Trim(), Body = parts.Length > 1 ? parts[1].Trim() : string.Empty };
            }

            // Fallback: first line subject, rest body
            var lines = text.Split(new[] { '\n' }, 2);
            return new AiEmailResult { Subject = lines[0].Trim(), Body = lines.Length > 1 ? lines[1].Trim() : string.Empty };
        }
    }
}
