# 📚 BÁO CÁO CHI TIẾT: TRANG BÀI TẬP ĐỌC (READING EXERCISES)

## 1. TỔNG QUAN

Trang Bài tập đọc là một trong những tính năng cốt lõi của hệ thống EngAce, cho phép học viên:
- Xem danh sách bài tập đọc hiểu TOEIC (Part 5, 6, 7)
- Tạo bài tập mới tự động bằng AI (Gemini hoặc OpenAI GPT)
- Lọc bài tập theo độ khó và nguồn gốc
- Làm bài và nộp kết quả để tính điểm

## 2. GIẢI THÍCH CÁC PHẦN CODE CHÍNH

### 2.1. Giao diện trang web (Frontend): `ReadingExercises.tsx`

**File này làm gì?**
File này là giao diện chính của trang Bài tập Đọc. Nó giống như một bức tranh hiển thị tất cả bài tập và cho phép bạn tương tác với chúng.

**Trang này có những phần gì?**

1. **Đầu trang (Header)**: Có tiêu đề "Reading Exercises" và nút "Generate with AI" để mở form tạo bài tập mới.

2. **Bộ lọc (Filter)**: Hai dropdown menu cho phép bạn:
   - Lọc theo độ khó: Tất cả / Beginner (Cơ bản) / Intermediate (Trung bình) / Advanced (Nâng cao)
   - Lọc theo nguồn: Tất cả / AI Generated (Tạo bởi AI) / Admin Upload (Upload bởi admin)

3. **Form tạo bài tập bằng AI**: Khi nhấn "Generate with AI", một form sẽ hiện ra với các ô nhập:
   - **Topic**: Nhập chủ đề bạn muốn (ví dụ: "Business Communication")
   - **Level**: Chọn độ khó
   - **Type**: Chọn loại bài (Part 5, 6, hoặc 7)
   - **Provider**: Chọn AI nào tạo bài (Gemini miễn phí hoặc OpenAI GPT có phí)

4. **Danh sách bài tập**: Hiển thị tất cả bài tập dưới dạng các thẻ card, mỗi card có:
   - Tên bài tập
   - Độ khó
   - Loại bài (Part 5/6/7)
   - Nút để làm bài

**Cách hoạt động đơn giản:**
- Khi trang được mở, nó tự động gọi API để lấy danh sách bài tập từ server
- Khi bạn chọn filter, danh sách sẽ tự động lọc lại theo tiêu chí bạn chọn
- Khi bạn nhấn tạo bài mới, form sẽ gửi yêu cầu đến server, server sẽ gọi AI để tạo bài, sau đó hiển thị bài mới trong danh sách

### 2.2. Công cụ kết nối với server: `useReadingExercises.ts`

**File này làm gì?**
File này giống như một người trung gian giữa giao diện và server. Nó chịu trách nhiệm:
- Lấy danh sách bài tập từ server
- Gửi yêu cầu tạo bài mới đến server
- Gửi kết quả làm bài đến server

**Ba chức năng chính:**

1. **Lấy danh sách bài tập**:
   - Tự động gọi API khi trang được mở
   - Lưu kết quả vào bộ nhớ tạm (cache) trong 5 phút để không phải gọi lại nhiều lần
   - Nếu gọi API lỗi, sẽ trả về danh sách rỗng thay vì làm ứng dụng bị lỗi

2. **Tạo bài tập mới bằng AI**:
   - Nhận thông tin từ form (chủ đề, độ khó, loại bài, AI provider)
   - Gửi yêu cầu đến server
   - Khi server trả về bài mới, tự động thêm vào danh sách hiện tại
   - Hiển thị thông báo "AI Exercise Generated!"

3. **Nộp kết quả làm bài**:
   - Nhận ID bài tập và câu trả lời của học viên
   - Gửi đến server để chấm điểm
   - Server sẽ tính điểm và lưu vào database

**Tại sao cần file này?**
Thay vì viết code gọi API ở nhiều chỗ khác nhau, chúng ta viết tập trung ở một chỗ, sau đó dùng lại. Giống như có một số điện thoại chung để gọi server, mọi nơi chỉ cần gọi số đó thôi.

### 2.3. Phần xử lý phía server (Backend): `ReadingExerciseController.cs`

