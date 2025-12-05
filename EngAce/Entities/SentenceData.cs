namespace Entities
{
    public class SentenceData
    {
        public int Id { get; set; }
        public string Vietnamese { get; set; } = string.Empty;
        public string CorrectAnswer { get; set; } = string.Empty;
        public SentenceSuggestion? Suggestion { get; set; }
    }

    public class SentenceSuggestion
    {
        public List<VocabularyItem> Vocabulary { get; set; } = new();
        public string Structure { get; set; } = string.Empty;
    }

    public class VocabularyItem
    {
        public string Word { get; set; } = string.Empty;
        public string Meaning { get; set; } = string.Empty;
    }

    public class SentenceResponse
    {
        public List<SentenceData> Sentences { get; set; } = new();
    }
}
