import { useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import { ArrowLeft, Lightbulb, Check, X, ChevronRight, RotateCcw } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Textarea } from "@/components/ui/textarea";
import { Badge } from "@/components/ui/badge";
import { toast } from "sonner";
import type { ConversationTopic, ConversationDialogue } from "@/types/conversation";

interface WritingReviewResponse {
  requirement: string;
  content: string;
  aiEvaluation: {
    score: number;
    comment: string;
    grammar: string;
    suggestion: string;
    structureTip: string;
  };
}

const WritingPractice = () => {
  const location = useLocation();
  const navigate = useNavigate();
  const { generatedData, level, language } = location.state || {};
  
  const [currentIndex, setCurrentIndex] = useState(0);
  const [userTranslation, setUserTranslation] = useState("");
  const [isReviewing, setIsReviewing] = useState(false);
  const [aiFeedback, setAiFeedback] = useState<WritingReviewResponse | null>(null);
  const [showSuggestions, setShowSuggestions] = useState(false);
  const [completedCount, setCompletedCount] = useState(0);

  if (!generatedData || !generatedData.dialogue || generatedData.dialogue.length === 0) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center p-6">
        <Card className="p-8 text-center max-w-md">
          <X className="w-12 h-12 text-destructive mx-auto mb-4" />
          <h2 className="text-xl font-bold mb-2">Không tìm thấy đề bài</h2>
          <p className="text-muted-foreground mb-4">
            Vui lòng tạo đề bài mới để bắt đầu luyện viết.
          </p>
          <Button onClick={() => navigate("/create-lesson")}>
            Tạo đề bài mới
          </Button>
        </Card>
      </div>
    );
  }

  const topic: ConversationTopic = generatedData;
  const currentSentence: ConversationDialogue = topic.dialogue[currentIndex];
  const totalSentences = topic.dialogue.length;

  const handleSubmitForReview = async () => {
    if (!userTranslation.trim()) {
      toast.error("Vui lòng nhập bản dịch của bạn!");
      return;
    }

    setIsReviewing(true);
    try {
      const response = await fetch("https://btl-d39f.onrender.com/api/WritingAi/Review", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          Level: level,
          Requirement: currentSentence.vi,
          Content: userTranslation
        })
      });

      if (!response.ok) throw new Error("Failed to get review");
      
      const textResponse = await response.text();
      let data;
      try {
        data = JSON.parse(textResponse);
      } catch (parseError) {
        const jsonMatch = textResponse.match(/\{[\s\S]*\}/);
        if (jsonMatch) {
          data = JSON.parse(jsonMatch[0]);
        } else {
          throw new Error("Invalid response format");
        }
      }
      
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
      if (aiFeedback) {
        setCompletedCount(completedCount + 1);
      }
    } else {
      toast.success("Bạn đã hoàn thành tất cả các câu!");
      navigate("/create-lesson");
    }
  };

  const handleRewrite = () => {
    setUserTranslation("");
    setAiFeedback(null);
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-purple/10 to-primary/10">
      <div className="max-w-7xl mx-auto p-4 md:p-6">
        {/* Header */}
        <div className="mb-6">
          <Button 
            variant="ghost" 
            onClick={() => navigate("/create-lesson")}
            className="mb-4"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
          
          <div className="flex items-center gap-4 flex-wrap">
            <Badge variant="outline" className="text-sm">
              Ngôn ngữ luyện: <span className="font-semibold ml-1">Tiếng Anh</span>
            </Badge>
            <Badge variant="outline" className="text-sm">
              Chủ đề: <span className="font-semibold ml-1">{topic.topic}</span>
            </Badge>
            <Badge variant="outline" className="text-sm">
              Độ khó: <span className="font-semibold ml-1">{topic.difficulty}</span>
            </Badge>
            <Badge variant="outline" className="text-sm">
              ⭐ Points còn lại: <span className="font-semibold ml-1">{completedCount}</span>
            </Badge>
          </div>
        </div>

        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4 md:gap-6">
          {/* Left Column - Conversation and Input */}
          <div className="space-y-4">
            {/* Conversation Card */}
            <Card className="p-4 md:p-6 bg-primary text-primary-foreground">
              <div className="flex items-center justify-between mb-4">
                <h3 className="font-semibold text-lg flex items-center gap-2">
                  💬 Hội thoại song ngữ
                </h3>
                <Badge className="bg-primary-foreground/20 text-primary-foreground">
                  Câu {currentIndex + 1}/{totalSentences}
                </Badge>
              </div>

              {/* All Dialogue Items */}
              <div className="space-y-3 max-h-[400px] overflow-y-auto pr-2">
                {topic.dialogue.map((item, index) => (
                  <div 
                    key={item.id} 
                    className={`p-3 rounded border transition-all ${
                      index === currentIndex 
                        ? 'bg-amber-100 border-amber-300' 
                        : index < currentIndex 
                        ? 'bg-primary-foreground/10 border-primary-foreground/20 opacity-60' 
                        : 'bg-primary-foreground/5 border-primary-foreground/10 opacity-40'
                    }`}
                  >
                    <div className="font-medium text-sm mb-1 flex items-center gap-2">
                      <span>{item.speaker}:</span>
                      {index < currentIndex && <Check className="w-4 h-4 text-green-600" />}
                    </div>
                    <p className="text-sm mb-1 font-medium text-foreground">
                      {item.vi}
                    </p>
                    {index < currentIndex && (
                      <p className="text-xs opacity-70">{item.en}</p>
                    )}
                  </div>
                ))}
              </div>
            </Card>

            {/* Translation Input */}
            <Card className="p-4 md:p-6">
              <div className="mb-4">
                <div className="flex items-center justify-between mb-2">
                  <h3 className="font-semibold flex items-center gap-2">
                    ✍️ Dịch câu này sang tiếng Anh
                  </h3>
                  {currentSentence.ai_suggestion && (
                    <Button
                      variant="ghost"
                      size="sm"
                      onClick={() => setShowSuggestions(!showSuggestions)}
                    >
                      <Lightbulb className="w-4 h-4 mr-1" />
                      Gợi ý
                    </Button>
                  )}
                </div>
                <div className="p-3 bg-amber-50 border border-amber-200 rounded mb-3">
                  <p className="text-sm font-medium">
                    <span className="text-muted-foreground">{currentSentence.speaker}:</span>{" "}
                    {currentSentence.vi}
                  </p>
                </div>
              </div>

              <Textarea
                placeholder="Nhập bản dịch tiếng Anh của bạn tại đây..."
                value={userTranslation}
                onChange={(e) => setUserTranslation(e.target.value)}
                className="min-h-[120px] text-base mb-4"
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
                  className="bg-primary hover:bg-primary/90 text-primary-foreground flex-1"
                >
                  {isReviewing ? (
                    <>
                      <div className="w-4 h-4 mr-2 border-2 border-primary-foreground/30 border-t-primary-foreground rounded-full animate-spin" />
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
            </Card>
          </div>

          {/* Right Column - Suggestions and AI Feedback */}
          <div className="space-y-4">
            {/* AI Suggestions */}
            {showSuggestions && currentSentence.ai_suggestion && (
              <Card className="p-4 md:p-6 bg-gradient-to-br from-blue-50 to-purple-50 border-blue-200">
                <h3 className="font-semibold text-lg mb-4 flex items-center gap-2 text-primary">
                  💡 Gợi ý từ vựng & cấu trúc
                </h3>

                {/* Vocabulary */}
                <div className="mb-4">
                  <h4 className="font-semibold text-sm mb-2 text-green-700">Từ vựng</h4>
                  <div className="space-y-2">
                    {currentSentence.ai_suggestion.vocabulary.map((vocab, index) => (
                      <div key={index} className="p-2 bg-white rounded border">
                        <p className="font-medium text-sm text-green-700">{vocab.word}</p>
                        <p className="text-xs text-muted-foreground">{vocab.meaning}</p>
                      </div>
                    ))}
                  </div>
                </div>

                {/* Structure */}
                <div className="p-3 bg-white rounded border border-blue-200">
                  <h4 className="font-semibold text-sm mb-2 text-blue-700">Cấu trúc câu</h4>
                  <p className="text-sm leading-relaxed">
                    {currentSentence.ai_suggestion.structure}
                  </p>
                </div>
              </Card>
            )}

            {/* AI Feedback */}
            {aiFeedback ? (
              <Card className="p-4 md:p-6">
                <h3 className="font-semibold text-lg mb-4 flex items-center gap-2 bg-gradient-to-r from-primary to-purple-600 bg-clip-text text-transparent">
                  📋 Đánh giá từ AI
                </h3>

                {/* Score */}
                <div className="text-center pb-4 mb-4 border-b">
                  <div className="flex items-center justify-center gap-2 mb-2">
                    {aiFeedback.aiEvaluation.score >= 8 ? (
                      <Check className="w-6 h-6 text-green-600" />
                    ) : (
                      <X className="w-6 h-6 text-orange-600" />
                    )}
                    <span className="text-4xl font-bold text-primary">
                      {aiFeedback.aiEvaluation.score}/10
                    </span>
                  </div>
                  <p className="text-sm font-medium">
                    {aiFeedback.aiEvaluation.score >= 8 ? "✓ Tốt!" : "Cần cải thiện"}
                  </p>
                </div>

                {/* Training Tips */}
                <div className="mb-4 p-3 bg-blue-50 rounded border border-blue-200">
                  <h4 className="font-semibold text-sm mb-2 flex items-center gap-2">
                    👍 Hướng dẫn luyện tập
                  </h4>
                  <p className="text-sm leading-relaxed">
                    {aiFeedback.aiEvaluation.comment}
                  </p>
                </div>

                {/* Structure Tip */}
                {aiFeedback.aiEvaluation.structureTip && (
                  <div className="mb-4 p-3 bg-yellow-50 rounded border border-yellow-200">
                    <h4 className="font-semibold text-sm mb-2 flex items-center gap-2">
                      💡 Gợi ý cấu trúc câu:
                    </h4>
                    <p className="text-sm leading-relaxed">
                      {aiFeedback.aiEvaluation.structureTip}
                    </p>
                  </div>
                )}

                {/* Grammar Issues */}
                {aiFeedback.aiEvaluation.grammar && aiFeedback.aiEvaluation.grammar !== "No major issues." && (
                  <div className="mb-4 p-3 bg-orange-50 rounded border border-orange-200">
                    <h4 className="font-semibold text-sm mb-2 flex items-center gap-2">
                      ✏️ Điểm cần cải thiện
                    </h4>
                    <p className="text-sm leading-relaxed">
                      {aiFeedback.aiEvaluation.grammar}
                    </p>
                  </div>
                )}

                {/* Suggestion */}
                <div className="p-3 bg-red-50 rounded border border-red-200 mb-4">
                  <h4 className="font-semibold text-sm mb-2 text-red-900 flex items-center gap-2">
                    💡 Gợi ý câu trúc câu:
                  </h4>
                  <p className="text-sm leading-relaxed text-red-700">
                    {currentSentence.speaker}: {aiFeedback.aiEvaluation.suggestion}
                  </p>
                </div>

                {/* Action Buttons */}
                <div className="flex gap-3">
                  <Button
                    variant="outline"
                    onClick={handleRewrite}
                    className="flex-1"
                  >
                    Viết lại
                  </Button>
                  <Button
                    onClick={handleNextSentence}
                    className="bg-primary hover:bg-primary/90 text-primary-foreground flex-1"
                  >
                    {currentIndex < totalSentences - 1 ? (
                      <>
                        Tiếp tục
                        <ChevronRight className="w-4 h-4 ml-2" />
                      </>
                    ) : (
                      "Hoàn thành"
                    )}
                  </Button>
                </div>
              </Card>
            ) : (
              <Card className="p-4 md:p-6 bg-gradient-to-br from-purple-50 to-blue-50">
                <h3 className="font-semibold text-lg mb-4 flex items-center gap-2 text-primary">
                  🤖 Trợ lý học tập AI của bạn
                </h3>
                <div className="text-center py-8">
                  <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                    <Lightbulb className="w-8 h-8 text-primary" />
                  </div>
                  <h4 className="font-semibold text-lg mb-2">Hướng dẫn luyện tập</h4>
                  <p className="text-sm text-muted-foreground leading-relaxed mb-4">
                    Click vào button "Gợi ý" nếu bạn gặp khó khăn, AI sẽ giúp bạn 💡
                  </p>
                  <div className="p-3 bg-blue-100 border border-blue-300 rounded text-left">
                    <p className="text-xs text-blue-900 leading-relaxed">
                      💡 <strong>Lời khuyên:</strong> Hãy thử dịch trước khi xem gợi ý để tăng hiệu quả học tập!
                    </p>
                  </div>
                </div>
              </Card>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};

export default WritingPractice;