**File này làm gì?**
File này là bộ não của server, nhận yêu cầu từ giao diện và xử lý chúng. Giống như một nhân viên phục vụ trong nhà hàng, nhận order từ khách và chuyển đến bếp.

**Ba chức năng chính của server:**

1. **Trả về danh sách bài tập** (GET `/api/ReadingExercise`):
   - Nhận yêu cầu từ giao diện: "Cho tôi danh sách bài tập"
   - Vào database tìm tất cả bài tập đang hoạt động
   - Có thể lọc theo độ khó nếu có yêu cầu
   - Sắp xếp theo ngày tạo (bài mới nhất lên đầu)
   - Gửi danh sách về giao diện

2. **Tạo bài tập mới bằng AI** (POST `/api/ReadingExercise/generate-ai`):
   - Nhận thông tin từ form: chủ đề, độ khó, loại bài, AI provider
   - Bước 1: Tạo nội dung cơ bản từ chủ đề
   - Bước 2: Xác định số câu hỏi cần tạo:
     * Part 5: 30 câu
     * Part 6: 16 câu  
     * Part 7: 54 câu
   - Bước 3: Gọi AI service (Gemini hoặc OpenAI):
     * Nếu là Part 6 hoặc 7: AI sẽ tạo cả đoạn văn (passage) và câu hỏi
     * Nếu là Part 5: AI chỉ tạo câu hỏi (không có đoạn văn)
   - Bước 4: Lưu bài tập vào database với:
     * Tiêu đề, nội dung, câu hỏi (dạng JSON)
     * Đánh dấu là "ai" (tạo bởi AI)
     * Thời gian tạo, người tạo
   - Bước 5: Gửi bài tập vừa tạo về giao diện

3. **Chấm điểm khi nộp bài** (POST `/api/ReadingExercise/submit-result`):
   - Nhận ID bài tập và câu trả lời của học viên
   - Bước 1: Tìm bài tập trong database và lấy đáp án đúng
   - Bước 2: So sánh câu trả lời với đáp án đúng:
     * Đếm số câu đúng
     * Tính điểm: (số câu đúng / tổng số câu) × 100
   - Bước 3: Đếm xem học viên đã làm bài này bao nhiêu lần:
     * Nếu đã làm rồi: lần làm = số lần cũ + 1
     * Nếu chưa làm: lần làm = 1
   - Bước 4: Lưu kết quả vào database:
     * Điểm số, số câu đúng, câu trả lời của học viên
     * Số lần làm bài
     * Thời gian hoàn thành
   - Bước 5: Gửi điểm về giao diện để hiển thị cho học viên

## 3. QUY TRÌNH HOẠT ĐỘNG CỤ THỂ

### 3.1. Khi học viên mở trang và xem danh sách bài tập

**Tình huống:** Học viên vào trang Reading Exercises để xem có những bài tập nào.

**Các bước diễn ra:**

1. Học viên mở trang trên trình duyệt
2. Giao diện tự động gửi yêu cầu đến server: "Cho tôi danh sách bài tập"
3. Server vào database, tìm tất cả bài tập đang hoạt động
4. Server gửi danh sách về giao diện (dạng JSON - một dạng dữ liệu máy tính dễ đọc)
5. Giao diện nhận được, hiển thị dưới dạng các card (thẻ) xinh xắn
6. Nếu học viên chọn lọc (ví dụ: chỉ xem bài Intermediate), danh sách tự động lọc lại ngay lập tức

**Ví dụ cụ thể:**
- Học viên thấy 10 bài tập hiển thị trên màn hình
- Chọn filter "Intermediate"
- Chỉ còn 5 bài Intermediate hiển thị, 5 bài khác bị ẩn đi

### 3.2. Khi học viên tạo bài tập mới bằng AI

**Tình huống:** Học viên muốn tạo một bài tập mới về chủ đề "Business Communication", độ khó Intermediate, loại Part 7.

**Các bước diễn ra:**

1. Học viên nhấn nút "Generate with AI"
2. Form hiện ra, học viên điền thông tin:
   - Chủ đề: "Business Communication"
   - Độ khó: "Intermediate" 
   - Loại bài: "Part 7"
   - Chọn AI: "Gemini" (miễn phí)
