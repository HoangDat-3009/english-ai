using System.Linq;
using EngAce.Api.DTO.Core;
using Entities;
using Entities.Enums;
using Entities.Models;

namespace EngAce.Api.Helpers;

public static class ToeicPartHelper
{
    private record PartDefinition(
        string Key,
        string Part,
        string Title,
        string Description,
        string Skill,
        IReadOnlyList<AssignmentType> QuestionTypes);

    private record PartAggregate(decimal TotalScore, int Attempts);

    private static readonly IReadOnlyList<PartDefinition> Definitions = new List<PartDefinition>
    {
        new(
            "part1",
            "Part 1",
            "Photographs",
            "Chọn câu mô tả đúng nhất cho hình ảnh.",
            "Listening",
            new[]
            {
                AssignmentType.PronunciationAndStress,
                AssignmentType.Vocabulary,
                AssignmentType.WordMeaningInContext
            }),
        new(
            "part2",
            "Part 2",
            "Question - Response",
            "Nghe câu hỏi và chọn câu trả lời phù hợp.",
            "Listening",
            new[]
            {
                AssignmentType.DialogueCompletion,
                AssignmentType.SentenceOrdering
            }),
        new(
            "part3",
            "Part 3",
            "Conversations",
            "Nghe các đoạn hội thoại ngắn để trả lời câu hỏi.",
            "Listening",
            new[]
            {
                AssignmentType.DialogueCompletion,
                AssignmentType.SentenceCombination,
                AssignmentType.WordMeaningInContext
            }),
        new(
            "part4",
            "Part 4",
            "Short Talks",
            "Nghe các bài nói ngắn và trả lời câu hỏi.",
            "Listening",
            new[]
            {
                AssignmentType.MatchingHeadings,
                AssignmentType.WordMeaningInContext,
                AssignmentType.ClozeTest
            }),
        new(
            "part5",
            "Part 5",
            "Incomplete Sentences",
            "Chọn đáp án đúng để hoàn thành câu.",
            "Reading",
            new[]
            {
                AssignmentType.WordChoice,
                AssignmentType.VerbConjugation,
                AssignmentType.FillTheBlank,
                AssignmentType.Grammar,
                AssignmentType.ErrorIdentification
            }),
        new(
            "part6",
            "Part 6",
            "Text Completion",
            "Điền từ/đoạn văn phù hợp vào đoạn văn.",
            "Reading",
            new[]
            {
                AssignmentType.ClozeTest,
                AssignmentType.SentenceCombination,
                AssignmentType.WordFormation,
                AssignmentType.WordMeaningInContext
            }),
        new(
            "part7",
            "Part 7",
            "Reading Comprehension",
            "Đọc hiểu một hoặc nhiều đoạn văn và trả lời câu hỏi.",
            "Reading",
            new[]
            {
                AssignmentType.ReadingComprehension,
                AssignmentType.MatchingHeadings,
                AssignmentType.WordMeaningInContext,
                AssignmentType.SentenceOrdering
            })
    };

    private static string NormalizeKey(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "part7";
        }

