using System.Linq;
using System.Reflection;
using EngAce.Api.Controllers;
using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using Events;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace EngAce.Tests.Controllers;

public class ListeningControllerTests
{
    static ListeningControllerTests()
    {
        Environment.SetEnvironmentVariable("ACCESS_KEY", "test-access-key");
        Environment.SetEnvironmentVariable("GEMINI_API_KEY", "test-gemini-key");
    }

    [Fact]
    public async Task Generate_ShouldReturnBadRequest_WhenGenreIsInvalid()
    {
        // Arrange
        var controller = CreateController();
        var request = new GenerateListeningExercise
        {
            Genre = (ListeningGenre)999,
            EnglishLevel = EnglishLevel.Intermediate,
            TotalQuestions = ListeningScope.MinTotalQuestions
        };

        // Act
        var result = await controller.Generate(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Thể loại bài nghe không hợp lệ.");
    }

    [Theory]
    [InlineData(ListeningScope.MinTotalQuestions - 1)]
    [InlineData(ListeningScope.MaxTotalQuestions + 1)]
    public async Task Generate_ShouldValidateQuestionRange(sbyte totalQuestions)
    {
        // Arrange
        var controller = CreateController();
        var request = new GenerateListeningExercise
        {
            Genre = ListeningGenre.Storytelling,
            EnglishLevel = EnglishLevel.Intermediate,
            TotalQuestions = totalQuestions
        };

        // Act
        var result = await controller.Generate(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be($"Số lượng câu hỏi phải nằm trong khoảng {ListeningScope.MinTotalQuestions} đến {ListeningScope.MaxTotalQuestions}.");
    }


    [Fact]
    public async Task Generate_ShouldLimitCustomTopicWordCount()
    {
        // Arrange
        var controller = CreateController();
        var longTopic = string.Join(' ', Enumerable.Range(1, 20).Select(i => $"word{i}"));
        var request = new GenerateListeningExercise
        {
            Genre = ListeningGenre.Storytelling,
            EnglishLevel = EnglishLevel.Intermediate,
            TotalQuestions = 5,
            CustomTopic = longTopic
        };

        // Act
        var result = await controller.Generate(request);

        // Assert
        result.Result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().Be("Chủ đề tùy chỉnh không được vượt quá 12 từ.");
    }

    [Fact]
    public void Grade_ShouldReturnNotFound_WhenExerciseMissing()
    {
        // Arrange
        var controller = CreateController();
        var request = new GradeListeningExercise
        {
            ExerciseId = Guid.NewGuid(),
            Answers = new List<ListeningAnswerSubmission>
            {
                new ListeningAnswerSubmission { QuestionIndex = 0, SelectedOptionIndex = 1 }
            }
        };

        // Act
        var result = controller.Grade(request);

        // Assert
        result.Result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().Be("Không tìm thấy bài nghe hoặc bài đã hết hạn.");
    }

    [Fact]
    public void Grade_ShouldReturnDetailedScore_WhenExerciseCached()
    {
        // Arrange
        var cache = CreateCache();
        var controller = CreateController(cache);
        var exerciseId = SeedListeningExercise(cache, new ListeningExercise
        {
            Title = "Forest Conversations",
            Transcript = "Sample transcript",
            Genre = ListeningGenre.DailyConversation,
            EnglishLevel = EnglishLevel.Beginner,
            Questions = new List<Quiz>
            {
                new()
                {
                    Question = "What did Anna forget?",
                    Options = new List<string> { "Keys", "Phone", "Wallet", "Notebook" },
                    RightOptionIndex = 0,
                    ExplanationInVietnamese = "Anna quên chìa khóa."
                },
                new()
                {
                    Question = "Where are they heading?",
                    Options = new List<string> { "Office", "Cafe", "Airport", "Gym" },
                    RightOptionIndex = 1,
                    ExplanationInVietnamese = "Họ đến quán cafe."
                }
            }
        });

        var request = new GradeListeningExercise
        {
            ExerciseId = exerciseId,
            Answers = new List<ListeningAnswerSubmission>
            {
                new() { QuestionIndex = 0, SelectedOptionIndex = 0 },
                new() { QuestionIndex = 1, SelectedOptionIndex = 0 }
            }
        };

        // Act
        var result = controller.Grade(request);

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var payload = okResult.Value.Should().BeOfType<GradeListeningExerciseResponse>().Which;

        payload.ExerciseId.Should().Be(exerciseId);
        payload.TotalQuestions.Should().Be(2);
        payload.CorrectAnswers.Should().Be(1);
        payload.Score.Should().Be(50);
        payload.Questions.Should().HaveCount(2);
        payload.Questions[0].IsCorrect.Should().BeTrue();
        payload.Questions[1].IsCorrect.Should().BeFalse();
    }

    [Fact]
    public void GetGenres_ShouldReturnDescriptionsForEveryEnumValue()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetGenres();

        // Assert
        var okResult = result.Result.Should().BeOfType<OkObjectResult>().Which;
        var payload = okResult.Value.Should().BeOfType<Dictionary<int, string>>().Which;

        var expectedCount = Enum.GetValues(typeof(ListeningGenre)).Length;
        payload.Should().HaveCount(expectedCount);
        payload.Keys.Should().BeEquivalentTo(Enum.GetValues(typeof(ListeningGenre)).Cast<ListeningGenre>().Select(g => (int)g));
        payload.Values.Should().OnlyContain(value => !string.IsNullOrWhiteSpace(value));
    }

    private static ListeningController CreateController(IMemoryCache? cache = null)
    {
        cache ??= CreateCache();
        return new ListeningController(cache, NullLogger<ListeningController>.Instance);
    }

    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());

    private static Guid SeedListeningExercise(IMemoryCache cache, ListeningExercise exercise, string? audioContent = null)
    {
        var cacheItemType = typeof(ListeningController).GetNestedType("ListeningExerciseCacheItem", BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Không tìm thấy kiểu cache item.");
        var cacheItem = Activator.CreateInstance(cacheItemType, exercise, audioContent)
            ?? throw new InvalidOperationException("Không thể khởi tạo cache item.");

        var exerciseId = Guid.NewGuid();
        cache.Set($"ListeningExercise-{exerciseId}", cacheItem, TimeSpan.FromMinutes(45));
        return exerciseId;
    }
}
