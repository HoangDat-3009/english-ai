namespace EngAce.Api.Services.AI;

/// <summary>
/// 🤖 Interface cho Gemini AI Service - Tạo bài tập TOEIC tự động
/// </summary>
public interface IGeminiService
{
    /// <summary>
    /// 🤖 Tạo câu hỏi từ AI (Part 5) - hỗ trợ Gemini và OpenAI
    /// </summary>
    Task<List<GeneratedQuestion>> GenerateQuestionsAsync(string content, string exerciseType, string level, int questionCount = 5, string provider = "gemini");

    /// <summary>
    /// 🤖 Tạo giải thích cho câu hỏi
    /// </summary>
    Task<string> GenerateExplanationAsync(string questionText, string correctAnswer);

    /// <summary>
    /// 🤖 Test kết nối Gemini API
    /// </summary>
    Task<bool> TestConnectionAsync();

    /// <summary>
    /// 🤖 Lấy raw response từ Gemini (debug)
    /// </summary>
    Task<string?> GetRawGeminiResponseAsync(string content, string exerciseType, string level, int questionCount);

    /// <summary>
    /// 🤖 Tạo câu hỏi + passage từ AI (Part 6/7) - hỗ trợ Gemini và OpenAI
    /// </summary>
    Task<(List<GeneratedQuestion> questions, string passage)> GenerateQuestionsWithPassageAsync(string content, string exerciseType, string level, int questionCount = 5, string provider = "gemini");

    /// <summary>
    /// 🤖 Tạo chat response với provider support
    /// </summary>
    Task<string> GenerateChatResponseAsync(string prompt, string provider = "gemini");

    /// <summary>
    /// 🤖 Tạo dictionary search với provider support
    /// </summary>
    Task<string> GenerateDictionarySearchAsync(string prompt, string provider = "gemini");

    /// <summary>
    /// 🤖 Tạo quiz/exercise với provider support
    /// </summary>
    Task<string> GenerateQuizResponseAsync(string prompt, string provider = "gemini");
    
    /// <summary>
    /// 🤖 Generate AI response với provider và maxTokens support
    /// </summary>
    Task<string> GenerateResponseAsync(string prompt, string provider = "gemini", int maxTokens = 2048);
    
    Task<string> GenerateResponseOpenAI(string prompt, int maxTokens);
    Task<string> GenerateResponse(string prompt, int maxTokens);
}