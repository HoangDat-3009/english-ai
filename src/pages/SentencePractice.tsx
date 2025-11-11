import { useState } from "react";
import { ArrowLeft, Volume2, Lightbulb, ThumbsUp } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Textarea } from "@/components/ui/textarea";
import { useNavigate } from "react-router-dom";
import { Progress } from "@/components/ui/progress";
import { useVocabularySession } from "@/hooks/useVocabularySession";
import { toast } from "sonner";

const SentencePractice = () => {
  const navigate = useNavigate();
  const {
    currentWord,
    progress,
    goToNextWord,
    goToPreviousWord,
    recordResult,
    session,
  } = useVocabularySession(8);
  
  const [userSentence, setUserSentence] = useState("");
  const [showExample, setShowExample] = useState(false);
  const [evaluation, setEvaluation] = useState<{
    show: boolean;
    message: string;
  }>({ show: false, message: "" });

  const handleEvaluate = () => {
    if (!userSentence.trim()) {
      toast.error("Vui lòng nhập câu của bạn!");
      return;
    }

    const includesWord = userSentence.toLowerCase().includes(currentWord.word.toLowerCase());
    
    if (includesWord) {
      setEvaluation({
        show: true,
        message: "Tuyệt vời! Bạn đã sử dụng từ vựng chính xác trong câu. Câu của bạn có nghĩa và ngữ pháp hợp lý.",
      });
      recordResult({
        wordId: currentWord.id,
        correct: true,
        timeSpent: 0,
        difficulty: 'medium',
      });
      toast.success("Câu của bạn rất tốt!");
    } else {
      setEvaluation({
        show: true,
        message: `Câu của bạn chưa chứa từ "${currentWord.word}". Hãy thử lại và đảm bảo sử dụng từ này trong câu.`,
      });
      toast.warning("Hãy sử dụng từ vựng trong câu của bạn!");
    }
  };

  const handleNext = () => {
    setUserSentence("");
    setShowExample(false);
    setEvaluation({ show: false, message: "" });
    goToNextWord();
  };

  return (
    <div className="min-h-screen bg-background">
      {/* Orange Header */}
      <div className="bg-orange-600 text-white py-12 px-6">
        <div className="max-w-4xl mx-auto text-center">
          <h1 className="text-3xl font-bold mb-2">Luyện tập câu</h1>
          <p className="text-orange-100">
            Tạo câu với từ vựng đã học để ghi nhớ hiệu quả và sử dụng thành thạo
          </p>
        </div>
      </div>

      <div className="px-6 py-8">
        <div className="max-w-4xl mx-auto">
          <Button 
            variant="ghost" 
            className="mb-6"
            onClick={() => navigate("/vocabulary-group")}
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>

          {/* Progress Bar */}
          <div className="mb-8">
            <div className="flex items-center justify-between mb-2">
              <span className="font-semibold">
                {session.currentIndex + 1}/{session.words.length}
              </span>
              <span className="font-semibold">{Math.round(progress)}%</span>
            </div>
            <Progress value={progress} className="h-2" />
          </div>

          {/* Practice Card */}
          <Card className="p-8 mb-6">
            <div className="mb-6">
              <span className="text-sm text-muted-foreground mb-4 block">
                Từ {session.currentIndex + 1}/{session.words.length}
              </span>
              <p className="text-lg font-semibold mb-6">
                Tạo một câu hoàn chỉnh sử dụng từ vựng sau:
              </p>

              <div className="bg-orange-50 dark:bg-orange-950 border-l-4 border-orange-600 p-6 rounded-r-lg mb-8">
                <div className="flex items-center justify-between">
                  <div>
                    <h3 className="text-2xl font-bold text-orange-900 dark:text-orange-100 mb-2">
                      {currentWord.word}
                    </h3>
                    <p className="text-sm text-orange-700 dark:text-orange-300 mb-2">
                      {currentWord.pronunciation}
                    </p>
                    <p className="text-base text-orange-800 dark:text-orange-200">
                      {currentWord.translation}
                    </p>
                  </div>
                  <Button
                    size="icon"
                    className="bg-orange-600 hover:bg-orange-700 rounded-full flex-shrink-0"
                    onClick={() => toast.success("Phát âm thanh...")}
                  >
                    <Volume2 className="w-4 h-4" />
                  </Button>
                </div>
              </div>

              {showExample && currentWord.example && (
                <div className="bg-blue-50 dark:bg-blue-950 border-l-4 border-blue-600 p-4 rounded-r-lg mb-4">
                  <p className="text-sm font-semibold text-blue-900 dark:text-blue-100 mb-1">
                    Câu ví dụ:
                  </p>
                  <p className="text-blue-800 dark:text-blue-200">
                    {currentWord.example}
                  </p>
                </div>
              )}

              <Textarea
                placeholder="Nhập câu của bạn tại đây..."
                value={userSentence}
                onChange={(e) => setUserSentence(e.target.value)}
                className="min-h-[120px] text-base mb-4"
              />

              {evaluation.show && (
                <div className="bg-green-50 dark:bg-green-950 border-l-4 border-green-600 p-4 rounded-r-lg mb-4">
                  <p className="text-green-900 dark:text-green-100">
                    {evaluation.message}
                  </p>
                </div>
              )}

              <div className="flex gap-3">
                <Button 
                  variant="outline" 
                  className="flex-1"
                  onClick={() => setShowExample(!showExample)}
                >
                  <Lightbulb className="w-4 h-4 mr-2" />
                  💡 {showExample ? "Ẩn" : "Xem"} câu ví dụ
                </Button>
                <Button 
                  className="flex-1 bg-orange-600 hover:bg-orange-700 text-white"
                  onClick={handleEvaluate}
                  disabled={!userSentence.trim()}
                >
                  <ThumbsUp className="w-4 h-4 mr-2" />
                  👍 Đánh giá
                </Button>
              </div>
            </div>
          </Card>

          {/* Navigation */}
          <div className="flex items-center justify-between mb-6">
            <Button 
              variant="outline" 
              className="px-8"
              onClick={() => {
                setUserSentence("");
                setShowExample(false);
                setEvaluation({ show: false, message: "" });
                goToPreviousWord();
              }}
              disabled={session.currentIndex === 0}
            >
              ← Từ trước
            </Button>
            <Button 
              className="bg-orange-600 hover:bg-orange-700 text-white px-8"
              onClick={handleNext}
              disabled={session.currentIndex === session.words.length - 1}
            >
              Từ tiếp →
            </Button>
          </div>

          {/* Keyboard Shortcuts */}
          <Card className="bg-muted/50 p-6 mb-6">
            <p className="text-sm text-center">
              <span className="font-semibold">Phím tắt:</span> S - để nghe audio, Ctrl+Enter - để kiểm tra, ← - từ trước, → - từ tiếp
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

export default SentencePractice;
