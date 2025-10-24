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
              <SelectItem value="uploaded">Uploaded</SelectItem>
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
          <h3 className="font-semibold text-lg mb-4">Generate New Exercise with Gemini</h3>
          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4">
            <Input
              placeholder="Topic (e.g., business email)"
              value={topic}
              onChange={(e) => setTopic(e.target.value)}
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
                <SelectItem value="Part 5">Part 5</SelectItem>
                <SelectItem value="Part 6">Part 6</SelectItem>
                <SelectItem value="Part 7">Part 7</SelectItem>
              </SelectContent>
            </Select>
            <Button onClick={handleGenerate} disabled={isGenerating || !topic.trim()}>
              {isGenerating ? "Generating..." : "Generate"}
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
            {filteredExercises.map((exercise) => (
              <Card
                key={exercise.id}
                className="p-6 cursor-pointer hover:shadow-elegant transition-all hover:-translate-y-1"
                onClick={() => setSelectedExercise(String(exercise.id))}
              >
                <div className="space-y-4">
                  <div className="flex items-start justify-between gap-2">
                    <h3 className="font-semibold text-lg">{exercise.name}</h3>
                    {exercise.sourceType === 'ai' ? (
                      <Sparkles className="h-5 w-5 text-primary flex-shrink-0" />
                    ) : (
                      <Database className="h-5 w-5 text-secondary flex-shrink-0" />
                    )}
                  </div>
                  <div className="flex items-center gap-2 flex-wrap">
                    <Badge variant="secondary">{exercise.type}</Badge>
                    <Badge
                      variant={
                        exercise.level === "Beginner"
                          ? "default"
                          : exercise.level === "Intermediate"
                          ? "secondary"
                          : "destructive"
                      }
                    >
                      {exercise.level}
                    </Badge>
                    <span className="text-sm text-muted-foreground">
                      {exercise.questions.length} questions
                    </span>
                  </div>
                  <p className="text-sm text-muted-foreground line-clamp-3">
                    {exercise.content.substring(0, 150)}...
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
