import { useState, useEffect } from "react";
import { useNavigate, useSearchParams } from "react-router-dom";
import { ArrowLeft, Check, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";
import { useVocabularyTopic } from "@/hooks/useVocabularyTopics";
import { ToeicVocabularyWord } from "@/types/vocabulary";

const VocabularyTest = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const topicName = searchParams.get("topic") || "Từ vựng CEFR";
  
  const { data, loading } = useVocabularyTopic(topicName);
  const [currentIndex, setCurrentIndex] = useState(0);
  const [selectedAnswer, setSelectedAnswer] = useState<string>("");
  const [isAnswered, setIsAnswered] = useState(false);
  const [score, setScore] = useState({ correct: 0, incorrect: 0 });
  const [questions, setQuestions] = useState<Array<{
    word: ToeicVocabularyWord;
    options: string[];
    correctAnswer: string;
  }>>([]);

  useEffect(() => {
    if (data?.items) {
      // Create questions with multiple choice options
      const questionList = data.items.map(word => {
        const allWords = data.items.filter(w => w.id !== word.id);
        const wrongAnswers = allWords
          .sort(() => Math.random() - 0.5)
          .slice(0, 3)
          .map(w => w.meaningVi);
        
        const options = [...wrongAnswers, word.meaningVi]
          .sort(() => Math.random() - 0.5);
        
        return {
          word,
          options,
          correctAnswer: word.meaningVi,
        };
      });
      
      setQuestions(questionList);
    }
  }, [data]);

  const currentQuestion = questions[currentIndex];
  const progress = ((currentIndex + 1) / questions.length) * 100;
  const isLastQuestion = currentIndex === questions.length - 1;

  const handleSubmitAnswer = () => {
    if (!selectedAnswer) return;
    
    setIsAnswered(true);
    const isCorrect = selectedAnswer === currentQuestion.correctAnswer;
    
    setScore(prev => ({
      correct: prev.correct + (isCorrect ? 1 : 0),
      incorrect: prev.incorrect + (isCorrect ? 0 : 1),
    }));
  };

  const handleNext = () => {
    if (!isLastQuestion) {
      setCurrentIndex(prev => prev + 1);
      setSelectedAnswer("");
      setIsAnswered(false);
    }
  };

  if (loading || !currentQuestion) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Đang tải...</p>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-gray-900 dark:to-gray-800">
      {/* Header */}
      <div className="bg-card border-b shadow-sm">
        <div className="max-w-4xl mx-auto px-6 py-4">
          <div className="flex items-center justify-between mb-4">
            <Button variant="ghost" onClick={() => navigate(-1)}>
              <ArrowLeft className="w-4 h-4 mr-2" />
              Quay lại
            </Button>
            <h1 className="text-xl font-bold">Kiểm tra từ vựng</h1>
            <div className="text-sm text-muted-foreground">
              {currentIndex + 1} / {questions.length}
            </div>
          </div>
          <Progress value={progress} />
        </div>
      </div>

      {/* Content */}
      <div className="max-w-4xl mx-auto px-6 py-8">
        {/* Score */}
        <div className="grid grid-cols-2 gap-4 mb-8">
          <Card className="p-4 text-center bg-green-50 dark:bg-green-950">
            <div className="flex items-center justify-center gap-2 mb-2">
              <Check className="w-5 h-5 text-green-600" />
              <span className="text-2xl font-bold text-green-600">{score.correct}</span>
            </div>
            <div className="text-sm text-muted-foreground">Đúng</div>
          </Card>
          <Card className="p-4 text-center bg-red-50 dark:bg-red-950">
            <div className="flex items-center justify-center gap-2 mb-2">
              <X className="w-5 h-5 text-red-600" />
              <span className="text-2xl font-bold text-red-600">{score.incorrect}</span>
            </div>
            <div className="text-sm text-muted-foreground">Sai</div>
          </Card>
        </div>

        {/* Question */}
        <Card className="p-8 mb-8">
          <div className="mb-8">
            <h2 className="text-3xl font-bold mb-4">{currentQuestion.word.word}</h2>
            <div className="text-lg text-muted-foreground mb-2">{currentQuestion.word.phonetic}</div>
            <span className="inline-block px-3 py-1 bg-primary/10 text-primary rounded-full text-sm font-medium">
              {currentQuestion.word.pos}
            </span>
          </div>

          {currentQuestion.word.exampleEn && (
            <div className="mb-8 p-4 bg-muted/50 rounded-lg">
              <p className="text-sm text-muted-foreground mb-1">Ví dụ:</p>
              <p className="italic">{currentQuestion.word.exampleEn}</p>
            </div>
          )}

          <div className="mb-6">
            <h3 className="font-semibold mb-4">Chọn nghĩa đúng:</h3>
            <RadioGroup value={selectedAnswer} onValueChange={setSelectedAnswer} disabled={isAnswered}>
              {currentQuestion.options.map((option, index) => {
                const isCorrect = option === currentQuestion.correctAnswer;
                const isSelected = option === selectedAnswer;
                const showResult = isAnswered;
                
                return (
                  <div
                    key={index}
                    className={`flex items-center space-x-3 p-4 rounded-lg border-2 transition-colors ${
                      showResult && isCorrect
                        ? 'border-green-500 bg-green-50 dark:bg-green-950'
                        : showResult && isSelected && !isCorrect
                        ? 'border-red-500 bg-red-50 dark:bg-red-950'
                        : 'border-border hover:border-primary'
                    }`}
                  >
                    <RadioGroupItem value={option} id={`option-${index}`} />
                    <Label
                      htmlFor={`option-${index}`}
                      className="flex-1 cursor-pointer text-base"
                    >
                      {option}
                    </Label>
                    {showResult && isCorrect && (
                      <Check className="w-5 h-5 text-green-600" />
                    )}
                    {showResult && isSelected && !isCorrect && (
                      <X className="w-5 h-5 text-red-600" />
                    )}
                  </div>
                );
              })}
            </RadioGroup>
          </div>

          <div className="flex gap-4">
            {!isAnswered ? (
              <Button
                onClick={handleSubmitAnswer}
                disabled={!selectedAnswer}
                className="flex-1"
                size="lg"
              >
                Kiểm tra
              </Button>
            ) : (
              <Button
                onClick={handleNext}
                disabled={isLastQuestion}
                className="flex-1"
                size="lg"
              >
                {isLastQuestion ? 'Xem kết quả' : 'Câu tiếp theo'}
              </Button>
            )}
          </div>
        </Card>

        {/* Results Summary */}
        {isLastQuestion && isAnswered && (
          <Card className="p-8 text-center bg-gradient-to-r from-blue-500 to-purple-500 text-white">
            <h2 className="text-3xl font-bold mb-4">🎉 Hoàn thành!</h2>
            <div className="text-5xl font-bold mb-4">
              {Math.round((score.correct / questions.length) * 100)}%
            </div>
            <p className="text-lg mb-6">
              Bạn trả lời đúng {score.correct}/{questions.length} câu
            </p>
            <Button
              size="lg"
              variant="secondary"
              onClick={() => navigate(-1)}
            >
              Quay lại
            </Button>
          </Card>
        )}
      </div>
    </div>
  );
};

export default VocabularyTest;
