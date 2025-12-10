namespace EngAce.Api.DTO.Exercises;

public class CreatePassageRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string PartType { get; set; } = "Part 7"; // Part 6 or Part 7
    public string? Level { get; set; }
    public string? CreatedBy { get; set; }
}
