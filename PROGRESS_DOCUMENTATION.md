# 🎯 TÀI LIỆU TRANG TIẾN ĐỘ CÁ NHÂN (Progress)

## 📋 MỤC LỤC
1. [Tổng quan](#tổng-quan)
2. [Các file liên quan](#các-file-liên-quan)
3. [Cấu trúc code](#cấu-trúc-code)
4. [Logic và quy trình hoạt động](#logic-và-quy-trình-hoạt-động)
5. [Luồng dữ liệu](#luồng-dữ-liệu)
6. [Các tính năng chính](#các-tính-năng-chính)

---

## 🎯 TỔNG QUAN

Trang **Tiến độ cá nhân** hiển thị chi tiết progress của học viên bao gồm:
- **Tổng quan**: Total score, XP, study time, completed exercises
- **4 Skills**: Listening, Speaking, Reading, Writing scores
- **TOEIC Parts**: Điểm chi tiết từng phần (Part 1-7)
- **Weekly Progress**: Biểu đồ tiến độ theo tuần
- **Recent Activities**: Danh sách các hoạt động gần đây
- **Achievements**: Các thành tựu đã đạt được

**Tính năng chính:**
- Real-time progress tracking
- Interactive charts (Line charts, Progress bars)
- Filter theo skill và TOEIC parts
- Weekly/daily breakdown
- Achievement system

---

## 📁 CÁC FILE LIÊN QUAN

### **Backend (C# .NET)**

#### Controllers
- `EngAce/EngAce.Api/Controllers/ProgressController.cs`
  - Controller chính xử lý progress APIs
  - Routes: `/api/Progress/*`

#### Services
- `EngAce/EngAce.Api/Services/ProgressService.cs`
  - Service layer xử lý business logic
  - Calculate progress, weekly stats, activities

#### Interfaces
- `EngAce/EngAce.Api/Services/Interfaces/IProgressService.cs`
  - Interface định nghĩa methods

#### Helpers
- `EngAce/EngAce.Api/Helpers/ToeicPartHelper.cs`
  - Helper để tính điểm các phần TOEIC
  - BuildPartScores(), SumListening(), SumReading()

- `EngAce/EngAce.Api/Helpers/UserProfileHelper.cs`
  - Helper để tính profile metadata
  - GetProfileTier(int totalXp)
  - CalculateStudyStreak(IEnumerable<Completion>)

#### DTOs
- `EngAce/EngAce.Api/DTO/Core/ProgressDto.cs`
  - `UserProgressDto`: DTO cho user progress tổng quan
  - `WeeklyProgressDto`: DTO cho weekly progress
  - `ActivityDto`: DTO cho recent activities

- `EngAce/EngAce.Api/DTO/Shared/ToeicPartDto.cs`
  - `ToeicPartScore`: DTO cho điểm từng phần TOEIC

### **Frontend (React TypeScript)**

#### Pages
- `english-mentor-buddy/src/pages/Progress.tsx`
  - Component chính của trang progress

#### Services
- `english-mentor-buddy/src/services/api.ts`
  - API service cho progress calls

#### Hooks
- Custom hooks inline trong component hoặc separate files:
  - `useUserProgress(userId)`
  - `useWeeklyProgress(userId)`
  - `useUserActivities(userId)`

#### Utils
- `english-mentor-buddy/src/utils/toeicParts.ts`
  - Utilities cho TOEIC parts normalization

#### Constants
- `english-mentor-buddy/src/constants/toeicParts.ts`
  - `TOEIC_PARTS`: Constants định nghĩa các phần TOEIC

---

## 🏗️ CẤU TRÚC CODE

### **Backend Architecture**

```
ProgressController
    ├── ProgressService (Business Logic)
    │   ├── ToeicPartHelper (Part Score Calculation)
    │   ├── UserProfileHelper (Profile Metadata)
    │   └── ApplicationDbContext (Database Access)
    └── DTOs (Data Transfer Objects)
```

### **Frontend Architecture**

```
Progress.tsx (Page Component)
    ├── Stats Cards Component
    ├── TOEIC Parts Breakdown Component
    ├── Weekly Progress Chart Component
    ├── Recent Activities Table Component
    └── API Hooks
        └── apiService (API Client)
```

---

## 🔄 LOGIC VÀ QUY TRÌNH HOẠT ĐỘNG

### **1. HIỂN THỊ TIẾN ĐỘ TỔNG QUAN**

#### Quy trình:
```
User mở trang Progress
    ↓
Frontend: Progress.tsx mount
    ↓
Hook: useUserProgress(userId)
    ↓
API Call: GET /api/Progress/user/{userId}
    ↓
Backend: ProgressController.GetUserProgress(userId)
    ↓
Service: ProgressService.GetUserProgressAsync(userId)
    ↓
Database Query:
    - SELECT * FROM users WHERE id = {userId}
    - SELECT * FROM exercise_completions 
        WHERE user_id = {userId} AND is_completed = 1
    - INCLUDE exercises để get type (Part 1-7)
    ↓
Calculate Statistics:
    - Completed exercises count
    - Average score
    - Total XP (from user.total_xp)
    - Total study time (from user.total_study_time)
    ↓
Build TOEIC Parts:
    - Use ToeicPartHelper.BuildPartScores(completions)
    - Group completions by exercise type
    - Calculate average score cho mỗi part
    ↓
Calculate Skills:
    - Listening = Sum(Part 1-4 scores)
    - Reading = Sum(Part 5-7 scores)
    - Speaking = 0 (chưa implement)
    - Writing = 0 (chưa implement)
    ↓
Calculate Profile Metadata:
    - Level = UserProfileHelper.GetProfileTier(totalXp)
    - StudyStreak = UserProfileHelper.CalculateStudyStreak(completions)
    ↓
Build DTO:
    - UserProgressDto với tất cả statistics
    ↓
Response: UserProgressDto
    ↓
Frontend: Display stats cards và charts
```

#### Code chi tiết:

**Backend - Controller:**
```csharp
[HttpGet("user/{userId}")]
public async Task<ActionResult<UserProgressDto>> GetUserProgress(int userId)
{
    // 1. Call service để get progress
    var progress = await _progressService.GetUserProgressAsync(userId);
    
    if (progress == null)
        return NotFound(new { message = $"User progress for ID {userId} not found" });

    // 2. Format response cho frontend
    var response = new
    {
        userId = progress.UserId,
        username = progress.Username,
        totalScore = progress.TotalScore,
        listening = progress.Listening,
        reading = progress.Reading,
        totalXP = progress.TotalXP,
        totalStudyTime = (int)progress.TotalStudyTime.TotalMinutes,
        toeicParts = progress.ToeicParts,
        // ... other fields
    };

    return Ok(response);
}
```

**Backend - Service:**
```csharp
public async Task<UserProgressDto?> GetUserProgressAsync(int userId)
{
    // 1. Get user
    var user = await _context.Users.FindAsync(userId);
    if (user == null) return null;

    // 2. Get completions
    var userCompletions = await _context.Completions
        .Where(c => c.UserId == userId && c.IsCompleted && c.CompletedAt.HasValue)
        .Include(c => c.Exercise)
        .ToListAsync();

    // 3. Calculate statistics
    var completedExercises = userCompletions.Count;
    var uniqueExercises = userCompletions
        .Select(c => c.ExerciseId)
        .Distinct()
        .Count();
    var averageScore = userCompletions.Any()
        ? (double)userCompletions.Average(c => c.Score ?? 0)
        : 0;

    // 4. Build TOEIC parts
    var toeicParts = ToeicPartHelper.BuildPartScores(userCompletions);
    var listeningScore = ToeicPartHelper.SumListening(toeicParts);
    var readingScore = ToeicPartHelper.SumReading(toeicParts);

    // 5. Calculate profile metadata
    var profileTier = UserProfileHelper.GetProfileTier(user.TotalXp);
    var studyStreak = UserProfileHelper.CalculateStudyStreak(userCompletions);

    // 6. Build DTO
    return new UserProgressDto
    {
        UserId = user.Id,
        Username = user.Username,
        TotalScore = (int)Math.Round(averageScore),
        Listening = listeningScore,
        Reading = readingScore,
        TotalXP = user.TotalXp,
        TotalStudyTime = TimeSpan.FromMinutes(user.TotalStudyTime),
        CompletedExercises = uniqueExercises,
        AverageAccuracy = averageScore,
        Level = profileTier,
        StudyStreak = studyStreak,
        ToeicParts = toeicParts
    };
}
```

**Frontend - Hook:**
```typescript
const useUserProgress = (userId: number) => {
  return useQuery({
    queryKey: ["userProgress", userId],
    queryFn: async (): Promise<UserProgress> => {
      const response = await apiService.get<UserProgress>(
        `/api/Progress/user/${userId}`
      );
      return response;
    },
    refetchInterval: 60000, // Refresh every minute
  });
};
```

---

### **2. HIỂN THỊ WEEKLY PROGRESS**

#### Quy trình:
```
User click vào tab "Weekly Progress"
    ↓
Hook: useWeeklyProgress(userId)
    ↓
API Call: GET /api/Progress/weekly/{userId}
    ↓
Backend: ProgressController.GetWeeklyProgress(userId)
    ↓
Service: ProgressService.GetWeeklyProgressAsync(userId)
    ↓
Database Query:
    - SELECT * FROM exercise_completions
        WHERE user_id = {userId}
        AND completed_at >= DATE_SUB(NOW(), INTERVAL 7 DAY)
        AND is_completed = 1
    ↓
Group by Date:
    - Group completions by completed_at date
    - For each day in last 7 days:
        - Count exercises completed
        - Sum time spent
        - Sum XP earned
    ↓
Build Weekly DTO:
    - DailyProgressDto[] với 7 days
    ↓
Response: WeeklyProgressDto
    ↓
Frontend: Display line chart với daily data
```

#### Code chi tiết:

**Backend - Service:**
```csharp
public async Task<WeeklyProgressDto> GetWeeklyProgressAsync(int userId)
{
    // 1. Get last 7 days
    var endDate = DateTime.UtcNow.Date;
    var startDate = endDate.AddDays(-6); // 7 days including today

    // 2. Get completions trong 7 ngày
    var completions = await _context.Completions
        .Where(c => c.UserId == userId 
            && c.IsCompleted 
            && c.CompletedAt.HasValue
            && c.CompletedAt.Value.Date >= startDate
            && c.CompletedAt.Value.Date <= endDate)
        .ToListAsync();

    // 3. Group by date
    var dailyProgress = new List<DailyProgressDto>();
    
    for (int i = 0; i < 7; i++)
    {
        var date = startDate.AddDays(i);
        var dayCompletions = completions
            .Where(c => c.CompletedAt!.Value.Date == date)
            .ToList();

        dailyProgress.Add(new DailyProgressDto
        {
            Day = date.DayOfWeek.ToString().Substring(0, 3), // "Mon", "Tue", ...
            Date = date,
            ExercisesCompleted = dayCompletions.Count,
            TimeSpentMinutes = dayCompletions
                .Sum(c => c.TimeSpentMinutes ?? 0),
            XPEarned = CalculateXPForCompletions(dayCompletions)
        });
    }

    return new WeeklyProgressDto
    {
        DailyProgress = dailyProgress,
        TotalExercises = completions.Count,
        TotalTime = TimeSpan.FromMinutes(completions
            .Sum(c => c.TimeSpentMinutes ?? 0)),
        TotalXP = CalculateXPForCompletions(completions)
    };
}
```

**Frontend - Chart:**
```typescript
const Progress = () => {
  const { data: weeklyProgress } = useWeeklyProgress(userId);

  // Prepare chart data
  const chartData = weeklyProgress?.dailyProgress.map(day => ({
    day: day.day,
    exercises: day.exercisesCompleted,
    time: day.timeSpentMinutes,
    xp: day.xpEarned
  })) || [];

  return (
    <LineChart data={chartData}>
      <CartesianGrid strokeDasharray="3 3" />
      <XAxis dataKey="day" />
      <YAxis />
      <Tooltip />
      <Legend />
      <Line type="monotone" dataKey="exercises" stroke="#8884d8" />
      <Line type="monotone" dataKey="time" stroke="#82ca9d" />
      <Line type="monotone" dataKey="xp" stroke="#ffc658" />
    </LineChart>
  );
};
```

---

### **3. HIỂN THỊ RECENT ACTIVITIES**

#### Quy trình:
```
User scroll đến phần "Recent Activities"
    ↓
Hook: useUserActivities(userId, limit = 10)
    ↓
API Call: GET /api/Progress/activities/{userId}?limit=10
    ↓
Backend: ProgressController.GetUserActivities(userId, limit)
    ↓
Service: ProgressService.GetUserActivitiesAsync(userId, limit)
    ↓
Database Query:
    - SELECT * FROM exercise_completions
        WHERE user_id = {userId}
        AND is_completed = 1
        ORDER BY completed_at DESC
        LIMIT {limit}
    - INCLUDE exercises để get title, type
    ↓
Build Activity DTOs:
    - Map completion to ActivityDto
    ↓
Response: List<ActivityDto>
    ↓
Frontend: Display activities table
```

#### Code chi tiết:

**Backend - Service:**
```csharp
public async Task<IEnumerable<ActivityDto>> GetUserActivitiesAsync(
    int userId, 
    int limit = 10)
{
    var completions = await _context.Completions
        .Where(c => c.UserId == userId && c.IsCompleted && c.CompletedAt.HasValue)
        .Include(c => c.Exercise)
        .OrderByDescending(c => c.CompletedAt)
        .Take(limit)
        .ToListAsync();

    return completions.Select(c => new ActivityDto
    {
        Id = c.CompletionId,
        Type = c.Exercise?.Type ?? "Unknown",
        Topic = c.Exercise?.Title ?? "Unknown",
        Date = c.CompletedAt!.Value,
        Score = (int)Math.Round(c.Score ?? 0),
        Duration = c.TimeSpentMinutes ?? 0,
        Status = "completed"
    });
}
```

---

### **4. TÍNH PROFILE TIER VÀ STUDY STREAK**

#### Profile Tier:

```csharp
public static string GetProfileTier(int totalXp)
{
    var tierBoundaries = new[]
    {
        (6000, "Legendary"),
        (4000, "Elite"),
        (2500, "Advanced"),
        (1200, "Skilled"),
        (0, "Foundation")
    };

    foreach (var (threshold, tier) in tierBoundaries)
    {
        if (totalXp >= threshold)
            return tier;
    }

    return "Foundation";
}
```

#### Study Streak:

```csharp
public static int CalculateStudyStreak(IEnumerable<Completion> completions)
{
    // Get distinct dates
    var dates = completions
        .Where(c => c.CompletedAt.HasValue)
        .Select(c => c.CompletedAt!.Value.Date)
        .Distinct()
        .OrderByDescending(d => d)
        .ToList();

    if (dates.Count == 0) return 0;

    // Calculate streak từ hôm nay backwards
    var streak = 0;
    var expectedDate = DateTime.UtcNow.Date;

    foreach (var date in dates)
    {
        if (date == expectedDate)
        {
            streak++;
            expectedDate = expectedDate.AddDays(-1);
        }
        else if (date < expectedDate)
        {
            break; // Streak broken
        }
    }

    return streak;
}
```

---

## 📊 LUỒNG DỮ LIỆU

### **Flow Diagram (Text-based):**

```
┌─────────────────────────────────────────────────────────────┐
│                     FRONTEND LAYER                          │
├─────────────────────────────────────────────────────────────┤
│  Progress.tsx                                               │
│    ├─ Stats Cards Component                                │
│    ├─ TOEIC Parts Breakdown Component                      │
│    ├─ Weekly Progress Chart Component                      │
│    ├─ Recent Activities Table Component                    │
│    └─ API Hooks                                            │
│         ├─ useUserProgress(userId)                         │
│         ├─ useWeeklyProgress(userId)                       │
│         └─ useUserActivities(userId)                       │
└─────────────────┬───────────────────────────────────────────┘
                  │ HTTP GET Requests
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                     API LAYER                               │
├─────────────────────────────────────────────────────────────┤
│  ProgressController                                         │
│    ├─ GET /api/Progress/user/{userId}                      │
│    ├─ GET /api/Progress/weekly/{userId}                    │
│    └─ GET /api/Progress/activities/{userId}                │
└─────────────────┬───────────────────────────────────────────┘
                  │ Method Calls
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    SERVICE LAYER                            │
├─────────────────────────────────────────────────────────────┤
│  ProgressService                                            │
│    ├─ GetUserProgressAsync()                               │
│    │   ├─ Query User                                       │
│    │   ├─ Query Completions                                │
│    │   ├─ Calculate Statistics                             │
│    │   ├─ Build TOEIC Parts                                │
│    │   ├─ Calculate Skills                                 │
│    │   └─ Calculate Profile Metadata                       │
│    ├─ GetWeeklyProgressAsync()                             │
│    │   ├─ Query Last 7 Days Completions                    │
│    │   ├─ Group by Date                                    │
│    │   └─ Build Daily Progress                             │
│    └─ GetUserActivitiesAsync()                             │
│        ├─ Query Recent Completions                         │
│        └─ Build Activity DTOs                              │
└─────────────────┬───────────────────────────────────────────┘
                  │ Database Queries
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                   DATABASE LAYER                            │
├─────────────────────────────────────────────────────────────┤
│  MySQL Database                                             │
│    ├─ users table                                          │
│    │   ├─ id, username, total_xp                          │
│    │   └─ total_study_time                                │
│    ├─ exercise_completions table                          │
│    │   ├─ user_id, exercise_id                            │
│    │   ├─ score, completed_at                             │
│    │   ├─ time_spent_minutes                              │
│    │   └─ is_completed = 1                                │
│    └─ exercises table                                      │
│        └─ type ('Part 1', 'Part 2', ...)                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 CÁC TÍNH NĂNG CHÍNH

### **1. Stats Cards**
- ✅ Total Score: Average score từ tất cả completions
- ✅ Total XP: XP tích lũy từ user.total_xp
- ✅ Study Time: Tổng thời gian học (minutes)
- ✅ Completed Exercises: Số bài đã hoàn thành

### **2. TOEIC Parts Breakdown**
- ✅ Display scores cho từng phần (Part 1-7)
- ✅ Color-coded theo skill (Listening/Reading)
- ✅ Show attempts count
- ✅ Interactive filter theo part

### **3. Weekly Progress Chart**
- ✅ Line chart hiển thị 7 ngày gần nhất
- ✅ Multiple metrics (exercises, time, XP)
- ✅ Tooltip với detailed info

### **4. Recent Activities**
- ✅ Table hiển thị 10 hoạt động gần nhất
- ✅ Sortable và filterable
- ✅ Show exercise type, score, date

### **5. Profile Metadata**
- ✅ Level/Tier: Dựa trên total XP
- ✅ Study Streak: Số ngày liên tiếp học
- ✅ Achievements: List các thành tựu

---

## 🔧 TECHNICAL NOTES

### **Performance Optimization:**
- Index database trên `user_id`, `completed_at`
- Use eager loading với Include() cho exercises
- Cache progress data trong React Query
- Batch queries cho weekly progress

### **XP Calculation:**
```csharp
private int CalculateXPForCompletions(List<Completion> completions)
{
    int totalXP = 0;
    foreach (var completion in completions)
    {
        if (completion.Score.HasValue)
        {
            // Base XP = 10 per completion
            int baseXP = 10;
            
            // Bonus XP based on score
            int bonusXP = (int)Math.Round(completion.Score.Value / 10);
            
            totalXP += baseXP + bonusXP;
        }
    }
    return totalXP;
}
```

### **Chart Libraries:**
- Frontend sử dụng Recharts cho line charts
- Responsive design với ResponsiveContainer
- Custom colors theo TOEIC parts

---

## 📝 KẾT LUẬN

Trang Tiến độ cá nhân cung cấp:
- **Insights**: Chi tiết progress của học viên
- **Visualization**: Charts và graphs dễ hiểu
- **Motivation**: Achievements và streaks
- **Tracking**: Weekly/daily breakdowns

Code structure clean, scalable, và dễ maintain. Tích hợp tốt với Leaderboard và Reading Exercises modules.

