import { useState } from "react";
import { useParams, useNavigate } from "react-router-dom";
import { ArrowLeft, FileText } from "lucide-react";
import { Button } from "@/components/ui/button";
import { Card } from "@/components/ui/card";
import { Checkbox } from "@/components/ui/checkbox";
import { Switch } from "@/components/ui/switch";
import Sidebar from "@/components/Sidebar";
import { useTestDetail } from "@/hooks/useTestDetail";

const TestConfiguration = () => {
  const { testId } = useParams();
  const navigate = useNavigate();
  const { data: testDetail, isLoading } = useTestDetail(testId || null);
  
  const [isFullTest, setIsFullTest] = useState(true);
  const [selectedParts, setSelectedParts] = useState<string[]>([]);

  const parts = [
    "Part 1: Mô tả hình",
    "Part 2: Hỏi & Đáp",
    "Part 3: Hội thoại ngắn",
    "Part 4: Bài nói chuyện ngắn",
    "Part 5: Hoàn thành câu",
  ];

  const handleStartTest = () => {
    navigate(`/test-exam/${testId}`, {
      state: {
        isFullTest,
        selectedParts: isFullTest ? [] : selectedParts,
      },
    });
  };

  if (isLoading) {
    return (
      <div className="flex min-h-screen bg-background">
        <Sidebar />
        <main className="flex-1 ml-64 p-8 flex items-center justify-center">
          <div className="animate-pulse">Đang tải...</div>
        </main>
      </div>
    );
  }

  const questionCount = testDetail?.questions.length || 0;
  const estimatedTime = Math.ceil(questionCount * 0.6); // ~0.6 phút/câu

  return (
    <div className="flex min-h-screen bg-background">
      <Sidebar />
      
      <main className="flex-1 ml-64 p-8">
        <div className="max-w-3xl mx-auto">
          <Button
            variant="ghost"
            onClick={() => navigate("/test-list")}
            className="mb-6"
          >
            <ArrowLeft className="w-4 h-4 mr-2" />
            Quay lại
          </Button>

          <Card className="p-8 mb-6">
            <div className="flex flex-col items-center text-center space-y-4 mb-8">
              <div className="w-20 h-20 bg-primary/10 rounded-xl flex items-center justify-center">
                <FileText className="w-10 h-10 text-primary" />
              </div>
              <div>
                <h1 className="text-3xl font-bold mb-2">{testDetail?.title}</h1>
                <p className="text-muted-foreground">
                  {estimatedTime} phút - {questionCount} câu
                </p>
              </div>
            </div>

            <div className="space-y-6">
              <div className="flex items-center justify-between p-4 bg-accent/50 rounded-lg">
                <span className="font-medium">Bật chế độ thi thật</span>
                <Switch checked={false} disabled />
              </div>

              <div>
                <div className="flex items-center justify-between p-4 bg-accent/50 rounded-lg mb-4">
                  <span className="font-medium">Làm Full Test</span>
                  <Checkbox
                    checked={isFullTest}
                    onCheckedChange={(checked) => {
                      setIsFullTest(checked as boolean);
                      if (checked) setSelectedParts([]);
                    }}
                  />
                </div>

                {!isFullTest && (
                  <div className="space-y-3 pl-4">
                    <p className="text-sm font-medium text-muted-foreground mb-3">
                      Hoặc làm từng phần
                    </p>
                    {parts.map((part) => (
                      <div
                        key={part}
                        className="flex items-center space-x-3 p-3 rounded-lg hover:bg-accent/30 transition-colors"
                      >
                        <Checkbox
                          id={part}
                          checked={selectedParts.includes(part)}
                          onCheckedChange={(checked) => {
                            if (checked) {
                              setSelectedParts([...selectedParts, part]);
                            } else {
                              setSelectedParts(selectedParts.filter((p) => p !== part));
                            }
                          }}
                        />
                        <label
                          htmlFor={part}
                          className="text-sm cursor-pointer flex-1"
                        >
                          {part}
                        </label>
                      </div>
                    ))}
                  </div>
                )}
              </div>
            </div>
          </Card>

          <Button
            size="lg"
            className="w-full text-lg py-6"
            onClick={handleStartTest}
            disabled={!isFullTest && selectedParts.length === 0}
          >
            BẮT ĐẦU
          </Button>
        </div>
      </main>
    </div>
  );
};

export default TestConfiguration;
