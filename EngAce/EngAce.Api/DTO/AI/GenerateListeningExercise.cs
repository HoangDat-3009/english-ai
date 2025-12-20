using Entities.Enums;

namespace EngAce.Api.DTO.AI;

/// <summary>
/// The request payload for generating a listening comprehension exercise.
/// </summary>
public class GenerateListeningExercise
{
    /// <summary>
    /// The genre or style of the listening passage to generate.
    /// </summary>
    public ListeningGenre Genre { get; set; }

    /// <summary>
    /// The learner's English proficiency level used to calibrate difficulty.
    /// </summary>
    public EnglishLevel EnglishLevel { get; set; } = EnglishLevel.Intermediate;

    /// <summary>
    /// The total number of questions that the exercise should contain.
    /// </summary>
    public sbyte TotalQuestions { get; set; } = 5;

    /// <summary>
    /// An optional custom topic that the generated listening passage should focus on.
    /// </summary>
    public string? CustomTopic { get; set; }

    /// <summary>
    /// The AI model that will be used to craft the exercise content.
    /// </summary>
    public AiModel AiModel { get; set; } = AiModel.Gpt5Preview;
}


