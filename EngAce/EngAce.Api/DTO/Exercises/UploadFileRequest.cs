namespace EngAce.Api.DTO.Exercises;

public class UploadFileRequest
{
    public string FileName { get; set; } = string.Empty;
    public string FileContent { get; set; } = string.Empty; // Base64 encoded
    public string ExerciseName { get; set; } = string.Empty;
    public string PartType { get; set; } = "Part 7";
    public string Level { get; set; } = "Intermediate";
    public string CreatedBy { get; set; } = "System";
}
