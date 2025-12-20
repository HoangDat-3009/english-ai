using Entities.Enums;

namespace EngAce.Api.DTO.Listening;

/// <summary>
/// Request phân tích giọng nói
/// </summary>
public class AnalyzeSpeechRequest
{
    /// <summary>
    /// ID bài tập
    /// </summary>
    public string ExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// Văn bản đã được nhận dạng từ giọng nói (transcribed text từ Web Speech API)
    /// </summary>
    public string AudioData { get; set; } = string.Empty;

    /// <summary>
    /// Mô hình AI dùng để phân tích.
    /// </summary>
    public AiModel AiModel { get; set; } = AiModel.GeminiFlashLite;
}


