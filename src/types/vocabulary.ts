export interface VocabularyWord {
  id: string;
  word: string;
  pronunciation: string;
  translation: string;
  example?: string;
  image?: string;
  audio?: string;
  level: 'easy' | 'medium' | 'hard';
  lastReviewed?: Date;
  nextReview?: Date;
  correctCount: number;
  incorrectCount: number;
}

export interface VocabularySession {
  words: VocabularyWord[];
  currentIndex: number;
  correctAnswers: number;
  incorrectAnswers: number;
  startTime: Date;
  endTime?: Date;
}

export interface PracticeResult {
  wordId: string;
  correct: boolean;
  timeSpent: number;
  difficulty: 'easy' | 'medium' | 'hard' | 'known';
}

// New types for TOEIC API
export interface ToeicVocabularyWord {
  id: number;
  word: string;
  phonetic: string;
  pos: string; // part of speech
  meaningVi: string;
  exampleEn: string;
}

export interface ToeicTopicResponse {
  topic: string;
  total: number;
  items: ToeicVocabularyWord[];
}

export interface VocabularyTopic {
  id: string;
  name: string;
  totalWords: number;
  studyingWords: number;
  dueWords: number;
  masteredWords: number;
  category: string;
}
