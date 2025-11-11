import { useState, useEffect, useCallback } from "react";
import { GrammarExercise, QuestionStatus, GrammarSessionState } from "@/types/grammar";

interface UseGrammarPracticeProps {
  topicId: number;
  apiUrl?: string;
}

export const useGrammarPractice = ({ 
  topicId,
  apiUrl = `https://dethi-40sj.onrender.com/api/Questions/Topic/${topicId}`
}: UseGrammarPracticeProps) => {
  const [session, setSession] = useState<GrammarSessionState>({
    exercise: null,
    currentQuestionIndex: 0,
    questionStatuses: new Map(),
    startTime: new Date(),
    isCompleted: false,
  });
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedOption, setSelectedOption] = useState<string | null>(null);

  // Fetch exercise data
  useEffect(() => {
    const fetchExercise = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await fetch(apiUrl);
        
        if (!response.ok) {
          throw new Error(`Failed to fetch exercise: ${response.statusText}`);
        }
        
        const data: GrammarExercise[] = await response.json();
        const exercise = data[0]; // Get first exercise from array
        
        // Initialize question statuses
        const statuses = new Map<number, QuestionStatus>();
        exercise.questions.forEach(q => {
          statuses.set(q.questionId, {
            questionId: q.questionId,
            answered: false,
            selectedOption: null,
            isCorrect: null,
          });
        });
        
        setSession(prev => ({
          ...prev,
          exercise,
          questionStatuses: statuses,
        }));
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load exercise");
      } finally {
        setLoading(false);
      }
    };

    fetchExercise();
  }, [apiUrl]);

  const currentQuestion = session.exercise?.questions[session.currentQuestionIndex];

  // Load saved answer when changing questions
  useEffect(() => {
    if (currentQuestion) {
      const status = session.questionStatuses.get(currentQuestion.questionId);
      setSelectedOption(status?.selectedOption || null);
    }
  }, [session.currentQuestionIndex, currentQuestion, session.questionStatuses]);

  const selectAnswer = useCallback((option: string) => {
    if (!currentQuestion) return;

    setSelectedOption(option);
    
    const isCorrect = option === currentQuestion.correctOption;
    
    setSession(prev => {
      const newStatuses = new Map(prev.questionStatuses);
      newStatuses.set(currentQuestion.questionId, {
        questionId: currentQuestion.questionId,
        answered: true,
        selectedOption: option,
        isCorrect,
      });

      return {
        ...prev,
        questionStatuses: newStatuses,
      };
    });
  }, [currentQuestion]);

  const goToQuestion = useCallback((index: number) => {
    if (!session.exercise) return;
    if (index >= 0 && index < session.exercise.questions.length) {
      setSession(prev => ({
        ...prev,
        currentQuestionIndex: index,
      }));
    }
  }, [session.exercise]);

  const nextQuestion = useCallback(() => {
    if (!session.exercise) return;
    if (session.currentQuestionIndex < session.exercise.questions.length - 1) {
      goToQuestion(session.currentQuestionIndex + 1);
    }
  }, [session.currentQuestionIndex, session.exercise, goToQuestion]);

  const previousQuestion = useCallback(() => {
    if (session.currentQuestionIndex > 0) {
      goToQuestion(session.currentQuestionIndex - 1);
    }
  }, [session.currentQuestionIndex, goToQuestion]);

  const resetSession = useCallback(() => {
    if (!session.exercise) return;

    const statuses = new Map<number, QuestionStatus>();
    session.exercise.questions.forEach(q => {
      statuses.set(q.questionId, {
        questionId: q.questionId,
        answered: false,
        selectedOption: null,
        isCorrect: null,
      });
    });

    setSession({
      exercise: session.exercise,
      currentQuestionIndex: 0,
      questionStatuses: statuses,
      startTime: new Date(),
      isCompleted: false,
    });
    setSelectedOption(null);
  }, [session.exercise]);

  const completeSession = useCallback(() => {
    setSession(prev => ({
      ...prev,
      endTime: new Date(),
      isCompleted: true,
    }));
  }, []);

  // Calculate statistics
  const getStatistics = useCallback(() => {
    const statuses = Array.from(session.questionStatuses.values());
    const answered = statuses.filter(s => s.answered);
    const correct = answered.filter(s => s.isCorrect === true);
    const incorrect = answered.filter(s => s.isCorrect === false);
    const unanswered = statuses.filter(s => !s.answered);

    return {
      total: statuses.length,
      answered: answered.length,
      correct: correct.length,
      incorrect: incorrect.length,
      unanswered: unanswered.length,
    };
  }, [session.questionStatuses]);

  return {
    session,
    loading,
    error,
    currentQuestion,
    currentQuestionIndex: session.currentQuestionIndex,
    totalQuestions: session.exercise?.questions.length || 0,
    selectedOption,
    selectAnswer,
    goToQuestion,
    nextQuestion,
    previousQuestion,
    resetSession,
    completeSession,
    getStatistics,
  };
};
