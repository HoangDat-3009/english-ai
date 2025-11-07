using EngAce.Api.DTO.Exercises;
using EngAce.Api.Services.Interfaces;
using EngAce.Api.Services.AI;
using Entities.Data;
using Entities.Models;
using Microsoft.EntityFrameworkCore;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Canvas.Parser;

namespace EngAce.Api.Services;

public class ReadingExerciseService : IReadingExerciseService
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<ReadingExerciseService> _logger;
    private readonly IGeminiService _geminiService;

    public ReadingExerciseService(ApplicationDbContext context, ILogger<ReadingExerciseService> logger, IGeminiService geminiService)
    {
        _context = context;
        _logger = logger;
        _geminiService = geminiService;
    }

    public async Task<IEnumerable<ReadingExerciseDto>> GetAllExercisesAsync()
    {
        var exercises = await _context.ReadingExercises
            .Include(e => e.Questions)
            .Where(e => e.IsActive)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        return exercises.Select(MapToDto);
    }

    public async Task<ReadingExerciseDto?> GetExerciseByIdAsync(int id)
    {
        var exercise = await _context.ReadingExercises
            .Include(e => e.Questions.OrderBy(q => q.OrderNumber))
            .FirstOrDefaultAsync(e => e.Id == id && e.IsActive);

        return exercise != null ? MapToDto(exercise) : null;
    }

    public async Task<ReadingExerciseDto> CreateExerciseAsync(ReadingExercise exercise)
    {
        exercise.CreatedAt = DateTime.UtcNow;
        exercise.IsActive = true;

        _context.ReadingExercises.Add(exercise);
        await _context.SaveChangesAsync();

        return MapToDto(exercise);
    }

    public async Task<ReadingExerciseDto?> UpdateExerciseAsync(int id, ReadingExercise exercise)
    {
        var existingExercise = await _context.ReadingExercises.FindAsync(id);
        if (existingExercise == null) return null;

        existingExercise.Name = exercise.Name;
        existingExercise.Content = exercise.Content;
        existingExercise.Level = exercise.Level;
        existingExercise.Type = exercise.Type;
        existingExercise.Description = exercise.Description;
        existingExercise.EstimatedMinutes = exercise.EstimatedMinutes;
        existingExercise.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return MapToDto(existingExercise);
    }

    public async Task<bool> DeleteExerciseAsync(int id)
    {
        var exercise = await _context.ReadingExercises.FindAsync(id);
        if (exercise == null) return false;

        exercise.IsActive = false;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<IEnumerable<ReadingExerciseDto>> GetExercisesByLevelAsync(string level)
    {
        var exercises = await _context.ReadingExercises
            .Include(e => e.Questions)
            .Where(e => e.Level == level && e.IsActive)
            .OrderBy(e => e.CreatedAt)
            .ToListAsync();

        return exercises.Select(MapToDto);
    }

    public async Task<ReadingExerciseDto> SubmitExerciseResultAsync(int exerciseId, int userId, List<int> answers)
    {
        var exercise = await _context.ReadingExercises
            .Include(e => e.Questions.OrderBy(q => q.OrderNumber))
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        if (exercise == null)
            throw new ArgumentException($"Exercise with ID {exerciseId} not found");

        var questions = exercise.Questions.OrderBy(q => q.OrderNumber).ToList();
        int correctAnswers = 0;

        // Calculate score
        for (int i = 0; i < Math.Min(answers.Count, questions.Count); i++)
        {
            if (answers[i] == questions[i].CorrectAnswer)
                correctAnswers++;
        }

        var score = (int)Math.Round((double)correctAnswers / questions.Count * 100);

        // Save result
        var result = new ReadingExerciseResult
        {
            UserId = userId,
            ReadingExerciseId = exerciseId,
            Score = score,
            TotalQuestions = questions.Count,
            CorrectAnswers = correctAnswers,
            UserAnswers = string.Join(",", answers),
            TimeSpent = TimeSpan.FromMinutes(exercise.EstimatedMinutes),
            StartedAt = DateTime.UtcNow.AddMinutes(-exercise.EstimatedMinutes),
            CompletedAt = DateTime.UtcNow,
            IsCompleted = true
        };

        _context.ReadingExerciseResults.Add(result);

        // Update user progress
        var userProgress = await _context.UserProgresses.FindAsync(userId);
        if (userProgress != null)
        {
            userProgress.CompletedExercises++;
            userProgress.ReadingScore = Math.Max(userProgress.ReadingScore, score);
            userProgress.TotalScore += score / 4; // Assuming 4 skills
            userProgress.LastUpdated = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        var exerciseDto = MapToDto(exercise);
        exerciseDto.UserResult = new UserResultDto
        {
            Score = score,
            CorrectAnswers = correctAnswers,
            TotalQuestions = questions.Count,
            CompletedAt = DateTime.UtcNow
        };

        return exerciseDto;
    }

    public async Task<ReadingExerciseDto> AddQuestionsToExerciseAsync(int exerciseId, List<ReadingQuestion> questions)
    {
        var exercise = await _context.ReadingExercises
            .Include(e => e.Questions)
            .FirstOrDefaultAsync(e => e.Id == exerciseId);

        if (exercise == null)
        {
            throw new ArgumentException($"Exercise with ID {exerciseId} not found");
        }

        // Add questions to exercise
        foreach (var question in questions)
        {
            question.ReadingExerciseId = exerciseId;
            question.CreatedAt = DateTime.UtcNow;
            _context.ReadingQuestions.Add(question);
        }

        // Activate exercise now that it has questions
        exercise.IsActive = true;
        exercise.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Reload to get updated questions
        await _context.Entry(exercise).ReloadAsync();
        await _context.Entry(exercise).Collection(e => e.Questions).LoadAsync();

        return MapToDto(exercise);
    }

    public async Task<ReadingExercise> ProcessUploadedFileAsync(IFormFile file, string createdBy)
    {
        string content = string.Empty;

        try
        {
            if (file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                content = await ExtractTextFromDocx(file);
            }
            else if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                content = await ExtractTextFromPdf(file);
            }
            else if (file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                content = await ExtractTextFromTxt(file);
            }
            else
            {
                throw new ArgumentException("Unsupported file format. Only .docx, .pdf and .txt files are supported.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("No text content could be extracted from the file.");
            }

            var exercise = new ReadingExercise
            {
                Name = Path.GetFileNameWithoutExtension(file.FileName),
                Content = content.Trim(),
                Level = "Intermediate", // Default level
                Type = "Part 6", // Default type for uploaded files
                SourceType = "uploaded",
                CreatedBy = createdBy,
                OriginalFileName = file.FileName,
                Description = $"Reading exercise generated from uploaded file: {file.FileName}",
                // For uploaded exercises we intentionally do not set a time estimate
                EstimatedMinutes = 0,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            // Try to detect an answer key inside the extracted content (only for text files or combined files)
            // Supported markers: "Answer Key", "Answers:", "=== ANSWERS ===" (case-insensitive)
            try
            {
                var (passage, answers) = ParsePassageAndAnswers(content);
                if (!string.IsNullOrWhiteSpace(passage))
                {
                    exercise.Content = passage.Trim();
                }

                if (answers != null && answers.Count > 0)
                {
                    var questions = new List<ReadingQuestion>();
                    for (int i = 0; i < answers.Count; i++)
                    {
                        var letter = answers[i];
                        var correctIndex = LetterToIndex(letter);

                        var rq = new ReadingQuestion
                        {
                            // placeholder question text; later we can call AI to generate full questions
                            QuestionText = $"Auto-generated question {i + 1}",
                            OptionA = "A",
                            OptionB = "B",
                            OptionC = "C",
                            OptionD = "D",
                            CorrectAnswer = correctIndex >= 0 ? correctIndex : 0,
                            Explanation = "Imported answer key",
                            OrderNumber = i + 1,
                            Difficulty = 3,
                            CreatedAt = DateTime.UtcNow
                        };
                        questions.Add(rq);
                    }

                    // Attach to exercise so EF will insert them together when saving
                    exercise.Questions = questions;
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "No parsable answer key found in uploaded file {FileName}", file.FileName);
            }

            return exercise;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing uploaded file: {FileName}", file.FileName);
            throw new InvalidOperationException($"Failed to process the uploaded file: {ex.Message}", ex);
        }
    }

    public async Task<ReadingExercise> ProcessCompleteFileAsync(IFormFile file, string exerciseName, string partType, string level, string createdBy)
    {
        string content = string.Empty;

        try
        {
            // Extract text from file
            if (file.FileName.EndsWith(".docx", StringComparison.OrdinalIgnoreCase))
            {
                content = await ExtractTextFromDocx(file);
            }
            else if (file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase))
            {
                content = await ExtractTextFromPdf(file);
            }
            else if (file.FileName.EndsWith(".txt", StringComparison.OrdinalIgnoreCase))
            {
                content = await ExtractTextFromTxt(file);
            }
            else
            {
                throw new ArgumentException("Unsupported file format. Only .docx, .pdf and .txt files are supported.");
            }

            if (string.IsNullOrWhiteSpace(content))
            {
                throw new InvalidOperationException("No text content could be extracted from the file.");
            }

            // Parse passage and questions/answers from content
            var (passage, questions) = ParseCompleteFileContent(content, partType);

            // For Part 5, passage is optional (grammar questions don't need reading passage)
            // For Part 6 & 7, passage is required
            if (string.IsNullOrWhiteSpace(passage) && (partType == "Part 6" || partType == "Part 7"))
            {
                throw new InvalidOperationException($"{partType} requires a reading passage. Please include passage content before questions.");
            }

            // If no passage for Part 5, use placeholder
            var contentText = string.IsNullOrWhiteSpace(passage) 
                ? $"Grammar and Vocabulary Questions ({partType})" 
                : passage.Trim();

            // Validate we have questions
            if (questions == null || questions.Count == 0)
            {
                throw new InvalidOperationException("No questions could be parsed from the file. Please check file format.");
            }

            var exercise = new ReadingExercise
            {
                Name = exerciseName,
                Content = contentText,
                Level = level,
                Type = partType,
                SourceType = "uploaded",
                CreatedBy = createdBy,
                OriginalFileName = file.FileName,
                Description = $"Complete exercise uploaded from: {file.FileName}",
                EstimatedMinutes = GetEstimatedMinutes(partType),
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                Questions = questions
            };

            return exercise;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing complete file: {FileName}", file.FileName);
            throw new InvalidOperationException($"Failed to process the uploaded file: {ex.Message}", ex);
        }
    }

    private async Task<string> ExtractTextFromDocx(IFormFile file)
    {
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var doc = WordprocessingDocument.Open(stream, false);
            var body = doc.MainDocumentPart?.Document?.Body;
            
            if (body == null) 
            {
                _logger.LogWarning("Document body is null for file: {FileName}", file.FileName);
                return string.Empty;
            }

            var paragraphs = body.Elements<Paragraph>();
            var text = string.Join("\n", paragraphs.Select(p => p.InnerText));
            _logger.LogInformation("Extracted {Length} characters from DOCX: {FileName}", text.Length, file.FileName);
            return text;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from DOCX file: {FileName}", file.FileName);
            throw new InvalidOperationException($"Failed to extract text from DOCX file: {ex.Message}", ex);
        }
    }

    private async Task<string> ExtractTextFromPdf(IFormFile file)
    {
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var pdfReader = new PdfReader(stream);
            using var pdfDocument = new PdfDocument(pdfReader);
            
            var text = new List<string>();
            
            for (int i = 1; i <= pdfDocument.GetNumberOfPages(); i++)
            {
                var page = pdfDocument.GetPage(i);
                var pageText = PdfTextExtractor.GetTextFromPage(page);
                if (!string.IsNullOrWhiteSpace(pageText))
                {
                    text.Add(pageText);
                }
            }

            var result = string.Join("\n", text);
            _logger.LogInformation("Extracted {Length} characters from PDF: {FileName}", result.Length, file.FileName);
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from PDF file: {FileName}", file.FileName);
            throw new InvalidOperationException($"Failed to extract text from PDF file: {ex.Message}", ex);
        }
    }

    private async Task<string> ExtractTextFromTxt(IFormFile file)
    {
        try
        {
            using var stream = new MemoryStream();
            await file.CopyToAsync(stream);
            stream.Position = 0;

            using var reader = new StreamReader(stream, System.Text.Encoding.UTF8);
            var content = await reader.ReadToEndAsync();
            
            _logger.LogInformation("Extracted {Length} characters from TXT: {FileName}", content?.Length ?? 0, file.FileName);
            return content ?? string.Empty;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting text from TXT file: {FileName}", file.FileName);
            throw new InvalidOperationException($"Failed to extract text from TXT file: {ex.Message}", ex);
        }
    }

    // Parse a combined text that may include passage + answer key.
    // Returns (passageText, listOfAnswerLetters) where answer letters are like "A","B","C","D".
    private (string passage, List<string>? answers) ParsePassageAndAnswers(string content)
    {
        if (string.IsNullOrWhiteSpace(content)) return (string.Empty, null);

        // Look for common answer key markers (case-insensitive)
        var markers = new[] { "=== ANSWERS ===", "answer key", "answers:", "answer:" };
        var idx = -1;
        var lower = content.ToLowerInvariant();
        foreach (var m in markers)
        {
            var pos = lower.IndexOf(m.ToLowerInvariant(), StringComparison.Ordinal);
            if (pos >= 0)
            {
                idx = pos;
                break;
            }
        }

        if (idx < 0)
        {
            // Try to detect a block of short lines at the end like "1. A" or "1) B"
            var lines = content.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(l => l.Trim()).ToList();
            // consider last 30 lines as potential answers
            var tail = lines.Skip(Math.Max(0, lines.Count - 30)).ToList();
            var answerCandidates = new List<string>();
            foreach (var line in tail)
            {
                // match lines that start with a number and contain A-D letter
                var m = System.Text.RegularExpressions.Regex.Match(line, "^\\s*\\d+\\W*\\s*([A-Da-d])\\b");
                if (m.Success)
                {
                    answerCandidates.Add(m.Groups[1].Value.ToUpperInvariant());
                }
            }

            if (answerCandidates.Count >= 1)
            {
                // assume passage is everything before tail block
                var passageLines = lines.Take(lines.Count - answerCandidates.Count).ToList();
                var passageText = string.Join("\n", passageLines).Trim();
                return (passageText, answerCandidates);
            }

            return (content.Trim(), null);
        }

        // We found an explicit marker; split content
        var passagePart = content.Substring(0, idx).Trim();
        var answersPart = content.Substring(idx).Trim();

        // Extract letters from answersPart
        var answerLines = answersPart.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Trim()).ToList();

        var answers = new List<string>();
        foreach (var line in answerLines)
        {
            var m = System.Text.RegularExpressions.Regex.Match(line, "([A-Da-d])");
            if (m.Success)
            {
                answers.Add(m.Groups[1].Value.ToUpperInvariant());
            }
        }

        return (passagePart, answers.Count > 0 ? answers : null);
    }

    private int LetterToIndex(string? letter)
    {
        if (string.IsNullOrWhiteSpace(letter)) return -1;
        switch (letter.Trim().ToUpperInvariant())
        {
            case "A": return 0;
            case "B": return 1;
            case "C": return 2;
            case "D": return 3;
            default: return -1;
        }
    }

    public async Task<ReadingExerciseDto> CreateExerciseWithAIQuestionsAsync(CreateExerciseWithAIRequest request)
    {
        try
        {
            // Create the exercise first
            var exercise = new ReadingExercise
            {
                Name = request.Name,
                Content = request.Content,
                Level = request.Level,
                Type = request.Type,
                Description = request.Description,
                EstimatedMinutes = request.EstimatedMinutes,
                CreatedBy = request.CreatedBy,
                SourceType = "ai", // ✅ Mark as AI-generated
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.ReadingExercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Generate questions using Gemini AI
            var generatedQuestions = await _geminiService.GenerateQuestionsAsync(
                request.Content, 
                request.Type, 
                request.Level, 
                request.Type == "Part 6" ? 4 : (request.QuestionCount ?? 5)  // Part 6 always has 4 questions
            );

            // For Part 6, update the exercise content with the complete passage from Gemini
            if (request.Type == "Part 6" && generatedQuestions.Any())
            {
                // Extract passage from Gemini response or use template as fallback
                var part6Content = await ExtractPart6PassageFromGemini(request.Content, request.Level) 
                                   ?? CreatePart6Passage(request.Content, request.Level);
                exercise.Content = part6Content;
                _context.ReadingExercises.Update(exercise);
                await _context.SaveChangesAsync();
            }

            // For Part 7, update the exercise content with reading passage(s) from Gemini
            if (request.Type == "Part 7" && generatedQuestions.Any())
            {
                // Extract reading passage from Gemini response or use template as fallback
                var part7Content = await ExtractPart7PassageFromGeminiResponse(request.Content, request.Level) 
                                   ?? CreatePart7Passage(request.Content, request.Level);
                exercise.Content = part7Content;
                _context.ReadingExercises.Update(exercise);
                await _context.SaveChangesAsync();
            }

            // Convert generated questions to ReadingQuestion entities
            var questions = new List<ReadingQuestion>();
            for (int i = 0; i < generatedQuestions.Count; i++)
            {
                var gq = generatedQuestions[i];
                var question = new ReadingQuestion
                {
                    ReadingExerciseId = exercise.Id,
                    QuestionText = gq.QuestionText,
                    OptionA = gq.Options.ElementAtOrDefault(0) ?? "",
                    OptionB = gq.Options.ElementAtOrDefault(1) ?? "",
                    OptionC = gq.Options.ElementAtOrDefault(2) ?? "",
                    OptionD = gq.Options.ElementAtOrDefault(3) ?? "",
                    CorrectAnswer = gq.CorrectAnswer,
                    Explanation = gq.Explanation,
                    Difficulty = gq.Difficulty,
                    OrderNumber = i + 1,
                    CreatedAt = DateTime.UtcNow
                };
                questions.Add(question);
            }

            if (questions.Any())
            {
                _context.ReadingQuestions.AddRange(questions);
                await _context.SaveChangesAsync();
                exercise.Questions = questions;
            }

            _logger.LogInformation($"Created exercise '{exercise.Name}' with {questions.Count} AI-generated questions");
            return MapToDto(exercise);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating exercise with AI questions");
            throw new Exception($"Failed to create exercise with AI questions: {ex.Message}");
        }
    }

    public async Task<bool> GenerateAdditionalQuestionsAsync(int exerciseId, int questionCount = 3)
    {
        try
        {
            var exercise = await _context.ReadingExercises
                .Include(e => e.Questions)
                .FirstOrDefaultAsync(e => e.Id == exerciseId);

            if (exercise == null)
            {
                _logger.LogWarning($"Exercise with ID {exerciseId} not found");
                return false;
            }

            // Generate additional questions
            var generatedQuestions = await _geminiService.GenerateQuestionsAsync(
                exercise.Content, 
                exercise.Type, 
                exercise.Level, 
                questionCount
            );

            // Get the next order number
            var maxOrderNumber = exercise.Questions?.Max(q => q.OrderNumber) ?? 0;

            // Convert to ReadingQuestion entities
            var newQuestions = new List<ReadingQuestion>();
            for (int i = 0; i < generatedQuestions.Count; i++)
            {
                var gq = generatedQuestions[i];
                var question = new ReadingQuestion
                {
                    ReadingExerciseId = exerciseId,
                    QuestionText = gq.QuestionText,
                    OptionA = gq.Options.ElementAtOrDefault(0) ?? "",
                    OptionB = gq.Options.ElementAtOrDefault(1) ?? "",
                    OptionC = gq.Options.ElementAtOrDefault(2) ?? "",
                    OptionD = gq.Options.ElementAtOrDefault(3) ?? "",
                    CorrectAnswer = gq.CorrectAnswer,
                    Explanation = gq.Explanation,
                    Difficulty = gq.Difficulty,
                    OrderNumber = maxOrderNumber + i + 1,
                    CreatedAt = DateTime.UtcNow
                };
                newQuestions.Add(question);
            }

            if (newQuestions.Any())
            {
                _context.ReadingQuestions.AddRange(newQuestions);
                await _context.SaveChangesAsync();
                _logger.LogInformation($"Added {newQuestions.Count} AI-generated questions to exercise {exerciseId}");
                return true;
            }

            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating additional questions for exercise {exerciseId}");
            return false;
        }
    }

    private static ReadingExerciseDto MapToDto(ReadingExercise exercise)
    {
        return new ReadingExerciseDto
        {
            Id = exercise.Id,
            Name = exercise.Name ?? string.Empty,
            Title = exercise.Name ?? string.Empty, // Alias for frontend
            Content = exercise.Content ?? string.Empty,
            Level = exercise.Level ?? "Beginner",
            Type = exercise.Type ?? "Part 7",
            SourceType = exercise.SourceType ?? "manual",
            Description = exercise.Description,
            EstimatedMinutes = exercise.EstimatedMinutes,
            Duration = exercise.EstimatedMinutes, // Alias for frontend
            CreatedBy = exercise.CreatedBy ?? "System",
            CreatedAt = exercise.CreatedAt,
            DateCreated = exercise.CreatedAt, // Alias for frontend
            Questions = exercise.Questions?.Select(q => new QuestionDto
            {
                Id = q.Id,
                QuestionText = q.QuestionText ?? string.Empty,
                Text = q.QuestionText ?? string.Empty, // Alias for frontend
                Options = new List<string> { q.OptionA ?? "", q.OptionB ?? "", q.OptionC ?? "", q.OptionD ?? "" },
                Choices = new List<string> { q.OptionA ?? "", q.OptionB ?? "", q.OptionC ?? "", q.OptionD ?? "" }, // Alias
                CorrectAnswer = q.CorrectAnswer,
                Answer = q.CorrectAnswer, // Alias for frontend
                Explanation = q.Explanation,
                Difficulty = q.Difficulty
            }).ToList() ?? new List<QuestionDto>()
        };
    }

    public async Task<string> ClearAllExercisesAsync()
    {
        try
        {
            // Delete all questions first (foreign key constraint)
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ReadingQuestions");
            
            // Delete all exercises  
            await _context.Database.ExecuteSqlRawAsync("DELETE FROM ReadingExercises");
            
            // Reset identity seeds
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ReadingQuestions', RESEED, 0)");
            await _context.Database.ExecuteSqlRawAsync("DBCC CHECKIDENT ('ReadingExercises', RESEED, 0)");

            _logger.LogInformation("Successfully cleared all reading exercises and questions");
            return "All reading exercises and questions have been cleared successfully";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error clearing all exercises");
            throw new Exception($"Failed to clear exercises: {ex.Message}");
        }
    }

    public async Task<string> FixAISourceTypeAsync()
    {
        try
        {
            // Step 1: Set default 'manual' for NULL or empty SourceType
            var nullCount = await _context.Database.ExecuteSqlRawAsync(
                @"UPDATE ReadingExercises 
                  SET SourceType = 'manual' 
                  WHERE SourceType IS NULL OR SourceType = ''");

            // Step 2: Update AI-generated exercises (based on CreatedBy pattern)
            var aiCount = await _context.Database.ExecuteSqlRawAsync(
                @"UPDATE ReadingExercises 
                  SET SourceType = 'ai' 
                  WHERE (CreatedBy LIKE '%AI%' OR CreatedBy = 'System') 
                    AND SourceType != 'ai'");

            _logger.LogInformation("Fixed SourceType: {NullFixed} set to 'manual', {AIFixed} set to 'ai'", 
                nullCount, aiCount);

            return $"Fixed SourceType successfully: {nullCount} exercises set to 'manual', {aiCount} exercises set to 'ai'";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fixing AI sourceType");
            throw new Exception($"Failed to fix sourceType: {ex.Message}");
        }
    }

    private string CreatePart6Passage(string topic, string level)
    {
        // Create a sample Part 6 passage based on topic and level
        var templates = new Dictionary<string, string>
        {
            ["business"] = @"Dear Team Members,

We are pleased to announce that our company (1) _____ be moving to a new office location next month. The new facility (2) _____ provide us with more space and better amenities.

Please (3) _____ any questions you may have about the relocation to the HR department. We will provide more details (4) _____ the coming weeks.

Best regards,
Management Team",
            
            ["meeting"] = @"Subject: Weekly Team Meeting

Hello Everyone,

Our weekly team meeting (1) _____ take place this Friday at 2:00 PM in Conference Room A. Please (2) _____ your attendance by Thursday evening.

We (3) _____ discuss the quarterly goals and upcoming projects. (4) _____ you cannot attend, please send your updates via email.

Thank you,
Project Manager",
            
            ["default"] = @"Dear Valued Customer,

Thank you for your interest in our services. We (1) _____ be happy to assist you with your inquiry. Our team (2) _____ provide you with detailed information about our products.

Please (3) _____ free to contact us if you have any questions. We look forward (4) _____ hearing from you soon.

Sincerely,
Customer Service Team"
        };

        // Choose template based on topic
        var key = templates.Keys.FirstOrDefault(k => topic.ToLower().Contains(k)) ?? "default";
        return templates[key];
    }

    private async Task<string?> ExtractPart6PassageFromGemini(string topic, string level)
    {
        try
        {
            // Get the raw response from Gemini to extract the passage
            // For now, we'll use the template. In future, we could store the raw Gemini response
            // and extract the passage text before the ```json block
            return null; // Use template fallback for now
        }
        catch
        {
            return null; // Use template fallback
        }
    }

    private string CreatePart7Passage(string topic, string level)
    {
        var templates = new Dictionary<string, Dictionary<string, string>>
        {
            ["beginner"] = new Dictionary<string, string>
            {
                ["meeting"] = @"Company Meeting Notice

Date: Friday, March 15th
Time: 2:00 PM - 4:00 PM  
Location: Conference Room A

All employees must attend the monthly meeting. We will discuss:
- New company policies
- Quarterly sales results  
- Upcoming projects
- Employee benefits

Please bring your notebooks and pens. Light refreshments will be provided.

For questions, contact HR at extension 234.

Thank you,
Management Team",

                ["business"] = @"New Store Opening

Grand Opening Sale!

Come visit our new store location at 123 Main Street. We are excited to serve customers in the downtown area.

Store Hours:
Monday - Saturday: 9:00 AM - 9:00 PM
Sunday: 10:00 AM - 6:00 PM

Special Offers:
- 20% off all items
- Free coffee for first 100 customers
- Free parking available

Join us for the ribbon cutting ceremony on March 20th at 10:00 AM.

Visit our website: www.newstore.com
Phone: (555) 123-4567",

                ["default"] = @"Job Announcement

Position: Customer Service Representative
Company: ABC Corporation
Location: Downtown Office

Requirements:
- High school diploma required
- Good communication skills
- Computer experience preferred
- Full-time position available

Benefits:
- Health insurance
- Paid vacation time
- Training provided

To apply, send your resume to jobs@abc.com or call (555) 987-6543.

Application deadline: March 30th.

ABC Corporation is an equal opportunity employer."
            },

            ["intermediate"] = new Dictionary<string, string>
            {
                ["meeting"] = @"Quarterly Business Review Meeting

TO: All Department Heads
FROM: Executive Committee
RE: Q1 Performance Review

Our quarterly business review will be held on March 25th at 9:00 AM in the Executive Conference Room. This meeting is mandatory for all department managers and team leaders.

Agenda Items:
1. Financial performance analysis for Q1
2. Department goal achievement reviews
3. Resource allocation for Q2 planning
4. Strategic initiative updates
5. Customer satisfaction metrics discussion

Please prepare detailed reports on your department's performance, including challenges faced and proposed solutions. Digital copies should be submitted to the executive assistant by March 23rd.

The meeting is expected to conclude by 12:00 PM, followed by a working lunch where we will discuss implementation strategies for Q2 objectives.

For technical support or presentation requirements, contact IT at ext. 445.

Best regards,
Sarah Johnson
Executive Assistant",

                ["business"] = @"New Product Launch Announcement

Dear Valued Partners,

We are thrilled to announce the launch of our revolutionary SmartTech Pro series, featuring cutting-edge technology designed to enhance productivity and efficiency in modern workplaces.

Product Highlights:
- Advanced AI-powered automation capabilities
- Seamless integration with existing systems
- Energy-efficient design reducing operational costs by 30%
- 24/7 technical support included

Launch Timeline:
- Pre-orders begin: April 1st
- Product demonstration webinars: April 15th-20th
- Official release date: May 1st
- Training sessions for partners: Throughout May

Pricing Structure:
- Early bird discount: 15% off for orders placed before April 15th
- Volume discounts available for orders exceeding 50 units
- Flexible payment terms for qualified partners

To schedule a product demonstration or discuss partnership opportunities, please contact our business development team at partnerships@smarttech.com or call (800) 555-0199.

We look forward to a successful product launch and continued collaboration.

Sincerely,
Michael Chen
Product Development Manager",

                ["default"] = @"Company Policy Update

MEMORANDUM

TO: All Employees
FROM: Human Resources Department
DATE: March 10, 2024
RE: Updated Remote Work Policy

Effective April 1st, 2024, our remote work policy will be updated to provide greater flexibility while maintaining operational efficiency.

Key Changes:
1. Employees may work remotely up to 3 days per week (previously 2 days)
2. Core collaboration hours established: 10:00 AM - 2:00 PM EST
3. Monthly in-person team meetings required for all departments
4. Home office equipment allowance increased to $500 annually

Eligibility Requirements:
- Minimum 6 months employment with the company
- Satisfactory performance reviews for the past year
- Completion of remote work training certification
- Supervisor approval required

Application Process:
Submit Form HR-205 to your direct supervisor by March 25th. Approvals will be processed within 10 business days. The new policy takes effect immediately upon approval.

For questions regarding this policy update, please contact the HR department at hr@company.com or extension 300.

Thank you for your continued dedication.

Jennifer Martinez
HR Director"
            },

            ["advanced"] = new Dictionary<string, string>
            {
                ["meeting"] = @"DOCUMENT 1: Board Meeting Invitation

Confidential Executive Board Meeting

Dear Board Members,

You are cordially invited to attend an urgent executive board meeting to address critical strategic decisions affecting our company's future direction.

Meeting Details:
Date: March 28th, 2024
Time: 8:00 AM - 12:00 PM EST
Location: Executive Boardroom, 45th Floor
Format: Hybrid (in-person and virtual attendance available)

Primary Agenda:
1. Quarterly financial performance and market analysis
2. Proposed acquisition of TechInnovate Solutions
3. Restructuring of international operations
4. Board composition changes and succession planning
5. Risk assessment and mitigation strategies

Please review the attached confidential briefing documents prior to the meeting. Due to the sensitive nature of the discussions, all materials must be treated with utmost confidentiality.

RSVP required by March 25th to executive.assistant@company.com

Best regards,
Robert Sterling, Chairman

---

DOCUMENT 2: Acquisition Proposal Summary

TechInnovate Solutions - Acquisition Overview

Financial Highlights:
- Acquisition price: $127 million
- Annual revenue: $45 million (growing 23% YoY)
- Employee count: 340 highly skilled professionals
- Market position: Leader in AI automation solutions

Strategic Rationale:
The acquisition of TechInnovate Solutions represents a transformative opportunity to expand our technological capabilities and market reach. This strategic move will:

- Strengthen our position in the rapidly growing AI automation sector
- Provide access to proprietary algorithms and patent portfolio (47 patents)
- Integrate 340 top-tier engineers and data scientists into our workforce
- Establish presence in three new international markets (Germany, Singapore, Brazil)

Due Diligence Findings:
Our comprehensive 90-day due diligence process has revealed strong fundamentals with minimal operational risks. Key findings include:
- Clean financial records with consistent profitability for past 5 years
- Strong intellectual property portfolio with significant barriers to entry
- Excellent customer retention rate of 94% and expanding client base
- Minimal legal or regulatory compliance issues identified

Integration Timeline:
- Regulatory approval process: 4-6 months
- Systems integration: 12-18 months  
- Full operational integration: 24 months
- Expected synergies realization: 18-36 months post-closing

Risk Considerations:
While the acquisition presents substantial opportunities, the board should consider:
- Integration complexity and potential cultural challenges
- Retention of key technical talent during transition
- Regulatory scrutiny in international markets
- Technology obsolescence risks in rapidly evolving AI sector

Recommendation: Proceed with acquisition pending board approval and favorable regulatory review.",

                ["business"] = @"DOCUMENT 1: Annual Shareholders Report - Executive Summary

GlobalTech Corporation - 2023 Annual Performance

Fellow Shareholders,

I am pleased to present our 2023 annual performance report, highlighting a year of unprecedented growth and strategic expansion that has positioned GlobalTech as an industry leader.

Financial Performance:
- Total revenue: $2.4 billion (18% increase from 2022)
- Net income: $387 million (22% increase from previous year)  
- Earnings per share: $4.25 (surpassing analyst expectations by 12%)
- Operating margin improved to 16.2% (up from 14.8% in 2022)

Strategic Achievements:
Our strategic initiatives have delivered exceptional results across all business segments. The successful launch of our AI-powered enterprise solutions generated $340 million in new revenue streams, while our international expansion into Asian markets exceeded projections by 28%.

Key operational milestones include the completion of our $150 million R&D facility in Austin, Texas, and the acquisition of three complementary technology companies that have enhanced our competitive positioning in cloud computing and cybersecurity sectors.

Outlook for 2024:
We anticipate continued growth driven by increasing demand for our innovative solutions and expanding market opportunities in emerging technologies. Our investment in sustainable technologies and ESG initiatives positions us favorably for long-term value creation.

Sincerely,
Amanda Richardson, CEO

---

DOCUMENT 2: Market Analysis and Growth Projections  

Industry Trends and Competitive Landscape

The technology sector continues to evolve at an accelerated pace, with artificial intelligence, cloud computing, and cybersecurity representing the fastest-growing segments. Market research indicates that global spending on AI technologies will reach $500 billion by 2026, representing a compound annual growth rate of 26%.

GlobalTech's Competitive Position:
Our comprehensive portfolio of enterprise solutions has established us as the preferred partner for Fortune 500 companies seeking digital transformation. With a 23% market share in cloud-based AI solutions, we maintain significant competitive advantages:

- Proprietary machine learning algorithms with 40+ patents
- Strategic partnerships with major cloud infrastructure providers
- Comprehensive cybersecurity integration across all product lines
- 99.9% uptime reliability record and industry-leading customer support

Growth Strategy Implementation:
The three-year growth strategy outlined in our 2022 investor presentation is proceeding ahead of schedule. Key initiatives include:

1. International Expansion: Establishing operations in 12 new countries by 2025
2. Product Innovation: $400 million annual R&D investment targeting next-generation AI capabilities  
3. Strategic Acquisitions: Identifying and acquiring complementary technologies and talent
4. Sustainability Leadership: Achieving carbon neutrality by 2026 while developing green technology solutions

Market Challenges and Risk Mitigation:
While opportunities abound, we remain vigilant regarding potential challenges including regulatory changes, cybersecurity threats, and intense competition from both established players and emerging startups. Our robust risk management framework and diversified revenue streams provide resilience against market volatility.

Investment in human capital remains our top priority, with expanded hiring in key technical roles and comprehensive training programs to ensure our team maintains its competitive edge in this dynamic environment.",

                ["default"] = @"DOCUMENT 1: Corporate Restructuring Announcement

Strategic Organizational Restructuring Initiative

TO: All Employees, Stakeholders, and Partners
FROM: Executive Leadership Team
DATE: March 15, 2024

After extensive analysis and strategic planning, Pinnacle Industries announces a comprehensive organizational restructuring designed to optimize operational efficiency and position the company for sustained growth in evolving markets.

Restructuring Overview:
This initiative consolidates our seven regional divisions into four strategic business units, each focused on specific market segments and customer needs. The restructuring addresses operational redundancies while strengthening our core competencies in manufacturing excellence and customer service delivery.

Organizational Changes:
- Creation of Advanced Manufacturing Division (consolidating Northeast and Southeast regions)
- Formation of Global Markets Division (combining International and Pacific operations)  
- Establishment of Innovation and Technology Center (centralizing R&D activities)
- Development of Customer Solutions Division (integrating sales and support functions)

Implementation Timeline:
Phase 1 (April-June 2024): Leadership appointments and team assignments
Phase 2 (July-September 2024): Systems integration and process standardization
Phase 3 (October-December 2024): Full operational integration and performance optimization

Employee Impact and Support:
We recognize that organizational change creates uncertainty. Our commitment to transparency and employee support includes:
- Individual consultations with affected employees
- Comprehensive transition assistance and retraining programs
- Enhanced severance packages exceeding industry standards for displaced positions
- Internal job placement priority for qualified candidates

Dr. Elena Vasquez
Chief Executive Officer

---

DOCUMENT 2: Financial Impact Assessment and Projections

Restructuring Financial Analysis and Future Projections  

The comprehensive organizational restructuring requires significant upfront investment while delivering substantial long-term financial benefits through operational efficiencies and enhanced market positioning.

Investment Requirements:
- One-time restructuring costs: $45 million
- Technology infrastructure upgrades: $28 million
- Employee transition and training expenses: $12 million
- Facility consolidation and optimization: $18 million
- Total restructuring investment: $103 million

Projected Benefits and ROI:
Financial modeling indicates the restructuring will generate significant returns through operational efficiencies and improved market responsiveness:

Year 1 Benefits (2024):
- Operational cost savings: $25 million
- Improved productivity metrics: 12% increase
- Reduced administrative overhead: $8 million annually

Years 2-5 Projections (2025-2029):
- Annual cost savings: $52 million (fully realized by Year 3)
- Revenue growth acceleration: 8-12% annually through improved market focus
- Return on investment: 340% over five-year period
- Payback period: 24 months from implementation completion

Market Competitive Advantages:
The restructured organization will deliver enhanced competitive positioning through:
- Faster decision-making and reduced bureaucracy
- Improved customer response times and service quality
- Increased innovation capacity through centralized R&D
- Enhanced scalability for future growth opportunities
- Stronger financial performance supporting strategic investments

Risk Mitigation Strategies:
Comprehensive risk assessment has identified potential challenges and corresponding mitigation approaches:
- Employee retention strategies for critical talent
- Customer communication plans to ensure service continuity  
- Technology integration protocols to prevent operational disruptions
- Financial reserves allocated for contingency scenarios

The restructuring positions Pinnacle Industries for sustained leadership in our industry while creating value for shareholders, employees, and customers alike."
            }
        };

        // Get templates for the level
        var levelTemplates = templates.ContainsKey(level.ToLower()) 
            ? templates[level.ToLower()] 
            : templates["intermediate"];

        // Find matching topic or use default
        var key = levelTemplates.Keys.FirstOrDefault(k => topic.ToLower().Contains(k)) ?? "default";
        return levelTemplates[key];
    }

    private async Task<string?> ExtractPart7PassageFromGeminiResponse(string topic, string level)
    {
        try
        {
            // We need to get the raw response from Gemini to extract passage content
            // For now, we'll use a simple approach to call Gemini again specifically for content
            var rawResponse = await _geminiService.GetRawGeminiResponseAsync(topic, "Part 7", level, 4);
            
            if (!string.IsNullOrEmpty(rawResponse))
            {
                // Look for passage content in the JSON
                var jsonStart = rawResponse.IndexOf("```json");
                var jsonEnd = rawResponse.LastIndexOf("```");
                
                if (jsonStart >= 0 && jsonEnd > jsonStart)
                {
                    var jsonText = rawResponse.Substring(jsonStart + 7, jsonEnd - jsonStart - 7).Trim();
                    
                    // Try to find passage content
                    if (jsonText.Contains("\"passage\":"))
                    {
                        var passageStart = jsonText.IndexOf("\"passage\":");
                        if (passageStart >= 0)
                        {
                            var passageValueStart = jsonText.IndexOf("\"", passageStart + 10) + 1;
                            var passageValueEnd = jsonText.IndexOf("\",", passageValueStart);
                            if (passageValueEnd == -1) passageValueEnd = jsonText.IndexOf("\"}", passageValueStart);
                            
                            if (passageValueStart > 0 && passageValueEnd > passageValueStart)
                            {
                                var passageContent = jsonText.Substring(passageValueStart, passageValueEnd - passageValueStart);
                                // Unescape JSON string
                                passageContent = passageContent.Replace("\\n", "\n").Replace("\\\"", "\"").Replace("\\\\", "\\");
                                return passageContent;
                            }
                        }
                    }
                }
            }
            
            return null; // Use template fallback
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error extracting Part 7 passage from Gemini response");
            return null; // Use template fallback
        }
    }

    private (string passage, List<ReadingQuestion> questions) ParseCompleteFileContent(string content, string partType)
    {
        try
        {
            var lines = content.Split('\n', StringSplitOptions.RemoveEmptyEntries)
                              .Select(line => line.Trim())
                              .Where(line => !string.IsNullOrEmpty(line))
                              .ToList();

            var passage = new List<string>();
            var questions = new List<ReadingQuestion>();
            var currentQuestion = new Dictionary<string, object>();
            var answers = new List<string>();
            
            bool inQuestions = false;
            bool inAnswers = false;
            int questionNumber = 0;

        foreach (var line in lines)
        {
            // Check for section markers
            if (line.ToLower().Contains("questions:") || line.ToLower().Contains("question ") && line.Contains("1."))
            {
                inQuestions = true;
                inAnswers = false;
                continue;
            }
            
            if (line.ToLower().Contains("answers:") || line.ToLower().Contains("answer key") || line.ToLower().Contains("đáp án"))
            {
                inQuestions = false;
                inAnswers = true;
                continue;
            }

            if (inAnswers)
            {
                // Parse answer line: "1. A" or "1-5: A B C D A"
                var answerMatch = System.Text.RegularExpressions.Regex.Match(line, @"(\d+)[\.\-]\s*([ABCD])");
                if (answerMatch.Success)
                {
                    answers.Add(answerMatch.Groups[2].Value);
                }
                else
                {
                    // Try multi-answer format: "1-5: A B C D A"
                    var multiMatch = System.Text.RegularExpressions.Regex.Match(line, @"[\d\-\s]+:\s*([ABCD\s]+)");
                    if (multiMatch.Success)
                    {
                        var answerLetters = multiMatch.Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        answers.AddRange(answerLetters);
                    }
                }
            }
            else if (inQuestions)
            {
                // Parse question number
                var questionMatch = System.Text.RegularExpressions.Regex.Match(line, @"^(\d+)[\.\)]\s*(.+)");
                if (questionMatch.Success)
                {
                    // Save previous question
                    if (currentQuestion.ContainsKey("text"))
                    {
                        questions.Add(CreateReadingQuestionFromParsed(currentQuestion, ++questionNumber));
                    }
                    
                    // Start new question
                    currentQuestion.Clear();
                    currentQuestion["text"] = questionMatch.Groups[2].Value;
                    currentQuestion["options"] = new List<string>();
                }
                else
                {
                    // Parse options A, B, C, D
                    var optionMatch = System.Text.RegularExpressions.Regex.Match(line, @"^([ABCD])[\.\)]\s*(.+)");
                    if (optionMatch.Success && currentQuestion.ContainsKey("options"))
                    {
                        ((List<string>)currentQuestion["options"]).Add(optionMatch.Groups[2].Value);
                    }
                }
            }
            else if (!inQuestions && !inAnswers)
            {
                // This is passage content
                passage.Add(line);
            }
        }

        // Add last question
        if (currentQuestion.ContainsKey("text"))
        {
            questions.Add(CreateReadingQuestionFromParsed(currentQuestion, ++questionNumber));
        }

        // Match answers with questions
        for (int i = 0; i < Math.Min(questions.Count, answers.Count); i++)
        {
            questions[i].CorrectAnswer = LetterToIndex(answers[i]);
        }

            var passageText = string.Join("\n", passage);
            return (passageText, questions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing complete file content");
            // Return safe defaults if parsing fails
            return (content, new List<ReadingQuestion>());
        }
    }

    private ReadingQuestion CreateReadingQuestionFromParsed(Dictionary<string, object> questionData, int orderNumber)
    {
        // Safely get options with null checking
        var options = new List<string>();
        if (questionData.ContainsKey("options") && questionData["options"] is List<string> optionsList)
        {
            options = optionsList;
        }
        
        // Safely get question text
        var questionText = questionData.ContainsKey("text") && questionData["text"] is string text 
            ? text 
            : "No question text found";
        
        return new ReadingQuestion
        {
            QuestionText = questionText,
            OptionA = options.Count > 0 ? options[0] : "",
            OptionB = options.Count > 1 ? options[1] : "",
            OptionC = options.Count > 2 ? options[2] : "",
            OptionD = options.Count > 3 ? options[3] : "",
            CorrectAnswer = 0, // Will be set later when matching with answers
            Explanation = "Uploaded question",
            OrderNumber = orderNumber,
            Difficulty = 3,
            CreatedAt = DateTime.UtcNow
        };
    }

    private int GetEstimatedMinutes(string partType)
    {
        return partType switch
        {
            "Part 5" => 15,
            "Part 6" => 20,
            "Part 7" => 30,
            _ => 20
        };
    }
}