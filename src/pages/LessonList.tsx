import { useState } from "react";
import { Keyboard, Globe, BookOpen, Layers, Star as StarIcon } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";

const lessons = [
  { id: 1, title: "First snowfall", sentences: 21, level: "A1", practiced: false },
  { id: 2, title: "Jessica's first day of school", sentences: 25, level: "A1", practiced: false },
  { id: 3, title: "My flower garden", sentences: 19, level: "A1", practiced: false },
  { id: 4, title: "Going camping", sentences: 22, level: "A1", practiced: false },
  { id: 5, title: "My house", sentences: 21, level: "A1", practiced: false },
  { id: 6, title: "My first pet", sentences: 21, level: "A1", practiced: false },
  { id: 7, title: "Jennifer the firefighter", sentences: 17, level: "A1", practiced: false },
  { id: 8, title: "Mark's big game", sentences: 20, level: "A1", practiced: false },
];

const LessonList = () => {
  const [selectedLesson, setSelectedLesson] = useState<number | null>(2);

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="border-b px-6 py-4 flex items-center justify-between">
        <div>
          <div className="flex items-center gap-2 mb-1">
            <Keyboard className="w-5 h-5" />
            <h1 className="text-xl font-bold">Luyện nghe</h1>
          </div>
          <p className="text-sm text-muted-foreground">
            Luyện nghe với audio/video, transcript, giải thích, dịch nghĩa.
          </p>
        </div>
        <Button className="bg-warning hover:bg-warning/90 text-warning-foreground">
          Góp ý
        </Button>
      </header>

      <div className="px-6 py-6">
        {/* Filters */}
        <Card className="p-6 mb-6">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div>
              <label className="flex items-center gap-2 font-semibold mb-2 text-sm">
                <Globe className="w-4 h-4" />
                NGÔN NGỮ
              </label>
              <Select defaultValue="en">
                <SelectTrigger>
                  <SelectValue placeholder="Chọn ngôn ngữ" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="en">🇬🇧 Tiếng Anh</SelectItem>
                  <SelectItem value="vi">🇻🇳 Tiếng Việt</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="flex items-center gap-2 font-semibold mb-2 text-sm">
                <BookOpen className="w-4 h-4" />
                CHỦ ĐỀ
              </label>
              <Select defaultValue="short">
                <SelectTrigger>
                  <SelectValue placeholder="Chọn chủ đề" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="short">Short Stories</SelectItem>
                  <SelectItem value="news">News</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="flex items-center gap-2 font-semibold mb-2 text-sm">
                <Layers className="w-4 h-4" />
                PHẦN
              </label>
              <Select defaultValue="all">
                <SelectTrigger>
                  <SelectValue placeholder="Chọn phần" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả phần</SelectItem>
                  <SelectItem value="1">Phần 1</SelectItem>
                </SelectContent>
              </Select>
            </div>

            <div>
              <label className="flex items-center gap-2 font-semibold mb-2 text-sm">
                <StarIcon className="w-4 h-4" />
                CẤP ĐỘ
              </label>
              <Select defaultValue="all">
                <SelectTrigger>
                  <SelectValue placeholder="Chọn cấp độ" />
                </SelectTrigger>
                <SelectContent>
                  <SelectItem value="all">Tất cả cấp độ</SelectItem>
                  <SelectItem value="a1">A1</SelectItem>
                  <SelectItem value="a2">A2</SelectItem>
                </SelectContent>
              </Select>
            </div>
          </div>
        </Card>

        {/* Lesson Grid */}
        <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-6">
          {lessons.map((lesson) => (
            <Card
              key={lesson.id}
              className={`p-5 cursor-pointer transition-all hover:shadow-lg ${
                selectedLesson === lesson.id ? "ring-2 ring-primary" : ""
              }`}
              onClick={() => setSelectedLesson(lesson.id)}
            >
              <div className="flex items-start justify-between mb-3">
                <h3 className="font-semibold text-primary">
                  {lesson.id}. {lesson.title}
                </h3>
                <span className="bg-success text-success-foreground px-2 py-1 rounded text-xs font-bold whitespace-nowrap">
                  {lesson.sentences} CÂU
                </span>
              </div>

              <div className="flex items-center gap-2 mb-4">
                <span className="bg-destructive text-destructive-foreground px-2 py-1 rounded text-xs font-bold flex items-center gap-1">
                  ⭐ {lesson.level}
                </span>
                <div className="flex items-center gap-1 text-xs text-muted-foreground">
                  <StarIcon className="w-3 h-3 fill-yellow-400 text-yellow-400" />
                  <span>Chưa luyện tập</span>
                </div>
              </div>

              <Button className="w-full bg-primary hover:bg-primary/90 text-primary-foreground">
                ▶️ Bắt đầu luyện nghe
              </Button>
            </Card>
          ))}
        </div>

        {/* Pagination */}
        <div className="flex items-center justify-between">
          <p className="text-sm text-muted-foreground">
            Hiển thị 1 - 12 của 289 bài học
          </p>
          <div className="flex gap-2">
            <Button variant="outline" size="sm" disabled>
              &lt;
            </Button>
            <Button size="sm" className="bg-primary text-primary-foreground">
              1
            </Button>
            <Button variant="outline" size="sm">
              2
            </Button>
            <Button variant="outline" size="sm">
              ...
            </Button>
            <Button variant="outline" size="sm">
              25
            </Button>
            <Button variant="outline" size="sm">
              &gt;
            </Button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default LessonList;