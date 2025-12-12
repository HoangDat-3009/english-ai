using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace Helper
{
    /// <summary>
    /// Lightweight HTTP client for invoking GPT-5 chat completions with JSON schema outputs.
    /// </summary>
    public static class Gpt5Client
    {
        private static readonly HttpClient HttpClient = new()
        {
            BaseAddress = new Uri("https://api.openai.com/v1/")
        };

        public static async Task<string> GenerateJsonResponseAsync(
            string systemInstruction,
            string prompt,
            object responseSchema,
            float temperature = 0.4f,
            string schemaName = "structured_output",
            CancellationToken cancellationToken = default)
        {
            var apiKey = HttpContextHelper.GetGpt5ApiKey();
            if (string.IsNullOrWhiteSpace(apiKey))
            {
                throw new InvalidOperationException("GPT-5 API key is not configured.");
            }

            var payload = new
            {
                model = "gpt-5.1",
                temperature,
                response_format = new
                {
                    type = "json_schema",
                    json_schema = new
                    {
                        name = schemaName,
                        schema = responseSchema
                    }
                },
                messages = new object[]
                {
                    new { role = "system", content = systemInstruction },
                    new { role = "user", content = prompt }
                }
            };

            using var request = new HttpRequestMessage(HttpMethod.Post, "chat/completions")
            {
                Content = JsonContent.Create(payload)
            };
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);

            using var response = await HttpClient.SendAsync(request, cancellationToken);
            var raw = await response.Content.ReadAsStringAsync(cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var statusCode = (int)response.StatusCode;
                if (statusCode == 429)
                {
                    throw new InvalidOperationException($"RATE_LIMIT_EXCEEDED: GPT-5 API rate limit exceeded. Please try again in 1 minute.");
                }
                throw new InvalidOperationException($"GPT-5 API error ({statusCode}): {raw}");
            }

            return ExtractContent(raw);
        }

        private static string ExtractContent(string raw)
        {
            try
            {
                using var document = JsonDocument.Parse(raw);
                var root = document.RootElement;

                if (root.TryGetProperty("choices", out var choices) &&
                    choices.ValueKind == JsonValueKind.Array &&
                    choices.GetArrayLength() > 0)
                {
                    var message = choices[0].GetProperty("message");
                    if (message.TryGetProperty("content", out var content))
                    {
                        if (content.ValueKind == JsonValueKind.Array && content.GetArrayLength() > 0)
                        {
                            var first = content[0];
                            if (first.ValueKind == JsonValueKind.Object && first.TryGetProperty("text", out var textNode))
                            {
                                return textNode.GetString() ?? string.Empty;
                            }

                            return first.GetString() ?? first.ToString();
                        }

                        if (content.ValueKind == JsonValueKind.String)
                        {
                            return content.GetString() ?? string.Empty;
                        }

                        return content.ToString();
                    }
                }
            }
            catch (JsonException)
            {
                // Fall back to returning the raw payload when parsing fails.
            }

            return raw;
        }
    }
}
