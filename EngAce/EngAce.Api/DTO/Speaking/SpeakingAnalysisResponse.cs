using Entities;

namespace EngAce.Api.DTO.Speaking;

/// <summary>
/// Response phân tích giọng nói
/// </summary>
public class SpeakingAnalysisResponse
{
    /// <summary>
    /// Văn bản được nhận dạng
    /// </summary>
    public string TranscribedText { get; set; } = string.Empty;

    /// <summary>
    /// Điểm tổng thể
    /// </summary>
    public double OverallScore { get; set; }

    /// <summary>
    /// Điểm phát âm
    /// </summary>
    public double PronunciationScore { get; set; }

    /// <summary>
    /// Điểm ngữ pháp
    /// </summary>
    public double GrammarScore { get; set; }

    /// <summary>
    /// Điểm từ vựng
    /// </summary>
    public double VocabularyScore { get; set; }

    /// <summary>
    /// Điểm độ trôi chảy
    /// </summary>
    public double FluencyScore { get; set; }

    /// <summary>
    /// Các lỗi ngữ pháp
    /// </summary>
    public List<GrammarErrorDto> GrammarErrors { get; set; } = new();

    /// <summary>
    /// Nhận xét chung
    /// </summary>
    public string OverallFeedback { get; set; } = string.Empty;

    /// <summary>
    /// Gợi ý cải thiện
    /// </summary>
    public List<string> Suggestions { get; set; } = new();

    public static SpeakingAnalysisResponse FromEntity(SpeakingAnalysis entity)
    {
        return new SpeakingAnalysisResponse
        {
            TranscribedText = entity.TranscribedText,
            OverallScore = entity.OverallScore,
            PronunciationScore = entity.PronunciationScore,
            GrammarScore = entity.GrammarScore,
            VocabularyScore = entity.VocabularyScore,
            FluencyScore = entity.FluencyScore,
            GrammarErrors = entity.GrammarErrors.Select(GrammarErrorDto.FromEntity).ToList(),
            OverallFeedback = entity.OverallFeedback,
            Suggestions = entity.Suggestions
        };
    }
}

public class GrammarErrorDto
{
    public int StartIndex { get; set; }
    public int EndIndex { get; set; }
    public string ErrorText { get; set; } = string.Empty;
    public string ErrorType { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Correction { get; set; } = string.Empty;
    public string ExplanationInVietnamese { get; set; } = string.Empty;

    public static GrammarErrorDto FromEntity(GrammarError entity)
    {
        return new GrammarErrorDto
        {
            StartIndex = entity.StartIndex,
            EndIndex = entity.EndIndex,
            ErrorText = entity.ErrorText,
            ErrorType = entity.ErrorType,
            Description = entity.Description,
            Correction = entity.Correction,
            ExplanationInVietnamese = entity.ExplanationInVietnamese
        };
    }
}


