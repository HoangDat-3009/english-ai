using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Helper
{
    public class GeminiKeyManager
    {
        private readonly List<GeminiApiKey> _apiKeys;
        private readonly ILogger<GeminiKeyManager> _logger;
        private int _currentKeyIndex = 0;
        private readonly object _lock = new object();

        public GeminiKeyManager(IConfiguration configuration, ILogger<GeminiKeyManager> logger)
        {
            _logger = logger;
            var keysSection = configuration.GetSection("GeminiApiKeys").Get<List<string>>();
            
            if (keysSection == null || !keysSection.Any())
            {
                throw new InvalidOperationException("Không tìm thấy Gemini API Keys trong cấu hình. Vui lòng thêm mục 'GeminiApiKeys' vào appsettings.json");
            }

            _apiKeys = keysSection.Select((key, index) => new GeminiApiKey
            {
                Key = key,
                Index = index,
                IsActive = true,
                FailureCount = 0,
                LastUsed = DateTime.UtcNow
            }).ToList();

            _logger.LogInformation("Đã khởi tạo GeminiKeyManager với {Count} API keys", _apiKeys.Count);
        }

        /// <summary>
        /// Lấy key hiện tại đang sử dụng
        /// </summary>
        public string GetCurrentKey()
        {
            lock (_lock)
            {
                var activeKeys = _apiKeys.Where(k => k.IsActive).ToList();
                
                if (!activeKeys.Any())
                {
                    // Reset tất cả keys nếu tất cả đều bị disable
                    _logger.LogWarning("Tất cả keys đều bị disable, đang reset lại...");
                    _apiKeys.ForEach(k => 
                    {
                        k.IsActive = true;
                        k.FailureCount = 0;
                    });
                    activeKeys = _apiKeys;
                }

                // Tìm key active tiếp theo từ vị trí hiện tại
                for (int i = 0; i < _apiKeys.Count; i++)
                {
                    int index = (_currentKeyIndex + i) % _apiKeys.Count;
                    var key = _apiKeys[index];
                    
                    if (key.IsActive)
                    {
                        _currentKeyIndex = index;
                        key.LastUsed = DateTime.UtcNow;
                        _logger.LogDebug("Đang sử dụng API key #{Index}", index);
                        return key.Key;
                    }
                }

                // Fallback: trả về key đầu tiên
                _currentKeyIndex = 0;
                _apiKeys[0].LastUsed = DateTime.UtcNow;
                return _apiKeys[0].Key;
            }
        }

        /// <summary>
        /// Đánh dấu key hiện tại đã hết quota và chuyển sang key tiếp theo
        /// </summary>
        public string RotateToNextKey(string reason = "Quota exceeded")
        {
            lock (_lock)
            {
                var currentKey = _apiKeys[_currentKeyIndex];
                currentKey.FailureCount++;
                
                _logger.LogWarning("API key #{Index} gặp lỗi: {Reason}. Failure count: {Count}", 
                    _currentKeyIndex, reason, currentKey.FailureCount);

                // Tạm thời disable key nếu bị lỗi nhiều lần
                if (currentKey.FailureCount >= 3)
                {
                    currentKey.IsActive = false;
                    _logger.LogWarning("API key #{Index} đã bị tạm thời disable do quá nhiều lỗi", _currentKeyIndex);
                }

                // Chuyển sang key tiếp theo
                _currentKeyIndex = (_currentKeyIndex + 1) % _apiKeys.Count;
                
                var nextKey = GetCurrentKey();
                _logger.LogInformation("Đã chuyển sang API key #{Index}", _currentKeyIndex);
                
                return nextKey;
            }
        }

        /// <summary>
        /// Đánh dấu key hiện tại sử dụng thành công (reset failure count)
        /// </summary>
        public void MarkSuccess()
        {
            lock (_lock)
            {
                var currentKey = _apiKeys[_currentKeyIndex];
                if (currentKey.FailureCount > 0)
                {
                    currentKey.FailureCount = 0;
                    _logger.LogDebug("Reset failure count cho API key #{Index}", _currentKeyIndex);
                }
            }
        }

        /// <summary>
        /// Lấy thông tin trạng thái của tất cả keys
        /// </summary>
        public List<KeyStatus> GetKeysStatus()
        {
            lock (_lock)
            {
                return _apiKeys.Select(k => new KeyStatus
                {
                    Index = k.Index,
                    IsActive = k.IsActive,
                    IsCurrent = k.Index == _currentKeyIndex,
                    FailureCount = k.FailureCount,
                    LastUsed = k.LastUsed,
                    KeyPreview = k.Key.Length > 10 ? k.Key.Substring(0, 10) + "..." : k.Key
                }).ToList();
            }
        }

        /// <summary>
        /// Reset một key cụ thể về trạng thái active
        /// </summary>
        public void ResetKey(int index)
        {
            lock (_lock)
            {
                if (index >= 0 && index < _apiKeys.Count)
                {
                    _apiKeys[index].IsActive = true;
                    _apiKeys[index].FailureCount = 0;
                    _logger.LogInformation("Đã reset API key #{Index}", index);
                }
            }
        }

        /// <summary>
        /// Reset tất cả keys về trạng thái active
        /// </summary>
        public void ResetAllKeys()
        {
            lock (_lock)
            {
                _apiKeys.ForEach(k =>
                {
                    k.IsActive = true;
                    k.FailureCount = 0;
                });
                _logger.LogInformation("Đã reset tất cả API keys");
            }
        }
    }

    internal class GeminiApiKey
    {
        public string Key { get; set; } = string.Empty;
        public int Index { get; set; }
        public bool IsActive { get; set; }
        public int FailureCount { get; set; }
        public DateTime LastUsed { get; set; }
    }

    public class KeyStatus
    {
        public int Index { get; set; }
        public bool IsActive { get; set; }
        public bool IsCurrent { get; set; }
        public int FailureCount { get; set; }
        public DateTime LastUsed { get; set; }
        public string KeyPreview { get; set; } = string.Empty;
    }
}
