using Entities;
using Entities.Enums;
using EngAce.Api.DTO;
using Gemini.NET;
using Gemini.NET.Helpers;
using Helper;
using Models.Enums;
using Newtonsoft.Json;
using System.Text;

namespace Events
{
    public static class ListeningScope
    {
        public const sbyte MaxTotalWordsOfTopic = 15;
        public const sbyte MinTotalQuestions = 5;
        public const sbyte MaxTotalQuestions = 20;
        public const int ThreeDaysAsCachingAge = 259200;
        public const int MaxTimeAsCachingAge = int.MaxValue;

        private const string Instruction = @"
You are an expert English teacher specialized in creating listening comprehension exercises. You have over 20 years of experience teaching English to Vietnamese students. Create engaging listening exercises that help students improve their listening skills while learning about interesting topics.

### Requirements:

1. **English Proficiency Level (CEFR)**:
   - A1: Basic conversations, simple vocabulary, clear pronunciation
   - A2: Short dialogues, familiar topics, everyday situations  
   - B1: Longer conversations, some complex grammar, varied topics
   - B2: Detailed discussions, abstract topics, natural speech patterns
   - C1: Complex conversations, implicit meanings, advanced vocabulary
   - C2: Native-like conversations, sophisticated language, nuanced meanings

2. **Content Creation**:
   - Create a natural conversation or monologue about the given topic
   - The content should be engaging and educational
   - Length should match the specified duration (approximately 150-180 words per minute)
   - Use appropriate vocabulary and grammar for the specified level

3. **Question Types**:
   - GeneralComprehension: Overall understanding of the content
   - SpecificDetails: Specific information mentioned in the audio
   - Inference: Conclusions drawn from the content
   - AttitudeEmotion: Speaker's attitude or emotions
   - VocabularyInContext: Meaning of words in context
   - Purpose: Purpose or main idea of the conversation
   - FillInTheBlanks: Missing words in transcript segments
   - SoundRecognition: Specific sounds or pronunciation

4. **Output Format**:
   Return a JSON object with:
   - id: unique identifier
   - title: engaging title for the exercise
   - topic: the topic
   - level: English level
   - audioContent: the full text content to be converted to speech
   - audioUrl: empty string (will be filled later)
   - questions: array of questions with id, question, options (4 options), correctOptionIndex, explanationInVietnamese, type
   - duration: estimated duration in seconds

### Example Topic: ""Travel Adventures""
### Level: B1
### Questions: 8
### Duration: 300 seconds (5 minutes)
";

        public static async Task<ListeningExercise> GenerateExerciseAsync(
            string topic, 
            List<ListeningQuestionType> questionTypes, 
            EnglishLevel level, 
            int totalQuestions,
            int durationInMinutes,
            string preferredAccent)
        {
            try
            {
                var prompt = BuildPrompt(topic, questionTypes, level, totalQuestions, durationInMinutes, preferredAccent);
                
                var response = await GeminiApi.CallAsync(prompt, LanguageModel.GeminiFlash);
                
                if (response?.StatusCode == System.Net.HttpStatusCode.OK && !string.IsNullOrEmpty(response.Response))
                {
                    var exercise = JsonConvert.DeserializeObject<ListeningExercise>(response.Response);
                    if (exercise != null)
                    {
                        // Generate unique ID if not provided
                        if (string.IsNullOrEmpty(exercise.Id))
                        {
                            exercise.Id = Guid.NewGuid().ToString();
                        }

                        // Ensure question IDs are unique
                        for (int i = 0; i < exercise.Questions.Count; i++)
                        {
                            if (string.IsNullOrEmpty(exercise.Questions[i].Id))
                            {
                                exercise.Questions[i].Id = $"{exercise.Id}-Q{i + 1}";
                            }
                        }

                        return exercise;
                    }
                }

                throw new Exception("Failed to generate listening exercise from AI response");
            }
            catch (Exception ex)
            {
                throw new Exception($"Error generating listening exercise: {ex.Message}", ex);
            }
        }

        private static string BuildPrompt(
            string topic, 
            List<ListeningQuestionType> questionTypes, 
            EnglishLevel level, 
            int totalQuestions,
            int durationInMinutes,
            string preferredAccent)
        {
            var sb = new StringBuilder();
            sb.AppendLine(Instruction);
            sb.AppendLine();
            sb.AppendLine($"**Topic**: {topic}");
            sb.AppendLine($"**English Level**: {level} ({GetLevelDescription(level)})");
            sb.AppendLine($"**Total Questions**: {totalQuestions}");
            sb.AppendLine($"**Duration**: {durationInMinutes} minutes");
            sb.AppendLine($"**Preferred Accent**: {preferredAccent}");
            sb.AppendLine();
            sb.AppendLine("**Question Types to Include**:");
            foreach (var questionType in questionTypes)
            {
                sb.AppendLine($"- {questionType}: {GetQuestionTypeDescription(questionType)}");
            }
            sb.AppendLine();
            sb.AppendLine("**Important Notes**:");
            sb.AppendLine("- Create natural, engaging content appropriate for the level");
            sb.AppendLine("- Questions should test different listening skills");
            sb.AppendLine("- Provide clear Vietnamese explanations for answers");
            sb.AppendLine("- Ensure audio content flows naturally and is interesting");
            sb.AppendLine("- Return ONLY valid JSON, no additional text or formatting");

            return sb.ToString();
        }

