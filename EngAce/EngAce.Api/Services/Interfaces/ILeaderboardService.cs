using EngAce.Api.DTO.Core;

namespace EngAce.Api.Services.Interfaces;

public interface ILeaderboardService
{
    Task<IEnumerable<LeaderboardEntryDto>> GetLeaderboardAsync(string? timeFilter = null, string? skill = null);
    Task<UserRankDto?> GetUserRankAsync(int userId);
    Task<IEnumerable<LeaderboardEntryDto>> GetTopUsersAsync(int count = 10);
    Task<LeaderboardStatsDto> GetLeaderboardStatsAsync();
}