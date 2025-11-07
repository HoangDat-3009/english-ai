# 🏗️ CẤU TRÚC FRONTEND - HỆ THỐNG HỌC TIẾNG ANH

## 📁 **CẤU TRÚC THƯ MỤC CHI TIẾT**

### `src/` - Thư mục mã nguồn chính
``
src/
├── App.css                    # File CSS cho component App chính
├── App.tsx                    # Component gốc của ứng dụng, định tuyến chính
├── index.css                  # CSS toàn cục cho toàn bộ ứng dụng
├── main.tsx                   # Điểm khởi đầu ứng dụng, render App
├── vite-env.d.ts             # Khai báo type cho Vite environment
│
├── components/               # 📦 THÀNH PHẦN GIAO DIỆN TÁI SỬ DỤNG
│   ├── admin/               # 👨‍💼 Component dành riêng cho quản trị viên
│   │   ├── AdminProtectedRoute.tsx      # Bảo vệ route chỉ admin mới truy cập được
│   │   ├── AdminReadingExercisesManager.tsx  # Quản lý bài tập đọc hiểu cho admin
│   │   ├── FileRow.tsx                  # Hiển thị thông tin 1 dòng file trong danh sách
│   │   └── SectionBox.tsx               # Khung chứa nội dung các phần trong admin
│   │
│   ├── ui/                  # 🎨 Thành phần giao diện cơ bản (Shadcn/UI)
│   │   ├── accordion.tsx               # Danh sách thu gọn/mở rộng
│   │   ├── alert-dialog.tsx           # Hộp thoại cảnh báo với nút xác nhận
│   │   ├── alert.tsx                   # Thông báo cảnh báo inline
│   │   ├── aspect-ratio.tsx            # Giữ tỉ lệ khung hình cho hình ảnh/video
│   │   ├── avatar.tsx                  # Hiển thị ảnh đại diện người dùng
│   │   ├── badge.tsx                   # Nhãn hiển thị trạng thái/danh mục
│   │   ├── breadcrumb.tsx              # Đường dẫn điều hướng (Home > Admin > Users)
│   │   ├── button.tsx                  # Nút bấm với nhiều kiểu dáng
│   │   ├── calendar.tsx                # Lịch chọn ngày tháng
│   │   ├── card.tsx                    # Thẻ chứa nội dung có viền và bóng
│   │   ├── carousel.tsx                # Băng chuyền hiển thị nhiều item
│   │   ├── chart.tsx                   # Component vẽ biểu đồ
│   │   ├── checkbox.tsx                # Ô tích chọn true/false
│   │   ├── collapsible.tsx             # Nội dung có thể thu gọn/mở rộng
│   │   ├── command.tsx                 # Thanh tìm kiếm với gợi ý lệnh
│   │   ├── context-menu.tsx            # Menu ngữ cảnh khi click phải
│   │   ├── dialog.tsx                  # Hộp thoại popup
│   │   ├── drawer.tsx                  # Ngăn kéo từ cạnh màn hình
│   │   ├── dropdown-menu.tsx           # Menu thả xuống
│   │   ├── form.tsx                    # Form nhập liệu với validation
│   │   ├── hover-card.tsx              # Thẻ hiển thị khi hover chuột
│   │   ├── input-otp.tsx               # Nhập mã OTP (6 số)
│   │   ├── input.tsx                   # Ô nhập văn bản
│   │   ├── label.tsx                   # Nhãn mô tả cho input
│   │   ├── menubar.tsx                 # Thanh menu ngang
│   │   ├── navigation-menu.tsx         # Menu điều hướng đa cấp
│   │   ├── pagination.tsx              # Phân trang cho danh sách dài
│   │   ├── popover.tsx                 # Popup nhỏ hiển thị thông tin
│   │   ├── progress.tsx                # Thanh tiến độ
│   │   ├── radio-group.tsx             # Nhóm nút radio chọn 1 trong nhiều
│   │   ├── resizable.tsx               # Vùng có thể thay đổi kích thước
│   │   ├── scroll-area.tsx             # Vùng cuộn nội dung dài
│   │   ├── select.tsx                  # Dropdown chọn giá trị
│   │   ├── separator.tsx               # Đường kẻ ngăn cách
│   │   ├── sheet.tsx                   # Bảng trượt từ cạnh màn hình
│   │   ├── sidebar.tsx                 # Thanh bên điều hướng
│   │   ├── skeleton.tsx                # Hiệu ứng loading giả lập nội dung
│   │   ├── slider.tsx                  # Thanh trượt chọn giá trị số
│   │   ├── sonner.tsx                  # Thông báo toast đẹp
│   │   ├── switch.tsx                  # Công tắc bật/tắt
│   │   ├── table.tsx                   # Bảng hiển thị dữ liệu
│   │   ├── tabs.tsx                    # Tab chuyển đổi nội dung
│   │   ├── textarea.tsx                # Ô nhập văn bản nhiều dòng
│   │   ├── toast.tsx                   # Thông báo nổi tạm thời
│   │   ├── toaster.tsx                 # Container chứa các toast
│   │   ├── toggle-group.tsx            # Nhóm nút toggle
│   │   ├── toggle.tsx                  # Nút bật/tắt
│   │   ├── tooltip.tsx                 # Chú thích khi hover
│   │   └── use-toast.ts                # Hook để hiển thị toast
│   │
│   ├── AuthContext.tsx          # Quản lý trạng thái đăng nhập toàn ứng dụng
│   ├── FeatureCard.tsx          # Thẻ giới thiệu tính năng trên trang chủ
│   ├── Navbar.tsx               # Thanh điều hướng chính của ứng dụng
│   ├── ProtectedRoute.tsx       # Bảo vệ route cần đăng nhập
│   ├── ReadingExerciseCard.tsx  # Thẻ hiển thị thông tin bài tập đọc
│   ├── SearchInput.tsx          # Ô tìm kiếm với icon và placeholder
│   ├── StatsCard.tsx           # Thẻ hiển thị thống kê số liệu
│   └── ThemeProvider.tsx        # Cung cấp chủ đề sáng/tối
│
├── hooks/                      # 🎣 HOOK TÙY CHỈNH REACT
│   ├── use-api.ts              # Hook gọi API với cache và error handling
│   ├── use-mobile.tsx          # Hook phát hiện thiết bị di động
│   ├── use-toast.ts           # Hook hiển thị thông báo toast
│   ├── useAdminProgress.ts     # Hook quản lý dữ liệu admin và user
│   ├── useDatabaseStats.ts     # Hook thống kê cơ sở dữ liệu
│   ├── useReadingExercises.ts  # Hook quản lý bài tập đọc hiểu
│   └── useStats.ts             # Hook thống kê học tập người dùng
│
├── layouts/                    # 🏗️ BỐ CỤC TRANG
│   ├── admin/                  # Bố cục cho trang quản trị
│   │   └── AdminLayout.tsx     # Layout chính cho trang admin với sidebar
│   └── MainLayout.tsx          # Layout chính cho trang công khai
│
├── lib/                        # 📚 THỦ VIỆN VÀ TIỆN ÍCH
│   └── utils.ts               # Các hàm tiện ích chung
│
├── pages/                      # 📄 CÁC TRANG CHÍNH
│   ├── admin/                  # 👨‍💼 Trang dành cho quản trị viên
│   │   ├── AccountPage.tsx     # Quản lý thông tin tài khoản admin
│   │   ├── ContentManagement.tsx # Quản lý nội dung bài học
│   │   ├── Dashboard.tsx       # Trang tổng quan thống kê hệ thống
│   │   ├── ProfilePage.tsx     # Trang thông tin cá nhân admin
│   │   ├── Settings.tsx        # Cài đặt hệ thống
│   │   ├── SimpleAdminTest.tsx # Trang test chức năng admin đơn giản
│   │   ├── TestsPage.tsx       # Quản lý đề thi và bài kiểm tra
│   │   ├── UploadPage.tsx      # Trang tải lên file bài tập
│   │   └── UserManagement.tsx  # Quản lý người dùng (CRUD users)
│   │
│   ├── Chat.tsx               # Trang chat với AI hỗ trợ học tập
│   ├── Dictionary.tsx         # Trang tra từ điển
│   ├── DictionaryResult.tsx   # Hiển thị kết quả tra từ
│   ├── EnglishTopicCards.tsx  # Thẻ các chủ đề tiếng Anh
│   ├── Exercises.tsx          # Trang danh sách bài tập tổng quát
│   ├── Index.tsx             # Trang chủ với giới thiệu tính năng
│   ├── Leaderboard.tsx       # Bảng xếp hạng học viên
│   ├── Login.tsx             # Trang đăng nhập chính
│   ├── LoginAlt.tsx          # Trang đăng nhập phiên bản khác
│   ├── NotFound.tsx          # Trang lỗi 404 - không tìm thấy
│   ├── Progress.tsx          # Trang theo dõi tiến độ cá nhân
│   ├── ReadingExercises.tsx  # Trang luyện tập đọc hiểu TOEIC
│   └── Register.tsx          # Trang đăng ký tài khoản mới
│
└── services/                  # 🔧 DỊCH VỤ XỬ LÝ DỮ LIỆU
    ├── adminService.ts        # Quản lý users, thống kê hệ thống, Excel
    ├── adminUploadService.ts  # Upload file và tạo bài tập từ nội dung
    ├── api.ts                 # Service gọi API chung với error handling
    ├── consultationService.ts # Tư vấn học tập và lộ trình cá nhân
    ├── databaseStatsService.ts # Thống kê chi tiết cơ sở dữ liệu
    ├── dictionaryService.ts   # Tra cứu từ điển và nghĩa từ
    ├── elevenLabsService.ts   # Chuyển văn bản thành giọng nói
    ├── exerciseService.ts     # Quản lý bài tập tổng quát
    ├── readingExerciseService.ts # Xử lý bài tập đọc hiểu TOEIC
    ├── statsService.ts        # Thống kê học tập và tiến độ
    ├── supabaseClient.ts      # Kết nối cơ sở dữ liệu Supabase
    ├── userManagementService.ts # Quản lý thông tin người dùng
    └── userSettingsService.ts # Cài đặt cá nhân người dùng
