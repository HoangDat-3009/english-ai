namespace Entities
{
    public class ListeningExerciseResult
    {
        public required string ExerciseId { get; set; }
        public required List<ListeningAnswer> Answers { get; set; }
        public required int TotalQuestions { get; set; }
        public required int CorrectAnswers { get; set; }
        public required double ScorePercentage { get; set; }
        public required TimeSpan TimeSpent { get; set; }
        public DateTime CompletedAt { get; set; } = DateTime.UtcNow;
    }

    public class ListeningAnswer
    {
        public required string QuestionId { get; set; }
        public required int SelectedOptionIndex { get; set; }
        public required bool IsCorrect { get; set; }
        public TimeSpan? TimeSpentOnQuestion { get; set; }
    }
}