import { useState } from "react";
import { ArrowLeft, Lightbulb, Volume2 } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { useNavigate, useSearchParams } from "react-router-dom";
import { Progress } from "@/components/ui/progress";
import { useFlashcardWords } from "@/hooks/useFlashcardGroups";
import { toast } from "sonner";

const GuessWord = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const groupId = searchParams.get("groupId");
  const groupName = searchParams.get("groupName") || "Từ vựng";
  
  const { words, loading, error } = useFlashcardWords(groupId);
  
  const [currentIndex, setCurrentIndex] = useState(0);
  const [userAnswer, setUserAnswer] = useState("");
  const [showHint, setShowHint] = useState(false);
  const [feedback, setFeedback] = useState<{
    show: boolean;
    correct: boolean;
    message: string;
  }>({ show: false, correct: false, message: "" });
  
  const currentWord = words[currentIndex];
  const progress = words.length > 0 ? ((currentIndex + 1) / words.length) * 100 : 0;

  const checkAnswer = (answer: string, correctAnswer: string) => {
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
  };

  const handleCheck = () => {
    const isCorrect = checkAnswer(userAnswer, currentWord.english_word);
    
    if (isCorrect) {
      setShowHint(false);
      setTimeout(() => {
        goToNext();
      }, 1500);
    }
  };

  const handleSkip = () => {
    setFeedback({
      show: true,
      correct: false,
      message: `Đáp án đúng là: ${currentWord.english_word}`,
    });
    setShowHint(false);
    setTimeout(() => {
      goToNext();
    }, 1500);
  };

  const goToNext = () => {
    if (currentIndex < words.length - 1) {
      setCurrentIndex(prev => prev + 1);
      setUserAnswer("");
      setShowHint(false);
      setFeedback({ show: false, correct: false, message: "" });
    }
  };

  const goToPrevious = () => {
    if (currentIndex > 0) {
      setCurrentIndex(prev => prev - 1);
      setUserAnswer("");
      setShowHint(false);
      setFeedback({ show: false, correct: false, message: "" });
    }
  };

  const getHint = () => {
    setShowHint(true);
    const wordLength = currentWord.english_word.length;
    const hint = currentWord.english_word[0] + "_".repeat(wordLength - 2) + currentWord.english_word[wordLength - 1];
    toast.info(`Gợi ý: ${hint} (${wordLength} ký tự)`);
  };

  const playAudio = () => {
    if ('speechSynthesis' in window && currentWord) {
      const utterance = new SpeechSynthesisUtterance(currentWord.english_word);
      utterance.lang = 'en-US';
      speechSynthesis.speak(utterance);
    }
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Đang tải từ vựng...</p>
        </div>
      </div>
    );
  }

  if (error || !currentWord) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <Card className="p-8 max-w-md">
          <h2 className="text-xl font-bold text-red-600 mb-2">Không thể tải dữ liệu</h2>
          <p className="text-muted-foreground mb-4">{error || "Không có từ vựng"}</p>
          <Button onClick={() => navigate(-1)}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Purple Header */}
      <div className="bg-gradient-to-r from-purple-600 to-pink-600 text-white py-12 px-6">
        <div className="max-w-4xl mx-auto">
          <Button 
            variant="ghost" 
            className="mb-4 text-white hover:bg-white/20"
            onClick={() => navigate(-1)}
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
          <h1 className="text-3xl font-bold mb-2">Đoán và Gõ Từ</h1>
          <p className="text-purple-100">
            Nghe audio, đoán và gõ lại từ vựng chính xác để ghi nhớ hiệu quả
          </p>
        </div>
      </div>

      <div className="px-6 py-8">
        <div className="max-w-4xl mx-auto">
          {/* Progress Bar */}
          <div className="mb-8">
            <div className="flex items-center justify-between mb-2">
              <span className="font-semibold">
                {currentIndex + 1}/{words.length}
              </span>
              <span className="font-semibold">{Math.round(progress)}%</span>
            </div>
            <Progress value={progress} className="h-2" />
          </div>

          {/* Word Card */}
          <Card className="p-12 mb-6 text-center">
            {currentWord.example_sentence_en && (
              <div className="mb-8 p-6 bg-muted/30 rounded-lg">
                <p className="text-lg italic">{currentWord.example_sentence_en}</p>
                <p className="text-sm text-muted-foreground mt-2">{currentWord.example_sentence_vi}</p>
              </div>
            )}

            <p className="text-sm text-muted-foreground mb-2">{currentWord.phonetic}</p>
            <h2 className="text-3xl font-bold mb-8">{currentWord.vietnamese_meaning}</h2>

            <Input
              placeholder="Gõ từ vựng bạn nghe được..."
              value={userAnswer}
              onChange={(e) => setUserAnswer(e.target.value)}
              className="text-lg p-6 text-center mb-6"
              disabled={feedback.show && feedback.correct}
              onKeyPress={(e) => {
                if (e.key === 'Enter' && userAnswer.trim()) {
                  handleCheck();
                }
              }}
            />

            {feedback.show && (
              <div className={`mb-6 p-4 rounded-lg ${
                feedback.correct 
                  ? 'bg-green-50 dark:bg-green-950 border-l-4 border-green-600' 
                  : 'bg-red-50 dark:bg-red-950 border-l-4 border-red-600'
              }`}>
                <p className={`font-semibold ${
                  feedback.correct ? 'text-green-900 dark:text-green-100' : 'text-red-900 dark:text-red-100'
                }`}>
                  {feedback.message}
                </p>
              </div>
            )}

            {/* Action Buttons */}
            <div className="flex gap-3 justify-center flex-wrap">
              <Button 
                className="bg-indigo-600 hover:bg-indigo-700 text-white px-6"
                onClick={getHint}
              >
                <Lightbulb className="w-4 h-4 mr-2" />
                Gợi ý
              </Button>
              <Button 
                className="bg-green-600 hover:bg-green-700 text-white px-6"
                onClick={playAudio}
              >
                <Volume2 className="w-4 h-4 mr-2" />
                Phát Audio
              </Button>
              <Button 
                variant="outline" 
                className="px-6"
                onClick={handleCheck}
                disabled={!userAnswer.trim() || (feedback.show && feedback.correct)}
              >
                ✓ Kiểm tra
              </Button>
              <Button 
                className="bg-orange-600 hover:bg-orange-700 text-white px-6"
                onClick={handleSkip}
              >
                ⏭ Bỏ qua
              </Button>
            </div>
          </Card>

          {/* Navigation */}
          <div className="flex items-center justify-between mb-6">
            <Button 
              variant="outline" 
              className="px-8"
              onClick={goToPrevious}
              disabled={currentIndex === 0}
            >
              ← Từ trước
            </Button>
            <Button 
              className="bg-purple-600 hover:bg-purple-700 text-white px-8"
              onClick={goToNext}
              disabled={currentIndex === words.length - 1}
            >
              Từ tiếp →
            </Button>
          </div>

          {/* Keyboard Shortcuts */}
          <Card className="bg-muted/50 p-6 mb-6">
            <p className="text-sm text-center">
              <span className="font-semibold">Phím tắt:</span> S - để nghe audio, Enter - để kiểm tra, H - hiển thị gợi ý, ← - từ trước, → - từ tiếp
            </p>
          </Card>

          {/* End Button */}
          <div className="text-center">
            <Button variant="destructive" className="px-8">
              🏁 Kết thúc
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default GuessWord;