```

---

## 📱 **COMPONENTS - CÁC THÀNH PHẦN GIAO DIỆN**

### 🔐 **components/admin/** - Quản trị viên
- **`AdminLayout.tsx`** - Bố cục trang quản trị với thanh điều hướng và sidebar
- **`AdminNavbar.tsx`** - Thanh điều hướng dành cho quản trị viên
- **`AdminProtectedRoute.tsx`** - Bảo vệ các trang chỉ dành cho quản trị viên
- **`AdminSidebar.tsx`** - Thanh bên với menu chức năng quản trị

### 🎨 **components/ui/** - Giao diện cơ bản
- **`badge.tsx`** - Nhãn hiển thị trạng thái hoặc thông tin ngắn
- **`button.tsx`** - Nút bấm với nhiều kiểu dáng khác nhau
- **`card.tsx`** - Thẻ chứa nội dung với viền và bóng đổ
- **`dialog.tsx`** - Hộp thoại popup cho thông báo và form
- **`input.tsx`** - Ô nhập liệu văn bản
- **`label.tsx`** - Nhãn mô tả cho các trường nhập liệu
- **`progress.tsx`** - Thanh tiến độ hiển thị phần trăm hoàn thành
- **`select.tsx`** - Danh sách lựa chọn thả xuống
- **`table.tsx`** - Bảng hiển thị dữ liệu có sắp xếp và lọc
- **`tabs.tsx`** - Tab chuyển đổi giữa các nội dung khác nhau
- **`textarea.tsx`** - Ô nhập văn bản nhiều dòng
- **`toast.tsx`** - Thông báo nổi xuất hiện tạm thời

### 🧩 **components/khác**
- **`AuthContext.tsx`** - Quản lý trạng thái đăng nhập người dùng
- **`Navbar.tsx`** - Thanh điều hướng chính của ứng dụng
- **`ThemeProvider.tsx`** - Cung cấp chủ đề sáng/tối cho toàn ứng dụng

---

## 📄 **PAGES - CÁC TRANG CHÍNH**

### 🏠 **Trang công cộng**
- **`Home.tsx`** - Trang chủ với giới thiệu và tính năng nổi bật
- **`Leaderboard.tsx`** - Bảng xếp hạng người học giỏi nhất
- **`Progress.tsx`** - Trang theo dõi tiến độ học tập cá nhân
- **`Reading.tsx`** - Trang luyện tập đọc hiểu với các bài tập TOEIC

### 👥 **pages/admin/** - Quản trị hệ thống
- **`Dashboard.tsx`** - Trang tổng quan thống kê hệ thống
- **`SimpleAdminTest.tsx`** - Trang kiểm tra chức năng quản trị đơn giản
- **`UserManagement.tsx`** - Quản lý người dùng: thêm, sửa, xóa, xem chi tiết

---

## 🔧 **SERVICES - DỊCH VỤ XỬ LÝ DỮ LIỆU**

### 🌐 **Kết nối API**
- **`api.ts`** - Dịch vụ gọi API chung với xử lý lỗi và token
- **`supabaseClient.ts`** - Kết nối cơ sở dữ liệu Supabase

### 👨‍💼 **Dành cho quản trị viên**
- **`adminService.ts`** - Quản lý người dùng, thống kê hệ thống, xuất nhập Excel
- **`adminUploadService.ts`** - Tải lên file và tạo bài tập từ nội dung

### 📚 **Học tập và luyện tập**
- **`exerciseService.ts`** - Quản lý bài tập tổng quát
- **`readingExerciseService.ts`** - Xử lý bài tập đọc hiểu TOEIC
- **`dictionaryService.ts`** - Tra cứu từ điển và nghĩa từ vựng

### 📊 **Thống kê và báo cáo**
- **`statsService.ts`** - Thống kê học tập và tiến độ người dùng
- **`databaseStatsService.ts`** - Thống kê cơ sở dữ liệu chi tiết
- **`userManagementService.ts`** - Quản lý thông tin và hoạt động người dùng
- **`userSettingsService.ts`** - Cài đặt cá nhân của người dùng

### 🎯 **Dịch vụ chuyên biệt**
- **`consultationService.ts`** - Tư vấn học tập và lộ trình cá nhân
- **`elevenLabsService.ts`** - Chuyển văn bản thành giọng nói

---

## 🎣 **HOOKS - HOOK TÙY CHỈNH**

### 📊 **Quản trị và thống kê**
- **`useAdminProgress.ts`** - Hook quản lý tiến độ và người dùng cho quản trị viên
- **`useDatabaseStats.ts`** - Hook thống kê cơ sở dữ liệu
- **`useStats.ts`** - Hook thống kê học tập tổng quát

### 📚 **Học tập**
- **`useReadingExercises.ts`** - Hook quản lý bài tập đọc hiểu

### 🔧 **Tiện ích**
- **`use-api.ts`** - Hook gọi API với cache và error handling
- **`use-mobile.tsx`** - Hook phát hiện thiết bị di động
- **`use-toast.ts`** - Hook hiển thị thông báo toast

---

## 🎨 **LAYOUTS - BỐ CỤC TRANG**

### 👨‍💼 **layouts/admin/**
- **`AdminLayout.tsx`** - Bố cục chính cho trang quản trị với sidebar và header

---

## 📚 **LIB - THỦ VIỆN VÀ TIỆN ÍCH**

- **`utils.ts`** - Các hàm tiện ích chung như định dạng ngày, xử lý chuỗi

---

## 📋 **CHỨC NĂNG CHÍNH CỦA HỆ THỐNG**

### 🎯 **Dành cho học viên**
1. **Luyện tập đọc hiểu** - Làm bài tập TOEIC Part 5, 6, 7
2. **Theo dõi tiến độ** - Xem điểm số, thứ hạng, chuỗi học tập
3. **Bảng xếp hạng** - So sánh với học viên khác
4. **Tra từ điển** - Tra nghĩa từ vựng trong bài tập

### 👨‍💼 **Dành cho quản trị viên**
1. **Quản lý người dùng** - Thêm, sửa, xóa, xem chi tiết học viên
2. **Thống kê hệ thống** - Theo dõi hoạt động và hiệu suất
3. **Quản lý bài tập** - Tải lên và tạo bài tập mới
4. **Xuất báo cáo** - Export dữ liệu Excel

### 🔧 **Tính năng kỹ thuật**
1. **Xác thực người dùng** - Đăng nhập, phân quyền
2. **Giao diện responsive** - Tương thích điện thoại, máy tính bảng
3. **Chủ đề sáng/tối** - Chuyển đổi giao diện theo sở thích
4. **Cache thông minh** - Tăng tốc độ tải trang
5. **Xử lý lỗi tự động** - Hiển thị thông báo khi có sự cố

---

## 🎨 **CÔNG NGHỆ SỬ DỤNG**

- **React 18** - Thư viện giao diện người dùng
- **TypeScript** - Ngôn ngữ lập trình có kiểu dữ liệu
- **Vite** - Công cụ build và dev server nhanh
- **React Query** - Quản lý state và cache API
- **React Router** - Điều hướng trang
- **Tailwind CSS** - Framework CSS tiện ích
- **Shadcn/ui** - Thư viện component giao diện
- **Supabase** - Cơ sở dữ liệu và backend
- **Recharts** - Vẽ biểu đồ thống kê

---

## 🚀 **LUỒNG HOẠT ĐỘNG**

### 📱 **Học viên sử dụng app**
1. Truy cập trang chủ → Đăng nhập → Chọn bài tập đọc
2. Làm bài tập → Nhận kết quả → Xem tiến độ
3. Kiểm tra bảng xếp hạng → Tiếp tục học tập

### 👨‍💼 **Quản trị viên quản lý**
1. Đăng nhập admin → Xem dashboard thống kê
2. Quản lý người dùng → Thêm/sửa/xóa học viên
3. Tải lên bài tập mới → Theo dõi hoạt động hệ thống

Hệ thống được thiết kế tập trung vào **bài tập đọc hiểu TOEIC** với giao diện thân thiện và quản lý hiệu quả!