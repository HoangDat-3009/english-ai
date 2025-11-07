namespace EngAce.Api.DTO.Exercises;

public class AddQuestionsRequest
{
    public List<QuestionData> Questions { get; set; } = new();
}

public class QuestionData
{
    public string QuestionText { get; set; } = string.Empty;
    public string OptionA { get; set; } = string.Empty;
    public string OptionB { get; set; } = string.Empty;
    public string OptionC { get; set; } = string.Empty;
    public string OptionD { get; set; } = string.Empty;
    public int CorrectAnswer { get; set; } // 1, 2, 3, or 4
    public string? Explanation { get; set; }
    public int OrderNumber { get; set; }
}
