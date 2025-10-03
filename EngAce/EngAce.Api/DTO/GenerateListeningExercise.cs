using Entities.Enums;

namespace EngAce.Api.DTO
{
    public class GenerateListeningExercise
    {
        public required string Topic { get; set; }
        public required EnglishLevel Level { get; set; }
        public required int TotalQuestions { get; set; }
        public required List<ListeningQuestionType> QuestionTypes { get; set; }
        public int? DurationInMinutes { get; set; } = 5; // Default 5 minutes
        public string? PreferredAccent { get; set; } = "American"; // American, British, Australian
    }
}