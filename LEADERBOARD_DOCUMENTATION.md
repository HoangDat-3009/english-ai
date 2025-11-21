# 🏆 TÀI LIỆU TRANG BẢNG XẾP HẠNG (Leaderboard)

## 📋 MỤC LỤC
1. [Tổng quan](#tổng-quan)
2. [Các file liên quan](#các-file-liên-quan)
3. [Cấu trúc code](#cấu-trúc-code)
4. [Logic và quy trình hoạt động](#logic-và-quy-trình-hoạt-động)
5. [Luồng dữ liệu](#luồng-dữ-liệu)
6. [Các tính năng chính](#các-tính-năng-chính)

---

## 🎯 TỔNG QUAN

Trang **Bảng xếp hạng** hiển thị thứ hạng của các học viên dựa trên:
- **Total Score**: Tổng điểm từ tất cả các bài thi TOEIC
- **Listening Score**: Điểm phần nghe (Part 1-4)
- **Reading Score**: Điểm phần đọc (Part 5-7)
- **Individual Parts**: Điểm từng phần riêng lẻ (Part 1, 2, 3, ...)

**Tính năng chính:**
- Ranking system với real-time updates
- Filter theo thời gian (today, week, month, all)
- Filter theo skill (total, listening, reading, individual parts)
- User search và profile modal
- Badge system (Gold, Silver, Bronze)
- Percentile ranking

---

## 📁 CÁC FILE LIÊN QUAN

### **Backend (C# .NET)**

#### Controllers
- `EngAce/EngAce.Api/Controllers/LeaderboardController.cs`
  - Controller chính xử lý leaderboard APIs
  - Routes: `/api/Leaderboard/*`

#### Services
- `EngAce/EngAce.Api/Services/LeaderboardService.cs`
  - Service layer xử lý business logic
  - Calculate rankings, filtering, statistics

#### Interfaces
- `EngAce/EngAce.Api/Services/Interfaces/ILeaderboardService.cs`
  - Interface định nghĩa methods

#### Helpers
- `EngAce/EngAce.Api/Helpers/ToeicPartHelper.cs`
  - Helper để tính điểm các phần TOEIC
  - Build part scores từ completions

- `EngAce/EngAce.Api/Helpers/UserProfileHelper.cs`
  - Helper để tính profile tier và study streak
  - GetProfileTier(int totalXp)
  - CalculateStudyStreak(IEnumerable<Completion>)

#### DTOs
- `EngAce/EngAce.Api/DTO/Core/ProgressDto.cs`
  - `LeaderboardEntryDto`: DTO cho leaderboard entry
  - `UserRankDto`: DTO cho user rank
  - `LeaderboardStatsDto`: DTO cho statistics

- `EngAce/EngAce.Api/DTO/Shared/ToeicPartDto.cs`
  - `ToeicPartScore`: DTO cho điểm từng phần TOEIC

### **Frontend (React TypeScript)**

#### Pages
- `english-mentor-buddy/src/pages/Leaderboard.tsx`
  - Component chính của trang leaderboard

#### Services
- `english-mentor-buddy/src/services/api.ts`
  - API service cho leaderboard calls

#### Hooks
- `english-mentor-buddy/src/hooks/useLeaderboardData.ts` (hoặc inline trong component)
  - Custom hook quản lý leaderboard state

#### Utils
- `english-mentor-buddy/src/utils/toeicParts.ts`
  - Utilities cho TOEIC parts normalization
  - `normalizeToeicParts()`

#### Constants
- `english-mentor-buddy/src/constants/toeicParts.ts`
  - `TOEIC_PARTS`: Constants định nghĩa các phần TOEIC
  - Part metadata (title, label, skill, description)

---

## 🏗️ CẤU TRÚC CODE

### **Backend Architecture**

```
LeaderboardController
    ├── LeaderboardService (Business Logic)
    │   ├── ToeicPartHelper (Part Score Calculation)
    │   ├── UserProfileHelper (Profile Metadata)
    │   └── ApplicationDbContext (Database Access)
    └── DTOs (Data Transfer Objects)
```

### **Frontend Architecture**

```
Leaderboard.tsx (Page Component)
    ├── useLeaderboardData (Custom Hook)
    │   └── apiService (API Client)
    ├── Filter Components (Time, Skill)
    └── Leaderboard Table Component
```

---

## 🔄 LOGIC VÀ QUY TRÌNH HOẠT ĐỘNG

### **1. HIỂN THỊ BẢNG XẾP HẠNG**

#### Quy trình:
```
User mở trang Leaderboard
    ↓
Frontend: Leaderboard.tsx mount
    ↓
Hook: useLeaderboardData(timeFilter, skillFilter)
    ↓
API Call: GET /api/Leaderboard?timeFilter={filter}&skill={skill}
    ↓
Backend: LeaderboardController.GetLeaderboard()
    ↓
Service: LeaderboardService.GetLeaderboardAsync()
    ↓
Database Query:
    - SELECT users.* FROM users WHERE status = 'active'
    - LEFT JOIN exercise_completions ON users.id = completions.user_id
    - Filter theo timeFilter (today/week/month/all)
    ↓
Calculate Scores:
    - Group completions by user
    - Calculate total score, listening score, reading score
    - Calculate individual part scores (Part 1-7)
    ↓
Apply Skill Filter:
    - If skill = 'listening': Sort by listening score
    - If skill = 'reading': Sort by reading score
    - If skill = 'total': Sort by total score
    - If skill = 'part1': Sort by Part 1 score
    - ...
    ↓
Build TOEIC Parts:
    - Use ToeicPartHelper.BuildPartScores()
    - Map completions to TOEIC parts
    ↓
Calculate Rankings:
    - Assign rank based on sorted scores
    - Calculate percentile for each user
    ↓
Build DTOs:
    - LeaderboardEntryDto với rank, score, parts
    - UserProfileHelper.GetProfileTier() cho level
    - UserProfileHelper.CalculateStudyStreak() cho streak
    ↓
Response: List<LeaderboardEntryDto>
    ↓
Frontend: Display leaderboard table với rankings
```

#### Code chi tiết:

**Backend - Controller:**
```csharp
[HttpGet]
public async Task<ActionResult<object>> GetLeaderboard(
    [FromQuery] string? timeFilter = null, 
    [FromQuery] string? skill = null)
{
    // 1. Call service để get leaderboard
    var leaderboard = await _leaderboardService.GetLeaderboardAsync(timeFilter, skill);
    
    // 2. Format response cho frontend
    var response = new
    {
        users = leaderboard.Select(entry => new
        {
            rank = entry.Rank,
            username = entry.Username,
            totalScore = entry.TotalScore,
            listening = entry.Listening,
            reading = entry.Reading,
            parts = entry.ToeicParts.Select(part => new
            {
                key = part.Key,
                part = part.Part,
                score = part.Score,
                attempts = part.Attempts
            })
        })
    };
    
    return Ok(response);
}
```

**Backend - Service:**
```csharp
public async Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(
    string? timeFilter = null, 
    string? skill = null)
{
    // 1. Query users và completions
    var completionsQuery = _context.Completions
        .Include(c => c.Exercise)
        .Where(c => c.IsCompleted && c.Score.HasValue);

    // 2. Apply time filter
    if (!string.IsNullOrEmpty(timeFilter))
    {
        var filterDate = timeFilter.ToLower() switch
        {
            "today" => DateTime.UtcNow.Date,
            "week" => DateTime.UtcNow.AddDays(-7),
            "month" => DateTime.UtcNow.AddDays(-30),
            _ => DateTime.MinValue
        };

        if (filterDate != DateTime.MinValue)
        {
            completionsQuery = completionsQuery
                .Where(c => c.CompletedAt >= filterDate);
        }
    }

    // 3. Group by user và calculate scores
    var userProgressData = await _context.Users
        .Where(u => u.Status == "active")
        .GroupJoin(completionsQuery,
            u => u.Id,
            c => c.UserId,
            (u, completions) => new { User = u, Completions = completions.ToList() })
        .ToListAsync();

    // 4. Build leaderboard entries
    var leaderboard = userProgressData.Select(uc =>
    {
        var toeicParts = ToeicPartHelper.BuildPartScores(uc.Completions);
        var listeningScore = ToeicPartHelper.SumListening(toeicParts);
        var readingScore = ToeicPartHelper.SumReading(toeicParts);
        var totalScore = listeningScore + readingScore;

        return new LeaderboardEntryDto
        {
            UserId = uc.User.Id,
            Username = uc.User.Username,
            TotalScore = totalScore,
            Listening = listeningScore,
            Reading = readingScore,
            ToeicParts = toeicParts,
            Level = UserProfileHelper.GetProfileTier(uc.User.TotalXp),
            StudyStreak = UserProfileHelper.CalculateStudyStreak(uc.Completions)
        };
    }).ToList();

    // 5. Apply skill filter và sort
    var sortedLeaderboard = skill?.ToLower() switch
    {
        "listening" => leaderboard.OrderByDescending(e => e.Listening),
        "reading" => leaderboard.OrderByDescending(e => e.Reading),
        "part1" => leaderboard.OrderByDescending(e => 
            e.ToeicParts.FirstOrDefault(p => p.Key == "part1")?.Score ?? 0),
        // ... other parts
        _ => leaderboard.OrderByDescending(e => e.TotalScore)
    };

    // 6. Assign ranks
    int rank = 1;
    decimal? previousScore = null;
    foreach (var entry in sortedLeaderboard)
    {
        var currentScore = skill?.ToLower() switch
        {
            "listening" => entry.Listening,
            "reading" => entry.Reading,
            "part1" => entry.ToeicParts.FirstOrDefault(p => p.Key == "part1")?.Score ?? 0,
            _ => entry.TotalScore
        };

        if (previousScore.HasValue && currentScore != previousScore.Value)
            rank++;

        entry.Rank = rank;
        entry.Percentile = CalculatePercentile(rank, sortedLeaderboard.Count);
        previousScore = currentScore;
    }

    return sortedLeaderboard;
}
```

**Frontend - Hook:**
```typescript
const useLeaderboardData = (
  timeFilter: string = "all",
  filter: LeaderboardFilter = "total"
) => {
  return useQuery({
    queryKey: ["leaderboard", timeFilter, filter],
    queryFn: async (): Promise<LeaderboardResponse> => {
      const params = new URLSearchParams({
        timeFilter,
        skill: filter
      });
      
      const response = await apiService.get<LeaderboardResponse>(
        `/api/Leaderboard?${params}`
      );
      
      return response;
    },
    refetchInterval: 30000, // Refresh every 30 seconds
  });
};
```

---

### **2. TÍNH ĐIỂM CÁC PHẦN TOEIC**

#### Logic:

**ToeicPartHelper.BuildPartScores():**
```csharp
public static List<ToeicPartScore> BuildPartScores(IEnumerable<Completion> completions)
{
    var parts = new List<ToeicPartScore>();
    
    // Group completions by exercise type (Part 1-7)
    var partGroups = completions
        .Where(c => c.Exercise != null)
        .GroupBy(c => c.Exercise.Type) // "Part 1", "Part 2", ...
        .ToList();
    
    foreach (var partGroup in partGroups)
    {
        var partType = partGroup.Key; // "Part 1"
        var partScores = partGroup.Where(c => c.Score.HasValue);
        
        // Calculate average score cho part này
        var avgScore = partScores.Any() 
            ? partScores.Average(c => c.Score.Value) 
            : 0;
        
        // Count attempts
        var attempts = partScores.Count();
        
        // Map to TOEIC part key
        var partKey = MapTypeToPartKey(partType); // "part1"
        var partInfo = GetPartInfo(partKey); // Metadata
        
        parts.Add(new ToeicPartScore
        {
            Key = partKey,
            Part = partInfo.Part,
            Label = partInfo.Label,
            Skill = partInfo.Skill, // "Listening" hoặc "Reading"
            Score = (int)Math.Round(avgScore),
            Attempts = attempts
        });
    }
    
    return parts;
}
```

**Sum Listening/Reading:**
```csharp
public static int SumListening(List<ToeicPartScore> parts)
{
    return parts
        .Where(p => p.Skill == "Listening")
        .Sum(p => p.Score);
}

public static int SumReading(List<ToeicPartScore> parts)
{
    return parts
        .Where(p => p.Skill == "Reading")
        .Sum(p => p.Score);
}
```

---

### **3. FILTER VÀ SORT**

#### Time Filter:

```csharp
var filterDate = timeFilter.ToLower() switch
{
    "today" => DateTime.UtcNow.Date,
    "week" => DateTime.UtcNow.AddDays(-7),
    "month" => DateTime.UtcNow.AddDays(-30),
    _ => DateTime.MinValue // "all"
};

if (filterDate != DateTime.MinValue)
{
    completionsQuery = completionsQuery
        .Where(c => c.CompletedAt >= filterDate);
}
```

#### Skill Filter:

```csharp
var sortedLeaderboard = skill?.ToLower() switch
{
    "listening" => leaderboard.OrderByDescending(e => e.Listening),
    "reading" => leaderboard.OrderByDescending(e => e.Reading),
    "part1" => leaderboard.OrderByDescending(e => 
        e.ToeicParts.FirstOrDefault(p => p.Key == "part1")?.Score ?? 0),
    "part2" => leaderboard.OrderByDescending(e => 
        e.ToeicParts.FirstOrDefault(p => p.Key == "part2")?.Score ?? 0),
    // ... other parts
    _ => leaderboard.OrderByDescending(e => e.TotalScore) // "total"
};
```

---

### **4. TÍNH PERCENTILE VÀ RANK**

#### Rank Assignment:

```csharp
int rank = 1;
decimal? previousScore = null;

foreach (var entry in sortedLeaderboard)
{
    var currentScore = GetScoreForFilter(entry, skill);
    
    // Tăng rank nếu score khác với entry trước
    if (previousScore.HasValue && currentScore != previousScore.Value)
        rank++;
    
    entry.Rank = rank;
    previousScore = currentScore;
}
```

#### Percentile Calculation:

```csharp
private int CalculatePercentile(int rank, int totalUsers)
{
    if (totalUsers == 0) return 0;
    
    // Percentile = (totalUsers - rank) / totalUsers * 100
    return (int)Math.Round((double)(totalUsers - rank) / totalUsers * 100);
}
```

---

## 📊 LUỒNG DỮ LIỆU

### **Flow Diagram (Text-based):**

```
┌─────────────────────────────────────────────────────────────┐
│                     FRONTEND LAYER                          │
├─────────────────────────────────────────────────────────────┤
│  Leaderboard.tsx                                            │
│    ├─ Filter State (timeFilter, skillFilter)               │
│    ├─ Search State (searchQuery)                           │
│    └─ useLeaderboardData Hook                              │
│         └─ apiService.get('/api/Leaderboard?params')       │
└─────────────────┬───────────────────────────────────────────┘
                  │ HTTP GET Request
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                     API LAYER                               │
├─────────────────────────────────────────────────────────────┤
│  LeaderboardController.GetLeaderboard()                     │
│    ├─ Extract query params (timeFilter, skill)             │
│    └─ Call LeaderboardService                              │
└─────────────────┬───────────────────────────────────────────┘
                  │ Method Call
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    SERVICE LAYER                            │
├─────────────────────────────────────────────────────────────┤
│  LeaderboardService.GetLeaderboardAsync()                   │
│    ├─ Query Users (WHERE status = 'active')                │
│    ├─ Query Completions (LEFT JOIN)                        │
│    ├─ Apply Time Filter                                    │
│    ├─ Group by User                                        │
│    ├─ Calculate Scores (Total, Listening, Reading)         │
│    ├─ Build TOEIC Parts (ToeicPartHelper)                  │
│    ├─ Apply Skill Filter & Sort                            │
│    ├─ Assign Ranks                                         │
│    ├─ Calculate Percentiles                                │
│    └─ Build DTOs                                           │
└─────────────────┬───────────────────────────────────────────┘
                  │ Database Queries
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                   DATABASE LAYER                            │
├─────────────────────────────────────────────────────────────┤
│  MySQL Database                                             │
│    ├─ users table                                          │
│    │   ├─ id, username, total_xp                          │
│    │   └─ status = 'active'                               │
│    ├─ exercise_completions table                          │
│    │   ├─ user_id, exercise_id                            │
│    │   ├─ score, completed_at                             │
│    │   └─ is_completed = 1                                │
│    └─ exercises table                                      │
│        └─ type ('Part 1', 'Part 2', ...)                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 CÁC TÍNH NĂNG CHÍNH

### **1. Real-time Ranking**
- ✅ Auto-refresh mỗi 30 giây
- ✅ Calculate rank dựa trên score
- ✅ Handle ties (same score = same rank)

### **2. Filtering**
- ✅ **Time Filter**: today, week, month, all
- ✅ **Skill Filter**: total, listening, reading, individual parts

### **3. TOEIC Parts Breakdown**
- ✅ Display scores cho từng phần (Part 1-7)
- ✅ Group by skill (Listening vs Reading)
- ✅ Show attempts count

### **4. User Profile**
- ✅ Profile modal với detailed stats
- ✅ Badge system (Gold, Silver, Bronze)
- ✅ Percentile ranking
- ✅ Study streak và level

### **5. Search**
- ✅ Search users by username
- ✅ Filter results trong real-time

---

## 🔧 TECHNICAL NOTES

### **Performance Optimization:**
- Index database trên `user_id`, `exercise_id`, `completed_at`
- Use LEFT JOIN thay vì multiple queries
- Cache leaderboard data trong React Query
- Lazy load user profiles

### **Ranking Algorithm:**
- Same score = same rank
- Next rank = current rank + number of users với score đó
- Percentile = (totalUsers - rank) / totalUsers * 100

### **TOEIC Parts Mapping:**
```
Part 1-4 → Listening Skill
Part 5-7 → Reading Skill

Part 1: Photographs (Listening)
Part 2: Question Response (Listening)
Part 3: Conversations (Listening)
Part 4: Short Talks (Listening)
Part 5: Incomplete Sentences (Reading)
Part 6: Text Completion (Reading)
Part 7: Reading Comprehension (Reading)
```

---

## 📝 KẾT LUẬN

Trang Bảng xếp hạng cung cấp:
- **Gamification**: Khuyến khích học viên cạnh tranh
- **Transparency**: Hiển thị công khai rankings
- **Flexibility**: Multiple filters và views
- **Real-time**: Auto-refresh rankings

Code structure clean, scalable, và dễ maintain.

