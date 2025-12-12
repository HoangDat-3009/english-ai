using Entities.Enums;

namespace Entities;

/// <summary>
/// Bài tập nói
/// </summary>
public class SpeakingExercise
{
    /// <summary>
    /// ID bài tập
    /// </summary>
    public string ExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// Chủ đề
    /// </summary>
    public SpeakingTopic Topic { get; set; }

    /// <summary>
    /// Cấp độ tiếng Anh
    /// </summary>
    public EnglishLevel EnglishLevel { get; set; }

    /// <summary>
    /// Tiêu đề bài tập
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Đoạn văn mẫu để người dùng đọc
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gợi ý cho người dùng
    /// </summary>
    public string Hint { get; set; } = string.Empty;

    /// <summary>
    /// Thời gian tạo
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
