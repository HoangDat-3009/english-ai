# Hướng Dẫn Sử Dụng Hệ Thống Auto Rotation Gemini API Keys

## Tổng quan
Hệ thống này tự động quản lý và rotate giữa các Gemini API keys khi gặp lỗi quota/rate limit.

## Cấu hình

### 1. Thêm API Keys vào `appsettings.json`

```json
{
  "GeminiApiKeys": [
    "AIzaSyA4ntpoBtLUwsppNOH7sXE9Dk4XuQ-maO8",
    "AIzaSyAJr8lR7jbN5lyV8rcDI3I-cpSk2CcCA34",
    "AIzaSyD7okyjX6svswxbwiFwHcTcdW4JOpzdU1A",
    "AIzaSyD_your_fourth_key_here"
  ]
}
```

**Lưu ý:** 
- Thêm bao nhiêu key tùy thích
- Keys sẽ được rotate theo thứ tự từ trên xuống dưới
- Cần ít nhất 1 key để hệ thống hoạt động

### 2. Thêm Keys vào `appsettings.Development.json` (cho môi trường dev)

```json
{
  "GeminiApiKeys": [
    "AIzaSyA4ntpoBtLUwsppNOH7sXE9Dk4XuQ-maO8",
    "AIzaSyAJr8lR7jbN5lyV8rcDI3I-cpSk2CcCA34",
    "AIzaSyD7okyjX6svswxbwiFwHcTcdW4JOpzdU1A"
  ]
}
```

## Cách sử dụng

### Cách 1: Sử dụng `GeminiApiHelper.ExecuteWithAutoRetry` (Khuyên dùng)

```csharp
using Helper;

// Tự động retry với key mới khi gặp lỗi quota
var result = await GeminiApiHelper.ExecuteWithAutoRetry(async (apiKey) =>
{
    var generator = new Generator(apiKey);
    var request = new ApiRequestBuilder()
        .WithPrompt("Your prompt here")
        .Build();
    
    return await generator.GenerateContentAsync(request);
});
```

### Cách 2: Sử dụng `GeminiApiHelper.GenerateContentWithRetry`

```csharp
using Helper;

var apiRequest = new ApiRequestBuilder()
    .WithSystemInstruction("Your instruction")
    .WithPrompt("Your prompt")
    .WithDefaultGenerationConfig()
    .Build();

// Tự động retry với ModelVersion mặc định
var response = await GeminiApiHelper.GenerateContentWithRetry(apiRequest);

// Hoặc chỉ định ModelVersion
var response = await GeminiApiHelper.GenerateContentWithRetry(
    apiRequest, 
    ModelVersion.Gemini_20_Flash_Lite
);
```

### Cách 3: Sử dụng trực tiếp (không auto retry)

```csharp
using Helper;

// Lấy key hiện tại
var apiKey = HttpContextHelper.GetAccessKey();

// Sử dụng như bình thường
var generator = new Generator(apiKey);
var response = await generator.GenerateContentAsync(request);

// Nếu muốn manually rotate khi gặp lỗi
try 
{
    var response = await generator.GenerateContentAsync(request);
}
catch (Exception ex)
{
    var keyManager = HttpContextHelper.GetKeyManager();
    if (keyManager != null)
    {
        var newKey = keyManager.RotateToNextKey(ex.Message);
        // Retry với key mới...
    }
}
```

## API Endpoints để quản lý Keys

### 1. Xem trạng thái tất cả keys
```
GET /api/KeyManagement/Status
```

Response:
```json
[
  {
    "Index": 0,
    "IsActive": true,
    "IsCurrent": true,
    "FailureCount": 0,
    "LastUsed": "2025-11-22T10:30:00Z",
    "KeyPreview": "AIzaSyA4nt..."
  },
  {
    "Index": 1,
    "IsActive": true,
    "IsCurrent": false,
    "FailureCount": 2,
    "LastUsed": "2025-11-22T09:15:00Z",
    "KeyPreview": "AIzaSyB123..."
  }
]
```

