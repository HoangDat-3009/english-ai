import { Layers } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { useNavigate } from "react-router-dom";
import { useFlashcardGroups } from "@/hooks/useFlashcardGroups";
import { Badge } from "@/components/ui/badge";
import { Header } from "@/components/Header";

const FlashcardGroups = () => {
  const navigate = useNavigate();
  const { groups, loading, error } = useFlashcardGroups();

  if (loading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Đang tải nhóm từ vựng...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <Card className="p-8 max-w-md">
          <h2 className="text-xl font-bold text-red-600 mb-2">Không thể tải dữ liệu</h2>
          <p className="text-muted-foreground mb-4">{error}</p>
          <Button onClick={() => navigate("/")}>
            Quay về trang chủ
          </Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-background">
      <Header />
      {/* Hero Section */}
      <div className="bg-gradient-to-r from-primary to-purple text-white py-12 px-6">
        <div className="max-w-6xl mx-auto">
          <h1 className="text-4xl font-bold mb-2">Flashcards Từ Vựng</h1>
          <p className="text-white/90">
            Chọn nhóm từ vựng để bắt đầu luyện tập
          </p>
        </div>
      </div>

      {/* Groups Grid */}
      <div className="max-w-6xl mx-auto px-6 py-8">
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
          {groups.map((group) => (
            <Card key={group.id} className="p-6 hover:shadow-lg transition-shadow">
              <div className="flex items-start justify-between mb-4">
                <Badge className="bg-teal-500 hover:bg-teal-600">
                  {group.type}
                </Badge>
                <Layers className="w-5 h-5 text-muted-foreground" />
              </div>
              
              <h3 className="text-xl font-bold mb-3">{group.name}</h3>
              
              <div className="flex items-center justify-between mb-4">
                <div className="flex items-center gap-2">
                  <Badge variant="destructive" className="text-sm">
                    {group.flashcardCount} TỪ
                  </Badge>
                  <Badge variant="outline" className="text-sm bg-green-50 text-green-700 border-green-200">
                    0 ĐÃ HỌC
                  </Badge>
                  <Badge variant="outline" className="text-sm bg-yellow-50 text-yellow-700 border-yellow-200">
                    0 ĐẾN HẠN ÔN
                  </Badge>
                </div>
              </div>

              <div className="flex gap-2">
                <Button
                  variant="outline"
                  size="sm"
                  className="flex-1"
                  onClick={() => navigate(`/flashcard-selection?groupId=${group.id}&groupName=${encodeURIComponent(group.name)}&totalWords=${group.flashcardCount}`)}
                >
                  Chi tiết
                </Button>
                <Button
                  size="sm"
                  className="flex-1 bg-indigo-600 hover:bg-indigo-700"
                  onClick={() => navigate(`/flashcard-selection?groupId=${group.id}&groupName=${encodeURIComponent(group.name)}&totalWords=${group.flashcardCount}`)}
                >
                  ▶ Luyện tập
                </Button>
              </div>
            </Card>
          ))}
        </div>
      </div>
    </div>
  );
};

export default FlashcardGroups;
