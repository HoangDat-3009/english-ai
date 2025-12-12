using System.ComponentModel;
using System.Reflection;
using Entities.Enums;

namespace Helper
{
    public static class GeneralHelper
    {
        public static string GetEnumDescription(Enum value)
        {
            FieldInfo? fi = value.GetType().GetField(value.ToString());
            DescriptionAttribute[]? attributes = (DescriptionAttribute[]?)fi?.GetCustomAttributes(typeof(DescriptionAttribute), false);

            if (attributes != null && attributes.Length > 0)
            {
                return attributes[0].Description;
            }
            else
            {
                return value.ToString();
            }
        }

        public static string GetLevelDescription(EnglishLevel level)
        {
            return level switch
            {
                EnglishLevel.Beginner => "A1 (Beginner): Basic phrases, simple sentences, very limited vocabulary",
                EnglishLevel.Elementary => "A2 (Elementary): Simple conversations, everyday topics, basic grammar structures",
                EnglishLevel.Intermediate => "B1 (Intermediate): More complex sentences, can discuss familiar topics with some detail",
                EnglishLevel.UpperIntermediate => "B2 (Upper Intermediate): Fluent in most situations, can express opinions clearly",
                EnglishLevel.Advanced => "C1 (Advanced): Very fluent, sophisticated vocabulary, complex grammar structures",
                EnglishLevel.Proficient => "C2 (Proficient): Native-like proficiency, nuanced expression, academic and professional level",
                _ => "Unknown level"
            };
        }

        public static ushort GetTotalWords(string input)
        {
            if (string.IsNullOrWhiteSpace(input))
            {
                return 0;
            }

            char[] delimiters = [' ', '\r', '\n', '\t', '.', ',', ';', ':', '!', '?'];

            string[] words = input.Split(delimiters, StringSplitOptions.RemoveEmptyEntries);

            return (ushort)words.Length;
        }

        public static bool IsEnglish(string input)
        {
            char[] englishAlphabetAndPunctuation = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ.,!?;:'\"()[]{}$%&*+-/".ToCharArray();

            return input.All(c => englishAlphabetAndPunctuation.Contains(c) || char.IsWhiteSpace(c) || char.IsDigit(c));
        }

        public static List<int> GenerateRandomNumbers(int x, int y)
        {
            var rand = new Random();
            var result = new List<int>();

            for (int i = 0; i < x; i++)
            {
                result.Add(1);
            }

            int remaining = y - x;

            while (remaining > 0)
            {
                int index = rand.Next(0, x);
                result[index]++;
                remaining--;
            }

            return result;
        }
    }
}
