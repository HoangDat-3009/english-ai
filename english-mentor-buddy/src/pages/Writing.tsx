import { useState } from "react";
import { useNavigate } from "react-router-dom";
import Header from '@/components/Navbar';
import { ReviewForm } from "@/components/ReviewForm";
import { ReviewResult } from "@/components/ReviewResult";
import { reviewApi, GenerateReviewRequest } from "@/lib/api";
import { toast } from "sonner";
import { Sparkles, ArrowLeft } from "lucide-react";
import { Button } from "@/components/ui/button";

const Index = () => {
  const navigate = useNavigate();
  const [isLoading, setIsLoading] = useState(false);
  const [review, setReview] = useState<string | null>(null);
  const [aiProvider, setAiProvider] = useState<'gemini' | 'openai'>('gemini');

  const handleSubmit = async (data: GenerateReviewRequest) => {
    setIsLoading(true);
    
    try {
      const result = await reviewApi.generateReview(data, aiProvider);
      setReview(result);
      toast.success("Nhận xét đã được tạo thành công!");
    } catch (error: unknown) {
      console.error("Error generating review:", error);
      
      // Xử lý trường hợp backend busy (201 với message lỗi)
      const busyError = error as { isBusyError?: boolean; message?: string };
      if (busyError.isBusyError) {
        const message = busyError.message || "## CẢNH BÁO\n EngBuddy đang bận đi pha cà phê nên tạm thời vắng mặt. Cục cưng vui lòng ngồi chơi 3 phút rồi gửi lại cho EngBuddy nhận xét nha.\nYêu cục cưng nhiều lắm luôn á!";
        setReview(message);
        toast.warning("EngBuddy đang bận, vui lòng thử lại sau 3 phút");
        setIsLoading(false);
        return;
      }
      
      const err = error as { response?: { status?: number; data?: string } };
      
      if (err.response?.status === 400) {
        toast.error(err.response.data || "Dữ liệu không hợp lệ");
      } else if (err.response?.status === 401) {
        toast.error("Không có quyền truy cập. Vui lòng kiểm tra Access Key");
      } else {
        toast.error("Có lỗi xảy ra khi tạo nhận xét. Vui lòng thử lại!");
      }
    } finally {
      setIsLoading(false);
    }
  };

  const handleBack = () => {
    setReview(null);
  };

  return (
    <div className="min-h-screen bg-gradient-soft">
      <Header />
      
      <main className="container mx-auto px-4 py-8 max-w-4xl">
        {!review ? (
          <div className="space-y-8">
            <Button
              variant="ghost"
              onClick={() => navigate("/writing-mode")}
              className="mb-4"
            >
              <ArrowLeft className="w-4 h-4 mr-2" />
              Quay lại
            </Button>

            <div className="text-center space-y-4 py-8">
              <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-gradient-primary shadow-soft mb-4">
                <Sparkles className="w-8 h-8 text-primary-foreground" />
              </div>
              <h1 className="text-4xl font-bold bg-gradient-primary bg-clip-text text-transparent">
                LUYỆN VIẾT
              </h1>
              <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
                Nhận phản hồi chi tiết để nâng cao kỹ năng viết tiếng Anh của bạn
              </p>
            </div>

            <div className="flex justify-center gap-3 mb-6">
              <Button
                variant={aiProvider === 'gemini' ? 'default' : 'outline'}
                onClick={() => setAiProvider('gemini')}
                className="transition-all"
              >
                🤖 Gemini
              </Button>
              <Button
                variant={aiProvider === 'openai' ? 'default' : 'outline'}
                onClick={() => setAiProvider('openai')}
                className="transition-all"
              >
                ✨ ChatGPT
              </Button>
            </div>

            <ReviewForm onSubmit={handleSubmit} isLoading={isLoading} />
          </div>
        ) : (
          <ReviewResult review={review} onBack={handleBack} />
        )}
      </main>
    </div>
  );
};

export default Index;
