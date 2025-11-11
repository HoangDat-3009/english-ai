export interface ListeningSentence {
  sentenceId: number;
  orderNo: number;
  transcript: string;
  translation: string;
  audioUrl: string;
}

export interface ListeningLesson {
  lessonId: number;
  lessonTitle: string;
  sentences: ListeningSentence[];
}

export interface ListeningSubmission {
  lessonId: number;
  completedSentences: number;
  score: number;
}

export interface AnswerCheckResult {
  isCorrect: boolean;
  userAnswer: string;
  correctAnswer: string;
}

export interface LessonListItem {
  lessonId: number;
  title: string;
  level: string;
  totalQuestions: number;
}
