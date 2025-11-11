namespace EngAce.Api.DTO;

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
    /// Audio file (base64)
    /// </summary>
    public string AudioData { get; set; } = string.Empty;
}
