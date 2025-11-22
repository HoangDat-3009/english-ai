using System;
using System.Linq;
using EngAce.Api.Controllers;
using EngAce.Api.DTO;
using Entities;
using Entities.Enums;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging.Abstractions;

namespace EngAce.Tests.Controllers;

public class SpeakingControllerTests
{
    [Fact]
    public void GetTopics_ShouldReturnSevenLocalizedEntries()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetTopics();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        var topics = okResult.Value.Should().BeOfType<Dictionary<int, string>>().Which;

        topics.Should().HaveCount(7);
        topics.Keys.Should().BeEquivalentTo(Enum.GetValues(typeof(SpeakingTopic)).Cast<SpeakingTopic>().Select(topic => (int)topic));
        topics.Values.Should().OnlyContain(name => !string.IsNullOrWhiteSpace(name));
    }

    [Fact]
    public void GetEnglishLevels_ShouldReturnEveryLevel()
    {
        // Arrange
        var controller = CreateController();

        // Act
        var result = controller.GetEnglishLevels();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Which;
        var levels = okResult.Value.Should().BeOfType<Dictionary<int, string>>().Which;

        levels.Should().HaveCount(Enum.GetValues(typeof(EnglishLevel)).Length);
        levels.Keys.Should().BeEquivalentTo(Enum.GetValues(typeof(EnglishLevel)).Cast<EnglishLevel>().Select(level => (int)level));
    }

    [Fact]
    public async Task Analyze_ShouldReturnNotFound_WhenExerciseNotInCache()
    {
        // Arrange
        var controller = CreateController();
        var request = new AnalyzeSpeechRequest
        {
            ExerciseId = Guid.NewGuid().ToString(),
            AudioData = "Any transcript"
        };

        // Act
        var result = await controller.Analyze(request);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { message = "Bài tập không tồn tại hoặc đã hết hạn" });
    }

    [Fact]
    public async Task Analyze_ShouldReturnBadRequest_WhenTranscriptMissing()
    {
        // Arrange
        var cache = CreateCache();
        var controller = CreateController(cache);
        var exercise = new SpeakingExercise
        {
            ExerciseId = Guid.NewGuid().ToString(),
            Topic = SpeakingTopic.DailyLife,
            EnglishLevel = EnglishLevel.Elementary,
            Title = "Daily routines",
            Prompt = "Describe your morning routine.",
            Hint = "Nhắc đến 2-3 hoạt động chính"
        };
        cache.Set($"speaking_exercise_{exercise.ExerciseId}", exercise, TimeSpan.FromMinutes(45));

        var request = new AnalyzeSpeechRequest
        {
            ExerciseId = exercise.ExerciseId,
            AudioData = "   "
        };

        // Act
        var result = await controller.Analyze(request);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>()
            .Which.Value.Should().BeEquivalentTo(new { message = "Không phát hiện văn bản. Vui lòng thử lại." });
    }

    private static SpeakingController CreateController(IMemoryCache? cache = null)
    {
        cache ??= CreateCache();
        return new SpeakingController(cache, NullLogger<SpeakingController>.Instance);
    }

    private static IMemoryCache CreateCache() => new MemoryCache(new MemoryCacheOptions());
}
