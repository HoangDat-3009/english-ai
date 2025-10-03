using Entities.Enums;

namespace Entities
{
    public class ListeningQuestion
    {
        public required string Id { get; set; }
        public required string Question { get; set; }
        public required List<string> Options { get; set; }
        public required int CorrectOptionIndex { get; set; }
        public required string ExplanationInVietnamese { get; set; }
        public required ListeningQuestionType Type { get; set; }
        public int? StartTimeInSeconds { get; set; } // For questions tied to specific audio segments
        public int? EndTimeInSeconds { get; set; }
    }
}