### 2. Xem key hiện tại
```
GET /api/KeyManagement/Current
```

### 3. Rotate sang key tiếp theo (manual)
```
POST /api/KeyManagement/Rotate?reason=Testing
```

### 4. Reset một key cụ thể
```
POST /api/KeyManagement/Reset/0
```

### 5. Reset tất cả keys
```
POST /api/KeyManagement/ResetAll
```

## Cơ chế hoạt động

1. **Auto Detection**: Hệ thống tự động phát hiện các lỗi:
   - `quota exceeded`
   - `rate limit`
   - `resource exhausted`
   - `429 Too Many Requests`

2. **Auto Rotation**: Khi phát hiện lỗi quota:
   - Tăng failure count của key hiện tại
   - Tự động chuyển sang key tiếp theo
   - Retry request với key mới
   - Tối đa 3 lần retry (có thể config)

3. **Auto Recovery**:
   - Key bị disable tạm thời sau 3 lần lỗi liên tiếp
   - Khi tất cả keys đều disable, hệ thống tự động reset tất cả
   - Mỗi lần request thành công sẽ reset failure count

4. **Thread-safe**: Sử dụng lock để đảm bảo an toàn khi multi-threading

## Ví dụ cập nhật code hiện có

### Ví dụ 1: Cập nhật QuizScope.GenerateQuizes

**Trước:**
```csharp
var generator = new Generator(apiKey);
var response = await generator.GenerateContentAsync(apiRequest, ModelVersion.Gemini_20_Flash_Lite);
```

**Sau:**
```csharp
var response = await GeminiApiHelper.GenerateContentWithRetry(
    apiRequestBuilder,
    ModelVersion.Gemini_20_Flash_Lite
);
```

### Ví dụ 2: Cập nhật SearchScope.Search

**Trước:**
```csharp
public static async Task<string> Search(string apiKey, string keyword, string context)
{
    var generator = new Generator(apiKey);
    var response = await generator.GenerateContentAsync(request);
    return response.Result;
}
```

**Sau:**
```csharp
public static async Task<string> Search(string apiKey, string keyword, string context)
{
    return await GeminiApiHelper.ExecuteWithAutoRetry(async (key) =>
    {
        var generator = new Generator(key);
        var response = await generator.GenerateContentAsync(request);
        return response.Result;
    });
}
```

## Monitoring & Logging

Hệ thống tự động log các sự kiện:
- ✅ Key rotation events
- ⚠️ Quota exceeded warnings
- ❌ Key disabled events
- 🔄 Auto reset events

Check logs để theo dõi:
```
[Information] Đã khởi tạo GeminiKeyManager với 4 API keys
[Warning] API key #0 gặp lỗi: Quota exceeded. Failure count: 1
[Information] Đã chuyển sang API key #1
[Warning] API key #2 đã bị tạm thời disable do quá nhiều lỗi
```

## Troubleshooting

### Lỗi: "Không tìm thấy Gemini API Keys trong cấu hình"
➡️ Kiểm tra `appsettings.json` có mục `GeminiApiKeys` chưa

### Tất cả keys đều bị lỗi
➡️ Kiểm tra xem keys có còn quota không trên Google AI Studio
➡️ Gọi API `/api/KeyManagement/ResetAll` để reset

### Key không tự động rotate
➡️ Đảm bảo đang dùng `GeminiApiHelper.ExecuteWithAutoRetry`
➡️ Không dùng cách cũ là truyền trực tiếp `apiKey`

## Best Practices

1. ✅ Luôn dùng `GeminiApiHelper.ExecuteWithAutoRetry` cho các API call quan trọng
2. ✅ Thêm nhiều keys để tăng khả năng available
3. ✅ Monitor logs để biết khi nào cần thêm keys
4. ✅ Định kỳ check `/api/KeyManagement/Status`
5. ❌ Không hardcode API key trong code
6. ❌ Không share keys trong repository (dùng User Secrets hoặc Environment Variables)
