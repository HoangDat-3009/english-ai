// 📚 READING EXERCISES PAGE - Hệ thống bài tập đọc hiểu TOEIC với AI
// ✅ READY FOR GIT: Hoàn thành cấu trúc TOEIC (Parts 5, 6, 7) với 7 bài tập đầy đủ
// 🤖 TODO BACKEND: Tích hợp Gemini AI service qua .NET API để tạo bài tự động  
// 📊 Features: TOEIC format, difficulty filtering, AI generation, admin upload
// 🎯 Mock Data: 7 complete exercises covering all parts & difficulty levels

import ReadingExerciseCard from "@/components/ReadingExerciseCard";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { useReadingExercises } from "@/hooks/useReadingExercises";
import { Database, Sparkles } from "lucide-react";
import { useState } from "react";

type Level = 'Beginner' | 'Intermediate' | 'Advanced';
type Type = 'Part 5' | 'Part 6' | 'Part 7';

const ReadingExercises = () => {
  const { exercises, isLoading, generateExercise, isGenerating } = useReadingExercises();
  const [selectedExercise, setSelectedExercise] = useState<string | null>(null);
  const [filterLevel, setFilterLevel] = useState<string>("all");
  const [filterSource, setFilterSource] = useState<string>("all");
  const [showGenerator, setShowGenerator] = useState(false);
  const [topic, setTopic] = useState("");
  const [level, setLevel] = useState<Level>("Intermediate");
  const [type, setType] = useState<Type>("Part 7");
  const filteredExercises = exercises.filter((exercise) => {
    const levelMatch = filterLevel === "all" || exercise.level === filterLevel;
    const sourceMatch = filterSource === "all" || exercise.sourceType === filterSource;
    return levelMatch && sourceMatch;
  });

  const handleGenerate = () => {
    if (!topic.trim()) return;
    // 🤖 TẠO BÀI BẰNG AI: Gọi hook useReadingExercises để tạo bài tập với Gemini AI
    // Luồng: Frontend -> useReadingExercises hook -> API call -> Backend controller -> Gemini service -> Database
    generateExercise({ topic, level, type });
    setTopic("");
    setShowGenerator(false);
  };

  if (selectedExercise) {
    const exercise = exercises.find((ex) => String(ex.id) === selectedExercise);
    if (!exercise) return null;

    return (
      <ReadingExerciseCard
        exercise={exercise}
        onBack={() => setSelectedExercise(null)}
      />
    );
  }

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between flex-wrap gap-4">
        <div>
          <h2 className="text-2xl font-bold mb-2 bg-gradient-accent bg-clip-text text-transparent">
            Choose Your Exercise
          </h2>
          <p className="text-muted-foreground">
            Practice with uploaded exercises or generate new ones with AI
          </p>
        </div>
        <div className="flex items-center gap-2 flex-wrap">
          <Select value={filterLevel} onValueChange={setFilterLevel}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Level" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Levels</SelectItem>
              <SelectItem value="Beginner">Beginner</SelectItem>
              <SelectItem value="Intermediate">Intermediate</SelectItem>
              <SelectItem value="Advanced">Advanced</SelectItem>
            </SelectContent>
          </Select>
          <Select value={filterSource} onValueChange={setFilterSource}>
            <SelectTrigger className="w-[140px]">
              <SelectValue placeholder="Source" />
            </SelectTrigger>
            <SelectContent>
              <SelectItem value="all">All Sources</SelectItem>
                <SelectItem value="manual">Admin Upload</SelectItem>
              <SelectItem value="ai">AI Generated</SelectItem>
            </SelectContent>
          </Select>
          <Button onClick={() => setShowGenerator(!showGenerator)} variant="default">
            <Sparkles className="h-4 w-4 mr-2" />
            Generate with AI
          </Button>
        </div>
      </div>

      {showGenerator && (
        <Card className="p-6 bg-gradient-pink border-2">
          {/* 🤖 FORM TẠO BÀI BẰNG AI: Form tạo bài tập với Gemini AI */}
          {/* Input: topic, level, type -> Output: Bài tập TOEIC với questions JSON */}
          <h3 className="font-semibold text-lg mb-4">
            <Sparkles className="h-5 w-5 inline mr-2" />
            Generate New Exercise with Gemini AI
          </h3>
          <p className="text-sm text-muted-foreground mb-4">
            Your backend (.NET API) will call Gemini AI to generate a personalized TOEIC exercise based on your input.
          </p>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <Input
              placeholder="Topic (e.g., business meeting, travel, etc.)"
              value={topic}
              onChange={(e) => setTopic(e.target.value)}
              className="col-span-1 md:col-span-2 lg:col-span-1"
            />
            <Select value={level} onValueChange={(v) => setLevel(v as Level)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Beginner">Beginner</SelectItem>
                <SelectItem value="Intermediate">Intermediate</SelectItem>
                <SelectItem value="Advanced">Advanced</SelectItem>
              </SelectContent>
            </Select>
            <Select value={type} onValueChange={(v) => setType(v as Type)}>
              <SelectTrigger>
                <SelectValue />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="Part 5">Part 5 - Grammar</SelectItem>
                <SelectItem value="Part 6">Part 6 - Text Completion</SelectItem>
                <SelectItem value="Part 7">Part 7 - Reading Comprehension</SelectItem>
              </SelectContent>
            </Select>
            <Button onClick={handleGenerate} disabled={isGenerating || !topic.trim()} className="w-full">
              {isGenerating ? (
                <>
                  <Sparkles className="h-4 w-4 mr-2 animate-spin" />
                  Generating with AI...
                </>
              ) : (
                <>
                  <Sparkles className="h-4 w-4 mr-2" />
                  Generate Exercise
                </>
              )}
            </Button>
          </div>
        </Card>
      )}

      {isLoading ? (
        <div className="text-center py-12">
          <p className="text-muted-foreground">Loading exercises...</p>
        </div>
      ) : (
        <>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            {filteredExercises.map((exercise, index) => (
              <Card
                key={exercise.id || `exercise-${index}`}
                className="p-6 cursor-pointer hover:shadow-elegant transition-all hover:-translate-y-1"
                onClick={() => setSelectedExercise(String(exercise.id || index))}
              >
                <div className="space-y-4">
                  <div className="flex items-start justify-between gap-2">
                    <h3 className="font-semibold text-lg">{exercise.name || 'Untitled Exercise'}</h3>
                    <div className="flex items-center gap-1">
                      {exercise.sourceType === 'ai' ? (
                        <>
                          <Sparkles className="h-4 w-4 text-primary flex-shrink-0" />
                          <span className="text-xs text-primary font-medium">AI Generated</span>
                        </>
                      ) : (
                        <>
                          <Database className="h-4 w-4 text-secondary flex-shrink-0" />
                          <span className="text-xs text-secondary font-medium">Admin Upload</span>
                        </>
                      )}
                    </div>
                  </div>
                  <div className="flex items-center gap-2 flex-wrap">
                    <Badge variant="secondary">{exercise.type || 'Unknown'}</Badge>
                    <Badge
                      variant={
                        exercise.level === "Beginner"
                          ? "default"
                          : exercise.level === "Intermediate"
                          ? "secondary"
                          : "destructive"
                      }
                    >
                      {exercise.level || 'Unknown'}
                    </Badge>
                    <span className="text-sm text-muted-foreground">
                      {exercise.questions?.length || 0} questions
                    </span>
                  </div>
                  <p className="text-sm text-muted-foreground line-clamp-3">
                    {exercise.content ? exercise.content.substring(0, 150) : 'No content available'}...
                  </p>
                </div>
              </Card>
            ))}
          </div>

          {filteredExercises.length === 0 && (
            <div className="text-center py-12">
              <p className="text-muted-foreground">
                No exercises found. Try adjusting filters or generate a new one with AI.
              </p>
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default ReadingExercises;
