using Entities.Enums;

namespace EngAce.Api.DTO.AI;

public class GenerateSentences
{
    public string Topic { get; set; } = string.Empty;
    public EnglishLevel Level { get; set; } = EnglishLevel.Intermediate;
    public int SentenceCount { get; set; } = 10;
    public string WritingStyle { get; set; } = "Communicative"; // "Communicative" or "Academic"
}


