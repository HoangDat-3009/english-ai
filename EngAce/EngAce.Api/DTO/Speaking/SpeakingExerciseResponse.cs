using Entities;

namespace EngAce.Api.DTO.Speaking;

/// <summary>
/// Response bài tập nói
/// </summary>
public class SpeakingExerciseResponse
{
    /// <summary>
    /// ID bài tập
    /// </summary>
    public string ExerciseId { get; set; } = string.Empty;

    /// <summary>
    /// Chủ đề
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Cấp độ
    /// </summary>
    public int EnglishLevel { get; set; }

    /// <summary>
    /// Tiêu đề
    /// </summary>
    public string Title { get; set; } = string.Empty;

    /// <summary>
    /// Đoạn văn yêu cầu người dùng nói
    /// </summary>
    public string Prompt { get; set; } = string.Empty;

    /// <summary>
    /// Gợi ý
    /// </summary>
    public string Hint { get; set; } = string.Empty;

    /// <summary>
    /// Audio mẫu (base64)
    /// </summary>
    public string? SampleAudio { get; set; }

    public static SpeakingExerciseResponse FromEntity(SpeakingExercise entity, string? audioContent = null)
    {
        return new SpeakingExerciseResponse
        {
            ExerciseId = entity.ExerciseId,
            Topic = entity.Topic.ToString(),
            EnglishLevel = (int)entity.EnglishLevel,
            Title = entity.Title,
            Prompt = entity.Prompt,
            Hint = entity.Hint,
            SampleAudio = audioContent
        };
    }
}