        private static string GetLevelDescription(EnglishLevel level)
        {
            return level switch
            {
                EnglishLevel.A1 => "Beginner - Basic vocabulary, simple sentences",
                EnglishLevel.A2 => "Elementary - Simple conversations, familiar topics",
                EnglishLevel.B1 => "Intermediate - Clear standard input on familiar topics",
                EnglishLevel.B2 => "Upper-intermediate - Detailed text on complex subjects",
                EnglishLevel.C1 => "Advanced - Complex subjects, implicit meanings",
                EnglishLevel.C2 => "Proficient - Near-native fluency",
                _ => "Intermediate"
            };
        }

        private static string GetQuestionTypeDescription(ListeningQuestionType type)
        {
            return type switch
            {
                ListeningQuestionType.GeneralComprehension => "Overall understanding of content",
                ListeningQuestionType.SpecificDetails => "Specific information mentioned",
                ListeningQuestionType.Inference => "Conclusions drawn from content",
                ListeningQuestionType.AttitudeEmotion => "Speaker's attitude or emotions",
                ListeningQuestionType.VocabularyInContext => "Word meanings in context",
                ListeningQuestionType.Purpose => "Main purpose or idea",
                ListeningQuestionType.FillInTheBlanks => "Missing words in transcript",
                ListeningQuestionType.SoundRecognition => "Specific sounds or pronunciation",
                _ => "General listening comprehension"
            };
        }

        public static ListeningExerciseResult EvaluateAnswers(
            ListeningExercise exercise, 
            List<ListeningAnswerDto> submittedAnswers, 
            TimeSpan timeSpent)
        {
            var answers = new List<ListeningAnswer>();
            int correctCount = 0;

            foreach (var submitted in submittedAnswers)
            {
                var question = exercise.Questions.FirstOrDefault(q => q.Id == submitted.QuestionId);
                if (question != null)
                {
                    bool isCorrect = question.CorrectOptionIndex == submitted.SelectedOptionIndex;
                    if (isCorrect) correctCount++;

                    answers.Add(new ListeningAnswer
                    {
                        QuestionId = submitted.QuestionId,
                        SelectedOptionIndex = submitted.SelectedOptionIndex,
                        IsCorrect = isCorrect,
                        TimeSpentOnQuestion = submitted.TimeSpentOnQuestion
                    });
                }
            }

            var scorePercentage = exercise.Questions.Count > 0 
                ? Math.Round((double)correctCount / exercise.Questions.Count * 100, 2)
                : 0;

            return new ListeningExerciseResult
            {
                ExerciseId = exercise.Id,
                Answers = answers,
                TotalQuestions = exercise.Questions.Count,
                CorrectAnswers = correctCount,
                ScorePercentage = scorePercentage,
                TimeSpent = timeSpent
            };
        }

        public static List<string> GetSuggestedTopics(EnglishLevel level)
        {
            return level switch
            {
                EnglishLevel.A1 => new List<string>
                {
                    "Daily Routines", "Family Members", "Food and Drinks", 
                    "Weather", "Shopping", "Greetings", "Numbers and Time"
                },
                EnglishLevel.A2 => new List<string>
                {
                    "Travel Plans", "Hobbies", "School Life", "Health and Body",
                    "Holidays", "Transportation", "Making Appointments"
                },
                EnglishLevel.B1 => new List<string>
                {
                    "Job Interviews", "Environmental Issues", "Technology", 
                    "Cultural Differences", "Education Systems", "Sports and Fitness"
                },
                EnglishLevel.B2 => new List<string>
                {
                    "Global Economy", "Social Media Impact", "Career Development",
                    "Climate Change", "Innovation", "Psychology"
                },
                EnglishLevel.C1 => new List<string>
                {
                    "Artificial Intelligence", "Philosophy", "Scientific Research",
                    "Political Systems", "Art and Literature", "Ethics"
                },
                EnglishLevel.C2 => new List<string>
                {
                    "Quantum Physics", "Geopolitical Analysis", "Advanced Economics",
                    "Neuroscience", "Complex Social Issues", "Academic Research"
                },
                _ => new List<string> { "General Conversation", "Current Events", "Technology" }
            };
        }
    }
}