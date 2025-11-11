import { useState } from "react";
import { ArrowRight, Loader2, Volume2, BookOpen } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Progress } from "@/components/ui/progress";
import { toast } from "sonner";
import { AudioRecorder } from "@/components/AudioRecorder";
import { useSpeakingPractice } from "@/hooks/useSpeakingPractice";
import type { ConversationTopic } from "@/types/conversation";
import { Header } from "@/components/Header";

const ConversationPractice = () => {
  const [conversation, setConversation] = useState<ConversationTopic | null>(null);
  const [currentDialogueIndex, setCurrentDialogueIndex] = useState(0);
  const [analysisResult, setAnalysisResult] = useState<any>(null);
  const [showSetup, setShowSetup] = useState(true);
  const [topic, setTopic] = useState("Daily routine");
  const [level, setLevel] = useState(2);
  
  const { isLoading, generateDialogue, analyzeSpeech, evaluateAnswer } = useSpeakingPractice();

  const handleStartPractice = async () => {
    const result = await generateDialogue(topic, "Speaking practice", level);
    if (result) {
      setConversation(result);
      setShowSetup(false);
      setCurrentDialogueIndex(0);
    }
  };

  const handleRecordingComplete = async (audioBlob: Blob) => {
    if (!conversation) return;

    const currentDialogue = conversation.dialogue[currentDialogueIndex];
    
    // Phân biệt câu hỏi mở và câu luyện theo mẫu
    // Câu hỏi mở: không có ai_suggestion HOẶC là câu hỏi (kết thúc bằng ?)
    const isOpenQuestion = !currentDialogue.ai_suggestion || currentDialogue.en.trim().endsWith('?');
    
    let result;
    if (isOpenQuestion && currentDialogue.en.includes('?')) {
      // Dùng EvaluateAnswer cho câu hỏi mở
      result = await evaluateAnswer(
        conversation.topic,
        currentDialogue.en,
        audioBlob
      );
    } else {
      // Dùng AnalyzeSpeech cho câu luyện theo mẫu
      result = await analyzeSpeech(
        conversation.topic,
        currentDialogue.en,
        audioBlob
      );
    }
    
    if (result) {
      setAnalysisResult(result);
      
      // Tính điểm trung bình để quyết định tự động next
      const avgScore = result.overallScore || 
        (result.pronunciationScore + result.fluencyScore + result.accuracyScore) / 3 ||
        (result.contentScore + result.grammarScore + result.vocabularyScore + 
         result.pronunciationScore + result.fluencyScore) / 5;
      
      // Nếu điểm >= 6, tự động chuyển sang câu tiếp sau 2 giây
      if (avgScore >= 6) {
        setTimeout(() => {
          handleNext();
        }, 2000);
      }
    }
  };

  const handleNext = () => {
    if (!conversation) return;
    
    if (currentDialogueIndex < conversation.dialogue.length - 1) {
      setCurrentDialogueIndex(prev => prev + 1);
      setAnalysisResult(null);
    } else {
      toast.success("🎉 Bạn đã hoàn thành bài luyện tập!");
      setShowSetup(true);
      setConversation(null);
      setCurrentDialogueIndex(0);
      setAnalysisResult(null);
    }
  };

  const handleRetry = () => {
    setAnalysisResult(null);
  };

  const playReference = () => {
    if (!conversation) return;
    const currentDialogue = conversation.dialogue[currentDialogueIndex];
    const utterance = new SpeechSynthesisUtterance(currentDialogue.en);
    utterance.lang = 'en-US';
    utterance.rate = 0.9;
    window.speechSynthesis.speak(utterance);
  };

  if (showSetup) {
    return (
      <>
        <Header />
        <div className="min-h-screen bg-gradient-to-b from-accent to-background flex items-center justify-center p-6">
          <Card className="max-w-md w-full p-8 border-primary/20">
            <div className="text-center mb-6">
              <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                <BookOpen className="w-8 h-8 text-primary" />
              </div>
              <h1 className="text-2xl font-bold mb-2">
                <span className="text-primary">EngBuddy</span> - Luyện Nói
              </h1>
              <p className="text-muted-foreground">Chọn chủ đề và cấp độ để bắt đầu luyện tập</p>
            </div>

          <div className="space-y-4">
            <div>
              <label className="text-sm font-medium mb-2 block">Chủ đề</label>
              <select
                value={topic}
                onChange={(e) => setTopic(e.target.value)}
                className="w-full p-3 border rounded-md bg-background"
              >
                <option value="Daily routine">Sinh hoạt hàng ngày</option>
                <option value="Job interview">Phỏng vấn việc làm</option>
                <option value="Travel">Du lịch</option>
                <option value="Shopping">Mua sắm</option>
                <option value="Restaurant">Nhà hàng</option>
                <option value="Weather">Thời tiết</option>
              </select>
            </div>

            <div>
              <label className="text-sm font-medium mb-2 block">Cấp độ</label>
              <select
                value={level}
                onChange={(e) => setLevel(Number(e.target.value))}
                className="w-full p-3 border rounded-md bg-background"
              >
                <option value={1}>Sơ cấp (Elementary)</option>
                <option value={2}>Trung cấp (Intermediate)</option>
                <option value={3}>Cao cấp (Advanced)</option>
              </select>
            </div>

            <Button
              onClick={handleStartPractice}
              disabled={isLoading}
              className="w-full"
              size="lg"
            >
              {isLoading ? (
                <>
                  <Loader2 className="w-4 h-4 mr-2 animate-spin" />
                  Đang tạo bài học...
                </>
              ) : (
                "Bắt đầu luyện tập"
              )}
            </Button>
          </div>
          </Card>
        </div>
      </>
    );
  }

  if (!conversation) return null;

  const currentDialogue = conversation.dialogue[currentDialogueIndex];
  const progress = ((currentDialogueIndex + 1) / conversation.dialogue.length) * 100;

  return (
    <>
      <Header />
      <div className="min-h-screen bg-gradient-to-b from-accent to-background">
        <div className="grid grid-cols-1 lg:grid-cols-[1fr,450px] gap-6 p-6 max-w-[1600px] mx-auto">
          {/* Left Column - Practice Area */}
          <div className="space-y-4">
            {/* Header */}
            <Card className="p-4 bg-primary text-primary-foreground border-primary">
              <div className="flex items-center justify-between mb-2">
                <div className="flex items-center gap-3">
                  <span className="font-semibold text-lg">{conversation.topic}</span>
                  <Badge className="bg-white/20 text-white border-white/30">
                    {conversation.difficulty}
                  </Badge>
                </div>
                <span className="font-semibold">Câu {currentDialogueIndex + 1}/{conversation.dialogue.length}</span>
              </div>
              <Progress value={progress} className="h-2 bg-white/20" />
            </Card>

          {/* Current Dialogue */}
          <Card className="p-6">
            <div className="space-y-4">
              <div className="p-4 rounded bg-primary/5 border-l-4 border-primary">
                <div className="flex items-center justify-between mb-2">
                  <span className="font-semibold text-primary">{currentDialogue.speaker}:</span>
                  <Button
                    size="sm"
                    variant="ghost"
                    onClick={playReference}
                    className="gap-2"
                  >
                    <Volume2 className="w-4 h-4" />
                    Phát mẫu
                  </Button>
                </div>
                <p className="text-lg mb-2 font-medium">{currentDialogue.vi}</p>
                <p className="text-sm text-muted-foreground italic">{currentDialogue.en}</p>
              </div>

              {currentDialogue.ai_suggestion && (
                <div className="bg-accent/50 p-4 rounded border">
                  <h4 className="font-semibold mb-2 flex items-center gap-2">
                    💡 Gợi ý từ AI
                  </h4>
                  <div className="space-y-2 text-sm">
                    <div>
                      <span className="font-medium">Từ vựng:</span>
                      <div className="flex flex-wrap gap-2 mt-1">
                        {currentDialogue.ai_suggestion.vocabulary.map((item, idx) => (
                          <Badge key={idx} variant="outline">
                            {item.word} - {item.meaning}
                          </Badge>
                        ))}
                      </div>
                    </div>
                    {currentDialogue.ai_suggestion.structure && (
                      <div>
                        <span className="font-medium">Cấu trúc:</span>
                        <p className="text-muted-foreground mt-1">{currentDialogue.ai_suggestion.structure}</p>
                      </div>
                    )}
                  </div>
                </div>
              )}
            </div>
          </Card>

          {/* Recording Area */}
          <Card className="p-6">
            <div className="text-center mb-4">
              <h3 className="font-semibold text-lg mb-2">🎤 Ghi âm câu trả lời của bạn</h3>
              <p className="text-sm text-muted-foreground">
                Nhấn nút bên dưới để ghi âm, sau đó gửi để nhận đánh giá từ AI
              </p>
            </div>

            <AudioRecorder
              onRecordingComplete={handleRecordingComplete}
              disabled={isLoading}
            />

            {analysisResult && (
              <div className="mt-6 flex gap-3">
                <Button
                  onClick={handleRetry}
                  variant="outline"
                  className="flex-1"
                >
                  Thử lại
                </Button>
                <Button
                  onClick={handleNext}
                  className="flex-1 gap-2"
                >
                  Tiếp tục
                  <ArrowRight className="w-4 h-4" />
                </Button>
              </div>
            )}
          </Card>
        </div>

        {/* Right Column - AI Feedback */}
        <div className="sticky top-6 h-fit">
          <Card className="p-6 bg-primary text-primary-foreground">
            <h3 className="font-semibold text-lg flex items-center gap-2">
              📊 Đánh giá từ AI
            </h3>
          </Card>

          {analysisResult ? (
            <Card className="p-6 mt-4 max-h-[calc(100vh-200px)] overflow-y-auto">
              <div className="space-y-4">
                {/* Transcript */}
                {analysisResult.transcript && (
                  <div className="pb-4 border-b">
                    <h4 className="font-semibold mb-2">📝 Bạn đã nói:</h4>
                    <p className="text-sm bg-accent/50 p-3 rounded italic">
                      "{analysisResult.transcript}"
                    </p>
                  </div>
                )}

                {/* Scores */}
                {analysisResult.pronunciationScore !== undefined && (
                  <div className="pb-4 border-b">
                    <h4 className="font-semibold mb-3">📊 Điểm số</h4>
                    <div className="space-y-3 text-sm">
                      {analysisResult.pronunciationScore > 0 && (
                        <div>
                          <div className="flex justify-between mb-1">
                            <span>Phát âm:</span>
                            <span className="font-semibold">{analysisResult.pronunciationScore}/10</span>
                          </div>
                          <Progress value={analysisResult.pronunciationScore * 10} />
                        </div>
                      )}
                      {analysisResult.fluencyScore > 0 && (
                        <div>
                          <div className="flex justify-between mb-1">
                            <span>Độ trôi chảy:</span>
                            <span className="font-semibold">{analysisResult.fluencyScore}/10</span>
                          </div>
                          <Progress value={analysisResult.fluencyScore * 10} />
                        </div>
                      )}
                      {analysisResult.intonationScore > 0 && (
                        <div>
                          <div className="flex justify-between mb-1">
                            <span>Ngữ điệu:</span>
                            <span className="font-semibold">{analysisResult.intonationScore}/10</span>
                          </div>
                          <Progress value={analysisResult.intonationScore * 10} />
                        </div>
                      )}
                      {analysisResult.accuracyScore > 0 && (
                        <div>
                          <div className="flex justify-between mb-1">
                            <span>Độ chính xác:</span>
                            <span className="font-semibold">{analysisResult.accuracyScore}/10</span>
                          </div>
                          <Progress value={analysisResult.accuracyScore * 10} />
                        </div>
                      )}
                      {analysisResult.contentScore > 0 && (
                        <div>
                          <div className="flex justify-between mb-1">
                            <span>Nội dung:</span>
                            <span className="font-semibold">{analysisResult.contentScore}/10</span>
                          </div>
                          <Progress value={analysisResult.contentScore * 10} />
                        </div>
                      )}
                      {analysisResult.grammarScore > 0 && (
                        <div>
                          <div className="flex justify-between mb-1">
                            <span>Ngữ pháp:</span>
                            <span className="font-semibold">{analysisResult.grammarScore}/10</span>
                          </div>
                          <Progress value={analysisResult.grammarScore * 10} />
                        </div>
                      )}
                      {analysisResult.vocabularyScore > 0 && (
                        <div>
                          <div className="flex justify-between mb-1">
                            <span>Từ vựng:</span>
                            <span className="font-semibold">{analysisResult.vocabularyScore}/10</span>
                          </div>
                          <Progress value={analysisResult.vocabularyScore * 10} />
                        </div>
                      )}
                      {analysisResult.overallScore > 0 && (
                        <div className="pt-2 mt-2 border-t">
                          <div className="flex justify-between mb-1">
                            <span className="font-semibold">Tổng điểm:</span>
                            <span className="font-bold text-primary">{analysisResult.overallScore}/10</span>
                          </div>
                          <Progress value={analysisResult.overallScore * 10} className="h-3" />
                        </div>
                      )}
                    </div>
                  </div>
                )}

                {/* Feedback */}
                {analysisResult.feedback && (
                  <div className="pb-4 border-b">
                    <h4 className="font-semibold mb-2">💬 Nhận xét</h4>
                    <p className="text-sm leading-relaxed">{analysisResult.feedback}</p>
                  </div>
                )}

                {/* Suggestions */}
                {analysisResult.suggestion && (
                  <div className="bg-amber-50 p-4 rounded border border-amber-200">
                    <h4 className="font-semibold mb-2 text-amber-900">💡 Gợi ý cải thiện</h4>
                    <p className="text-sm leading-relaxed">{analysisResult.suggestion}</p>
                  </div>
                )}

                {analysisResult.suggestions && (
                  <div className="bg-amber-50 p-4 rounded border border-amber-200">
                    <h4 className="font-semibold mb-2 text-amber-900">💡 Gợi ý cải thiện</h4>
                    <p className="text-sm leading-relaxed">{analysisResult.suggestions}</p>
                  </div>
                )}
              </div>
            </Card>
          ) : (
            <Card className="p-6 mt-4">
              <div className="text-center py-12">
                <div className="w-16 h-16 bg-primary/10 rounded-full flex items-center justify-center mx-auto mb-4">
                  <span className="text-3xl">🎤</span>
                </div>
                <h4 className="font-semibold text-lg mb-2">Sẵn sàng luyện nói?</h4>
                <p className="text-sm text-muted-foreground">
                  Ghi âm câu trả lời của bạn và nhận đánh giá chi tiết từ AI
                </p>
              </div>
            </Card>
          )}
        </div>
      </div>
    </div>
    </>
  );
};

export default ConversationPractice;