3. Học viên nhấn "Generate Exercise"
4. Giao diện gửi yêu cầu đến server với tất cả thông tin vừa nhập
5. Server nhận được, bắt đầu xử lý:
   - Tạo một đoạn hướng dẫn (prompt) cho AI: "Tạo một bài Part 7 về Business Communication, độ khó Intermediate, có 54 câu hỏi"
   - Gửi prompt này đến AI Gemini qua Internet
6. AI Gemini nhận được, suy nghĩ và tạo ra:
   - Một đoạn văn dài về Business Communication (passage)
   - 54 câu hỏi trắc nghiệm với 4 đáp án cho mỗi câu
   - Đánh dấu đáp án đúng cho từng câu
7. AI gửi kết quả về server
8. Server nhận được, chuyển đổi thành định dạng chuẩn và lưu vào database
9. Server gửi bài tập vừa tạo về giao diện
10. Giao diện nhận được, tự động thêm vào danh sách bài tập
11. Hiển thị thông báo "AI Exercise Generated!" 
12. Học viên thấy bài mới xuất hiện trong danh sách, có thể làm ngay

**Thời gian:** Quá trình này mất khoảng 10-30 giây tùy thuộc vào AI provider và độ phức tạp của bài tập.

### 3.3. Khi học viên làm bài và nộp kết quả

**Tình huống:** Học viên đã làm xong một bài tập Part 7 có 54 câu, đã chọn đáp án cho tất cả câu.

**Các bước diễn ra:**

1. Học viên làm bài trên giao diện, chọn đáp án cho từng câu
2. Sau khi làm xong, nhấn nút "Submit"
3. Giao diện gửi đến server:
   - ID bài tập
   - Mảng câu trả lời: [0, 1, 2, 0, 3, ...] (mỗi số là đáp án học viên chọn)
4. Server nhận được, bắt đầu chấm điểm:
   - Lấy bài tập từ database, có sẵn đáp án đúng
   - So sánh từng câu: câu trả lời của học viên vs đáp án đúng
   - Đếm số câu đúng: ví dụ 45/54 câu
   - Tính điểm: (45/54) × 100 = 83.33 điểm
5. Server kiểm tra xem học viên đã làm bài này bao nhiêu lần:
   - Lần đầu: ghi nhận là lần 1
   - Lần thứ 2: ghi nhận là lần 2
   - (Học viên có thể làm lại để cải thiện điểm)
6. Server lưu kết quả vào database:
   - Điểm số: 83.33
   - Số câu đúng: 45
   - Tổng số câu: 54
   - Lần làm bài: 1 (hoặc 2, 3...)
   - Thời gian hoàn thành: 2024-01-15 14:30:00
7. Server cập nhật thống kê của học viên:
   - Tổng điểm tích lũy (XP) tăng lên
   - Thời gian hoạt động cuối cùng được cập nhật
8. Server gửi kết quả về giao diện: "Bạn đạt 83.33 điểm, đúng 45/54 câu"
9. Giao diện hiển thị kết quả cho học viên với thông báo "Results Saved!"
10. Học viên có thể xem chi tiết hoặc quay lại danh sách bài tập

## 4. NHỮNG ĐIỂM QUAN TRỌNG CẦN BIẾT

### 4.1. Tích hợp AI - Hệ thống hỗ trợ 2 loại AI

**Hai công cụ AI có sẵn:**

1. **Gemini** (của Google):
   - Miễn phí sử dụng với một lượng nhất định mỗi tháng
   - Chất lượng tốt, tạo bài tập nhanh
   - Phù hợp cho việc sử dụng thường xuyên

2. **OpenAI GPT** (ChatGPT):
   - Có phí nhưng chất lượng rất cao
   - Tạo bài tập chi tiết và chính xác hơn
   - Phù hợp khi cần bài tập chất lượng cao

**Cách hoạt động:**
- Học viên tự chọn AI nào muốn dùng trong dropdown menu
- Server sẽ tự động gọi đúng AI mà học viên chọn
- Không cần phải cấu hình gì thêm, chọn là dùng được ngay

**Bảo mật:**
- Mã khóa API (giống như chìa khóa để truy cập AI) được lưu trong file cấu hình riêng
- File này không được upload lên Internet để tránh bị lộ
- Chỉ có server mới biết mã khóa, giao diện không biết

### 4.2. Cách hệ thống lưu trữ và quản lý dữ liệu

