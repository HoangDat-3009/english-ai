namespace EngAce.Api.DTO
{
    public class SubmitListeningExercise
    {
        public required string ExerciseId { get; set; }
        public required List<ListeningAnswerDto> Answers { get; set; }
        public required TimeSpan TimeSpent { get; set; }
    }

    public class ListeningAnswerDto
    {
        public required string QuestionId { get; set; }
        public required int SelectedOptionIndex { get; set; }
        public TimeSpan? TimeSpentOnQuestion { get; set; }
    }
}