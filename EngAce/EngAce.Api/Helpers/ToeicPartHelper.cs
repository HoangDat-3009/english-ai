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

        var digits = new string(raw.Where(char.IsDigit).ToArray());
        if (int.TryParse(digits, out var number) && number is >= 1 and <= 7)
        {
            return $"part{number}";
        }

        raw = raw.Trim();
        var match = Definitions.FirstOrDefault(part =>
            part.Part.Equals(raw, StringComparison.OrdinalIgnoreCase));

        return match?.Key ?? "part7";
    }

    public static string NormalizePartLabel(string? raw)
    {
        var key = NormalizeKey(raw);
        return Definitions.First(part => part.Key == key).Part;
    }

    public static List<ToeicPartScoreDto> BuildPartScores(IEnumerable<Completion> completions)
    {
        var aggregates = Definitions.ToDictionary(
            part => part.Part,
            _ => new PartAggregate(0m, 0));

        foreach (var completion in completions)
        {
            var exerciseType = completion.Exercise?.Type ?? completion.Exercise?.Category ?? completion.Exercise?.Title;
            var partLabel = NormalizePartLabel(exerciseType);
            var aggregate = aggregates[partLabel];
            aggregates[partLabel] = aggregate with
            {
                TotalScore = aggregate.TotalScore + (completion.Score ?? 0),
                Attempts = aggregate.Attempts + 1
            };
        }

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

