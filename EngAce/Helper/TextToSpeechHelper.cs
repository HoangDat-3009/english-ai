using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Helper
{
    public static class TextToSpeechHelper
    {
        private static readonly HttpClient HttpClient = new()
        {
            Timeout = TimeSpan.FromSeconds(30)
        };

        /// <summary>
        /// Generates speech audio from the specified text content using Google Cloud Text-to-Speech.
        /// </summary>
        /// <param name="apiKey">The API key used to authorize the request.</param>
        /// <param name="text">The text to convert to speech.</param>
        /// <param name="languageCode">The language code of the synthesized voice.</param>
        /// <param name="voiceName">The specific voice name to use for synthesis.</param>
        /// <param name="speakingRate">Optional speaking rate for the voice output.</param>
        /// <param name="pitch">Optional pitch adjustment for the voice output.</param>
        /// <returns>The base64 audio content or <c>null</c> if the synthesis fails.</returns>
        public static async Task<string?> SynthesizeAsync(
            string apiKey,
            string text,
            string languageCode = "en-US",
            string voiceName = "en-US-Neural2-C",
            double speakingRate = 1.0,
            double pitch = 0.0)
        {
            if (string.IsNullOrWhiteSpace(apiKey) || string.IsNullOrWhiteSpace(text))
            {
                return null;
            }

            var requestUri = $"https://texttospeech.googleapis.com/v1/text:synthesize?key={apiKey}";

            var payload = new TextToSpeechRequest
            {
                Input = new TextInput { Text = text },
                Voice = new VoiceSelection
                {
                    LanguageCode = languageCode,
                    Name = voiceName
                },
                AudioConfig = new AudioConfiguration
                {
                    AudioEncoding = "MP3",
                    SpeakingRate = speakingRate,
                    Pitch = pitch
                }
            };

            using var response = await HttpClient.PostAsJsonAsync(requestUri, payload);

            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var ttsResponse = await response.Content.ReadFromJsonAsync<TextToSpeechResponse>();
            return ttsResponse?.AudioContent;
        }

        private sealed class TextToSpeechRequest
        {
            [JsonPropertyName("input")]
            public TextInput Input { get; set; } = new();

            [JsonPropertyName("voice")]
            public VoiceSelection Voice { get; set; } = new();

            [JsonPropertyName("audioConfig")]
            public AudioConfiguration AudioConfig { get; set; } = new();
        }

        private sealed class TextInput
        {
            [JsonPropertyName("text")]
            public string Text { get; set; } = string.Empty;
        }

        private sealed class VoiceSelection
        {
            [JsonPropertyName("languageCode")]
            public string LanguageCode { get; set; } = "en-US";

            [JsonPropertyName("name")]
            public string Name { get; set; } = "en-US-Neural2-C";
        }

        private sealed class AudioConfiguration
        {
            [JsonPropertyName("audioEncoding")]
            public string AudioEncoding { get; set; } = "MP3";

            [JsonPropertyName("speakingRate")]
            public double SpeakingRate { get; set; } = 1.0;

            [JsonPropertyName("pitch")]
            public double Pitch { get; set; } = 0.0;
        }

        private sealed class TextToSpeechResponse
        {
            [JsonPropertyName("audioContent")]
            public string? AudioContent { get; set; }
        }
    }
}
