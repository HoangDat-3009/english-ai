import { useNavigate, useParams } from "react-router-dom";
import { ArrowLeft, MessageCircle, TrendingUp, Layers, CheckCircle } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { useVocabularyTopic } from "@/hooks/useVocabularyTopics";

const VocabularyGroup = () => {
  const navigate = useNavigate();
  const { topicId } = useParams<{ topicId: string }>();
  const { data, loading, error } = useVocabularyTopic(topicId || "Từ vựng CEFR");

  const topicName = data?.topic || topicId || "Từ vựng";
  const totalWords = data?.total || 0;
  const dueWords = 2; // Mock data

  const practiceMode = [
    { 
      name: "Flashcard", 
      icon: Layers, 
      description: "Luyện tập với thẻ ghi nhớ, lật thẻ để xem nghĩa và ví dụ",
      color: "bg-green-600 hover:bg-green-700",
      path: `/flashcard-practice?topic=${encodeURIComponent(topicName)}`
    },
    { 
      name: "Kiểm tra", 
      icon: CheckCircle, 
      description: "Trắc nghiệm và điền từ để kiểm tra kiến thức từ vựng",
      color: "bg-blue-600 hover:bg-blue-700",
      path: `/vocabulary-test?topic=${encodeURIComponent(topicName)}`
    },
    { 
      name: "Đoán và Gõ Từ", 
      icon: "⌨️", 
      description: "Nghe audio, đoán và gõ lại từ vựng chính xác",
      color: "bg-purple-600 hover:bg-purple-700",
      path: "/guess-word"
    },
    { 
      name: "Luyện câu", 
      icon: "✏️", 
      description: "Tạo câu với từ vựng và nhận phản hồi từ AI",
      color: "bg-orange-600 hover:bg-orange-700",
      path: "/sentence-practice"
    },
  ];

  if (loading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Đang tải dữ liệu...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <Card className="p-8 max-w-md">
          <h2 className="text-xl font-bold text-red-600 mb-2">Lỗi tải dữ liệu</h2>
          <p className="text-muted-foreground mb-4">{error}</p>
          <Button onClick={() => navigate("/vocabulary-topics")}>
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại danh sách
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      {/* Blue Header */}
      <div className="bg-gradient-to-r from-blue-600 to-blue-700 text-white py-16 px-6">
        <div className="max-w-7xl mx-auto text-center">
          <h1 className="text-4xl font-bold mb-3">Chọn chế độ luyện tập từ vựng</h1>
          <p className="text-blue-100 text-lg">
            Luyện tập từ vựng với phương pháp Spaced Repetition (lặp lại ngắt quãng) để ghi nhớ từ vựng hiệu quả
          </p>
        </div>
      </div>

      <div className="px-6 py-8">
        <div className="max-w-7xl mx-auto">
          <Button 
            variant="ghost" 
            className="mb-6"
            onClick={() => navigate("/vocabulary-topics")}
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Danh sách nhóm từ vựng
          </Button>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6 mb-8">
            {/* Statistics Section */}
            <Card className="p-6">
              <div className="flex items-center gap-3 mb-6">
                <TrendingUp className="w-5 h-5 text-primary" />
                <h2 className="text-xl font-bold">Thống kê luyện tập</h2>
              </div>
              
              <div className="grid grid-cols-2 gap-4">
                <div className="bg-muted/50 rounded-lg p-4 text-center">
                  <div className="text-3xl font-bold text-primary mb-1">{totalWords}</div>
                  <div className="text-sm text-muted-foreground">Tổng số từ</div>
                </div>
                <div className="bg-muted/50 rounded-lg p-4 text-center">
                  <div className="text-3xl font-bold text-primary mb-1">0</div>
                  <div className="text-sm text-muted-foreground">Từ vựng thành thạo</div>
                </div>
                <div className="bg-muted/50 rounded-lg p-4 text-center">
                  <div className="text-3xl font-bold text-primary mb-1">1</div>
                  <div className="text-sm text-muted-foreground">Phiên luyện tập</div>
                </div>
                <div className="bg-muted/50 rounded-lg p-4 text-center">
                  <div className="text-3xl font-bold text-primary mb-1">{dueWords}</div>
                  <div className="text-sm text-muted-foreground">Từ vựng ôn lại</div>
                </div>
              </div>
            </Card>

            {/* Practice Mode Cards */}
            <div className="space-y-4">
              <div className="flex items-center gap-3 mb-4">
                <span className="text-xl">🎓</span>
                <h2 className="text-xl font-bold">Chế độ luyện tập</h2>
              </div>
              <div className="grid grid-cols-2 gap-4">
                {practiceMode.map((mode, idx) => (
                  <Card
                    key={idx}
                    className={`${mode.color} text-white p-6 cursor-pointer transition-all hover:scale-105`}
                    onClick={() => navigate(mode.path)}
                  >
                    <div className="text-center">
                      {typeof mode.icon === "string" ? (
                        <div className="text-4xl mb-3">{mode.icon}</div>
                      ) : (
                        <mode.icon className="w-12 h-12 mx-auto mb-3" />
                      )}
                      <h3 className="font-bold text-lg mb-2">{mode.name}</h3>
                      <p className="text-sm text-white/90 leading-snug">
                        {mode.description}
                      </p>
                    </div>
                  </Card>
                ))}
              </div>
            </div>
          </div>

          {/* Practice Now Card */}
          <Card className="bg-gradient-to-r from-red-600 to-red-700 text-white p-8 mb-8 text-center shadow-lg">
            <div className="text-5xl font-bold mb-2">{dueWords}</div>
            <p className="text-white/90 mb-4">từ vựng đến hạn ôn tập lại hôm nay</p>
            <Button 
              className="bg-white text-red-600 hover:bg-white/90 font-bold"
              onClick={() => navigate(`/flashcard-practice?topic=${encodeURIComponent(topicName)}`)}
            >
              ▶ Luyện tập ngay
            </Button>
          </Card>

          {/* Information Section */}
          <div>
            <div className="flex items-center gap-3 mb-4">
              <span className="text-xl">ℹ️</span>
              <h2 className="text-xl font-bold">Thông tin về Spaced Repetition</h2>
            </div>

            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              <Card className="p-6">
                <h3 className="font-bold text-blue-600 mb-2">Spaced Repetition là gì?</h3>
                <p className="text-sm text-muted-foreground">
                  Spaced Repetition (lặp lại ngắt quãng) là phương pháp học từ vựng dựa trên việc ôn tập lại những từ đã học với khoảng cách thời gian ngày càng xa.
                </p>
              </Card>

              <Card className="p-6">
                <h3 className="font-bold text-blue-600 mb-2">Cách thức hoạt động</h3>
                <p className="text-sm text-muted-foreground">
                  Hệ thống sẽ tự động lên lịch ôn tập cho từng từ vựng dựa trên mức độ ghi nhớ của bạn. Từ nào bạn nhớ tốt sẽ được ôn lại ít hơn.
                </p>
              </Card>

              <Card className="p-6">
                <h3 className="font-bold text-blue-600 mb-2">Các cấp độ học tập</h3>
                <p className="text-sm text-muted-foreground">
                  Mới học → Nhớ tạm thời → Nhớ lâu → Thuộc. Mỗi từ sẽ trải qua các giai đoạn này để đảm bảo ghi nhớ lâu dài.
                </p>
              </Card>
            </div>
          </div>

          <Button 
            className="mt-6 bg-green-600 hover:bg-green-700 text-white"
            onClick={() => {}}
          >
            <MessageCircle className="w-4 h-4 mr-2" />
            💬 Góp ý
          </Button>
        </div>
      </div>
    </div>
  );
};

export default VocabularyGroup;
