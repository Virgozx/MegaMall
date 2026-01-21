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
                // Correct endpoint for Gemini models (v1beta/generateContent)
                // Use key in query param for easier access, but header is also supported if configured right.
                // However, the standard REST API usually expects key in query param unless using specialised client libs.
                // Let's use the query param approach which is most robust for HttpClient.
                var endpoint = $"https://generativelanguage.googleapis.com/v1beta/models/{_model}:generateContent?key={_apiKey}";
                
                var requestObj = new
                {
                    contents = new[] 
                    { 
                        new 
                        { 
                            parts = new[] 
                            { 
                                new { text = prompt } 
                            } 
                        } 
                    },
                    generationConfig = new 
                    {
                        maxOutputTokens = maxTokens,
                        temperature = temperature
                    }
                };

                var json = JsonSerializer.Serialize(requestObj);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Clear Authorization header if we use query param to avoid conflicts
                _http.DefaultRequestHeaders.Authorization = null;

                var res = await _http.PostAsync(endpoint, content);
                var resText = await res.Content.ReadAsStringAsync();

                if (!res.IsSuccessStatusCode)
                {
                    _logger.LogWarning("Gemini API call failed: {Status} {Body}", res.StatusCode, resText);
                    return null;
                }
                
                using var doc = JsonDocument.Parse(resText);
                if (doc.RootElement.TryGetProperty("candidates", out var candidates) && candidates.GetArrayLength() > 0)
                {
                    var firstCandidate = candidates[0];
                    if (firstCandidate.TryGetProperty("content", out var cContent) && 
                        cContent.TryGetProperty("parts", out var parts) && 
                        parts.GetArrayLength() > 0)
                    {
                        return parts[0].GetProperty("text").GetString();
                    }
                }

                return null;
            }
            catch (Exception ex)
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
