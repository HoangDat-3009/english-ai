using EngAce.Api.DTO.Core;

namespace EngAce.Api.Services.Interfaces;

public interface IProgressService
{
    Task<UserProgressDto?> GetUserProgressAsync(int userId);
    Task<WeeklyProgressDto> GetWeeklyProgressAsync(int userId);
    Task<IEnumerable<ActivityDto>> GetUserActivitiesAsync(int userId, int limit = 10);
    Task<UserProgressDto> UpdateUserProgressAsync(int userId, int exerciseScore, int timeSpent);
}