import { supabase } from "@/integrations/supabase/client";

export interface Question {
  question: string;
  options: string[];
  correctAnswer: number;
  explanation?: string;
}

export interface ReadingExercise {
  id: string;
  name: string;
  content: string;
  level: 'Beginner' | 'Intermediate' | 'Advanced';
  type: 'Part 5' | 'Part 6' | 'Part 7';
  questions: Question[];
  source_type: 'uploaded' | 'ai';
  prompt_used?: string;
  created_at?: string;
}

export interface UserResult {
  exercise_id: string;
  score: number;
  total: number;
  answers: number[];
  submitted_at: string;
}

// Mock uploaded exercises
export const mockExercises: ReadingExercise[] = [
  {
    id: "1",
    name: "Business Email: Meeting Schedule",
    content: "To: All Staff\nFrom: Management Team\nDate: October 15, 2025\nSubject: Quarterly Review Meeting\n\nDear Team Members,\n\nWe are pleased to announce that our quarterly review meeting will be held on November 1st at 2:00 PM in Conference Room A. During this meeting, we will discuss our achievements, challenges, and goals for the upcoming quarter. All department heads are required to prepare a brief presentation summarizing their team's performance. Light refreshments will be provided. Please confirm your attendance by replying to this email by October 25th.\n\nBest regards,\nManagement Team",
    level: "Intermediate",
    type: "Part 7",
    source_type: "uploaded",
    questions: [
      {
        question: "What is the main purpose of this email?",
        options: [
          "To cancel a meeting",
          "To announce a quarterly review meeting",
          "To request a budget increase",
          "To introduce new staff members"
        ],
        correctAnswer: 1,
        explanation: "The email clearly states it announces the quarterly review meeting."
      },
      {
        question: "When should staff confirm their attendance?",
        options: [
          "October 15th",
          "October 25th",
          "November 1st",
          "By the end of the quarter"
        ],
        correctAnswer: 1,
        explanation: "The email asks for confirmation by October 25th."
      },
      {
        question: "Who must prepare a presentation?",
        options: [
          "All staff members",
          "The management team only",
          "Department heads",
          "New employees"
        ],
        correctAnswer: 2,
        explanation: "Department heads are specifically mentioned as needing to prepare presentations."
      }
    ]
  },
  {
    id: "2",
    name: "Grammar: Verb Tenses",
    content: "Complete the sentences with the correct form of the verb.",
    level: "Beginner",
    type: "Part 5",
    source_type: "uploaded",
    questions: [
      {
        question: "The company _____ a new product next month.",
        options: ["launch", "launches", "will launch", "launched"],
        correctAnswer: 2,
        explanation: "'Next month' indicates future tense, so 'will launch' is correct."
      },
      {
        question: "She _____ in the marketing department for five years.",
        options: ["works", "worked", "has worked", "will work"],
        correctAnswer: 2,
        explanation: "'For five years' with present relevance requires present perfect."
      },
      {
        question: "The meeting _____ at 9 AM tomorrow.",
        options: ["starts", "started", "has started", "starting"],
        correctAnswer: 0,
        explanation: "Scheduled future events often use simple present tense."
      }
    ]
  }
];

export const generateExercise = async (
  topic: string,
  level: 'Beginner' | 'Intermediate' | 'Advanced',
  type: 'Part 5' | 'Part 6' | 'Part 7'
): Promise<ReadingExercise> => {
  const { data, error } = await supabase.functions.invoke('generate-reading-exercise', {
    body: { topic, level, type }
  });

  if (error) {
    throw new Error(`Failed to generate exercise: ${error.message}`);
  }

  return {
    ...data,
    id: `ai-${Date.now()}`,
    created_at: new Date().toISOString()
  };
};

export const saveResult = (result: UserResult): void => {
  const results = getResults();
  results.push(result);
  localStorage.setItem('reading_results', JSON.stringify(results));
};

export const getResults = (): UserResult[] => {
  const stored = localStorage.getItem('reading_results');
  return stored ? JSON.parse(stored) : [];
};

export const getAllExercises = (): ReadingExercise[] => {
  const aiExercises = getAIExercises();
  return [...mockExercises, ...aiExercises];
};

export const getAIExercises = (): ReadingExercise[] => {
  const stored = localStorage.getItem('ai_exercises');
  return stored ? JSON.parse(stored) : [];
};

export const saveAIExercise = (exercise: ReadingExercise): void => {
  const exercises = getAIExercises();
  exercises.push(exercise);
  localStorage.setItem('ai_exercises', JSON.stringify(exercises));
};
