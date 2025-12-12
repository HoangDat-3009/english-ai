using System.ComponentModel.DataAnnotations;

namespace EngAce.Api.DTO
{
    /// <summary>
    /// Represents the submission payload when a learner sends answers for grading.
    /// </summary>
    public class GradeListeningExercise
    {
        /// <summary>
        /// The unique identifier of the exercise that was previously generated.
        /// </summary>
        [Required]
        public Guid ExerciseId { get; set; }

        /// <summary>
        /// The list of selected answers for each question, matched by index.
        /// </summary>
        [Required]
        public List<ListeningAnswerSubmission> Answers { get; set; } = [];
    }

    /// <summary>
    /// Represents the answer chosen by the learner for a specific question.
    /// </summary>
    public class ListeningAnswerSubmission
    {
        /// <summary>
        /// The zero-based position of the question within the exercise.
        /// </summary>
        public int QuestionIndex { get; set; }

        /// <summary>
        /// The option index selected by the learner for the specified question.
        /// </summary>
        public sbyte SelectedOptionIndex { get; set; }
    }
}
