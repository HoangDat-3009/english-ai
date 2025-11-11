export interface ConversationDialogue {
  id: number;
  speaker: string;
  vi: string;
  en: string;
  ai_suggestion?: {
    vocabulary: Array<{
      word: string;
      meaning: string;
    }>;
    structure: string;
  };
}

export interface ConversationTopic {
  topic: string;
  purpose: string;
  difficulty: string;
  dialogue: ConversationDialogue[];
}

export interface WritingReviewRequest {
  Level: number;
  Requirement: string;
  Content: string;
}

export interface WritingReviewResponse {
  overallEvaluation: string;
  scoring: {
    taskAchievement: string;
    grammar: string;
    vocabulary: string;
    coherence: string;
  };
  strengths: string[];
  weaknesses: string[];
  suggestions: string[];
  correctedSample: string;
}
