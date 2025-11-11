import { useState, useMemo } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { ArrowLeft, Volume2, ChevronLeft, ChevronRight, RotateCw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { useFlashcardWords } from "@/hooks/useFlashcardGroups";

const FlashcardPractice = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const groupId = searchParams.get("groupId");
  const groupName = searchParams.get("groupName") || "Từ vựng";
  
  const { words, loading, error } = useFlashcardWords(groupId);
  
  const [currentIndex, setCurrentIndex] = useState(0);
  const [isFlipped, setIsFlipped] = useState(false);
  const [showAnswer, setShowAnswer] = useState(false);
  const [knownCount, setKnownCount] = useState(0);
  const [reviewCount, setReviewCount] = useState(0);
  
  const currentWord = words[currentIndex];
  const progress = words.length > 0 ? ((currentIndex + 1) / words.length) * 100 : 0;
  const isLastWord = currentIndex === words.length - 1;
  
  const flipCard = () => {
    setIsFlipped(prev => !prev);
    setShowAnswer(true);
  };
  
  const markAsKnown = () => {
    setKnownCount(prev => prev + 1);
    goToNext();
  };
  
  const markAsReview = () => {
    setReviewCount(prev => prev + 1);
    goToNext();
  };
  
  const goToNext = () => {
    if (!isLastWord) {
      setCurrentIndex(prev => prev + 1);
      setIsFlipped(false);
      setShowAnswer(false);
    }
  };
  
  const goToPrevious = () => {
    if (currentIndex > 0) {
      setCurrentIndex(prev => prev - 1);
      setIsFlipped(false);
      setShowAnswer(false);
    }
  };
  
  const goToWord = (index: number) => {
    if (index >= 0 && index < words.length) {
      setCurrentIndex(index);
      setIsFlipped(false);
      setShowAnswer(false);
    }
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

  if (error || words.length === 0) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <Card className="p-8 max-w-md">
          <h2 className="text-xl font-bold text-red-600 mb-2">Không thể tải dữ liệu</h2>
          <p className="text-muted-foreground mb-4">{error || "Không tìm thấy dữ liệu"}</p>
          <Button onClick={() => navigate(-1)}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
        </Card>
      </div>
    );
  }

  if (!currentWord) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <Card className="p-8 max-w-md text-center">
          <h2 className="text-xl font-bold mb-4">Không có từ vựng để học</h2>
          <Button onClick={() => navigate(-1)}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-green-50 to-emerald-50 dark:from-gray-900 dark:to-gray-800">
      {/* Header */}
      <div className="bg-card border-b shadow-sm">
        <div className="max-w-5xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between">
            <Button variant="ghost" onClick={() => navigate(-1)}>
              <ArrowLeft className="w-4 h-4 mr-2" />
              Quay lại
            </Button>
            <h1 className="text-xl font-bold">{groupName}</h1>
            <div className="text-sm text-muted-foreground">
              {currentIndex + 1} / {words.length}
            </div>
          </div>
          <Progress value={progress} className="mt-4" />
        </div>
      </div>

      {/* Main Content */}
      <div className="max-w-5xl mx-auto px-6 py-8">
        {/* Stats */}
        <div className="grid grid-cols-2 gap-4 mb-8">
          <Card className="p-4 text-center">
            <div className="text-2xl font-bold text-green-600">{knownCount}</div>
            <div className="text-sm text-muted-foreground">Đã biết</div>
          </Card>
          <Card className="p-4 text-center">
            <div className="text-2xl font-bold text-orange-600">{reviewCount}</div>
            <div className="text-sm text-muted-foreground">Cần ôn</div>
          </Card>
        </div>

        {/* Flashcard */}
        <div className="mb-8">
          <Card 
            className={`relative h-96 cursor-pointer transition-transform duration-500 ${
              isFlipped ? '[transform:rotateY(180deg)]' : ''
            }`}
            onClick={flipCard}
            style={{ transformStyle: 'preserve-3d' }}
          >
            <div className="absolute inset-0 flex flex-col items-center justify-center p-8">
              {!isFlipped ? (
                // Front - English word
                <div className="text-center">
                  <div className="text-5xl font-bold mb-4">{currentWord.english_word}</div>
                  <div className="text-xl text-muted-foreground mb-6">{currentWord.phonetic}</div>
                  <div className="mt-8">
                    <Button
                      variant="outline"
                      size="lg"
                      onClick={(e) => {
                        e.stopPropagation();
                        playAudio();
                      }}
                    >
                      <Volume2 className="w-5 h-5 mr-2" />
                      Phát âm
                    </Button>
                  </div>
                </div>
              ) : (
                // Back - Vietnamese meaning + example
                <div className="text-center w-full [transform:rotateY(180deg)]">
                  <div className="text-4xl font-bold mb-4 text-primary">{currentWord.vietnamese_meaning}</div>
                  {currentWord.example_sentence_en && (
                    <div className="mt-8 p-6 bg-muted/50 rounded-lg">
                      <div className="text-lg italic mb-2">{currentWord.example_sentence_en}</div>
                      <div className="text-sm text-muted-foreground mt-2">{currentWord.example_sentence_vi}</div>
                    </div>
                  )}
                </div>
              )}
            </div>
          </Card>
          <p className="text-center text-muted-foreground mt-4">
            {isFlipped ? '👆 Nhấn để lật thẻ' : '👆 Nhấn để xem nghĩa'}
          </p>
        </div>

        {/* Action Buttons */}
        {showAnswer && (
          <div className="grid grid-cols-2 gap-4 mb-8">
            <Button
              size="lg"
              variant="outline"
              className="h-16 text-lg border-orange-500 text-orange-600 hover:bg-orange-50"
              onClick={markAsReview}
            >
              🔄 Cần ôn lại
            </Button>
            <Button
              size="lg"
              className="h-16 text-lg bg-green-600 hover:bg-green-700"
              onClick={markAsKnown}
            >
              ✓ Đã biết
            </Button>
          </div>
        )}

        {/* Navigation */}
        <div className="flex items-center justify-between">
          <Button
            variant="outline"
            onClick={goToPrevious}
            disabled={currentIndex === 0}
          >
            <ChevronLeft className="w-4 h-4 mr-2" />
            Trước
          </Button>

          <Button
            variant="outline"
            onClick={() => goToWord(0)}
          >
            <RotateCw className="w-4 h-4 mr-2" />
            Bắt đầu lại
          </Button>

          <Button
            variant="outline"
            onClick={() => goToWord(currentIndex + 1)}
            disabled={isLastWord}
          >
            Tiếp theo
            <ChevronRight className="w-4 h-4 ml-2" />
          </Button>
        </div>

        {/* Word List Navigation */}
        <Card className="mt-8 p-6">
          <h3 className="font-bold mb-4">Danh sách từ vựng</h3>
          <div className="grid grid-cols-5 md:grid-cols-10 gap-2">
            {words.map((word, index) => (
              <button
                key={word.id}
                onClick={() => goToWord(index)}
                className={`p-2 rounded text-sm font-medium transition-colors ${
                  index === currentIndex
                    ? 'bg-primary text-primary-foreground'
                    : 'bg-muted hover:bg-muted/80'
                }`}
              >
                {index + 1}
              </button>
            ))}
          </div>
        </Card>
      </div>
    </div>
  );
};

export default FlashcardPractice;
