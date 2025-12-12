# Cấu hình ứng dụng

## Hướng dẫn thiết lập

1. **Copy file cấu hình mẫu:**
   ```bash
   cd EngAce.Api
   copy appsettings.example.json appsettings.json
   copy appsettings.Development.example.json appsettings.Development.json
   ```

2. **Cập nhật thông tin trong `appsettings.json`:**

   - **ConnectionStrings**: Thay `YOUR_PASSWORD` bằng mật khẩu MySQL của bạn
   - **GeminiSettings.ApiKey**: Thay `YOUR_GEMINI_API_KEY_HERE` bằng API key Gemini từ [Google AI Studio](https://aistudio.google.com/app/apikey)
   - **JwtSettings.SecretKey**: Tạo chuỗi bí mật tối thiểu 32 ký tự
   - **ApplicationInsights.ConnectionString**: (Tùy chọn) Thay bằng connection string từ Azure

3. **Cập nhật thông tin trong `appsettings.Development.json`:**

   - **ConnectionStrings**: Thay `YOUR_PASSWORD` bằng mật khẩu MySQL của bạn
   - **JwtSettings.SecretKey**: Tạo chuỗi bí mật khác với production

## Bảo mật

- ⚠️ **KHÔNG BAO GIỜ** commit file `appsettings.json` hoặc `appsettings.Development.json` lên Git
- File `.gitignore` đã được cấu hình để bỏ qua các file này
- Chỉ commit file `.example.json` để làm template cho người khác

## Lấy API Keys

### Google Gemini API Key
1. Truy cập [Google AI Studio](https://aistudio.google.com/app/apikey)
2. Đăng nhập tài khoản Google
3. Click "Create API Key"
4. Copy và paste vào `appsettings.json`

### JWT Secret Key
Tạo chuỗi ngẫu nhiên tối thiểu 32 ký tự, ví dụ:
```
EngAce_MyApp_Secret_Key_2024_Production_Min32Chars!
```

## Kiểm tra cấu hình

Chạy lệnh sau để kiểm tra ứng dụng đọc được cấu hình:
```bash
dotnet run --project EngAce.Api
```

Nếu thiếu cấu hình, ứng dụng sẽ báo lỗi cụ thể.
