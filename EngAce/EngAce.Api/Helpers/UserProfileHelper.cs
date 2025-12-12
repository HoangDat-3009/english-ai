using Entities.Models;

namespace EngAce.Api.Helpers;

/// <summary>
/// Utility helper for deriving learning profile metadata from aggregate stats.
/// Replaces legacy Level/StudyStreak columns that no longer exist in the MySQL schema.
/// </summary>
public static class UserProfileHelper
{
    private static readonly (int Threshold, string Tier)[] TierBoundaries =
    [
        (6000, "Legendary"),
        (4000, "Elite"),
        (2500, "Advanced"),
        (1200, "Skilled"),
        (0, "Foundation")
    ];

    public static string GetProfileTier(int totalXp)
    {
        foreach (var (threshold, tier) in TierBoundaries)
        {
            if (totalXp >= threshold)
            {
                return tier;
            }
        }

        return "Foundation";
    }

    public static int CalculateStudyStreak(IEnumerable<Completion> completions)
    {
        var dates = completions
            .Where(c => c.CompletedAt.HasValue)
            .Select(c => c.CompletedAt!.Value);

        return CalculateStudyStreak(dates);
    }

    public static int CalculateStudyStreak(IEnumerable<DateTime> completionDates)
    {
        var distinctDates = completionDates
            .Select(d => d.Date)
            .Distinct()
            .OrderByDescending(d => d)
            .ToList();

        if (distinctDates.Count == 0)
        {
            return 0;
        }

        var streak = 0;
        var expectedDate = DateTime.UtcNow.Date;

        foreach (var date in distinctDates)
        {
            if (date == expectedDate)
            {
                streak++;
                expectedDate = expectedDate.AddDays(-1);
            }
            else if (date == expectedDate.AddDays(-1))
            {
                streak++;
                expectedDate = date.AddDays(-1);
            }
            else if (date < expectedDate)
            {
                break;
            }
        }

        return streak;
    }
}

