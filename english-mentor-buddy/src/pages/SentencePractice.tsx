import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import Header from '@/components/Navbar';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { 
  ArrowLeft, 
  Check, 
  X, 
  ChevronRight, 
  RotateCcw, 
  Lightbulb,
  Sparkles
} from "lucide-react";
import { toast } from "sonner";
import ReactMarkdown from "react-markdown";

interface SentenceData {
  id: number;
  vietnamese: string;
  suggestion?: {
    vocabulary: Array<{ word: string; meaning: string }>;
    structure: string;
  };
}

interface AIFeedback {
  score: number;
  comment: string;
  grammar: string;
  suggestion: string;
  structureTip: string;
}

const SentencePractice = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { generatedData, topic, level } = location.state || {};
  
  // Debug log
  console.log("🔍 SentencePractice received state:", location.state);
  console.log("📦 generatedData:", generatedData);
  console.log("📝 topic:", topic);
  console.log("📊 level:", level);
  
  const [currentIndex, setCurrentIndex] = useState(0);
  const [userTranslation, setUserTranslation] = useState("");
  const [isReviewing, setIsReviewing] = useState(false);
  const [aiFeedback, setAiFeedback] = useState<AIFeedback | null>(null);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [completedCount, setCompletedCount] = useState(0);

  console.log("✅ Validation check:", {
    hasGeneratedData: !!generatedData,
    hasSentences: !!(generatedData && generatedData.sentences),
    sentencesLength: generatedData?.sentences?.length || 0
  });

  if (!generatedData || !generatedData.sentences || generatedData.sentences.length === 0) {
    return (
      <div className="min-h-screen bg-gradient-soft flex items-center justify-center p-6">
        <Card className="p-8 text-center max-w-md shadow-soft">
          <X className="w-12 h-12 text-destructive mx-auto mb-4" />
          <h2 className="text-xl font-bold mb-2">Không tìm thấy đề bài</h2>
          <p className="text-muted-foreground mb-4">
            Vui lòng tạo bài luyện mới để bắt đầu.
          </p>
          <Button onClick={() => navigate("/sentence-writing")}>
            Tạo bài luyện mới
          </Button>
        </Card>
      </div>
    );
  }

  const sentences: SentenceData[] = generatedData.sentences;
  const currentSentence = sentences[currentIndex];
  const totalSentences = sentences.length;

  const handleSubmitForReview = async () => {
    if (!userTranslation.trim()) {
      toast.error("Vui lòng nhập bản dịch của bạn!");
      return;
    }

    setIsReviewing(true);
    try {
      const { apiService } = await import('@/services/api');
      
      // Map level to backend enum value
      const levelMapping: Record<string, number> = {
        "Beginner": 1,
        "Elementary": 2,
        "Intermediate": 3,
        "UpperIntermediate": 4,
        "Advanced": 5,
        "Proficient": 6
      };

      const response = await fetch(`${apiService.getBaseUrl()}/api/SentenceWriting/Review`, {
        method: "POST",
        headers: { 
          "Content-Type": "application/json",
          ...apiService.getHeaders()
        },
        body: JSON.stringify({
          UserLevel: levelMapping[level] || 3,
          Requirement: currentSentence.vietnamese,
          Content: userTranslation
        })
      });

      if (!response.ok) {
        const errorText = await response.text();
        throw new Error(errorText || "Failed to get review");
      }
      
      const data = await response.json();
      setAiFeedback(data);
      toast.success("Đã nhận đánh giá từ AI!");
    } catch (error) {
      console.error("Error getting review:", error);
      toast.error("Không thể nhận đánh giá. Vui lòng thử lại.");
    } finally {
      setIsReviewing(false);
    }
  };

  const handleNextSentence = () => {
    if (currentIndex < totalSentences - 1) {
      setCurrentIndex(currentIndex + 1);
      setUserTranslation("");
      setAiFeedback(null);
      setShowSuggestions(false);
      if (aiFeedback && aiFeedback.score >= 7) {
        setCompletedCount(completedCount + 1);
      }
    } else {
      toast.success("Bạn đã hoàn thành tất cả các câu!");
      navigate("/sentence-writing");
    }
  };

  const handleRewrite = () => {
    setUserTranslation("");
    setAiFeedback(null);
  };

  return (
    <div className="min-h-screen bg-gradient-soft">
      <Header />
      
      <main className="container mx-auto px-4 py-8 max-w-7xl">
        {/* Header */}
        <div className="mb-6">
          <Button 
            variant="ghost" 
            onClick={() => navigate("/sentence-writing")}
            className="mb-4"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
          
          <div className="flex items-center gap-4 flex-wrap">
            <Badge variant="outline" className="text-sm">
              Chủ đề: <span className="font-semibold ml-1">{topic}</span>
            </Badge>
            <Badge variant="outline" className="text-sm">
              Trình độ: <span className="font-semibold ml-1">{level}</span>
            </Badge>
            <Badge variant="outline" className="text-sm bg-primary/10">
              Hoàn thành: <span className="font-semibold ml-1">{completedCount}/{totalSentences}</span>
            </Badge>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
          {/* Left Column - Sentences and Input */}
          <div className="space-y-6">
            {/* Current Sentence Card */}
            <Card className="shadow-soft">
              <CardHeader className="bg-gradient-primary text-primary-foreground">
                <CardTitle className="flex items-center justify-between">
                  <span className="flex items-center gap-2">
                    <Sparkles className="w-5 h-5" />
                    Câu cần dịch
                  </span>
                  <Badge className="bg-primary-foreground/20 text-primary-foreground">
                    Câu {currentIndex + 1}/{totalSentences}
                  </Badge>
                </CardTitle>
              </CardHeader>
              <CardContent className="pt-6">
                <div className="p-4 bg-accent rounded-lg border-2 border-primary/20">
                  <p className="text-lg font-medium">
                    {currentSentence.vietnamese}
                  </p>
                </div>
              </CardContent>
            </Card>

            {/* Translation Input */}
            <Card className="shadow-soft">
              <CardHeader>
                <CardTitle className="flex items-center justify-between">
                  <span>✍️ Bản dịch của bạn</span>
                  {currentSentence.suggestion && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setShowSuggestions(!showSuggestions)}
                    >
                      <Lightbulb className="w-4 h-4 mr-1" />
                      Gợi ý
                    </Button>
                  )}
                </CardTitle>
              </CardHeader>
              <CardContent className="space-y-4">
                <Textarea
                  placeholder="Nhập bản dịch tiếng Anh của bạn tại đây..."
                  value={userTranslation}
                  onChange={(e) => setUserTranslation(e.target.value)}
                  className="min-h-[120px] text-base"
                  disabled={isReviewing}
                />

                <div className="flex gap-3">
                  <Button
                    variant="outline"
                    onClick={handleRewrite}
                    disabled={isReviewing || !userTranslation}
                    className="flex-1"
                  >
                    <RotateCcw className="w-4 h-4 mr-2" />
                    Viết lại
                  </Button>
                  <Button
                    onClick={handleSubmitForReview}
                    disabled={isReviewing || !userTranslation.trim()}
                    className="flex-1"
                  >
                    {isReviewing ? (
                      <>
                        <div className="w-4 h-4 mr-2 border-2 border-current border-t-transparent rounded-full animate-spin" />
                        Đang kiểm tra...
                      </>
                    ) : (
                      <>
                        <Check className="w-4 h-4 mr-2" />
                        Kiểm tra
                      </>
                    )}
                  </Button>
                </div>
              </CardContent>
            </Card>

            {/* Progress List */}
            <Card className="shadow-soft">
              <CardHeader>
                <CardTitle>📋 Danh sách câu</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-2 max-h-[300px] overflow-y-auto">
                  {sentences.map((sentence, index) => (
                    <div
                      key={sentence.id}
                      className={`p-3 rounded-lg border transition-all ${
                        index === currentIndex
                          ? 'bg-primary/10 border-primary'
                          : index < currentIndex
                          ? 'bg-accent border-border opacity-60'
                          : 'bg-background border-border opacity-40'
                      }`}
                    >
                      <div className="flex items-center justify-between mb-1">
                        <span className="text-sm font-medium">Câu {index + 1}</span>
                        {index < currentIndex && <Check className="w-4 h-4 text-green-600" />}
                      </div>
                      <p className="text-sm text-muted-foreground line-clamp-2">
                        {sentence.vietnamese}
                      </p>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Right Column - Suggestions and AI Feedback */}
          <div className="space-y-6">
            {/* AI Suggestions */}
            {showSuggestions && currentSentence.suggestion && (
              <Card className="shadow-soft bg-gradient-to-br from-blue-50 to-purple-50 dark:from-blue-950/30 dark:to-purple-950/30">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Lightbulb className="w-5 h-5 text-primary" />
                    Gợi ý từ vựng & cấu trúc
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {/* Vocabulary */}
                  <div>
                    <h4 className="font-semibold text-sm mb-2 text-green-700 dark:text-green-400">
                      📚 Từ vựng
                    </h4>
                    <div className="space-y-2">
                      {currentSentence.suggestion.vocabulary.map((vocab, index) => (
                        <div key={index} className="p-2 bg-background rounded border">
                          <p className="font-medium text-sm">{vocab.word}</p>
                          <p className="text-xs text-muted-foreground">{vocab.meaning}</p>
                        </div>
                      ))}
                    </div>
                  </div>

                  <Separator />

                  {/* Structure */}
                  <div className="p-3 bg-background rounded border">
                    <h4 className="font-semibold text-sm mb-2 text-blue-700 dark:text-blue-400">
                      🔧 Cấu trúc câu
                    </h4>
                    <p className="text-sm leading-relaxed">
                      {currentSentence.suggestion.structure}
                    </p>
                  </div>
                </CardContent>
              </Card>
            )}

            {/* AI Feedback */}
            {aiFeedback ? (
              <Card className="shadow-soft">
                <CardHeader className="bg-gradient-primary text-primary-foreground">
                  <CardTitle className="flex items-center gap-2">
                    <Sparkles className="w-5 h-5" />
                    Đánh giá từ AI
                  </CardTitle>
                </CardHeader>
                <CardContent className="pt-6 space-y-4">
                  {/* Score */}
                  <div className="text-center pb-4 border-b">
                    <div className="flex items-center justify-center gap-2 mb-2">
                      {aiFeedback.score >= 7 ? (
                        <Check className="w-8 h-8 text-green-600" />
                      ) : (
                        <X className="w-8 h-8 text-orange-600" />
                      )}
                      <span className="text-5xl font-bold bg-gradient-primary bg-clip-text text-transparent">
                        {aiFeedback.score}/10
                      </span>
                    </div>
                    <p className="text-sm font-medium">
                      {aiFeedback.score >= 7 ? "✓ Xuất sắc!" : "Cần cải thiện"}
                    </p>
                  </div>

                  {/* Comment */}
                  <div className="p-4 bg-blue-50 dark:bg-blue-950/30 rounded-lg border border-blue-200 dark:border-blue-800">
                    <h4 className="font-semibold text-sm mb-2">💬 Nhận xét chung</h4>
                    <div className="text-sm prose prose-sm dark:prose-invert">
                      <ReactMarkdown>{aiFeedback.comment}</ReactMarkdown>
                    </div>
                  </div>

                  {/* Grammar */}
                  {aiFeedback.grammar && aiFeedback.grammar !== "No major issues." && (
                    <div className="p-4 bg-orange-50 dark:bg-orange-950/30 rounded-lg border border-orange-200 dark:border-orange-800">
                      <h4 className="font-semibold text-sm mb-2">✏️ Lỗi ngữ pháp</h4>
                      <div className="text-sm prose prose-sm dark:prose-invert">
                        <ReactMarkdown>{aiFeedback.grammar}</ReactMarkdown>
                      </div>
                    </div>
                  )}

                  {/* Structure Tip */}
                  {aiFeedback.structureTip && (
                    <div className="p-4 bg-yellow-50 dark:bg-yellow-950/30 rounded-lg border border-yellow-200 dark:border-yellow-800">
                      <h4 className="font-semibold text-sm mb-2">💡 Gợi ý cấu trúc</h4>
                      <div className="text-sm prose prose-sm dark:prose-invert">
                        <ReactMarkdown>{aiFeedback.structureTip}</ReactMarkdown>
                      </div>
                    </div>
                  )}

                  {/* Suggestion */}
                  <div className="p-4 bg-green-50 dark:bg-green-950/30 rounded-lg border border-green-200 dark:border-green-800">
                    <h4 className="font-semibold text-sm mb-2">✨ Câu gợi ý</h4>
                    <p className="text-sm font-medium">
                      {aiFeedback.suggestion}
                    </p>
                  </div>

                  {/* Action Buttons */}
                  <div className="flex gap-3 pt-2">
                    <Button
                      variant="outline"
                      onClick={handleRewrite}
                      className="flex-1"
                    >
                      Viết lại
                    </Button>
                    <Button
                      onClick={handleNextSentence}
                      className="flex-1"
                    >
                      {currentIndex < totalSentences - 1 ? (
                        <>
                          Câu tiếp theo
                          <ChevronRight className="w-4 h-4 ml-2" />
                        </>
                      ) : (
                        "Hoàn thành"
                      )}
                    </Button>
                  </div>
                </CardContent>
              </Card>
            ) : (
              <Card className="shadow-soft bg-gradient-to-br from-purple-50 to-blue-50 dark:from-purple-950/30 dark:to-blue-950/30">
                <CardContent className="pt-6">
                  <div className="text-center py-8">
                    <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                      <Sparkles className="w-8 h-8 text-primary" />
                    </div>
                    <h4 className="font-semibold text-lg mb-2">Sẵn sàng kiểm tra</h4>
                    <p className="text-sm text-muted-foreground leading-relaxed mb-4">
                      Hãy dịch câu tiếng Việt sang tiếng Anh và nhấn "Kiểm tra" để nhận đánh giá
                    </p>
                    <div className="p-3 bg-background rounded border text-left">
                      <p className="text-xs text-muted-foreground leading-relaxed">
                        💡 <strong>Mẹo:</strong> Click "Gợi ý" nếu bạn gặp khó khăn. Nhưng hãy thử tự dịch trước để học hiệu quả hơn!
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        </div>
      </main>
    </div>
  );
};

export default SentencePractice;