**Lưu trữ dữ liệu từ server (React Query):**
- Khi lấy danh sách bài tập từ server, hệ thống lưu vào bộ nhớ tạm trong 5 phút
- Trong 5 phút đó, nếu học viên quay lại trang, không cần gọi server lại, dùng dữ liệu đã lưu
- Giống như đọc báo, đọc xong để đó 5 phút, trong thời gian đó không cần tải lại
- Sau 5 phút, dữ liệu được coi là "cũ", sẽ tự động lấy mới khi cần

**Lưu trữ thông tin giao diện (Local State):**
- Những thứ như: filter đang chọn gì, form đang mở hay đóng, học viên đang chọn bài nào
- Những thông tin này chỉ tồn tại trong trình duyệt, không gửi lên server
- Mất đi khi học viên đóng trang, không lưu vĩnh viễn

### 4.3. Xử lý khi có lỗi xảy ra

**Khi giao diện gặp lỗi:**
- Nếu gọi server bị lỗi (mất mạng, server sập...), hệ thống không bị crash
- Thay vào đó, hiển thị danh sách rỗng hoặc thông báo lỗi nhẹ nhàng
- Học viên vẫn có thể sử dụng các chức năng khác của trang
- Giống như khi gọi điện thoại không được, bạn vẫn làm được việc khác

**Khi server gặp lỗi:**
- Server ghi lại lỗi vào file log để developer có thể xem và sửa
- Gửi thông báo lỗi về giao diện: "Đã xảy ra lỗi, vui lòng thử lại sau"
- Không để lộ thông tin nhạy cảm trong thông báo lỗi

### 4.4. Đếm số lần làm bài

**Vấn đề:**
- Khi học viên làm lại một bài tập, cần đếm xem đã làm bao nhiêu lần
- Server phải tìm trong database xem học viên đã làm bài này mấy lần rồi

**Cách giải quyết:**
1. Lấy tất cả lần làm bài của học viên cho bài tập đó từ database
2. Tìm lần làm có số cao nhất (ví dụ: đã làm lần 1, 2, 3 → lần làm tiếp theo là lần 4)
3. Nếu chưa làm lần nào, coi như lần 1

### 4.5. Cách lưu câu hỏi vào database

**Vấn đề:**
- Mỗi bài tập có nhiều câu hỏi (30, 54, hoặc 16 câu)
- Mỗi câu hỏi có: nội dung câu hỏi, 4 đáp án, đáp án đúng, giải thích...

**Giải pháp:**
- Tất cả câu hỏi được chuyển thành một đoạn văn bản dạng JSON (một dạng dữ liệu có cấu trúc)
- Lưu toàn bộ vào một cột trong database tên là "Questions"
- Khi cần dùng, server đọc đoạn văn bản đó ra và chuyển lại thành danh sách câu hỏi
- Giống như đóng gói tất cả câu hỏi vào một hộp, lưu hộp vào kho, khi cần thì mở hộp ra

**Ví dụ:**
```
Cột Questions trong database chứa:
[
  {"questionText": "Câu hỏi 1", "options": ["A", "B", "C", "D"], "correctAnswer": 0},
  {"questionText": "Câu hỏi 2", "options": ["A", "B", "C", "D"], "correctAnswer": 2},
  ...
]
```

### 4.6. Sự khác biệt giữa Part 5, Part 6, và Part 7

**Part 5 - Hoàn thành câu:**
- Không có đoạn văn dài
- Chỉ có các câu hỏi riêng lẻ, mỗi câu là một câu tiếng Anh chưa hoàn chỉnh
- Học viên chọn từ/cụm từ để điền vào chỗ trống
- Giống như bài điền từ vào chỗ trống

**Part 6 - Hoàn thành đoạn văn:**
- Có một đoạn văn ngắn (khoảng 200-300 từ)
- Có các câu hỏi yêu cầu điền từ vào chỗ trống trong đoạn văn
- Học viên vừa đọc đoạn văn, vừa làm bài

**Part 7 - Đọc hiểu:**
- Có một hoặc nhiều đoạn văn dài (có thể đến 1000 từ)
- Có nhiều câu hỏi đọc hiểu về nội dung đoạn văn
- Học viên phải đọc kỹ đoạn văn để trả lời câu hỏi
- Giống như bài đọc hiểu truyền thống

**Cách hệ thống xử lý:**
- Khi tạo Part 5: Chỉ cần AI tạo câu hỏi, không cần đoạn văn
- Khi tạo Part 6 hoặc 7: AI phải tạo cả đoạn văn VÀ câu hỏi liên quan đến đoạn văn đó

