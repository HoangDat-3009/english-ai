import { useState, useEffect, useCallback } from "react";
import { VocabularyWord, VocabularySession, PracticeResult } from "@/types/vocabulary";
import { getRandomWords, calculateNextReviewDate } from "@/data/vocabularyData";

export const useVocabularySession = (wordCount: number = 25) => {
  const [session, setSession] = useState<VocabularySession>({
    words: getRandomWords(wordCount),
    currentIndex: 0,
    correctAnswers: 0,
    incorrectAnswers: 0,
    startTime: new Date(),
  });

  const [isFlipped, setIsFlipped] = useState(false);
  const [userAnswer, setUserAnswer] = useState("");
  const [feedback, setFeedback] = useState<{
    show: boolean;
    correct: boolean;
    message: string;
  }>({ show: false, correct: false, message: "" });

  const currentWord = session.words[session.currentIndex];
  const progress = ((session.currentIndex + 1) / session.words.length) * 100;
  const isLastWord = session.currentIndex === session.words.length - 1;

  const goToNextWord = useCallback(() => {
    if (!isLastWord) {
      setSession(prev => ({
        ...prev,
        currentIndex: prev.currentIndex + 1,
      }));
      setIsFlipped(false);
      setUserAnswer("");
      setFeedback({ show: false, correct: false, message: "" });
    }
  }, [isLastWord]);

  const goToPreviousWord = useCallback(() => {
    if (session.currentIndex > 0) {
      setSession(prev => ({
        ...prev,
        currentIndex: prev.currentIndex - 1,
      }));
      setIsFlipped(false);
      setUserAnswer("");
      setFeedback({ show: false, correct: false, message: "" });
    }
  }, [session.currentIndex]);

  const recordResult = useCallback((result: PracticeResult) => {
    setSession(prev => {
      const updatedWords = [...prev.words];
      const wordIndex = updatedWords.findIndex(w => w.id === result.wordId);
      
      if (wordIndex !== -1) {
        const word = updatedWords[wordIndex];
        updatedWords[wordIndex] = {
          ...word,
          correctCount: result.correct ? word.correctCount + 1 : word.correctCount,
          incorrectCount: !result.correct ? word.incorrectCount + 1 : word.incorrectCount,
          lastReviewed: new Date(),
          nextReview: calculateNextReviewDate(result.difficulty),
        };
      }

      return {
        ...prev,
        words: updatedWords,
        correctAnswers: result.correct ? prev.correctAnswers + 1 : prev.correctAnswers,
        incorrectAnswers: !result.correct ? prev.incorrectAnswers + 1 : prev.incorrectAnswers,
      };
    });
  }, []);

  const checkAnswer = useCallback((answer: string, correctAnswer: string) => {
    const normalizedAnswer = answer.trim().toLowerCase();
    const normalizedCorrect = correctAnswer.trim().toLowerCase();
    
    const isCorrect = normalizedAnswer === normalizedCorrect;
    
    setFeedback({
      show: true,
      correct: isCorrect,
      message: isCorrect 
        ? "Chính xác! Tuyệt vời!" 
        : `Chưa đúng. Đáp án đúng là: ${correctAnswer}`,
    });

    return isCorrect;
  }, []);

  const flipCard = useCallback(() => {
    setIsFlipped(prev => !prev);
  }, []);

  const endSession = useCallback(() => {
    setSession(prev => ({
      ...prev,
      endTime: new Date(),
    }));
  }, []);

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyPress = (e: KeyboardEvent) => {
      if (e.key === "ArrowRight" || e.key === "Enter") {
        goToNextWord();
      } else if (e.key === "ArrowLeft") {
        goToPreviousWord();
      } else if (e.key === " " || e.key === "Space") {
        e.preventDefault();
        flipCard();
      }
    };

    window.addEventListener("keydown", handleKeyPress);
    return () => window.removeEventListener("keydown", handleKeyPress);
  }, [goToNextWord, goToPreviousWord, flipCard]);

  return {
    session,
    currentWord,
    progress,
    isLastWord,
    isFlipped,
    userAnswer,
    feedback,
    setUserAnswer,
    goToNextWord,
    goToPreviousWord,
    recordResult,
    checkAnswer,
    flipCard,
    endSession,
  };
};
