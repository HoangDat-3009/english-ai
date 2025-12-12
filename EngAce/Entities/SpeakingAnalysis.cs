namespace Entities;

/// <summary>
/// Kết quả phân tích bài nói
/// </summary>
public class SpeakingAnalysis
{
    /// <summary>
    /// Văn bản được nhận dạng từ giọng nói
    /// </summary>
    public string TranscribedText { get; set; } = string.Empty;

    /// <summary>
    /// Điểm tổng thể (0-100)
    /// </summary>
    public double OverallScore { get; set; }

    /// <summary>
    /// Điểm phát âm (0-100)
    /// </summary>
    public double PronunciationScore { get; set; }

    /// <summary>
    /// Điểm ngữ pháp (0-100)
    /// </summary>
    public double GrammarScore { get; set; }

    /// <summary>
    /// Điểm từ vựng (0-100)
    /// </summary>
    public double VocabularyScore { get; set; }

    /// <summary>
    /// Điểm độ trưng lưu (0-100)
    /// </summary>
    public double FluencyScore { get; set; }

    /// <summary>
    /// Các lỗi ngữ pháp được phát hiện
    /// </summary>
    public List<GrammarError> GrammarErrors { get; set; } = new();

    /// <summary>
    /// Nhận xét chung
    /// </summary>
    public string OverallFeedback { get; set; } = string.Empty;

    /// <summary>
    /// Gợi ý cải thiện
    /// </summary>
    public List<string> Suggestions { get; set; } = new();
}

/// <summary>
/// Lỗi ngữ pháp
/// </summary>
public class GrammarError
{
    /// <summary>
    /// Vị trí bắt đầu lỗi trong văn bản
    /// </summary>
    public int StartIndex { get; set; }

    /// <summary>
    /// Vị trí kết thúc lỗi trong văn bản
    /// </summary>
    public int EndIndex { get; set; }

    /// <summary>
    /// Phần văn bản bị lỗi
    /// </summary>
    public string ErrorText { get; set; } = string.Empty;

    /// <summary>
    /// Loại lỗi (grammar, spelling, punctuation, etc.)
    /// </summary>
    public string ErrorType { get; set; } = string.Empty;

    /// <summary>
    /// Mô tả lỗi
    /// </summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>
    /// Sửa đề xuất
    /// </summary>
    public string Correction { get; set; } = string.Empty;

    /// <summary>
    /// Giải thích bằng tiếng Việt
    /// </summary>
    public string ExplanationInVietnamese { get; set; } = string.Empty;
}
