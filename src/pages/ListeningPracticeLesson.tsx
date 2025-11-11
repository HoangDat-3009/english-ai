import { ArrowLeft, Keyboard, Eye, Volume2, CheckCircle2, XCircle, Lightbulb } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Card } from "@/components/ui/card";
import { Badge } from "@/components/ui/badge";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { useNavigate, useSearchParams } from "react-router-dom";
import { useListeningLesson } from "@/hooks/useListeningLesson";

const ListeningPracticeLesson = () => {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const lessonId = parseInt(searchParams.get("lessonId") || "1");

  const {
    lesson,
    currentSentence,
    currentIndex,
    totalSentences,
    progress,
    isLastSentence,
    isFirstSentence,
    userInput,
    setUserInput,
    showResult,
    showAnswer,
    showTranslation,
    setShowTranslation,
    playbackSpeed,
    setPlaybackSpeed,
    loading,
    error,
    correctCount,
    attemptedCount,
    revealedSentences,
    audioRef,
    checkAnswer,
    nextSentence,
    previousSentence,
    skipSentence,
    revealAnswer,
  } = useListeningLesson({ lessonId });

  if (loading) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center">
        <div className="text-center">
          <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-primary mx-auto mb-4"></div>
          <p className="text-muted-foreground">Đang tải bài học...</p>
        </div>
      </div>
    );
  }

  if (error || !lesson || !currentSentence) {
    return (
      <div className="min-h-screen bg-background flex items-center justify-center p-6">
        <Card className="p-6 max-w-md">
          <Alert variant="destructive">
            <AlertDescription>
              {error || "Không thể tải bài học. Vui lòng thử lại sau."}
            </AlertDescription>
          </Alert>
          <Button 
            className="mt-4 w-full" 
            onClick={() => navigate("/listening-practice")}
          >
            Quay lại
          </Button>
        </Card>
      </div>
    );
  }

  const handleCheckOrContinue = () => {
    if (showResult) {
      nextSentence();
    } else {
      checkAnswer();
    }
  };

  return (
    <div className="min-h-screen bg-background">
      {/* Header */}
      <header className="border-b px-6 py-6">
        <div className="max-w-7xl mx-auto flex items-center justify-between">
          <div>
            <div className="flex items-center gap-3 mb-2">
              <span className="text-2xl">🎧</span>
              <h1 className="text-2xl font-bold">Luyện nghe</h1>
            </div>
            <p className="text-sm text-muted-foreground">
              Luyện nghe với audio/video, transcript, giải thích, dịch nghĩa.
            </p>
          </div>
          <Button className="bg-warning hover:bg-warning/90 text-warning-foreground">
            💬 Góp ý
          </Button>
        </div>
      </header>

      <div className="px-6 py-6">
        <div className="max-w-7xl mx-auto">
          <Button 
            variant="destructive" 
            className="mb-6"
            onClick={() => navigate("/listening-practice")}
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>

          <div className="grid grid-cols-1 lg:grid-cols-[1fr,480px] gap-6">
            {/* Left Column */}
            <div className="space-y-6">
              {/* Keyboard Shortcuts */}
              <Card className="bg-info/10 border-info/20 p-6">
                <div className="flex items-center gap-6 flex-wrap">
                  <div className="flex items-center gap-3">
                    <Keyboard className="w-5 h-5 text-info" />
                    <span className="font-semibold text-info">Phím tắt</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <kbd className="px-4 py-2 bg-primary text-primary-foreground rounded font-semibold text-sm min-w-[80px] text-center">
                      Enter
                    </kbd>
                    <span className="text-sm">Kiểm tra / Tiếp tục</span>
                  </div>
                  <div className="flex items-center gap-2">
                    <kbd className="px-4 py-2 bg-primary text-primary-foreground rounded font-semibold text-sm min-w-[80px] text-center">
                      Ctrl
                    </kbd>
                    <span className="text-sm">Phát lại âm thanh</span>
                  </div>
                </div>
              </Card>

              {/* Progress and Stats */}
              <div className="flex items-center justify-between gap-4 flex-wrap">
                <div className="flex items-center gap-3">
                  <Badge className="bg-destructive text-destructive-foreground px-3 py-1 text-sm font-bold">
                    CÂU {currentIndex + 1}/{totalSentences}
                  </Badge>
                  <h2 className="text-xl font-semibold">{lesson.lessonTitle}</h2>
                </div>
                <div className="flex items-center gap-4 text-sm">
                  <span className="text-success">
                    ✓ {correctCount} đúng
                  </span>
                  {attemptedCount > 0 && (
                    <span className="text-muted-foreground">
                      / {attemptedCount} câu
                    </span>
                  )}
                </div>
              </div>

              {/* Audio Player */}
              <Card className="p-6">
                <div className="flex items-center gap-3 flex-wrap">
                  <audio 
                    ref={audioRef}
                    id="challengeAudio"
                    controls 
                    preload="metadata"
                    className="w-full max-w-[320px]"
                    src={currentSentence.audioUrl}
                  >
                    Audio not supported.
                  </audio>
                  <div className="flex items-center gap-2 ms-2">
                    <label htmlFor="audioSpeedSelect" className="text-sm font-medium mb-0">
                      Tốc độ:
                    </label>
                    <select 
                      id="audioSpeedSelect"
                      className="border rounded px-3 py-2 text-lg bg-background w-[110px]"
                      value={playbackSpeed}
                      onChange={(e) => setPlaybackSpeed(e.target.value)}
                    >
                      <option value="0.75">0.75x</option>
                      <option value="1">1x</option>
                      <option value="1.25">1.25x</option>
                    </select>
                  </div>
                </div>
              </Card>

              {/* Input Section */}
              <div>
                <h3 className="font-semibold text-lg mb-3">Nghe và gõ lại câu:</h3>
                <Card className="p-6">
                  <Textarea
                    placeholder="Nhập câu bạn nghe được..."
                    value={userInput}
                    onChange={(e) => setUserInput(e.target.value)}
                    className="min-h-[120px] mb-4 text-base"
                    disabled={!!showResult}
                    onKeyDown={(e) => {
                      if (e.key === "Enter" && !e.shiftKey) {
                        e.preventDefault();
                        handleCheckOrContinue();
                      }
                    }}
                  />

                  {/* Result feedback */}
                  {showResult && (
                    <Alert className={`mb-4 ${showResult.isCorrect ? 'bg-success/10 border-success' : 'bg-destructive/10 border-destructive'}`}>
                      <div className="flex items-start gap-3">
                        {showResult.isCorrect ? (
                          <CheckCircle2 className="h-5 w-5 text-success mt-0.5" />
                        ) : (
                          <XCircle className="h-5 w-5 text-destructive mt-0.5" />
                        )}
                        <div className="flex-1">
                          <p className={`font-semibold ${showResult.isCorrect ? 'text-success' : 'text-destructive'}`}>
                            {showResult.isCorrect ? "Chính xác! Tuyệt vời! 🎉" : "Chưa đúng!"}
                          </p>
                          {!showResult.isCorrect && showAnswer && (
                            <div className="mt-2 space-y-1">
                              <p className="text-sm">
                                <span className="font-medium">Đáp án đúng:</span> {showResult.correctAnswer}
                              </p>
                              <p className="text-sm text-muted-foreground">
                                <span className="font-medium">Câu trả lời của bạn:</span> {showResult.userAnswer || "(trống)"}
                              </p>
                            </div>
                          )}
                          {!showResult.isCorrect && !showAnswer && (
                            <Button
                              size="sm"
                              variant="outline"
                              onClick={revealAnswer}
                              className="mt-2"
                            >
                              <Lightbulb className="w-4 h-4 mr-2" />
                              Hiện đáp án
                            </Button>
                          )}
                        </div>
                      </div>
                    </Alert>
                  )}

                  <div className="flex gap-3">
                    {!showResult ? (
                      <>
                        <Button 
                          className="bg-success hover:bg-success/90 text-success-foreground flex-1 text-base py-6"
                          onClick={checkAnswer}
                          disabled={!userInput.trim()}
                        >
                          ✓ Kiểm tra
                        </Button>
                        <Button 
                          variant="outline" 
                          className="flex-1 text-base py-6"
                          onClick={skipSentence}
                          disabled={isLastSentence}
                        >
                          ⏩ Bỏ qua
                        </Button>
                      </>
                    ) : (
                      <Button 
                        className="flex-1 text-base py-6"
                        onClick={nextSentence}
                        disabled={isLastSentence}
                      >
                        {isLastSentence ? "✓ Hoàn thành" : "→ Tiếp tục"}
                      </Button>
                    )}
                  </div>
                </Card>
              </div>
            </div>

            {/* Right Column - Transcript */}
            <Card className="p-6 h-fit sticky top-6">
              <div className="flex items-center justify-between mb-6">
                <h3 className="font-semibold text-lg flex items-center gap-2">
                  <span className="text-xl">📄</span>
                  Transcript
                </h3>
                <div className="flex gap-2">
                  <Button
                    size="sm"
                    variant={!showTranslation ? "default" : "outline"}
                    onClick={() => setShowTranslation(false)}
                    className="text-xs"
                  >
                    <Eye className="w-3 h-3 mr-1" />
                    Hiện Transcript
                  </Button>
                  <Button
                    size="sm"
                    variant={showTranslation ? "default" : "outline"}
                    onClick={() => setShowTranslation(true)}
                    className="bg-warning hover:bg-warning/90 text-warning-foreground text-xs"
                  >
                    <Volume2 className="w-3 h-3 mr-1" />
                    Hiện Dịch
                  </Button>
                </div>
              </div>

              <div className="space-y-4 max-h-[600px] overflow-y-auto">
                {!showTranslation ? (
                  <div className="space-y-3">
                    {lesson.sentences.map((sentence, idx) => {
                      const isRevealed = revealedSentences.has(idx);
                      const isCurrentAndChecked = idx === currentIndex && showResult;
                      const isPastSentence = idx < currentIndex;
                      const shouldShowTranslation = isRevealed;
                      
                      return (
                        <div key={sentence.sentenceId}>
                          {isRevealed ? (
                            <div className={`border-l-4 pl-4 py-3 rounded-r ${
                              idx === currentIndex 
                                ? 'border-primary bg-primary/10' 
                                : 'border-muted bg-muted/5'
                            }`}>
                              <p className={`font-semibold font-mono text-sm mb-2 ${
                                idx === currentIndex ? 'text-primary' : 'text-foreground'
                              }`}>
                                {sentence.transcript}
                              </p>
                              {shouldShowTranslation && (
                                <div className="flex items-start gap-2 text-muted-foreground text-xs">
                                  <Volume2 className="w-3 h-3 mt-0.5 flex-shrink-0" />
                                  <p>{sentence.translation}</p>
                                </div>
                              )}
                            </div>
                          ) : (
                            <p className="text-muted-foreground font-mono text-sm pl-4">
                              {"* ".repeat(Math.floor(sentence.transcript.length / 2))}
                            </p>
                          )}
                        </div>
                      );
                    })}
                  </div>
                ) : (
                  <div className="space-y-3">
                    {lesson.sentences.map((sentence, idx) => {
                      const isRevealed = revealedSentences.has(idx);
                      const isCurrentAndChecked = idx === currentIndex && showResult;
                      const isPastSentence = idx < currentIndex;
                      const shouldShowTranslation = isRevealed;
                      
                      return (
                        <div key={sentence.sentenceId}>
                          {isRevealed && shouldShowTranslation ? (
                            <div className={`border-l-4 pl-4 py-2 rounded-r ${
                              idx === currentIndex 
                                ? 'border-primary bg-primary/10' 
                                : 'border-muted bg-muted/5'
                            }`}>
                              <p className="text-sm">{sentence.translation}</p>
                            </div>
                          ) : isRevealed ? (
                            <div className="border-l-4 border-muted pl-4 py-2 rounded-r bg-muted/5">
                              <p className="text-muted-foreground text-sm italic">Kiểm tra để xem bản dịch</p>
                            </div>
                          ) : (
                            <p className="text-muted-foreground text-sm pl-4">
                              {"* ".repeat(Math.floor(sentence.translation.length / 2))}
                            </p>
                          )}
                        </div>
                      );
                    })}
                  </div>
                )}
              </div>
            </Card>
          </div>
        </div>
      </div>
    </div>
  );
};

export default ListeningPracticeLesson;
