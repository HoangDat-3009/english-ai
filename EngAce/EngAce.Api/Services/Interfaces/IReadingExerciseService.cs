using EngAce.Api.DTO.Exercises;
using Entities.Models;

namespace EngAce.Api.Services.Interfaces;

public interface IReadingExerciseService
{
    Task<IEnumerable<ReadingExerciseDto>> GetAllExercisesAsync();
    Task<ReadingExerciseDto?> GetExerciseByIdAsync(int id);
    Task<ReadingExerciseDto> CreateExerciseAsync(ReadingExercise exercise);
    Task<ReadingExerciseDto?> UpdateExerciseAsync(int id, ReadingExercise exercise);
    Task<bool> DeleteExerciseAsync(int id);
    Task<IEnumerable<ReadingExerciseDto>> GetExercisesByLevelAsync(string level);
    Task<ReadingExerciseDto> SubmitExerciseResultAsync(int exerciseId, int userId, List<int> answers);
    Task<ReadingExerciseDto> AddQuestionsToExerciseAsync(int exerciseId, List<ReadingQuestion> questions);
    Task<ReadingExercise> ProcessUploadedFileAsync(IFormFile file, string createdBy);
    Task<ReadingExercise> ProcessCompleteFileAsync(IFormFile file, string exerciseName, string partType, string level, string createdBy);
    Task<ReadingExerciseDto> CreateExerciseWithAIQuestionsAsync(CreateExerciseWithAIRequest request);
    Task<bool> GenerateAdditionalQuestionsAsync(int exerciseId, int questionCount = 3);
    Task<string> ClearAllExercisesAsync();
    Task<string> FixAISourceTypeAsync();
}