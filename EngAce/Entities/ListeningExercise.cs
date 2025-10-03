using Entities.Enums;

namespace Entities
{
    public class ListeningExercise
    {
        public required string Id { get; set; }
        public required string Title { get; set; }
        public required string Topic { get; set; }
        public required EnglishLevel Level { get; set; }
        public required string AudioContent { get; set; } // Text content to be converted to audio
        public required string AudioUrl { get; set; } // URL or base64 of audio file
        public required List<ListeningQuestion> Questions { get; set; }
        public required int Duration { get; set; } // Duration in seconds
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}