using Gemini.NET;
using Microsoft.Extensions.Logging;

namespace Helper
{
    public static class GeminiApiHelper
    {
        /// <summary>
        /// Thực thi API request với tự động retry khi key hết quota
        /// </summary>
        public static async Task<T> ExecuteWithAutoRetry<T>(
            Func<string, Task<T>> apiCall,
            int maxRetries = 3)
        {
            var keyManager = HttpContextHelper.GetKeyManager();
            if (keyManager == null)
            {
                // Fallback: gọi trực tiếp với key hiện tại
                var fallbackKey = HttpContextHelper.GetAccessKey();
                return await apiCall(fallbackKey ?? string.Empty);
            }

            Exception? lastException = null;
            
            for (int attempt = 0; attempt < maxRetries; attempt++)
            {
                try
                {
                    var apiKey = keyManager.GetCurrentKey();
                    var result = await apiCall(apiKey);
                    
                    // Đánh dấu thành công
                    keyManager.MarkSuccess();
                    return result;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    var errorMessage = ex.Message.ToLower();
                    
                    // Kiểm tra các loại lỗi liên quan đến quota/rate limit
                    bool isQuotaError = errorMessage.Contains("quota") ||
                                       errorMessage.Contains("rate limit") ||
                                       errorMessage.Contains("resource exhausted") ||
                                       errorMessage.Contains("429") ||
                                       errorMessage.Contains("too many requests");

                    if (isQuotaError)
                    {
                        // Rotate sang key tiếp theo
                        var newKey = keyManager.RotateToNextKey($"Attempt {attempt + 1}: {ex.Message}");
                        
                        if (attempt < maxRetries - 1)
                        {
                            // Đợi 1 giây trước khi retry
                            await Task.Delay(1000);
                            continue;
                        }
                    }
                    else
                    {
                        // Lỗi không phải quota, không retry
                        throw;
                    }
                }
            }

            // Đã hết số lần retry
            throw new InvalidOperationException(
                $"Đã thử {maxRetries} API keys nhưng đều gặp lỗi. Lỗi cuối: {lastException?.Message}", 
                lastException);
        }

        /// <summary>
        /// Wrapper cho Generator.GenerateContentAsync với auto retry
        /// </summary>
        public static async Task<GeneratorResponse> GenerateContentWithRetry(
            ApiRequestBuilder requestBuilder,
            ModelVersion modelVersion = ModelVersion.Gemini_20_Flash)
        {
            return await ExecuteWithAutoRetry(async (apiKey) =>
            {
                var generator = new Generator(apiKey);
                var request = requestBuilder.Build();
                return await generator.GenerateContentAsync(request, modelVersion);
            });
        }
    }
}
