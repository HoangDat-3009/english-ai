using Entities.Enums;

namespace EngAce.Api.DTO.AI;

public class GenerateComment
{
    public string Requirement { get; set; }
    public string Content { get; set; }
    public EnglishLevel UserLevel { get; set; }
}


