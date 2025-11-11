import { useState } from "react";
import { useNavigate } from "react-router-dom";
import { BookOpen, GraduationCap, Clock, CheckCircle, Crown, Search, X } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useVocabularyTopics } from "@/hooks/useVocabularyTopics";
import { Header } from "@/components/Header";

const VocabularyTopics = () => {
  const navigate = useNavigate();
  const { topics, loading } = useVocabularyTopics();
  const [searchQuery, setSearchQuery] = useState("");
  const [selectedLanguage, setSelectedLanguage] = useState("us");
  const [selectedGroup, setSelectedGroup] = useState("all");

  const totalStats = {
    total: topics.reduce((sum, topic) => sum + topic.totalWords, 0),
    studying: topics.reduce((sum, topic) => sum + topic.studyingWords, 0),
    due: topics.reduce((sum, topic) => sum + topic.dueWords, 0),
    mastered: topics.reduce((sum, topic) => sum + topic.masteredWords, 0)
  };

  const filteredTopics = topics.filter(topic => 
    topic.name.toLowerCase().includes(searchQuery.toLowerCase())
  );

  return (
    <div className="min-h-screen bg-background">
      <Header />
      {/* Page Header */}
      <div className="bg-gradient-to-r from-primary to-purple text-white py-12 px-6">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-4xl font-bold mb-3">📚 Từ vựng mặc định</h1>
          <p className="text-white/90 text-lg">
            Luyện tập từ vựng với dữ liệu được chọn lọc kỹ càng
          </p>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="max-w-7xl mx-auto px-6 -mt-8 mb-8">
        <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
          <Card className="p-6 text-center bg-card">
            <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-blue-500/10 mb-3">
              <BookOpen className="w-6 h-6 text-blue-500" />
            </div>
            <div className="text-3xl font-bold text-blue-500 mb-1">{totalStats.total}</div>
            <div className="text-sm text-muted-foreground">Tổng số từ</div>
          </Card>
          
          <Card className="p-6 text-center bg-card">
            <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-cyan-500/10 mb-3">
              <GraduationCap className="w-6 h-6 text-cyan-500" />
            </div>
            <div className="text-3xl font-bold text-cyan-500 mb-1">{totalStats.studying}</div>
            <div className="text-sm text-muted-foreground">Đang học</div>
          </Card>
          
          <Card className="p-6 text-center bg-card">
            <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-orange-500/10 mb-3">
              <Clock className="w-6 h-6 text-orange-500" />
            </div>
            <div className="text-3xl font-bold text-orange-500 mb-1">{totalStats.due}</div>
            <div className="text-sm text-muted-foreground">Đến hạn ôn tập</div>
          </Card>
          
          <Card className="p-6 text-center bg-card">
            <div className="inline-flex items-center justify-center w-12 h-12 rounded-full bg-green-500/10 mb-3">
              <CheckCircle className="w-6 h-6 text-green-500" />
            </div>
            <div className="text-3xl font-bold text-green-500 mb-1">{totalStats.mastered}</div>
            <div className="text-sm text-muted-foreground">Đã thuộc</div>
          </Card>
        </div>
      </div>

      {/* Filters */}
      <div className="max-w-7xl mx-auto px-6 mb-8">
        <Card className="p-6 bg-gradient-to-r from-blue-500/10 to-purple-500/10">
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div>
              <label className="text-sm font-medium mb-2 block">Ngôn ngữ</label>
              <Select value={selectedLanguage} onValueChange={setSelectedLanguage}>
                <SelectTrigger className="bg-background">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="us">🇺🇸 Tiếng Anh</SelectItem>
                  <SelectItem value="jp">🇯🇵 Tiếng Nhật</SelectItem>
                  <SelectItem value="kr">🇰🇷 Tiếng Hàn</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="text-sm font-medium mb-2 block">Nhóm danh mục</label>
              <Select value={selectedGroup} onValueChange={setSelectedGroup}>
                <SelectTrigger className="bg-background">
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả nhóm</SelectItem>
                  <SelectItem value="cefr">Từ vựng CEFR</SelectItem>
                  <SelectItem value="common">Từ vựng thông dụng</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="text-sm font-medium mb-2 block">Tìm kiếm</label>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 -translate-y-1/2 w-4 h-4 text-muted-foreground" />
                <Input
                  placeholder="Tìm kiếm nhóm từ vựng..."
                  value={searchQuery}
                  onChange={(e) => setSearchQuery(e.target.value)}
                  className="pl-10 pr-10 bg-background"
                />
                {searchQuery && (
                  <button
                    onClick={() => setSearchQuery("")}
                    className="absolute right-3 top-1/2 -translate-y-1/2"
                  >
                    <X className="w-4 h-4 text-muted-foreground hover:text-foreground" />
                  </button>
                )}
              </div>
            </div>
          </div>
        </Card>
      </div>

      {/* Topics List */}
      <div className="max-w-7xl mx-auto px-6 pb-12">
        <div className="flex items-center gap-2 mb-6">
          <Crown className="w-5 h-5 text-primary" />
          <h2 className="text-2xl font-bold">Nhóm từ vựng</h2>
        </div>

        {loading ? (
          <div className="text-center py-12">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto"></div>
            <p className="text-muted-foreground mt-4">Đang tải...</p>
          </div>
        ) : (
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
            {filteredTopics.map((topic) => (
              <Card key={topic.id} className="p-6 hover:shadow-lg transition-all">
                <div className="mb-4">
                  <span className="inline-block px-3 py-1 text-xs font-medium rounded-full bg-green-500/10 text-green-700 dark:text-green-400">
                    {topic.category}
                  </span>
                </div>
                
                <h3 className="font-bold text-lg mb-4">{topic.name}</h3>
                
                <div className="space-y-2 mb-4">
                  <div className="flex justify-between text-sm">
                    <span className="inline-flex items-center gap-1.5">
                      <span className="w-2 h-2 rounded-full bg-red-500"></span>
                      <span className="font-medium">{topic.totalWords} TỪ</span>
                    </span>
                    <span className="inline-flex items-center gap-1.5">
                      <span className="w-2 h-2 rounded-full bg-green-500"></span>
                      <span className="font-medium">{topic.studyingWords} ĐÃ HỌC</span>
                    </span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="inline-flex items-center gap-1.5">
                      <span className="w-2 h-2 rounded-full bg-yellow-500"></span>
                      <span className="font-medium">{topic.dueWords} ĐẾN HẠN ÔN</span>
                    </span>
                  </div>
                </div>

                <div className="flex gap-2">
                  <Button 
                    variant="outline" 
                    size="sm" 
                    className="flex-1"
                    onClick={() => {/* View details */}}
                  >
                    👁️ Chi tiết
                  </Button>
                  <Button 
                    size="sm" 
                    className="flex-1 bg-blue-600 hover:bg-blue-700"
                    onClick={() => navigate(`/vocabulary-group/${encodeURIComponent(topic.id)}`)}
                  >
                    ▶ Luyện tập
                  </Button>
                </div>
              </Card>
            ))}
          </div>
        )}
      </div>
    </div>
  );
};

export default VocabularyTopics;
