import { useState, useEffect } from "react";
import { Headphones, Globe, BookOpen, Star as StarIcon, Play } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { useNavigate } from "react-router-dom";
import { LessonListItem } from "@/types/listening";
import { Header } from "@/components/Header";

const ListeningPractice = () => {
  const navigate = useNavigate();
  const [selectedLanguage, setSelectedLanguage] = useState("english");
  const [selectedTopic, setSelectedTopic] = useState("all");
  const [selectedSection, setSelectedSection] = useState("all");
  const [selectedLevel, setSelectedLevel] = useState("all");
  const [lessons, setLessons] = useState<LessonListItem[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchLessons = async () => {
      try {
        const response = await fetch("https://dethi-40sj.onrender.com/api/Listening/lessons");
        if (!response.ok) {
          throw new Error("Failed to fetch lessons");
        }
        const data: LessonListItem[] = await response.json();
        setLessons(data);
      } catch (error) {
        console.error("Error fetching lessons:", error);
      } finally {
        setLoading(false);
      }
    };

    fetchLessons();
  }, []);

  return (
    <div className="min-h-screen bg-background">
      <Header />
      {/* Page Header */}
      <div className="border-b px-6 py-6 bg-gradient-to-r from-primary/5 to-purple/5">
        <div className="max-w-7xl mx-auto">
          <div className="flex items-center gap-3 mb-2">
            <Headphones className="w-6 h-6 text-primary" />
            <h1 className="text-2xl font-bold">Luyện nghe</h1>
          </div>
          <p className="text-sm text-muted-foreground">
            Luyện nghe với audio/video, transcript, giải thích, dịch nghĩa.
          </p>
        </div>
      </div>

      <div className="px-6 py-6">
        <div className="max-w-7xl mx-auto">
          {/* Filters */}
          <Card className="p-6 mb-6">
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
              {/* Language Filter */}
              <div>
                <label className="flex items-center gap-2 text-sm font-medium mb-2">
                  <Globe className="w-4 h-4" />
                  NGÔN NGỮ
                </label>
                <Select value={selectedLanguage} onValueChange={setSelectedLanguage}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Chọn ngôn ngữ" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="english">🇬🇧 Tiếng Anh</SelectItem>
                    <SelectItem value="french">🇫🇷 Tiếng Pháp</SelectItem>
                    <SelectItem value="spanish">🇪🇸 Tiếng Tây Ban Nha</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* Topic Filter */}
              <div>
                <label className="flex items-center gap-2 text-sm font-medium mb-2">
                  <BookOpen className="w-4 h-4" />
                  CHỦ ĐỀ
                </label>
                <Select value={selectedTopic} onValueChange={setSelectedTopic}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Chọn chủ đề" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">Short Stories</SelectItem>
                    <SelectItem value="daily">Giao tiếp hàng ngày</SelectItem>
                    <SelectItem value="travel">Du lịch</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* Section Filter */}
              <div>
                <label className="flex items-center gap-2 text-sm font-medium mb-2">
                  <span className="w-4 h-4">📑</span>
                  PHẦN
                </label>
                <Select value={selectedSection} onValueChange={setSelectedSection}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Chọn phần" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">Tất cả phần</SelectItem>
                    <SelectItem value="part1">Phần 1</SelectItem>
                    <SelectItem value="part2">Phần 2</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              {/* Level Filter */}
              <div>
                <label className="flex items-center gap-2 text-sm font-medium mb-2">
                  <StarIcon className="w-4 h-4" />
                  CẤP ĐỘ
                </label>
                <Select value={selectedLevel} onValueChange={setSelectedLevel}>
                  <SelectTrigger className="w-full">
                    <SelectValue placeholder="Chọn cấp độ" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="all">Tất cả cấp độ</SelectItem>
                    <SelectItem value="a1">A1</SelectItem>
                    <SelectItem value="a2">A2</SelectItem>
                    <SelectItem value="b1">B1</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>
          </Card>

          {/* Lessons Grid */}
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
            {loading ? (
              <div className="col-span-full text-center py-8">
                <p className="text-muted-foreground">Đang tải bài học...</p>
              </div>
            ) : lessons.length === 0 ? (
              <div className="col-span-full text-center py-8">
                <p className="text-muted-foreground">Không có bài học nào</p>
              </div>
            ) : (
              lessons.map((lesson) => (
                <Card key={lesson.lessonId} className="p-5 hover:shadow-lg transition-shadow">
                  <div className="space-y-4">
                    <div>
                      <div className="flex items-start justify-between mb-2">
                        <h3 className="font-semibold text-primary">
                          {lesson.lessonId}. {lesson.title}
                        </h3>
                        <Badge className="bg-success text-success-foreground text-xs">
                          {lesson.totalQuestions} CÂU
                        </Badge>
                      </div>
                      
                      <div className="flex items-center gap-2 mb-3">
                        <Badge className="bg-destructive text-destructive-foreground">
                          ⭐ {lesson.level}
                        </Badge>
                        <div className="flex items-center gap-1 text-xs text-muted-foreground">
                          <StarIcon className="w-3 h-3 fill-warning text-warning" />
                          <span>Chưa luyện tập</span>
                        </div>
                      </div>
                    </div>

                    <Button 
                      className="w-full bg-primary hover:bg-primary/90 text-primary-foreground"
                      onClick={() => navigate(`/listening-practice-lesson?lessonId=${lesson.lessonId}`)}
                    >
                      <Play className="w-4 h-4 mr-2" />
                      Bắt đầu luyện nghe
                    </Button>
                  </div>
                </Card>
              ))
            )}
          </div>

          {/* Pagination */}
          {!loading && lessons.length > 0 && (
            <div className="flex items-center justify-between">
              <p className="text-sm text-muted-foreground">
                Hiển thị 1 - {lessons.length} của {lessons.length} bài học
              </p>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ListeningPractice;