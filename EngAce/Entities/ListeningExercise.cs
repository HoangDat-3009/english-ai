using Entities.Enums;

namespace Entities
{
    public class ListeningExercise
    {
        public required string Title { get; set; }
        public required string Transcript { get; set; }
        public required ListeningGenre Genre { get; set; }
        public required EnglishLevel EnglishLevel { get; set; }
        public List<Quiz> Questions { get; set; } = [];
    }
}
