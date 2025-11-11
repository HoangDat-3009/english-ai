import { useState, useEffect } from "react";
import { useNavigate } from "react-router-dom";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Progress } from "@/components/ui/progress";
import { RadioGroup, RadioGroupItem } from "@/components/ui/radio-group";
import { Label } from "@/components/ui/label";
import { useGrammarPractice } from "@/hooks/useGrammarPractice";
import { ChevronLeft, ChevronRight, Timer, Flag, BookOpen } from "lucide-react";
import { cn } from "@/lib/utils";

const GRAMMAR_TOPICS = [
  { id: 1, name: "NOUNS", apiPath: "1" },
  { id: 2, name: "ADJECTIVES", apiPath: "2" },
  { id: 3, name: "ADVERBS", apiPath: "3" },
  { id: 4, name: "NOUN, ADJECTIVE, ADVERB...", apiPath: "4" },
  { id: 5, name: "PRONOUNS", apiPath: "5" },
  { id: 6, name: "PREPOSITIONS", apiPath: "6" },
  { id: 7, name: "CONJUNCTIONS", apiPath: "7" },
];

const GrammarPractice = () => {
  const navigate = useNavigate();
  const [selectedTopic, setSelectedTopic] = useState(GRAMMAR_TOPICS[0]);
  const [elapsedTime, setElapsedTime] = useState(0);

  const {
    session,
    loading,
    error,
    currentQuestion,
    currentQuestionIndex,
    totalQuestions,
    selectedOption,
    selectAnswer,
    goToQuestion,
    nextQuestion,
    previousQuestion,
    resetSession,
    getStatistics,
  } = useGrammarPractice({ topicId: selectedTopic.id });

  // Timer
  useEffect(() => {
    const interval = setInterval(() => {
      setElapsedTime(Math.floor((Date.now() - session.startTime.getTime()) / 1000));
    }, 1000);

    return () => clearInterval(interval);
  }, [session.startTime]);

  const formatTime = (seconds: number) => {
    const hrs = Math.floor(seconds / 3600);
    const mins = Math.floor((seconds % 3600) / 60);
    const secs = seconds % 60;
    return `${hrs.toString().padStart(2, "0")}:${mins.toString().padStart(2, "0")}:${secs.toString().padStart(2, "0")}`;
  };

  const stats = getStatistics();

  const getQuestionStatus = (questionId: number) => {
    const status = session.questionStatuses.get(questionId);
    if (!status?.answered) return "unanswered";
    return status.isCorrect ? "correct" : "incorrect";
  };

  const handleTopicChange = (topic: typeof GRAMMAR_TOPICS[0]) => {
    setSelectedTopic(topic);
    setElapsedTime(0);
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-background via-background to-muted flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-16 w-16 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Loading exercise...</p>
        </div>
      </div>
    );
  }

  if (error) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-background via-background to-muted flex items-center justify-center p-4">
        <Card className="p-6 max-w-md">
          <p className="text-destructive mb-4">Error: {error}</p>
          <Button onClick={() => navigate("/")}>Go Home</Button>
        </Card>
      </div>
    );
  }

  return (
    <div className="min-h-screen bg-gradient-to-br from-background via-background to-muted">
      <div className="container mx-auto p-4 lg:p-6">
        {/* Header */}
        <div className="flex items-center justify-between mb-6">
          <Button
            variant="ghost"
            onClick={() => navigate("/")}
            className="gap-2"
          >
            <ChevronLeft className="w-4 h-4" />
            Back
          </Button>
          
          <div className="flex items-center gap-2 bg-card px-4 py-2 rounded-lg border shadow-sm">
            <Timer className="w-5 h-5 text-primary" />
            <span className="font-mono text-lg font-semibold">{formatTime(elapsedTime)}</span>
          </div>
        </div>

        <div className="grid lg:grid-cols-[300px_1fr] gap-6">
          {/* Sidebar */}
          <aside className="space-y-2">
            <Card className="p-4">
              <div className="flex items-center gap-2 mb-4">
                <BookOpen className="w-5 h-5 text-primary" />
                <h2 className="font-semibold">Grammar Topics</h2>
              </div>
              <div className="space-y-2">
                {GRAMMAR_TOPICS.map((topic) => {
                  const isSelected = topic.id === selectedTopic.id;
                  const topicStats = isSelected ? stats : { correct: 0, total: 0 };
                  
                  return (
                    <button
                      key={topic.id}
                      onClick={() => handleTopicChange(topic)}
                      className={cn(
                        "w-full flex items-center justify-between p-3 rounded-lg text-left transition-colors",
                        isSelected
                          ? "bg-primary text-primary-foreground"
                          : "hover:bg-accent"
                      )}
                    >
                      <span className="font-medium text-sm">{topic.name}</span>
                      <span className={cn(
                        "text-xs px-2 py-1 rounded-full",
                        isSelected
                          ? "bg-primary-foreground/20 text-primary-foreground"
                          : "bg-muted text-muted-foreground"
                      )}>
                        {topicStats.correct}/{topicStats.total}
                      </span>
                    </button>
                  );
                })}
              </div>
            </Card>
          </aside>

          {/* Main Content */}
          <main className="space-y-6">
            {currentQuestion && (
              <>
                {/* Question Card */}
                <Card className="p-6">
                  <div className="flex items-start justify-between mb-6">
                    <div>
                      <h3 className="text-lg font-semibold mb-2">
                        Câu hỏi {currentQuestionIndex + 1}.
                      </h3>
                      <p className="text-base leading-relaxed whitespace-pre-wrap">
                        {currentQuestion.questionText}
                      </p>
                    </div>
                    <Flag className="w-5 h-5 text-muted-foreground" />
                  </div>

                  <RadioGroup
                    value={selectedOption || ""}
                    onValueChange={selectAnswer}
                    className="space-y-3"
                  >
                    {["A", "B", "C", "D"].map((option) => {
                      const optionKey = `option${option}` as keyof typeof currentQuestion;
                      const optionText = currentQuestion[optionKey];
                      const status = session.questionStatuses.get(currentQuestion.questionId);
                      const isSelected = selectedOption === option;
                      const isCorrect = currentQuestion.correctOption === option;
                      const showFeedback = status?.answered;

                      return (
                        <div
                          key={option}
                          className={cn(
                            "flex items-center space-x-3 p-4 rounded-lg border-2 transition-all cursor-pointer",
                            isSelected && !showFeedback && "border-primary bg-primary/5",
                            showFeedback && isSelected && isCorrect && "border-green-500 bg-green-50 dark:bg-green-950",
                            showFeedback && isSelected && !isCorrect && "border-red-500 bg-red-50 dark:bg-red-950",
                            !isSelected && "border-border hover:border-muted-foreground"
                          )}
                        >
                          <RadioGroupItem value={option} id={`option-${option}`} />
                          <Label
                            htmlFor={`option-${option}`}
                            className="flex-1 cursor-pointer text-base"
                          >
                            {option}. {optionText}
                          </Label>
                        </div>
                      );
                    })}
                  </RadioGroup>
                </Card>

                {/* Statistics */}
                <Card className="p-4">
                  <div className="flex items-center justify-between mb-3">
                    <h4 className="font-semibold">Câu hỏi 1-{totalQuestions}</h4>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={resetSession}
                    >
                      Restart
                    </Button>
                  </div>
                  
                  <div className="flex items-center gap-4 mb-4 text-sm">
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full bg-green-500"></div>
                      <span>{stats.correct}/20 Đúng</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full bg-red-500"></div>
                      <span>{stats.incorrect}/20 Sai</span>
                    </div>
                    <div className="flex items-center gap-2">
                      <div className="w-3 h-3 rounded-full bg-muted"></div>
                      <span>{stats.unanswered}/20 Chưa trả lời</span>
                    </div>
                  </div>

                  {/* Question Navigation */}
                  <div className="flex flex-wrap gap-2">
                    {session.exercise?.questions.map((q, idx) => {
                      const status = getQuestionStatus(q.questionId);
                      return (
                        <button
                          key={q.questionId}
                          onClick={() => goToQuestion(idx)}
                          className={cn(
                            "w-10 h-10 rounded-full font-medium transition-all",
                            idx === currentQuestionIndex && "ring-2 ring-primary ring-offset-2",
                            status === "correct" && "bg-green-500 text-white hover:bg-green-600",
                            status === "incorrect" && "bg-red-500 text-white hover:bg-red-600",
                            status === "unanswered" && "bg-muted hover:bg-muted/80"
                          )}
                        >
                          {idx + 1}
                        </button>
                      );
                    })}
                  </div>
                </Card>

                {/* Navigation */}
                <div className="flex justify-between">
                  <Button
                    variant="outline"
                    onClick={previousQuestion}
                    disabled={currentQuestionIndex === 0}
                    className="gap-2"
                  >
                    <ChevronLeft className="w-4 h-4" />
                    Previous
                  </Button>
                  
                  <Button
                    onClick={nextQuestion}
                    disabled={currentQuestionIndex === totalQuestions - 1}
                    className="gap-2"
                  >
                    Next
                    <ChevronRight className="w-4 h-4" />
                  </Button>
                </div>
              </>
            )}
          </main>
        </div>
      </div>
    </div>
  );
};

export default GrammarPractice;
