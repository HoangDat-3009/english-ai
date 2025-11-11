import { useState, useEffect, useCallback, useRef } from "react";
import { ListeningLesson, ListeningSentence, AnswerCheckResult } from "@/types/listening";

interface UseListeningLessonProps {
  lessonId: number;
  apiUrl?: string;
}

export const useListeningLesson = ({ 
  lessonId, 
  apiUrl = `https://dethi-40sj.onrender.com/api/Listening/lessons/${lessonId}/sentences` 
}: UseListeningLessonProps) => {
  const [lesson, setLesson] = useState<ListeningLesson | null>(null);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [userInput, setUserInput] = useState("");
  const [showResult, setShowResult] = useState<AnswerCheckResult | null>(null);
  const [showAnswer, setShowAnswer] = useState(false);
  const [showTranslation, setShowTranslation] = useState(false);
  const [playbackSpeed, setPlaybackSpeed] = useState("1");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [correctCount, setCorrectCount] = useState(0);
  const [attemptedCount, setAttemptedCount] = useState(0);
  const [revealedSentences, setRevealedSentences] = useState<Set<number>>(new Set());

  const audioRef = useRef<HTMLAudioElement>(null);

  // Fetch lesson data
  useEffect(() => {
    const fetchLesson = async () => {
      try {
        setLoading(true);
        setError(null);
        const response = await fetch(apiUrl);
        
        if (!response.ok) {
          throw new Error(`Failed to fetch lesson: ${response.statusText}`);
        }
        
        // API returns array of sentences directly
        const sentences: ListeningSentence[] = await response.json();
        
        // Sort sentences by orderNo
        sentences.sort((a, b) => a.orderNo - b.orderNo);
        
        // Wrap in ListeningLesson object
        const lessonData: ListeningLesson = {
          lessonId: lessonId,
          lessonTitle: "First Snowfall",
          sentences: sentences
        };
        
        setLesson(lessonData);
      } catch (err) {
        // Fallback to mock data if API fails
        console.warn("API failed, using mock data:", err);
        const mockLesson: ListeningLesson = {
          lessonId: lessonId,
          lessonTitle: "First Snowfall (Mock Data)",
          sentences: [
            {
              sentenceId: 1,
              orderNo: 1,
              transcript: "Today is November 26th.",
              translation: "Hôm nay là ngày 26 tháng 11.",
              audioUrl: "https://autobot-161.s3.amazonaws.com//exercise/audio/2025-07-13-234908_1-2022-05-20-10-01-22.mp3"
            },
            {
              sentenceId: 2,
              orderNo: 2,
              transcript: "It snowed all day today.",
              translation: "Hôm nay tuyết rơi cả ngày.",
              audioUrl: "https://autobot-161.s3.amazonaws.com//exercise/audio/2025-07-13-234908_1-2022-05-20-10-01-22.mp3"
            },
            {
              sentenceId: 3,
              orderNo: 3,
              transcript: "The snow is beautiful.",
              translation: "Tuyết thật đẹp.",
              audioUrl: "https://autobot-161.s3.amazonaws.com//exercise/audio/2025-07-13-234908_1-2022-05-20-10-01-22.mp3"
            }
          ]
        };
        setLesson(mockLesson);
      } finally {
        setLoading(false);
      }
    };

    fetchLesson();
  }, [apiUrl, lessonId]);

  // Update playback speed
  useEffect(() => {
    if (audioRef.current) {
      audioRef.current.playbackRate = parseFloat(playbackSpeed);
    }
  }, [playbackSpeed]);

  // Normalize text for comparison (case-insensitive, remove punctuation)
  const normalizeText = (text: string): string => {
    return text
      .toLowerCase()
      .replace(/[.,!?;:'"]/g, "")
      .trim();
  };

  // Check user answer
  const checkAnswer = useCallback(() => {
    if (!lesson || !lesson.sentences[currentIndex]) return;

    const current = lesson.sentences[currentIndex];
    const normalizedInput = normalizeText(userInput);
    const normalizedCorrect = normalizeText(current.transcript);
    
    const isCorrect = normalizedInput === normalizedCorrect;
    
    setShowResult({
      isCorrect,
      userAnswer: userInput.trim(),
      correctAnswer: current.transcript,
    });
    
    setShowAnswer(!isCorrect);
    setAttemptedCount(prev => prev + 1);
    setRevealedSentences(prev => new Set(prev).add(currentIndex));
    
    if (isCorrect) {
      setCorrectCount(prev => prev + 1);
    }
  }, [lesson, currentIndex, userInput]);

  // Move to next sentence
  const nextSentence = useCallback(() => {
    if (!lesson) return;
    
    if (currentIndex < lesson.sentences.length - 1) {
      setCurrentIndex(prev => prev + 1);
      setUserInput("");
      setShowResult(null);
      setShowAnswer(false);
      setShowTranslation(false);
      
      // Reset audio
      if (audioRef.current) {
        audioRef.current.currentTime = 0;
      }
    }
  }, [lesson, currentIndex]);

  // Move to previous sentence
  const previousSentence = useCallback(() => {
    if (currentIndex > 0) {
      setCurrentIndex(prev => prev - 1);
      setUserInput("");
      setShowResult(null);
      setShowAnswer(false);
      setShowTranslation(false);
      
      // Reset audio
      if (audioRef.current) {
        audioRef.current.currentTime = 0;
      }
    }
  }, [currentIndex]);

  // Skip current sentence
  const skipSentence = useCallback(() => {
    setRevealedSentences(prev => new Set(prev).add(currentIndex));
    nextSentence();
  }, [currentIndex, nextSentence]);

  // Play audio
  const playAudio = useCallback(() => {
    if (audioRef.current) {
      audioRef.current.currentTime = 0;
      audioRef.current.play();
    }
  }, []);

  // Reveal answer
  const revealAnswer = useCallback(() => {
    setShowAnswer(true);
  }, []);

  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      // Prevent shortcuts when typing in input
      if (e.target instanceof HTMLTextAreaElement || e.target instanceof HTMLInputElement) {
        if (e.key === "Enter" && !e.shiftKey) {
          e.preventDefault();
          if (showResult) {
            nextSentence();
          } else {
            checkAnswer();
          }
        }
        return;
      }

      if (e.ctrlKey || e.metaKey) {
        e.preventDefault();
        playAudio();
      } else if (e.key === "ArrowRight") {
        e.preventDefault();
        nextSentence();
      } else if (e.key === "ArrowLeft") {
        e.preventDefault();
        previousSentence();
      }
    };

    window.addEventListener("keydown", handleKeyDown);
    return () => window.removeEventListener("keydown", handleKeyDown);
  }, [showResult, checkAnswer, nextSentence, previousSentence, playAudio]);

  const currentSentence = lesson?.sentences[currentIndex];
  const progress = lesson ? ((currentIndex + 1) / lesson.sentences.length) * 100 : 0;
  const isLastSentence = lesson ? currentIndex === lesson.sentences.length - 1 : false;
  const isFirstSentence = currentIndex === 0;

  return {
    lesson,
    currentSentence,
    currentIndex,
    totalSentences: lesson?.sentences.length || 0,
    progress,
    isLastSentence,
    isFirstSentence,
    userInput,
    setUserInput,
    showResult,
    showAnswer,
    showTranslation,
    setShowTranslation,
    playbackSpeed,
    setPlaybackSpeed,
    loading,
    error,
    correctCount,
    attemptedCount,
    revealedSentences,
    audioRef,
    checkAnswer,
    nextSentence,
    previousSentence,
    skipSentence,
    playAudio,
    revealAnswer,
  };
};