## 5. CÁCH THUYẾT TRÌNH TRƯỚC HỘI ĐỒNG

### 5.1. Phần mở đầu (30 giây)

"Kính thưa hội đồng, em xin trình bày về **Trang Bài tập Đọc** - một trong những tính năng cốt lõi của hệ thống EngAce, giúp học viên luyện tập các phần Reading của kỳ thi TOEIC (Part 5, 6, 7) với sự hỗ trợ của công nghệ AI."

### 5.2. Giới thiệu tính năng chính (1 phút)

"Trang này bao gồm 3 chức năng chính:

**Thứ nhất**, hiển thị danh sách bài tập đọc hiểu TOEIC với đầy đủ thông tin: độ khó (Beginner/Intermediate/Advanced), loại bài (Part 5/6/7), và nguồn gốc (AI Generated hoặc Admin Upload).

**Thứ hai**, cho phép học viên tự tạo bài tập mới bằng AI với chỉ một vài thao tác: nhập chủ đề, chọn độ khó, chọn loại bài, và chọn AI provider (Gemini hoặc OpenAI GPT). Hệ thống sẽ tự động tạo ra bài tập phù hợp với format TOEIC.

**Thứ ba**, học viên có thể làm bài và nộp kết quả ngay trên trang, hệ thống sẽ tự động chấm điểm và lưu vào lịch sử học tập."

### 5.3. Giải thích quy trình kỹ thuật (2 phút)

"Về mặt kỹ thuật, em xin trình bày quy trình tạo bài tập bằng AI như sau:

Khi học viên nhấn nút 'Generate Exercise', frontend sẽ gửi request đến backend API với các tham số: topic, level, type, và provider. Backend nhận request và gọi AI Service tương ứng (Gemini hoặc OpenAI).

AI Service sẽ tạo một prompt chi tiết dựa trên các tham số, sau đó gọi API của AI provider. Response từ AI sẽ được parse thành cấu trúc questions JSON chuẩn.

Backend sau đó tạo một Exercise entity mới, lưu vào database MySQL, và trả về cho frontend. Frontend cập nhật danh sách bài tập ngay lập tức mà không cần reload trang nhờ React Query cache."

### 5.4. Điểm nổi bật về công nghệ (1 phút)

"Có 3 điểm kỹ thuật đáng chú ý:

**Thứ nhất**, hệ thống hỗ trợ 2 AI provider (Gemini và OpenAI), cho phép học viên lựa chọn tùy theo nhu cầu và ngân sách.

**Thứ hai**, questions được lưu dưới dạng JSON trong database, cho phép linh hoạt về cấu trúc câu hỏi mà không cần thay đổi schema.

**Thứ ba**, hệ thống xử lý attempt tracking tự động - mỗi lần học viên làm lại bài, attempt number sẽ tăng lên, giúp theo dõi tiến độ cải thiện."

### 5.5. Demo (nếu có) (1 phút)

"Em xin mời hội đồng xem demo:
- Đầu tiên, em sẽ tạo một bài tập Part 7 về chủ đề 'Business Communication' bằng Gemini AI...
- Sau đó, em sẽ lọc bài tập theo level Intermediate...
- Cuối cùng, em sẽ làm bài và nộp kết quả để xem điểm số..."

### 5.6. Kết luận (30 giây)

"Tóm lại, Trang Bài tập Đọc là một tính năng hoàn chỉnh, tích hợp công nghệ AI hiện đại, giúp học viên luyện tập hiệu quả và theo dõi tiến độ một cách chi tiết. Tính năng này thể hiện sự kết hợp giữa frontend React hiện đại, backend .NET Core mạnh mẽ, và công nghệ AI tiên tiến.

Em xin cảm ơn hội đồng đã lắng nghe. Em sẵn sàng trả lời các câu hỏi."

---

**Tổng thời gian thuyết trình**: ~6-7 phút (không tính Q&A)

**Lưu ý khi thuyết trình**:
- Chuẩn bị sẵn demo để minh họa nếu có thể
- Nhấn mạnh vào điểm khác biệt: AI integration, dual provider support
- Sẵn sàng giải thích sâu hơn về quy trình AI nếu được hỏi
- Chuẩn bị trả lời về security của API keys

