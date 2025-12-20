# OAuth Setup Guide

Hướng dẫn cấu hình đăng nhập bằng Google và Facebook cho ứng dụng EngBuddy.

## Cấu hình Google OAuth

1. **Truy cập Google Cloud Console**
   - Đi đến: https://console.cloud.google.com/
   - Tạo project mới hoặc chọn project có sẵn

2. **Bật Google+ API**
   - Vào "APIs & Services" > "Library"
   - Tìm và bật "Google+ API"

3. **Tạo OAuth Client ID**
   - Vào "APIs & Services" > "Credentials"
   - Click "Create Credentials" > "OAuth client ID"
   - Chọn "Web application"
   - Cấu hình:
     - **Name**: EngBuddy OAuth (hoặc tên bạn muốn)
     - **Authorized JavaScript origins**: 
       - `http://localhost:5173` (cho development)
       - Thêm domain production khi deploy (ví dụ: `https://yourdomain.com`)
     - **Authorized redirect URIs**:
       - `http://localhost:5173` (cho development)
       - Thêm domain production khi deploy

4. **Copy Client ID**
   - Sau khi tạo xong, copy Client ID
   - Dán vào file `.env` với key `VITE_GOOGLE_CLIENT_ID`

## Cấu hình Facebook OAuth

1. **Truy cập Facebook Developers**
   - Đi đến: https://developers.facebook.com/
   - Click "My Apps" > "Create App"

2. **Chọn loại App**
   - Chọn "Consumer" hoặc "Business"
   - Điền tên App và email liên hệ

3. **Thêm Facebook Login Product**
   - Trong dashboard của App, click "Add Product"
   - Tìm và click "Set Up" cho "Facebook Login"

4. **Cấu hình Facebook Login**
   - Vào "Settings" > "Basic"
   - Thêm platform: chọn "Website"
   - Điền **Site URL**: 
     - `http://localhost:5173` (cho development)
     - Thêm domain production khi deploy
   - Lưu lại

5. **Copy App ID**
   - Copy App ID từ trang Settings > Basic
   - Dán vào file `.env` với key `VITE_FACEBOOK_APP_ID`

## Tạo file .env

Tạo file `.env` trong thư mục `english-mentor-buddy/` với nội dung:

```env
# Google OAuth Configuration
VITE_GOOGLE_CLIENT_ID=your_google_client_id_here

# Facebook OAuth Configuration
VITE_FACEBOOK_APP_ID=your_facebook_app_id_here
```

**Lưu ý**: 
- Thay `your_google_client_id_here` bằng Client ID từ Google Cloud Console
- Thay `your_facebook_app_id_here` bằng App ID từ Facebook Developers
- File `.env` không nên được commit vào Git (đã có trong .gitignore)

## Kiểm tra hoạt động

1. Restart development server:
   ```bash
   npm run dev
   ```

2. Vào trang đăng nhập: `http://localhost:5173/login`

3. Click nút "Google" hoặc "Facebook" để test

## Troubleshooting

### Lỗi "Google Client ID is not configured"
- Kiểm tra file `.env` có tồn tại không
- Kiểm tra `VITE_GOOGLE_CLIENT_ID` có giá trị không
- Restart development server sau khi thay đổi `.env`

### Lỗi "Popup blocked"
- Cho phép popup cho domain localhost
- Kiểm tra cấu hình Authorized JavaScript origins trong Google Cloud Console

### Lỗi "Invalid origin"
- Kiểm tra Authorized JavaScript origins trong Google Cloud Console có chứa `http://localhost:5173` không
- Kiểm tra Facebook Site URL có đúng không

### OAuth login thành công nhưng không redirect
- Kiểm tra console log xem có lỗi gì không
- Kiểm tra backend API có hoạt động không (`http://localhost:5000/api/auth/oauth-login`)

