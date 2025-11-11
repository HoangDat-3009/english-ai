import { useNavigate, useSearchParams } from "react-router-dom";
import { ArrowLeft, Layers, ClipboardCheck, Keyboard, MessageSquare } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";

const FlashcardPracticeSelection = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const groupId = searchParams.get("groupId");
  const groupName = searchParams.get("groupName") || "Từ vựng";
  const totalWords = searchParams.get("totalWords") || "0";

  const practiceOptions = [
    {
      title: "Flashcard",
      description: "Luyện tập với thẻ ghi nhớ, lật thẻ để xem nghĩa và ví dụ",
      icon: Layers,
      color: "from-green-500 to-emerald-500",
      path: `/flashcard-practice?groupId=${groupId}&groupName=${encodeURIComponent(groupName)}`,
    },
    {
      title: "Kiểm tra",
      description: "Trắc nghiệm và điền từ để kiểm tra kiến thức từ vựng",
      icon: ClipboardCheck,
      color: "from-blue-500 to-cyan-500",
      path: `/vocabulary-test?groupId=${groupId}&groupName=${encodeURIComponent(groupName)}`,
    },
    {
      title: "Đoán và Gõ Từ",
      description: "Nghe audio, đoán và gõ lại từ vựng chính xác",
      icon: Keyboard,
      color: "from-purple-500 to-pink-500",
      path: `/guess-word?groupId=${groupId}&groupName=${encodeURIComponent(groupName)}`,
    },
    {
      title: "Luyện câu",
      description: "Tạo câu từ từ vựng và nhận phản hồi từ AI",
      icon: MessageSquare,
      color: "from-orange-500 to-amber-500",
      path: `/sentence-practice?groupId=${groupId}&groupName=${encodeURIComponent(groupName)}`,
    },
  ];

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-gray-900 dark:to-gray-800">
      {/* Header */}
      <div className="bg-gradient-to-r from-blue-600 to-indigo-600 text-white py-12 px-6">
        <div className="max-w-4xl mx-auto">
          <Button 
            variant="ghost" 
            className="mb-4 text-white hover:bg-white/20"
            onClick={() => navigate(-1)}
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>
          <h1 className="text-4xl font-bold mb-2">Chọn chế độ luyện tập từ vựng</h1>
          <p className="text-blue-100">
            Luyện tập từ vựng với phương pháp Spaced Repetition (lặp lại ngắt quãng) để ghi nhớ từ vựng hiệu quả
          </p>
        </div>
      </div>

      {/* Stats Section */}
      <div className="max-w-4xl mx-auto px-6 py-8">
        <div className="mb-8">
          <h2 className="text-2xl font-bold mb-4">📊 Thống kê luyện tập</h2>
          <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
            <Card className="p-6 text-center">
              <div className="text-3xl font-bold text-primary mb-2">{totalWords}</div>
              <div className="text-sm text-muted-foreground">Tổng số từ</div>
            </Card>
            <Card className="p-6 text-center">
              <div className="text-3xl font-bold text-green-600 mb-2">0</div>
              <div className="text-sm text-muted-foreground">Từ vựng thành thạo</div>
            </Card>
            <Card className="p-6 text-center">
              <div className="text-3xl font-bold text-blue-600 mb-2">0</div>
              <div className="text-sm text-muted-foreground">Phiên luyện tập</div>
            </Card>
            <Card className="p-6 text-center">
              <div className="text-3xl font-bold text-orange-600 mb-2">0</div>
              <div className="text-sm text-muted-foreground">Từ vựng ôn lại</div>
            </Card>
          </div>
        </div>

        {/* Reminder Card */}
        <Card className="bg-red-50 dark:bg-red-950/20 border-red-200 dark:border-red-800 p-6 mb-8">
          <div className="text-center">
            <div className="text-4xl font-bold text-red-600 mb-2">0</div>
            <div className="text-sm text-red-800 dark:text-red-200">
              từ vựng đến hạn ôn tập lại hôm nay
            </div>
          </div>
        </Card>

        {/* Practice Options */}
        <div>
          <div className="flex items-center justify-between mb-6">
            <h2 className="text-2xl font-bold">🎓 Chế độ luyện tập</h2>
            <Button className="bg-green-600 hover:bg-green-700">
              💬 Góp ý
            </Button>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
            {practiceOptions.map((option) => {
              const Icon = option.icon;
              return (
                <Card
                  key={option.title}
                  className="group cursor-pointer overflow-hidden transition-all hover:shadow-xl"
                  onClick={() => navigate(option.path)}
                >
                  <div className={`bg-gradient-to-br ${option.color} p-6 text-white`}>
                    <Icon className="w-12 h-12 mb-4" />
                    <h3 className="text-2xl font-bold mb-2">{option.title}</h3>
                  </div>
                  <div className="p-6">
                    <p className="text-muted-foreground">{option.description}</p>
                  </div>
                </Card>
              );
            })}
          </div>
        </div>
      </div>
    </div>
  );
};

export default FlashcardPracticeSelection;
