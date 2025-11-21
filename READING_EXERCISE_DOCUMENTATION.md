# 📚 TÀI LIỆU TRANG BÀI TẬP ĐỌC (Reading Exercises)

## 📋 MỤC LỤC
1. [Tổng quan](#tổng-quan)
2. [Các file liên quan](#các-file-liên-quan)
3. [Cấu trúc code](#cấu-trúc-code)
4. [Logic và quy trình hoạt động](#logic-và-quy-trình-hoạt-động)
5. [Luồng dữ liệu](#luồng-dữ-liệu)
6. [Các tính năng chính](#các-tính-năng-chính)

---

## 🎯 TỔNG QUAN

Trang **Bài tập đọc** là module chính của ứng dụng, cho phép học viên:
- Xem danh sách bài tập TOEIC (Part 5, 6, 7)
- Lọc bài tập theo level (Beginner, Intermediate, Advanced)
- Lọc bài tập theo nguồn (Admin Upload, AI Generated)
- Tạo bài tập mới bằng AI (Gemini API)
- Làm bài tập và nộp kết quả
- Xem kết quả và điểm số chi tiết

---

## 📁 CÁC FILE LIÊN QUAN

### **Backend (C# .NET)**

#### Controllers
- `EngAce/EngAce.Api/Controllers/ReadingExerciseController.cs`
  - Controller chính xử lý các API endpoints cho bài tập đọc
  - Routes: `/api/ReadingExercise/*`

#### Services
- `EngAce/EngAce.Api/Services/ReadingExerciseService.cs`
  - Service layer xử lý business logic
  - Xử lý CRUD operations, file upload, AI generation

#### Interfaces
- `EngAce/EngAce.Api/Services/Interfaces/IReadingExerciseService.cs`
  - Interface định nghĩa các methods cần thiết

#### AI Service
- `EngAce/EngAce.Api/Services/AI/GeminiService.cs`
  - Service tích hợp với Google Gemini API
  - Xử lý generation bài tập bằng AI

#### DTOs
- `EngAce/EngAce.Api/DTO/Exercises/ReadingExerciseDto.cs`
  - Data Transfer Objects cho frontend-backend communication

#### Models (Entities)
- `EngAce/Entities/Models/Exercise.cs`
  - Entity model cho bảng `exercises` trong database
- `EngAce/Entities/Models/Completion.cs`
  - Entity model cho bảng `exercise_completions`
- `EngAce/Entities/Models/ReadingQuestion.cs`
  - Model tạm thời để parse và convert dữ liệu (không phải entity)

### **Frontend (React TypeScript)**

#### Pages
- `english-mentor-buddy/src/pages/ReadingExercises.tsx`
  - Component chính của trang bài tập đọc

#### Hooks
- `english-mentor-buddy/src/hooks/useReadingExercises.ts`
  - Custom hook quản lý state và API calls cho bài tập đọc

#### Services
- `english-mentor-buddy/src/services/api.ts`
  - Service chung cho API calls

#### Components
- `english-mentor-buddy/src/components/ReadingExerciseCard.tsx`
  - Component hiển thị chi tiết bài tập và cho phép làm bài

---

## 🏗️ CẤU TRÚC CODE

### **Backend Architecture**

```
ReadingExerciseController
    ├── ReadingExerciseService (Business Logic)
    │   ├── GeminiService (AI Generation)
    │   └── ApplicationDbContext (Database Access)
    └── DTOs (Data Transfer Objects)
```

### **Frontend Architecture**

```
ReadingExercises.tsx (Page Component)
    ├── useReadingExercises (Custom Hook)
    │   └── apiService (API Client)
    └── ReadingExerciseCard (Detail Component)
```

---

## 🔄 LOGIC VÀ QUY TRÌNH HOẠT ĐỘNG

### **1. XEM DANH SÁCH BÀI TẬP**

#### Quy trình:
```
User mở trang ReadingExercises
    ↓
Frontend: ReadingExercises.tsx mount
    ↓
Hook: useReadingExercises() được gọi
    ↓
API Call: GET /api/ReadingExercise
    ↓
Backend: ReadingExerciseController.GetAllExercises()
    ↓
Service: ReadingExerciseService.GetAllExercisesAsync()
    ↓
Database: SELECT * FROM exercises WHERE is_active = 1
    ↓
Parse JSON: ParseQuestionsJson(questions_json)
    ↓
Response: List<ExerciseDto>
    ↓
Frontend: Hiển thị danh sách bài tập với filters
```

#### Code chi tiết:

**Backend - Controller:**
```csharp
[HttpGet]
public async Task<ActionResult<IEnumerable<object>>> GetAllExercises([FromQuery] string? level = null)
{
    // 1. Query exercises từ database
    IQueryable<Exercise> query = _context.Exercises.Where(e => e.IsActive);
    
    // 2. Filter theo level nếu có
    if (!string.IsNullOrEmpty(level))
        query = query.Where(e => e.Level == level);
    
    // 3. Order by created date
    query = query.OrderByDescending(e => e.CreatedAt);
    
    // 4. Load từ database
    var rawExercises = await query.Include(e => e.CreatedByUser).ToListAsync();
    
    // 5. Parse JSON questions trong memory (không parse trong SQL)
    var exercises = rawExercises.Select(e => new
    {
        ExerciseId = e.ExerciseId,
        Title = e.Title,
        Questions = ParseQuestionsJson(e.Questions), // Parse JSON
        // ... other fields
    }).ToList();
    
    return Ok(exercises);
}
```

**Frontend - Hook:**
```typescript
export const useReadingExercises = () => {
  const [exercises, setExercises] = useState<ReadingExercise[]>([]);
  const [isLoading, setIsLoading] = useState(true);

  useEffect(() => {
    // Fetch exercises khi component mount
    const fetchExercises = async () => {
      setIsLoading(true);
      try {
        const response = await apiService.get<ReadingExercise[]>('/api/ReadingExercise');
        setExercises(response);
      } catch (error) {
        console.error('Error fetching exercises:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchExercises();
  }, []);

  return { exercises, isLoading, ... };
};
```

---

### **2. LỌC BÀI TẬP**

#### Logic lọc:

**Frontend Filter:**
```typescript
const filteredExercises = exercises.filter((exercise) => {
  const levelMatch = filterLevel === "all" || exercise.level === filterLevel;
  const sourceMatch = filterSource === "all" || exercise.sourceType === filterSource;
  return levelMatch && sourceMatch;
});
```

**Các filter options:**
- **Level**: All, Beginner, Intermediate, Advanced
- **Source**: All, Admin Upload (manual), AI Generated (ai)

---

### **3. TẠO BÀI TẬP BẰNG AI**

#### Quy trình:
```
User click "Generate with AI"
    ↓
Form hiển thị: topic, level, type (Part 5/6/7)
    ↓
User nhập topic và chọn level, type
    ↓
User click "Generate"
    ↓
Frontend: generateExercise({ topic, level, type })
    ↓
API Call: POST /api/ReadingExercise/generate-ai
    ↓
Backend: ReadingExerciseController.GenerateWithAI()
    ↓
Service: ReadingExerciseService.CreateExerciseWithAIQuestionsAsync()
    ↓
AI Service: GeminiService.GenerateExerciseContent()
    ↓
Gemini API: POST request với prompt
    ↓
Parse Response: Extract passage và questions từ JSON
    ↓
Database: INSERT INTO exercises (questions_json, correct_answers_json, ...)
    ↓
Response: ExerciseDto với questions
    ↓
Frontend: Refresh danh sách bài tập
```

#### Code chi tiết:

**Backend - AI Generation:**
```csharp
[HttpPost("generate-ai")]
public async Task<ActionResult<object>> GenerateWithAI([FromBody] GenerateAIRequest request)
{
    // 1. Validate request
    if (string.IsNullOrEmpty(request.Topic))
        return BadRequest(new { message = "Topic is required" });

    // 2. Call service để generate
    var exercise = await _readingExerciseService.CreateExerciseWithAIQuestionsAsync(request);

    // 3. Return created exercise
    return CreatedAtAction(nameof(GetExerciseById), 
        new { id = exercise.Id }, exercise);
}
```

**AI Service - Gemini Integration:**
```csharp
public async Task<string> GenerateExerciseContentAsync(string topic, string level, string partType, int questionCount)
{
    // 1. Build prompt cho Gemini
    var prompt = BuildPrompt(topic, level, partType, questionCount);
    
    // 2. Call Gemini API
    var response = await _httpClient.PostAsync(geminiUrl, content);
    
    // 3. Parse JSON response
    var jsonResponse = await response.Content.ReadAsStringAsync();
    var parsed = JsonSerializer.Deserialize<GeminiResponse>(jsonResponse);
    
    // 4. Extract text content
    var text = parsed.Candidates[0].Content.Parts[0].Text;
    
    // 5. Parse JSON từ text (handle truncated JSON)
    return ExtractPartialDataFromIncompleteJson(text);
}
```

**Frontend - Generate Handler:**
```typescript
const handleGenerate = () => {
  if (!topic.trim()) return;
  
  // Gọi hook để generate
  generateExercise({ topic, level, type });
  
  // Reset form
  setTopic("");
  setShowGenerator(false);
};
```

---

### **4. LÀM BÀI TẬP VÀ NỘP KẾT QUẢ**

#### Quy trình:
```
User click vào bài tập
    ↓
ReadingExerciseCard component hiển thị
    ↓
User làm bài: chọn đáp án cho từng câu
    ↓
User click "Submit"
    ↓
Frontend: Calculate score từ answers và correctAnswers
    ↓
API Call: POST /api/ReadingExercise/{id}/submit
    ↓
Backend: ReadingExerciseController.SubmitResult()
    ↓
Service: ReadingExerciseService.SubmitExerciseResultAsync()
    ↓
Calculate Score: So sánh user_answers với correct_answers_json
    ↓
Database: INSERT INTO exercise_completions (user_id, exercise_id, score, ...)
    ↓
Update User Progress: Update total_xp, total_study_time
    ↓
Response: ResultDto với score, correct answers, explanations
    ↓
Frontend: Hiển thị kết quả và explanations
```

#### Code chi tiết:

**Backend - Submit Result:**
```csharp
[HttpPost("{id}/submit")]
public async Task<ActionResult<object>> SubmitResult(
    int id, 
    [FromBody] SubmitResultRequest request)
{
    // 1. Get exercise
    var exercise = await _context.Exercises.FindAsync(id);
    if (exercise == null) return NotFound();

    // 2. Parse correct answers
    var correctAnswers = JsonSerializer.Deserialize<List<int>>(exercise.CorrectAnswers);
    
    // 3. Calculate score
    int correctCount = 0;
    for (int i = 0; i < request.Answers.Count; i++)
    {
        if (request.Answers[i] == correctAnswers[i])
            correctCount++;
    }
    
    decimal score = (decimal)correctCount / correctAnswers.Count * 100;
    
    // 4. Create completion record
    var completion = new Completion
    {
        UserId = request.UserId,
        ExerciseId = id,
        UserAnswers = JsonSerializer.Serialize(request.Answers),
        Score = score,
        TotalQuestions = correctAnswers.Count,
        IsCompleted = true,
        CompletedAt = DateTime.UtcNow
    };
    
    _context.Completions.Add(completion);
    
    // 5. Update user progress
    var user = await _context.Users.FindAsync(request.UserId);
    user.TotalXp += CalculateXP(score, correctAnswers.Count);
    user.TotalStudyTime += request.TimeSpent;
    
    await _context.SaveChangesAsync();
    
    // 6. Return result
    return Ok(new { score, correctCount, totalQuestions = correctAnswers.Count });
}
```

---

## 📊 LUỒNG DỮ LIỆU

### **Flow Diagram (Text-based):**

```
┌─────────────────────────────────────────────────────────────┐
│                     FRONTEND LAYER                          │
├─────────────────────────────────────────────────────────────┤
│  ReadingExercises.tsx                                       │
│    ├─ Filter State (level, source)                         │
│    ├─ Selected Exercise State                              │
│    └─ useReadingExercises Hook                             │
│         ├─ fetchExercises()                                │
│         ├─ generateExercise()                              │
│         └─ apiService calls                                │
└─────────────────┬───────────────────────────────────────────┘
                  │ HTTP Requests
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                     API LAYER                               │
├─────────────────────────────────────────────────────────────┤
│  ReadingExerciseController                                  │
│    ├─ GET /api/ReadingExercise                             │
│    ├─ GET /api/ReadingExercise/{id}                        │
│    ├─ POST /api/ReadingExercise/generate-ai                │
│    └─ POST /api/ReadingExercise/{id}/submit                │
└─────────────────┬───────────────────────────────────────────┘
                  │ Method Calls
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                    SERVICE LAYER                            │
├─────────────────────────────────────────────────────────────┤
│  ReadingExerciseService                                     │
│    ├─ GetAllExercisesAsync()                               │
│    ├─ CreateExerciseWithAIQuestionsAsync()                 │
│    ├─ SubmitExerciseResultAsync()                          │
│    └─ GeminiService.GenerateExerciseContentAsync()         │
└─────────────────┬───────────────────────────────────────────┘
                  │ Database Queries
                  ▼
┌─────────────────────────────────────────────────────────────┐
│                   DATABASE LAYER                            │
├─────────────────────────────────────────────────────────────┤
│  MySQL Database                                             │
│    ├─ exercises table                                      │
│    │   ├─ id, title, content                              │
│    │   ├─ questions_json (JSON)                           │
│    │   └─ correct_answers_json (JSON)                     │
│    ├─ exercise_completions table                          │
│    │   ├─ user_id, exercise_id                            │
│    │   ├─ score, user_answers_json                        │
│    │   └─ completed_at                                    │
│    └─ users table                                          │
│        ├─ total_xp, total_study_time                      │
│        └─ last_active_at                                  │
└─────────────────────────────────────────────────────────────┘
```

---

## 🎯 CÁC TÍNH NĂNG CHÍNH

### **1. Hiển thị danh sách bài tập**
- ✅ Fetch từ database với pagination
- ✅ Filter theo level và source
- ✅ Search theo title
- ✅ Sort theo created date

### **2. Tạo bài tập bằng AI**
- ✅ Integration với Google Gemini API
- ✅ Support Part 5, 6, 7 với prompts khác nhau
- ✅ Generate passage và questions tự động
- ✅ Handle truncated JSON responses

### **3. Làm bài tập**
- ✅ Interactive UI cho từng câu hỏi
- ✅ Timer hiển thị thời gian làm bài
- ✅ Auto-save answers (localStorage)
- ✅ Submit và tính điểm tự động

### **4. Xem kết quả**
- ✅ Hiển thị score và percentage
- ✅ Show correct/incorrect answers
- ✅ Display explanations
- ✅ Update user progress (XP, study time)

---

## 🔧 TECHNICAL NOTES

### **JSON Storage trong Database:**
- `questions_json`: Lưu array các question objects
- `correct_answers_json`: Lưu array các đáp án đúng (0, 1, 2, 3)
- Parse JSON trong memory, không parse trong SQL query

### **AI Generation:**
- Sử dụng Google Gemini 2.5 Flash model
- Max output tokens: 8192
- Handle partial JSON nếu response bị truncate
- Retry logic nếu API fails

### **Performance Optimization:**
- Lazy loading cho danh sách bài tập
- Caching trong React Query
- Index database cho faster queries
- Batch operations cho bulk updates

---

## 📝 KẾT LUẬN

Trang Bài tập đọc là core feature của ứng dụng, tích hợp:
- **Database**: MySQL với JSON columns
- **AI**: Google Gemini API cho content generation
- **Frontend**: React với TypeScript và React Query
- **Backend**: ASP.NET Core với EF Core

Code structure clean, maintainable, và scalable cho future features.

