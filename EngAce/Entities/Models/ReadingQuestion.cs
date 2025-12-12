namespace Entities.Models;

/// <summary>
/// Model tạm thời cho ReadingQuestion - chỉ dùng để parse và convert dữ liệu trong service
/// Không phải Entity (không map tới database)
/// </summary>
public class ReadingQuestion
{
    public int Id { get; set; }
    
    public int ReadingExerciseId { get; set; } // Chỉ để tương thích, không dùng trong entity
    
    public string QuestionText { get; set; } = string.Empty; // "The company will _____ a new product."
    
    public string OptionA { get; set; } = string.Empty; // "launch"
    
    public string OptionB { get; set; } = string.Empty; // "launches"
    
    public string OptionC { get; set; } = string.Empty; // "launching"
    
    public string OptionD { get; set; } = string.Empty; // "launched"
    
    public int CorrectAnswer { get; set; } // 0=A, 1=B, 2=C, 3=D
    
    public string? Explanation { get; set; } // "Future tense cần động từ nguyên mẫu"
    
    public int OrderNumber { get; set; } = 1; // Thứ tự câu hỏi trong bài
    
    public int Difficulty { get; set; } = 3; // 1=Very Easy, 5=Very Hard
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}