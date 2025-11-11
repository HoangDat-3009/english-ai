import { Link } from "react-router-dom";
import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Headphones, MessageCircle, Plus, BookOpen, FileText, BookMarked } from "lucide-react";

const Index = () => {
  return (
    <div className="min-h-screen bg-gradient-to-br from-purple/10 via-primary/10 to-info/10">
      <div className="container mx-auto px-6 py-12">
        <div className="text-center mb-12">
          <h1 className="text-5xl font-bold mb-4 bg-gradient-to-r from-purple to-primary bg-clip-text text-transparent">
            Hệ thống Luyện Nghe & Nói
          </h1>
          <p className="text-xl text-muted-foreground">
            Nâng cao kỹ năng ngôn ngữ của bạn với AI
          </p>
        </div>

        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 max-w-6xl mx-auto">
          <Link to="/listening-practice" className="group">
            <Card className="p-8 h-full hover:shadow-xl transition-all hover:-translate-y-1 cursor-pointer bg-gradient-to-br from-primary/5 to-primary/10 border-primary/20">
              <div className="flex flex-col items-center text-center space-y-4">
                <div className="p-4 bg-primary/10 rounded-full group-hover:bg-primary/20 transition-colors">
                  <Headphones className="w-12 h-12 text-primary" />
                </div>
                <h2 className="text-2xl font-bold">Luyện Nghe</h2>
                <p className="text-muted-foreground">
                  Thực hành nghe với audio, transcript và dịch nghĩa
                </p>
              </div>
            </Card>
          </Link>

          <Link to="/flashcard-groups" className="group">
            <Card className="p-8 h-full hover:shadow-xl transition-all hover:-translate-y-1 cursor-pointer bg-gradient-to-br from-success/5 to-success/10 border-success/20">
              <div className="flex flex-col items-center text-center space-y-4">
                <div className="p-4 bg-success/10 rounded-full group-hover:bg-success/20 transition-colors">
                  <BookOpen className="w-12 h-12 text-success" />
                </div>
                <h2 className="text-2xl font-bold">Flashcards Từ Vựng</h2>
                <p className="text-muted-foreground">
                  Học từ vựng với hệ thống flashcard thông minh
                </p>
              </div>
            </Card>
          </Link>

          <Link to="/create-lesson" className="group">
            <Card className="p-8 h-full hover:shadow-xl transition-all hover:-translate-y-1 cursor-pointer bg-gradient-to-br from-accent/5 to-accent/10 border-accent/20">
              <div className="flex flex-col items-center text-center space-y-4">
                <div className="p-4 bg-accent/10 rounded-full group-hover:bg-accent/20 transition-colors">
                  <Plus className="w-12 h-12 text-accent" />
                </div>
                <h2 className="text-2xl font-bold">Luyện Dịch Câu</h2>
                <p className="text-muted-foreground">
                  Thực hành dịch câu và cải thiện kỹ năng ngôn ngữ
                </p>
              </div>
            </Card>
          </Link>

          <Link to="/conversation-practice" className="group">
            <Card className="p-8 h-full hover:shadow-xl transition-all hover:-translate-y-1 cursor-pointer bg-gradient-to-br from-purple/5 to-purple/10 border-purple/20">
              <div className="flex flex-col items-center text-center space-y-4">
                <div className="p-4 bg-purple/10 rounded-full group-hover:bg-purple/20 transition-colors">
                  <MessageCircle className="w-12 h-12 text-purple" />
                </div>
                <h2 className="text-2xl font-bold">Luyện Hội Thoại</h2>
                <p className="text-muted-foreground">
                  Thực hành đối thoại với trợ lý AI thông minh
                </p>
              </div>
            </Card>
          </Link>

          <Link to="/test-list" className="group">
            <Card className="p-8 h-full hover:shadow-xl transition-all hover:-translate-y-1 cursor-pointer bg-gradient-to-br from-warning/5 to-warning/10 border-warning/20">
              <div className="flex flex-col items-center text-center space-y-4">
                <div className="p-4 bg-warning/10 rounded-full group-hover:bg-warning/20 transition-colors">
                  <FileText className="w-12 h-12 text-warning" />
                </div>
                <h2 className="text-2xl font-bold">Luyện Đề</h2>
                <p className="text-muted-foreground">
                  Luyện tập với các đề thi TOEIC thật
                </p>
              </div>
            </Card>
          </Link>

        </div>

        <div className="mt-16 text-center">
          <Card className="inline-block p-8 bg-gradient-to-r from-primary/10 to-purple/10">
            <h3 className="text-2xl font-bold mb-4">✨ Tính năng nổi bật</h3>
            <div className="grid grid-cols-1 md:grid-cols-3 gap-6 text-left">
              <div>
                <h4 className="font-semibold mb-2">🎧 Luyện nghe tương tác</h4>
                <p className="text-sm text-muted-foreground">
                  Audio chất lượng cao với transcript và phím tắt tiện lợi
                </p>
              </div>
              <div>
                <h4 className="font-semibold mb-2">🤖 AI thông minh</h4>
                <p className="text-sm text-muted-foreground">
                  Gợi ý từ vựng, cấu trúc câu và phản hồi theo thời gian thực
                </p>
              </div>
              <div>
                <h4 className="font-semibold mb-2">📊 Theo dõi tiến độ</h4>
                <p className="text-sm text-muted-foreground">
                  Hệ thống điểm và cấp độ giúp đo lường sự tiến bộ
                </p>
              </div>
            </div>
          </Card>
        </div>
      </div>
    </div>
  );
};

export default Index;