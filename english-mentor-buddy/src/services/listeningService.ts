import axios from 'axios';

const API_BASE_URL = import.meta.env.VITE_API_BASE_URL || 'http://localhost:5283';

export interface ListeningExercise {
  id: string;
  title: string;
  topic: string;
  level: EnglishLevel;
  audioContent: string;
  audioUrl: string;
  questions: ListeningQuestion[];
  duration: number;
  createdAt: string;
}

export interface ListeningQuestion {
  id: string;
  question: string;
  options: string[];
  correctOptionIndex: number;
  explanationInVietnamese: string;
  type: ListeningQuestionType;
  startTimeInSeconds?: number;
  endTimeInSeconds?: number;
}

export interface ListeningExerciseResult {
  exerciseId: string;
  answers: ListeningAnswer[];
  totalQuestions: number;
  correctAnswers: number;
  scorePercentage: number;
  timeSpent: string;
  completedAt: string;
}

export interface ListeningAnswer {
  questionId: string;
  selectedOptionIndex: number;
  isCorrect: boolean;
  timeSpentOnQuestion?: string;
}

export interface GenerateListeningExerciseParams {
  topic: string;
  level: EnglishLevel;
  totalQuestions: number;
  questionTypes: ListeningQuestionType[];
  durationInMinutes?: number;
  preferredAccent?: string;
}

export interface SubmitListeningExerciseParams {
  exerciseId: string;
  answers: {
    questionId: string;
    selectedOptionIndex: number;
    timeSpentOnQuestion?: string;
  }[];
  timeSpent: string;
}

export enum EnglishLevel {
  A1 = 'A1',
  A2 = 'A2', 
  B1 = 'B1',
  B2 = 'B2',
  C1 = 'C1',
  C2 = 'C2'
}

export enum ListeningQuestionType {
  GeneralComprehension = 1,
  SpecificDetails = 2,
  Inference = 3,
  AttitudeEmotion = 4,
  VocabularyInContext = 5,
  Purpose = 6,
  FillInTheBlanks = 7,
  SoundRecognition = 8
}

export const listeningService = {
  async generateExercise(params: GenerateListeningExerciseParams): Promise<ListeningExercise> {
    try {
      const response = await axios.post(`${API_BASE_URL}/api/listening/generate`, params);
      return response.data;
    } catch (error) {
      console.error('Error generating listening exercise:', error);
      throw new Error('Failed to generate listening exercise');
    }
  },

  async getExercise(exerciseId: string): Promise<ListeningExercise> {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/listening/${exerciseId}`);
      return response.data;
    } catch (error) {
      console.error('Error fetching listening exercise:', error);
      throw new Error('Failed to fetch listening exercise');
    }
  },

  async submitExercise(params: SubmitListeningExerciseParams): Promise<ListeningExerciseResult> {
    try {
      const response = await axios.post(`${API_BASE_URL}/api/listening/submit`, params);
      return response.data;
    } catch (error) {
      console.error('Error submitting listening exercise:', error);
      throw new Error('Failed to submit listening exercise');
    }
  },

  async getSuggestedTopics(level: EnglishLevel): Promise<string[]> {
    try {
      const response = await axios.get(`${API_BASE_URL}/api/listening/suggesttopics`, {
        params: { level }
      });
      return response.data;
    } catch (error) {
      console.error('Error fetching suggested topics:', error);
      throw new Error('Failed to fetch suggested topics');
    }
  },

  // Helper function to convert text to speech using Web Speech API
  async textToSpeech(text: string, accent: string = 'en-US'): Promise<void> {
    if (!('speechSynthesis' in window)) {
      throw new Error('Speech synthesis not supported');
    }

    return new Promise((resolve, reject) => {
      const utterance = new SpeechSynthesisUtterance(text);
      utterance.lang = accent;
      utterance.rate = 0.9; // Slightly slower for learning
      
      utterance.onend = () => resolve();
      utterance.onerror = (error) => reject(error);
      
      speechSynthesis.speak(utterance);
    });
  },

  // Helper function to stop speech
  stopSpeech(): void {
    if ('speechSynthesis' in window) {
      speechSynthesis.cancel();
    }
  }
};