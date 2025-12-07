import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import Header from '@/components/Navbar';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { ArrowLeft, FileText, Clock, Target, BookOpen, AlertCircle, Loader2 } from 'lucide-react';
import { toast } from 'sonner';
import writingExerciseService, { WritingExercise } from '@/services/writingExerciseService';

const WritingEssayLibrary = () => {
  const navigate = useNavigate();
  const [exercises, setExercises] = useState<WritingExercise[]>([]);
  const [loading, setLoading] = useState(true);
  const [filterLevel, setFilterLevel] = useState<string>('all');

  useEffect(() => {
    loadExercises();
  }, []);

  const loadExercises = async () => {
    try {
      setLoading(true);
      const data = await writingExerciseService.getWritingExercises('writing_essay');
      // Only show active exercises
      const activeExercises = data.filter(ex => ex.isActive);
      setExercises(activeExercises);
    } catch (error) {
      console.error('Error loading exercises:', error);
      toast.error('Không thể tải danh sách bài tập');
    } finally {
      setLoading(false);
    }
  };

  const filteredExercises = filterLevel === 'all' 
    ? exercises 
    : exercises.filter(ex => ex.level && ex.level === filterLevel);

  const handleStartExercise = (exerciseId: number) => {
    navigate(`/writing-practice/${exerciseId}`);
  };

  return (
    <div className="min-h-screen bg-gradient-soft">
      <Header />
      
      <main className="container mx-auto px-4 py-8 max-w-6xl">
        <Button
          variant="ghost"
          onClick={() => navigate("/writing-mode")}
          className="mb-6"
        >
          <ArrowLeft className="w-4 h-4 mr-2" />
          Quay lại chọn chế độ
        </Button>

        <div className="text-center space-y-4 py-8 mb-8">
          <div className="inline-flex items-center justify-center w-16 h-16 rounded-2xl bg-green-600 shadow-soft mb-4">
            <FileText className="w-8 h-8 text-white" />
          </div>
          <h1 className="text-4xl font-bold text-gray-900 dark:text-white">
            BÀI TẬP VIẾT ĐOẠN VĂN CÓ SẴN
          </h1>
          <p className="text-lg text-muted-foreground max-w-2xl mx-auto">
            Luyện viết với các bài tập được thiết kế sẵn bởi giáo viên. 
            Không cần kết nối AI, luôn sẵn sàng để bạn luyện tập.
          </p>
        </div>

        {/* Filter by level */}
        <Card className="mb-6">
          <CardContent className="pt-6">
            <div className="flex items-center gap-4 flex-wrap">
              <span className="font-medium">Lọc theo trình độ:</span>
              <div className="flex gap-2">
                <Button
                  variant={filterLevel === 'all' ? 'default' : 'outline'}
                  size="sm"
                  onClick={() => setFilterLevel('all')}
                >
                  Tất cả
                </Button>
                {['A1', 'A2', 'B1', 'B2', 'C1'].map((level) => (
                  <Button
                    key={level}
                    variant={filterLevel === level ? 'default' : 'outline'}
                    size="sm"
                    onClick={() => setFilterLevel(level)}
                  >
                    {level}
                  </Button>
                ))}
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Loading state */}
        {loading ? (
          <Card>
            <CardContent className="pt-6">
              <div className="flex items-center justify-center py-12">
                <Loader2 className="w-8 h-8 animate-spin text-primary" />
                <span className="ml-3 text-muted-foreground">Đang tải bài tập...</span>
              </div>
            </CardContent>
          </Card>
        ) : filteredExercises.length === 0 ? (
          <Card>
            <CardContent className="pt-6">
              <Alert>
                <AlertCircle className="h-4 w-4" />
                <AlertDescription>
                  {filterLevel === 'all' 
                    ? 'Chưa có bài tập nào. Giáo viên sẽ sớm thêm bài tập mới!'
                    : `Chưa có bài tập nào cho trình độ ${filterLevel}`
                  }
                </AlertDescription>
              </Alert>
            </CardContent>
          </Card>
        ) : (
          /* Exercise grid */
          <div className="grid md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredExercises.map((exercise) => (
              <Card 
                key={exercise.id} 
                className="hover:shadow-lg transition-all hover:border-green-500 cursor-pointer group"
                onClick={() => handleStartExercise(exercise.id)}
              >
                <CardHeader>
                  <div className="flex items-start justify-between mb-2">
                    <div className="w-10 h-10 rounded-lg bg-green-100 dark:bg-green-900/30 flex items-center justify-center group-hover:scale-110 transition-transform">
                      <FileText className="w-5 h-5 text-green-600 dark:text-green-400" />
                    </div>
                    {exercise.level && (
                      <Badge variant="secondary">
                        {exercise.level}
                      </Badge>
                    )}
                  </div>
                  <CardTitle className="text-xl line-clamp-2">
                    {exercise.title}
                  </CardTitle>
                </CardHeader>
                <CardContent className="space-y-4">
                  {exercise.description && (
                    <p className="text-sm text-muted-foreground line-clamp-3">
                      {exercise.description}
                    </p>
                  )}

                  <div className="space-y-2 text-sm">
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <BookOpen className="w-4 h-4" />
                      <span>Chủ đề: {exercise.category || 'Tổng hợp'}</span>
                    </div>
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <Clock className="w-4 h-4" />
                      <span>Thời gian: {exercise.timeLimit || 30} phút</span>
                    </div>
                    <div className="flex items-center gap-2 text-muted-foreground">
                      <Target className="w-4 h-4" />
                      <span>Điểm tối đa: 100</span>
                    </div>
                  </div>

                  <Button className="w-full bg-green-600 hover:bg-green-700">
                    Bắt đầu làm bài
                  </Button>
                </CardContent>
              </Card>
            ))}
          </div>
        )}

        {/* Info card */}
        <Card className="mt-8 bg-blue-50 dark:bg-blue-950 border-blue-200 dark:border-blue-800">
          <CardContent className="pt-6">
            <div className="flex items-start gap-4">
              <div className="w-10 h-10 rounded-full bg-blue-600 flex items-center justify-center flex-shrink-0">
                <AlertCircle className="w-5 h-5 text-white" />
              </div>
              <div className="space-y-2">
                <h3 className="font-semibold text-blue-900 dark:text-blue-100">
                  Lưu ý khi làm bài
                </h3>
                <ul className="text-sm text-blue-800 dark:text-blue-200 space-y-1 list-disc list-inside">
                  <li>Đọc kỹ đề bài và yêu cầu trước khi viết</li>
                  <li>Chú ý thời gian làm bài để hoàn thành đúng hạn</li>
                  <li>Viết đoạn văn mạch lạc, có bố cục rõ ràng</li>
                  <li>Kiểm tra lỗi chính tả và ngữ pháp trước khi nộp bài</li>
                </ul>
              </div>
            </div>
          </CardContent>
        </Card>
      </main>
    </div>
  );
};

export default WritingEssayLibrary;
