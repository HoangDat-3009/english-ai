using Entities.Enums;

namespace EngAce.Api.DTO.Listening;

/// <summary>
/// Represents the response returned after generating a listening exercise.
/// </summary>
public class ListeningExerciseResponse
{
    /// <summary>
    /// Unique identifier assigned to the generated exercise.
    /// </summary>
    public Guid ExerciseId { get; set; }

    /// <summary>
    /// The friendly title of the listening passage.
    /// </summary>
    public required string Title { get; set; }

    /// <summary>
    /// The description of the selected genre, presented in a localized format.
    /// </summary>
    public required string Genre { get; set; }

    /// <summary>
    /// The learner's English proficiency level that the exercise targets.
    /// </summary>
    public EnglishLevel EnglishLevel { get; set; }

    /// <summary>
    /// The full transcript of the listening passage.
    /// </summary>
    public required string Transcript { get; set; }

    /// <summary>
    /// Base64-encoded audio content generated from the transcript via text-to-speech.
    /// </summary>
    public string? AudioContent { get; set; }

    /// <summary>
    /// The question set that belongs to the listening exercise.
    /// </summary>
    public List<ListeningQuestionResponse> Questions { get; set; } = [];
}

/// <summary>
/// Lightweight projection of a listening comprehension question for the UI.
/// </summary>
public class ListeningQuestionResponse
{
    /// <summary>
    /// The prompt that the learner must answer.
    /// </summary>
    public required string Question { get; set; }

    /// <summary>
    /// The answer choices displayed to the learner.
    /// </summary>
    public List<string> Options { get; set; } = [];
}


