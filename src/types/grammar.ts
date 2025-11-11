export interface GrammarQuestion {
  questionId: number;
  questionText: string;
  optionA: string;
  optionB: string;
  optionC: string;
  optionD: string;
  correctOption: "A" | "B" | "C" | "D";
}

export interface GrammarExercise {
  exerciseId: number;
  title: string;
  questions: GrammarQuestion[];
}

export interface GrammarTopic {
  id: number;
  name: string;
  apiPath: string;
}

export interface QuestionStatus {
  questionId: number;
  answered: boolean;
  selectedOption: string | null;
  isCorrect: boolean | null;
}

export interface GrammarSessionState {
  exercise: GrammarExercise | null;
  currentQuestionIndex: number;
  questionStatuses: Map<number, QuestionStatus>;
  startTime: Date;
  endTime?: Date;
  isCompleted: boolean;
}
