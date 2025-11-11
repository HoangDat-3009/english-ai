import { useState, useEffect, useCallback } from "react";
import { ToeicVocabularyWord } from "@/types/vocabulary";

interface FlashcardSession {
  words: ToeicVocabularyWord[];
  currentIndex: number;
  correctCount: number;
  incorrectCount: number;
  knownCount: number;
  reviewCount: number;
}

export const useVocabularyFlashcard = (words: ToeicVocabularyWord[]) => {
  const [session, setSession] = useState<FlashcardSession>({
    words: words,
    currentIndex: 0,
    correctCount: 0,
    incorrectCount: 0,
    knownCount: 0,
    reviewCount: 0,
  });
  
  const [isFlipped, setIsFlipped] = useState(false);
  const [showAnswer, setShowAnswer] = useState(false);

  useEffect(() => {
    setSession(prev => ({ ...prev, words }));
  }, [words]);

  const currentWord = session.words[session.currentIndex];
  const progress = ((session.currentIndex + 1) / session.words.length) * 100;
  const isLastWord = session.currentIndex === session.words.length - 1;

  const flipCard = useCallback(() => {
    setIsFlipped(prev => !prev);
    setShowAnswer(true);
  }, []);

  const markAsKnown = useCallback(() => {
    setSession(prev => ({
      ...prev,
      knownCount: prev.knownCount + 1,
    }));
    goToNext();
  }, []);

  const markAsReview = useCallback(() => {
    setSession(prev => ({
      ...prev,
      reviewCount: prev.reviewCount + 1,
    }));
    goToNext();
  }, []);

  const markAsCorrect = useCallback(() => {
    setSession(prev => ({
      ...prev,
      correctCount: prev.correctCount + 1,
    }));
    goToNext();
  }, []);

  const markAsIncorrect = useCallback(() => {
    setSession(prev => ({
      ...prev,
      incorrectCount: prev.incorrectCount + 1,
    }));
    goToNext();
  }, []);

  const goToNext = () => {
    if (!isLastWord) {
      setSession(prev => ({
        ...prev,
        currentIndex: prev.currentIndex + 1,
      }));
      setIsFlipped(false);
      setShowAnswer(false);
    }
  };

  const goToPrevious = useCallback(() => {
    if (session.currentIndex > 0) {
      setSession(prev => ({
        ...prev,
        currentIndex: prev.currentIndex - 1,
      }));
      setIsFlipped(false);
      setShowAnswer(false);
    }
  }, [session.currentIndex]);

  const goToWord = useCallback((index: number) => {
    if (index >= 0 && index < session.words.length) {
      setSession(prev => ({
        ...prev,
        currentIndex: index,
      }));
      setIsFlipped(false);
      setShowAnswer(false);
    }
  }, [session.words.length]);

  return {
    session,
    currentWord,
    progress,
    isLastWord,
    isFlipped,
    showAnswer,
    flipCard,
    markAsKnown,
    markAsReview,
    markAsCorrect,
    markAsIncorrect,
    goToNext,
    goToPrevious,
    goToWord,
  };
};
