using Entities.Enums;

namespace EngAce.Api.DTO;

/// <summary>
/// Request tạo bài tập nói
/// </summary>
public class GenerateSpeakingExercise
{
    /// <summary>
    /// Chủ đề bài nói
    /// </summary>
    public SpeakingTopic Topic { get; set; }

    /// <summary>
    /// Cấp độ tiếng Anh
    /// </summary>
    public EnglishLevel EnglishLevel { get; set; }

    /// <summary>
    /// Chủ đề tùy chỉnh (không bắt buộc)
    /// </summary>
    public string? CustomTopic { get; set; }

        /// <summary>
        /// Lựa chọn mô hình AI dùng để tạo bài tập.
        /// </summary>
        public AiModel AiModel { get; set; } = AiModel.GeminiFlashLite;
}
