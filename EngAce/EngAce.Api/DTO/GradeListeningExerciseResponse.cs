namespace EngAce.Api.DTO
{
    /// <summary>
    /// Represents the grading outcome for a listening exercise submission.
    /// </summary>
    public class GradeListeningExerciseResponse
    {
        /// <summary>
        /// The identifier of the graded exercise.
        /// </summary>
        public Guid ExerciseId { get; set; }

        /// <summary>
        /// The title of the listening passage that was graded.
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        /// The transcript associated with the exercise.
        /// </summary>
        public required string Transcript { get; set; }

        /// <summary>
        /// The audio content associated with the transcript if available.
        /// </summary>
        public string? AudioContent { get; set; }

        /// <summary>
        /// Total number of questions contained in the exercise.
        /// </summary>
        public int TotalQuestions { get; set; }

        /// <summary>
        /// Number of questions the learner answered correctly.
        /// </summary>
        public int CorrectAnswers { get; set; }

        /// <summary>
        /// The final score expressed as a percentage.
        /// </summary>
        public double Score { get; set; }

        /// <summary>
        /// Detailed feedback for each question.
        /// </summary>
        public List<ListeningQuestionGradeResponse> Questions { get; set; } = [];
    }

    /// <summary>
    /// Represents the grading feedback for a single question.
    /// </summary>
    public class ListeningQuestionGradeResponse
    {
        /// <summary>
        /// The zero-based index of the question in the original exercise.
        /// </summary>
        public int QuestionIndex { get; set; }

        /// <summary>
        /// The question prompt shown to the learner.
        /// </summary>
        public required string Question { get; set; }

        /// <summary>
        /// The available answer choices.
        /// </summary>
        public List<string> Options { get; set; } = [];

        /// <summary>
        /// The option index selected by the learner, if any.
        /// </summary>
        public sbyte? SelectedOptionIndex { get; set; }

        /// <summary>
        /// The correct option index for the question.
        /// </summary>
        public sbyte RightOptionIndex { get; set; }

        /// <summary>
        /// Explanation in Vietnamese to help the learner review the concept.
        /// </summary>
        public required string ExplanationInVietnamese { get; set; }

        /// <summary>
        /// Indicates whether the learner's answer is correct.
        /// </summary>
        public bool IsCorrect { get; set; }
    }
}