        // First, try to extract digits (handles "Part 6", "Part6", "part 6", etc.)
        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out var number) && number is >= 1 and <= 7)
        {
            return $"part{number}";
        }

        // Second, try exact match with Part definitions (handles "Part 6", "Part 7", etc.)
        raw = raw.Trim();
        var match = Definitions.FirstOrDefault(part =>
            part.Part.Equals(raw, StringComparison.OrdinalIgnoreCase) ||
            part.Key.Equals(raw, StringComparison.OrdinalIgnoreCase));

        if (match != null)
        {
            return match.Key;
        }

        // Third, try partial match (handles "Part6", "part6", etc.)
        var normalizedRaw = raw.Replace(" ", "").ToLowerInvariant();
        match = Definitions.FirstOrDefault(part =>
            part.Key.Equals(normalizedRaw, StringComparison.OrdinalIgnoreCase));

        return match?.Key ?? "part7";
    }

    public static string NormalizePartLabel(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw))
        {
            return "Part 7"; // Default fallback
        }
        
        var key = NormalizeKey(raw);
        var definition = Definitions.FirstOrDefault(part => part.Key == key);
        
        if (definition == null)
        {
            System.Diagnostics.Debug.WriteLine($"Warning: NormalizeKey('{raw}') returned '{key}' but no definition found. Using fallback 'Part 7'.");
            return "Part 7"; // Fallback
        }
        
        System.Diagnostics.Debug.WriteLine($"NormalizePartLabel('{raw}') -> key='{key}' -> part='{definition.Part}'");
        return definition.Part;
    }

    public static List<ToeicPartScoreDto> BuildPartScores(IEnumerable<Completion> completions)
    {
        var completionList = completions.ToList();
        System.Diagnostics.Debug.WriteLine($"BuildPartScores: Processing {completionList.Count} completions");
        
        var aggregates = Definitions.ToDictionary(
            part => part.Part,
            _ => new PartAggregate(0m, 0));

        foreach (var completion in completionList)
        {
            // Ensure Exercise is loaded - if null, try to get from database context
            if (completion.Exercise == null)
            {
                // Skip if Exercise is not loaded - this should not happen if Include is used correctly
                System.Diagnostics.Debug.WriteLine($"Warning: Completion {completion.CompletionId} has null Exercise. Skipping.");
                continue;
            }

            var exerciseType = completion.Exercise.Type ?? completion.Exercise.Category ?? completion.Exercise.Title;
            
            // Debug: Log if exerciseType is null or empty
            if (string.IsNullOrWhiteSpace(exerciseType))
            {
                // Fallback to default Part 7 if type is missing
                System.Diagnostics.Debug.WriteLine($"Warning: Exercise {completion.ExerciseId} has no Type/Category/Title. Using fallback 'Part 7'.");
                exerciseType = "Part 7";
            }

            // Normalize exercise type - handle variations like "Part 6", "Part6", "part 6", etc.
            var partLabel = NormalizePartLabel(exerciseType);
            
            // Debug: Log for troubleshooting
            if (string.IsNullOrWhiteSpace(partLabel))
            {
                System.Diagnostics.Debug.WriteLine($"Warning: NormalizePartLabel returned empty for exerciseType '{exerciseType}'. Using fallback 'Part 7'.");
                partLabel = "Part 7"; // Fallback
            }
            
            // Ensure partLabel exists in aggregates dictionary
            if (!aggregates.ContainsKey(partLabel))
            {
                // Log warning but continue with fallback
                System.Diagnostics.Debug.WriteLine($"Warning: Part label '{partLabel}' (from '{exerciseType}') not found in aggregates. Available keys: {string.Join(", ", aggregates.Keys)}. Using fallback 'Part 7'.");
                partLabel = "Part 7"; // Fallback
                
                // Double check - if still not in aggregates, skip
                if (!aggregates.ContainsKey(partLabel))
                {
                    System.Diagnostics.Debug.WriteLine($"Error: Fallback 'Part 7' also not found in aggregates. Skipping completion {completion.CompletionId}.");
                    continue;
                }
            }

            var aggregate = aggregates[partLabel];
            var scoreToAdd = completion.Score ?? 0;
            System.Diagnostics.Debug.WriteLine($"Adding completion {completion.CompletionId}: ExerciseType='{exerciseType}' -> PartLabel='{partLabel}', Score={scoreToAdd}, Attempts={aggregate.Attempts + 1}");
            
            aggregates[partLabel] = aggregate with
            {
                TotalScore = aggregate.TotalScore + scoreToAdd,
                Attempts = aggregate.Attempts + 1
            };
        }
        
        System.Diagnostics.Debug.WriteLine($"BuildPartScores: Final aggregates - {string.Join(", ", aggregates.Select(kvp => $"{kvp.Key}: Score={kvp.Value.TotalScore}, Attempts={kvp.Value.Attempts}"))}");

        return Definitions
            .Select(definition =>
            {
                var aggregate = aggregates[definition.Part];
                var average = aggregate.Attempts > 0
                    ? (double)(aggregate.TotalScore / aggregate.Attempts)
                    : 0;

                return new ToeicPartScoreDto
                {
                    Key = definition.Key,
                    Part = definition.Part,
                    Label = $"{definition.Part} - {definition.Title}",
                    Title = definition.Title,
                    Skill = definition.Skill,
                    Description = definition.Description,
                    QuestionTypes = definition.QuestionTypes
                        .Select(type => NameAttribute.GetEnumName(type))
                        .ToList(),
                    Score = Math.Round(average, 2),
                    Attempts = aggregate.Attempts
                };
            })
            .ToList();
    }

    public static double SumListening(IEnumerable<ToeicPartScoreDto> parts) =>
        parts.Where(p => string.Equals(p.Skill, "Listening", StringComparison.OrdinalIgnoreCase))
             .Sum(p => p.Score);

    public static double SumReading(IEnumerable<ToeicPartScoreDto> parts) =>
        parts.Where(p => string.Equals(p.Skill, "Reading", StringComparison.OrdinalIgnoreCase))
             .Sum(p => p.Score);

    public static double GetPartScore(IEnumerable<ToeicPartScoreDto> parts, string? filter)
    {
        var normalizedKey = NormalizeKey(filter);
        return parts.FirstOrDefault(p => p.Key.Equals(normalizedKey, StringComparison.OrdinalIgnoreCase))?.Score ?? 0;
    }

    public static bool IsPartFilter(string? filter)
    {
        if (string.IsNullOrWhiteSpace(filter)) return false;
        var normalized = NormalizeKey(filter);
        return Definitions.Any(part => part.Key.Equals(normalized, StringComparison.OrdinalIgnoreCase));
    }

}

