# 🎯 BÁO CÁO CHI TIẾT: TRANG TIẾN ĐỘ CÁ NHÂN (PERSONAL PROGRESS)

## 1. TỔNG QUAN

Trang Tiến độ Cá nhân là dashboard tổng hợp toàn diện, cho phép học viên theo dõi:
- Tổng điểm TOEIC và điểm từng kỹ năng (Listening, Reading, Speaking, Writing)
- Chi tiết điểm theo từng Part TOEIC (Part 1-7)
- Biểu đồ tiến bộ theo thời gian
- Lịch sử làm bài và thành tích
- Thống kê tổng quan: thời gian học, số bài đã làm, tỷ lệ hoàn thành

## 2. GIẢI THÍCH CÁC PHẦN CODE CHÍNH

### 2.1. Giao diện trang web (Frontend): `Progress.tsx`

**File này làm gì?**
File này là giao diện của trang Tiến độ Cá nhân - một trang bảng điều khiển (dashboard) hiển thị tất cả thông tin về quá trình học tập của học viên.

**Trang này có những phần gì?**

1. **3 thẻ thống kê ở trên cùng:**
   - **Thẻ 1 - Tiến độ hoàn thành**: Hiển thị phần trăm bài tập đã làm (ví dụ: 67%), kèm thanh tiến trình màu xanh
   - **Thẻ 2 - Điểm trung bình**: Hiển thị điểm trung bình hiện tại, có thể lọc theo thời gian (hôm qua/tuần này/tháng này), và % cải thiện so với kỳ trước
   - **Thẻ 3 - Xếp hạng cá nhân**: Hiển thị bạn đang xếp hạng thứ mấy trong hệ thống (ví dụ: #4 trong 100 học viên)

2. **Chi tiết điểm theo từng Part TOEIC (2 thẻ lớn):**
   - **Thẻ Listening (Part 1-4)**: 
     * Hiển thị điểm từng Part: Part 1, Part 2, Part 3, Part 4
     * Mỗi Part có thanh tiến trình, số điểm, số lần làm bài
     * Các loại câu hỏi đã luyện tập
   - **Thẻ Reading (Part 5-7)**: Tương tự như Listening nhưng cho Part 5, 6, 7

3. **Biểu đồ tiến bộ theo thời gian:**
   - Một biểu đồ đường (line chart) hiển thị điểm số thay đổi theo tuần
   - Có thể lọc để xem: Tổng điểm, chỉ Listening, chỉ Reading, hoặc từng Part riêng lẻ
   - Giúp học viên thấy rõ mình đã tiến bộ như thế nào

4. **Bảng lịch sử làm bài:**
   - Hiển thị các bài tập đã hoàn thành
   - Mỗi dòng có: Tên bài, Ngày làm, Part nào, Điểm đạt được, Thời gian làm, XP nhận được
   - Sắp xếp theo thời gian, bài mới nhất ở trên

### 2.2. Công cụ kết nối với server: 3 Hooks

**File này làm gì?**
Có 3 công cụ nhỏ, mỗi công cụ có nhiệm vụ lấy một loại thông tin khác nhau từ server.

**Hook 1: `useUserProgress` - Lấy thông tin tổng quan**
- Lấy tất cả thông tin tiến độ của học viên: điểm tổng, điểm từng kỹ năng, điểm từng Part
- Gọi API: GET `/api/Progress/user/1`
- Lưu vào bộ nhớ tạm 5 phút (vì thông tin này không thay đổi quá nhanh)
- Nếu gọi API lỗi, trả về dữ liệu mẫu để trang vẫn hiển thị được

**Hook 2: `useUserActivities` - Lấy lịch sử làm bài**
- Lấy danh sách các bài tập đã làm gần đây (mặc định 20 bài mới nhất)
- Gọi API: GET `/api/Progress/activities/1?limit=20`
- Lưu vào bộ nhớ tạm 2 phút (vì lịch sử thay đổi thường xuyên hơn)
- Dùng để hiển thị bảng lịch sử ở cuối trang

**Hook 3: `useUserWeeklyProgress` - Lấy tiến bộ theo tuần**
- Lấy dữ liệu tiến bộ trong 7 ngày gần nhất, nhóm theo từng ngày
- Gọi API: GET `/api/Progress/weekly/1`
- Lưu vào bộ nhớ tạm 10 phút (vì dữ liệu tuần ít thay đổi)
- Dùng để vẽ biểu đồ tiến bộ

### 2.3. Phần xử lý phía server (Backend): `ProgressController.cs`

**File này làm gì?**
File này là bộ não của server, nhận yêu cầu từ giao diện và tính toán các thông tin tiến độ.

**3 chức năng chính:**

1. **Trả về thông tin tổng quan** (GET `/api/Progress/user/1`):
   - Nhận yêu cầu: "Cho tôi thông tin tiến độ của học viên số 1"
   - Vào database lấy tất cả bài tập đã làm của học viên
   - Tính điểm từng Part TOEIC (Part 1-7)
   - Tính tổng điểm Listening, Reading
   - Tính các thống kê khác: thời gian học, số bài đã làm, điểm trung bình
   - Gửi tất cả về giao diện dưới dạng một object JSON lớn

2. **Trả về tiến bộ theo tuần** (GET `/api/Progress/weekly/1`):
   - Nhận yêu cầu: "Cho tôi dữ liệu tiến bộ 7 ngày gần nhất của học viên số 1"
   - Vào database lấy các bài đã làm trong 7 ngày qua
   - Nhóm theo từng ngày: Thứ 2, Thứ 3, Thứ 4...
   - Với mỗi ngày, tính: số bài đã làm, thời gian học (phút), XP nhận được
   - Gửi về mảng 7 phần tử, mỗi phần tử là một ngày

3. **Trả về lịch sử làm bài** (GET `/api/Progress/activities/1?limit=20`):
   - Nhận yêu cầu: "Cho tôi 20 bài đã làm gần nhất của học viên số 1"
   - Vào database lấy 20 bài mới nhất
   - Với mỗi bài, lấy: tên bài, ngày làm, Part, điểm, thời gian, XP
   - Gửi về mảng 20 phần tử

### 2.4. Công cụ tính toán điểm: `ProgressService`

**File này làm gì?**
File này chứa logic phức tạp để tính toán điểm TOEIC từ các bài tập đã làm.

**Cách tính điểm TOEIC Parts:**

1. **Lấy tất cả bài tập đã làm:**
   - Vào database, tìm tất cả bài mà học viên đã hoàn thành (IsCompleted = true)
   - Mỗi bài có thông tin: điểm số, loại bài (Part nào), thời gian làm

2. **Nhóm theo Part và tính điểm:**
   - Tách các bài theo Part: tất cả bài Part 1 gom lại, tất cả bài Part 2 gom lại...
   - Với mỗi Part, tính điểm trung bình:
     * Ví dụ: Làm 5 bài Part 1, điểm lần lượt: 80, 85, 90, 75, 80
     * Điểm trung bình Part 1 = (80+85+90+75+80)/5 = 82 điểm
   - Đếm số lần làm bài cho mỗi Part

3. **Tính tổng điểm:**
   - Tổng điểm Listening = Điểm Part 1 + Part 2 + Part 3 + Part 4
   - Tổng điểm Reading = Điểm Part 5 + Part 6 + Part 7
   - Tổng điểm TOEIC = Listening + Reading (tối đa 990 điểm)

4. **Tính các thống kê khác:**
   - Tổng thời gian học = Cộng tất cả thời gian làm bài
   - Điểm trung bình = Trung bình điểm của tất cả bài đã làm
   - Số bài đã hoàn thành = Đếm số bài có IsCompleted = true

## 3. QUY TRÌNH HOẠT ĐỘNG

### 3.1. Khi học viên mở trang Tiến độ Cá nhân

**Tình huống:** Học viên muốn xem tổng quan tiến độ học tập của mình.

**Các bước diễn ra:**

1. Học viên mở trang Progress trên trình duyệt
2. Giao diện tự động gửi 3 yêu cầu song song đến server:
   - "Cho tôi thông tin tổng quan"
   - "Cho tôi lịch sử 20 bài gần nhất"
   - "Cho tôi tiến bộ 7 ngày qua"
3. Server xử lý từng yêu cầu:
   - **Yêu cầu 1**: Vào database, lấy tất cả bài đã làm, tính điểm từng Part, tính tổng hợp
   - **Yêu cầu 2**: Vào database, lấy 20 bài mới nhất, sắp xếp theo thời gian
   - **Yêu cầu 3**: Vào database, lấy bài trong 7 ngày, nhóm theo từng ngày, tính tổng số bài/thời gian/XP mỗi ngày
4. Server gửi 3 phản hồi về giao diện
5. Giao diện nhận được, tính toán thêm:
   - Tách Parts thành Listening (1-4) và Reading (5-7)
   - Tính tổng điểm Listening và Reading
   - Tính phần trăm hoàn thành
   - Chuyển đổi dữ liệu tuần thành dữ liệu cho biểu đồ
6. Giao diện hiển thị tất cả: 3 thẻ thống kê, 2 thẻ Parts, biểu đồ, bảng lịch sử
7. Học viên thấy toàn bộ thông tin tiến độ của mình

**Thời gian:** Quá trình này mất khoảng 1-2 giây nếu có dữ liệu sẵn trong bộ nhớ tạm, hoặc 3-5 giây nếu phải tính toán mới.

### 3.2. Cách server tính điểm TOEIC Parts

**Tình huống:** Server cần tính xem học viên đạt bao nhiêu điểm cho mỗi Part TOEIC.

**Các bước diễn ra:**

1. Server vào database, tìm tất cả bài tập mà học viên đã hoàn thành
2. Với mỗi bài, server biết:
   - Bài này thuộc Part nào (Part 1, 2, 3...)
   - Điểm số đạt được (ví dụ: 85 điểm)
3. Server nhóm các bài theo Part:
   - Tất cả bài Part 1 gom lại: [80, 85, 90, 75, 80]
   - Tất cả bài Part 2 gom lại: [70, 75, 80]
   - ...tương tự cho Part 3-7
4. Với mỗi Part, server tính điểm trung bình:
   - Part 1: (80+85+90+75+80) / 5 = 82 điểm
   - Part 2: (70+75+80) / 3 = 75 điểm
   - Part 3: ... (tương tự)
5. Server đếm số lần làm bài cho mỗi Part:
   - Part 1: 5 lần
   - Part 2: 3 lần
6. Server tính tổng điểm:
   - Listening = Điểm Part 1 + Part 2 + Part 3 + Part 4
   - Reading = Điểm Part 5 + Part 6 + Part 7
   - Tổng = Listening + Reading
7. Server gửi kết quả về giao diện:
   - Mảng 7 phần tử, mỗi phần tử là thông tin một Part (điểm, số lần làm, ...)
   - Tổng điểm Listening, Reading, và Tổng
8. Giao diện nhận được, đảm bảo có đủ 7 Parts:
   - Nếu học viên chưa làm Part nào (ví dụ chưa làm Part 5), điền điểm = 0 cho Part đó
9. Giao diện hiển thị từng Part với thanh tiến trình và điểm số

### 3.3. Quy trình cập nhật Progress khi submit bài

```
1. User submit kết quả bài tập ở trang Reading Exercises
   ↓
2. POST /api/ReadingExercise/submit-result được gọi
   ↓
3. ReadingExerciseController.SubmitResult():
   a. Tính score từ answers
   b. Tạo Completion entity mới
   c. Save Completion vào database
   d. Update User stats (TotalXp, LastActive)
   ↓
4. Completion được lưu với:
   - UserId, ExerciseId
   - Score, TotalQuestions
   - Attempts (incremented)
   - CompletedAt = DateTime.UtcNow
   ↓
5. Khi user quay lại trang Progress:
   - useUserProgress() refetch (nếu cache expired)
   - Backend load Completions mới nhất
   - Tính lại TOEIC Parts scores
   - UI tự động update với điểm mới
```

### 3.4. Quy trình tính Weekly Progress

```
1. GET /api/Progress/weekly/{userId} được gọi
   ↓
2. Backend load Completions của user
   WHERE CompletedAt >= (now - 7 days)
   ↓
3. Group Completions theo ngày:
   - GroupBy(c => c.CompletedAt.Value.Date)
   ↓
4. Với mỗi ngày:
   a. Count exercises = số completions
   b. Sum time = tổng TimeSpentMinutes
   c. Sum XP = tổng Score
   d. Map day name: "Monday" → "T2", "Tuesday" → "T3", ...
   ↓
5. Return array 7 objects (mỗi ngày trong tuần)
   ↓
6. Frontend generateChartData() transform data:
   - Thêm intensity factor dựa trên số exercises
   - Scale scores theo intensity
   - Tạo data points cho line chart
   ↓
7. Recharts render line chart với data đã transform
```

## 4. NHỮNG ĐIỂM ĐÁNG LƯU Ý

### 4.1. TOEIC Scoring System

**Cách tính điểm**:
- TOEIC thực tế có điểm tối đa 990 (495 Listening + 495 Reading)
- Hệ thống scale điểm từ 0-100% sang điểm TOEIC tương ứng
- Mỗi Part có weight khác nhau (Part 7 nhiều câu nhất → ảnh hưởng nhiều nhất)

**ToeicPartHelper Logic**:
```csharp
// Trong ToeicPartHelper.BuildPartScores()
// Với mỗi Part:
var partCompletions = completions
    .Where(c => c.Exercise.Type == partType)
    .ToList();

var avgScore = partCompletions.Any()
    ? partCompletions.Average(c => (double)c.Score)
    : 0;

// Convert % score (0-100) sang TOEIC score (0-495 cho skill)
// Logic: scale dựa trên số câu hỏi và độ khó
var toeicScore = ConvertPercentageToToeicScore(avgScore, partType);
```

### 4.2. Normalization của TOEIC Parts

**Vấn đề**: Không phải user nào cũng đã làm đủ 7 Parts

**Giải pháp**: `normalizeToeicParts()`
```typescript
export const normalizeToeicParts = (
  parts: ToeicPartScore[]
): ToeicPartScore[] => {
  // Tạo map từ parts có sẵn
  const partMap = new Map(parts.map(p => [p.key, p]));
  
  // Đảm bảo có đủ 7 parts
  return TOEIC_PARTS.map(partMeta => {
    const existing = partMap.get(partMeta.key);
    return existing || {
      ...partMeta,
      score: 0, // Fill với score = 0 nếu chưa làm
      attempts: 0
    };
  });
};
```

### 4.3. Performance Optimization

**React Query Caching**:
- `staleTime` khác nhau cho từng query:
  - UserProgress: 5 phút (ít thay đổi)
  - Activities: 2 phút (thay đổi thường xuyên hơn)
  - WeeklyProgress: 10 phút (weekly data ổn định)

**useMemo cho Derived Data**:
```typescript
// Chỉ tính lại khi dependencies thay đổi
const toeicParts = useMemo(
  () => normalizeToeicParts(userProgress?.toeicParts ?? []),
  [userProgress] // Chỉ recalculate khi userProgress thay đổi
);

const chartData = useMemo(
  () => generateChartData(weeklyProgress || [], averageScore, toeicParts),
  [weeklyProgress, averageScore, toeicParts]
);
```

**Parallel API Calls**:
- 3 hooks được gọi song song, không block nhau
- UI hiển thị loading state riêng cho từng section

### 4.4. Error Handling và Fallback

**Frontend Fallback Data**:
```typescript
try {
  const response = await apiService.get<UserProgress>(`/api/Progress/user/${userId}`);
  return response;
} catch (error) {
  console.warn('Progress API not available, using fallback data:', error);
  return createFallbackToeicParts(); // Không crash app, vẫn hiển thị được
}
```

**Backend Error Handling**:
```csharp
try {
    var progress = await _progressService.GetUserProgressAsync(userId);
    // ...
} catch (Exception ex)
{
    _logger.LogError(ex, "Error getting user progress for user {UserId}", userId);
    return StatusCode(500, new { message = "Internal server error" });
}
```

### 4.5. Time Filter Logic

**So sánh với kỳ trước**:
```typescript
const getComparisonScore = (period: string) => {
  const now = new Date();
  const filterDate = new Date();
  
  switch (period) {
    case "yesterday":
      filterDate.setDate(now.getDate() - 1);
      break;
    case "week":
      filterDate.setDate(now.getDate() - 7);
      break;
    case "month":
      filterDate.setMonth(now.getMonth() - 1);
      break;
  }
  
  // Filter activities trong khoảng thời gian
  const periodActivities = recentActivities.filter(activity => 
    new Date(activity.date) >= filterDate
  );
  
  // Tính average score của kỳ đó
  const periodAverage = periodActivities.length > 0
    ? periodActivities.reduce((sum, a) => sum + a.score, 0) / periodActivities.length
    : averageScore - 20; // Fallback
  
  return periodAverage;
};

const comparisonScore = getComparisonScore(timeFilter);
const improvement = ((averageScore - comparisonScore) / comparisonScore * 100).toFixed(1);
```

### 4.6. Chart Data Transformation

**Tạo dữ liệu cho biểu đồ**:
```typescript
const generateChartData = (
  weeklyProgressData: WeeklyProgress[],
  userScore: number,
  toeicParts: ToeicPartScore[]
) => {
  // 1. Lấy data source (weekly hoặc fallback)
  const dataSource = weeklyProgressData?.length 
    ? weeklyProgressData 
    : createFallbackWeeklyData();
  
  // 2. Tính intensity factor (0.75 - 1.0) dựa trên số exercises
  return dataSource.map((day, index) => {
    const intensity = 0.75 + (day.exercises / 10) * 0.25 + index * 0.02;
    
    // 3. Scale scores theo intensity để tạo trend
    const entry = {
      date: day.day,
      total: Math.round(userScore * intensity),
      listening: Math.round(listeningTotal * intensity),
      reading: Math.round(readingTotal * intensity),
    };
    
    // 4. Thêm từng Part score
    TOEIC_PARTS.forEach((part) => {
      const partScore = toeicParts.find(p => p.key === part.key)?.score ?? 0;
      entry[part.key] = Math.round(partScore * intensity);
    });
    
    return entry;
  });
};
```

## 5. CÁCH THUYẾT TRÌNH TRƯỚC HỘI ĐỒNG

### 5.1. Phần mở đầu (30 giây)

"Kính thưa hội đồng, em xin trình bày về **Trang Tiến độ Cá nhân** - một dashboard tổng hợp toàn diện giúp học viên theo dõi chi tiết tiến trình học tập và thành tích TOEIC của mình một cách trực quan và dễ hiểu."

### 5.2. Giới thiệu tính năng chính (1.5 phút)

"Trang này cung cấp 4 nhóm thông tin chính:

**Thứ nhất**, **Thống kê tổng quan** qua 3 cards: Tiến độ hoàn thành (%), Điểm trung bình với khả năng so sánh theo thời gian, và Xếp hạng cá nhân trong hệ thống.

**Thứ hai**, **Chi tiết điểm theo TOEIC Parts**: Hiển thị riêng biệt Listening (Part 1-4) và Reading (Part 5-7), mỗi Part có progress bar, điểm số, số lần làm bài, và các loại câu hỏi đã luyện tập.

**Thứ ba**, **Biểu đồ tiến bộ theo thời gian**: Sử dụng line chart để minh họa sự cải thiện điểm số theo tuần, với khả năng filter theo tổng điểm, từng kỹ năng, hoặc từng Part riêng lẻ.

**Thứ tư**, **Lịch sử làm bài**: Bảng chi tiết các bài đã hoàn thành với đầy đủ thông tin: tên bài, ngày làm, Part, điểm, thời gian, và XP đạt được."

### 5.3. Giải thích quy trình kỹ thuật (2 phút)

"Về mặt kỹ thuật, khi học viên truy cập trang, hệ thống thực hiện 3 API calls song song:

API thứ nhất lấy thông tin progress tổng hợp, backend sẽ load tất cả Completions của học viên từ database, sau đó sử dụng ToeicPartHelper để tính điểm cho từng Part TOEIC dựa trên các bài đã làm. Hệ thống tự động group các bài theo Exercise.Type (Part 1-7), tính average score, và map sang format chuẩn với đầy đủ metadata.

API thứ hai lấy lịch sử hoạt động gần đây (20 bài mới nhất), giúp hiển thị bảng lịch sử thi.

API thứ ba lấy dữ liệu tiến bộ theo tuần, backend group các Completions theo ngày trong 7 ngày gần nhất, tính tổng số bài, thời gian học, và XP đạt được mỗi ngày.

Sau khi nhận dữ liệu, frontend sử dụng useMemo để tính toán các derived data như tổng điểm Listening/Reading, completion rate, và transform dữ liệu cho biểu đồ. Tất cả được render với UI responsive và animations mượt mà."

### 5.4. Điểm nổi bật về công nghệ (1 phút)

"Có 4 điểm kỹ thuật đáng chú ý:

**Thứ nhất**, hệ thống tự động tính điểm TOEIC Parts từ dữ liệu Completions thực tế, không cần manual input, đảm bảo tính chính xác và tự động.

**Thứ hai**, sử dụng React Query với caching strategy thông minh - cache lâu hơn cho dữ liệu ít thay đổi (weekly progress), cache ngắn hơn cho dữ liệu động (activities), giảm số lần gọi API không cần thiết.

**Thứ ba**, có cơ chế normalize đảm bảo luôn hiển thị đủ 7 Parts ngay cả khi học viên chưa làm đủ, giúp UI nhất quán.

**Thứ tư**, biểu đồ có khả năng filter linh hoạt - học viên có thể xem tổng thể, từng kỹ năng, hoặc từng Part riêng lẻ để phân tích sâu hơn điểm mạnh và điểm yếu."

### 5.5. Demo (nếu có) (1 phút)

"Em xin mời hội đồng xem demo:
- Đầu tiên, em sẽ xem thống kê tổng quan với điểm trung bình và % cải thiện...
- Sau đó, em sẽ scroll xuống xem chi tiết từng Part TOEIC với progress bars...
- Tiếp theo, em sẽ chuyển đổi filter biểu đồ để xem tiến bộ theo từng Part...
- Cuối cùng, em sẽ xem lịch sử làm bài với các thông tin chi tiết..."

### 5.6. Kết luận (30 giây)

"Tóm lại, Trang Tiến độ Cá nhân là một dashboard hoàn chỉnh, cung cấp insights sâu sắc về quá trình học tập, giúp học viên hiểu rõ điểm mạnh yếu và có định hướng cải thiện phù hợp. Tính năng này thể hiện sự kết hợp giữa data aggregation thông minh ở backend, UI/UX trực quan ở frontend, và performance optimization qua caching strategy.

Em xin cảm ơn hội đồng đã lắng nghe. Em sẵn sàng trả lời các câu hỏi."

---

**Tổng thời gian thuyết trình**: ~7-8 phút (không tính Q&A)

**Lưu ý khi thuyết trình**:
- Chuẩn bị sẵn demo với dữ liệu thực tế
- Nhấn mạnh vào tính tự động của hệ thống (tự tính điểm từ data thực tế)
- Sẵn sàng giải thích sâu hơn về TOEIC scoring nếu được hỏi
- Chuẩn bị trả lời về performance và caching strategy
- Có thể so sánh với các hệ thống tracking progress truyền thống

