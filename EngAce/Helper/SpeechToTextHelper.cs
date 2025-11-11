using System.Text;
using System.Text.Json;

namespace Helper;

/// <summary>
/// Helper để chuyển đổi giọng nói thành văn bản sử dụng Google Cloud Speech-to-Text API
/// </summary>
public static class SpeechToTextHelper
{
    private const string ApiEndpoint = "https://speech.googleapis.com/v1/speech:recognize";

    /// <summary>
    /// Chuyển đổi audio (base64) thành văn bản
    /// </summary>
    /// <param name="audioBase64">Audio data dạng base64</param>
    /// <param name="languageCode">Mã ngôn ngữ (mặc định: en-US)</param>
    /// <returns>Văn bản được nhận dạng</returns>
    public static async Task<string> TranscribeAsync(string audioBase64, string languageCode = "en-US")
    {
        var apiKey = HttpContextHelper.GetGeminiApiKey(); // Sử dụng cùng API key

        var requestBody = new
        {
            config = new
            {
                encoding = "WEBM_OPUS",
                sampleRateHertz = 48000,
                languageCode = languageCode,
                enableAutomaticPunctuation = true,
                model = "default"
            },
            audio = new
            {
                content = audioBase64
            }
        };

        using var httpClient = new HttpClient();
        var jsonContent = JsonSerializer.Serialize(requestBody);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

        var response = await httpClient.PostAsync($"{ApiEndpoint}?key={apiKey}", content);
        var responseContent = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Speech-to-Text API error: {response.StatusCode} - {responseContent}");
        }

        var result = JsonSerializer.Deserialize<SpeechRecognitionResponse>(responseContent);
        
        if (result?.Results == null || result.Results.Count == 0)
        {
            throw new Exception("No speech detected in audio");
        }

        // Lấy transcript có confidence cao nhất
        var bestTranscript = result.Results
            .SelectMany(r => r.Alternatives)
            .OrderByDescending(a => a.Confidence)
            .FirstOrDefault()?.Transcript ?? string.Empty;

        return bestTranscript;
    }

    // Classes for JSON deserialization
    private class SpeechRecognitionResponse
    {
        public List<RecognitionResult> Results { get; set; } = new();
    }

    private class RecognitionResult
    {
        public List<SpeechRecognitionAlternative> Alternatives { get; set; } = new();
    }

    private class SpeechRecognitionAlternative
    {
        public string Transcript { get; set; } = string.Empty;
        public float Confidence { get; set; }
    }
}
