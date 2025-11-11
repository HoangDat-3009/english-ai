import { Link } from "react-router-dom";
import { Card } from "@/components/ui/card";
import { FileText } from "lucide-react";
import { useTestList } from "@/hooks/useTestList";
import { Skeleton } from "@/components/ui/skeleton";
import { Header } from "@/components/Header";

const TestList = () => {
  const { data: tests, isLoading } = useTestList();

  return (
    <div className="min-h-screen bg-background">
      <Header />
      
      <main className="container mx-auto px-6 py-8">
        <div className="max-w-7xl mx-auto">
          <h1 className="text-4xl font-bold mb-8">Luyện Đề TOEIC</h1>

          {isLoading ? (
            <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
              {[...Array(8)].map((_, i) => (
                <Card key={i} className="p-6">
                  <Skeleton className="w-full h-32" />
                </Card>
              ))}
            </div>
          ) : (
            <div className="space-y-8">
              <div className="flex items-center gap-3 mb-4">
                <FileText className="w-6 h-6 text-primary" />
                <h2 className="text-2xl font-semibold">Đề Thi EngBuddy</h2>
              </div>

              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
                {tests?.map((test) => (
                  <Link 
                    key={test.testId}
                    to={`/test-config/${test.testId}`}
                    className="group"
                  >
                    <Card className="p-6 h-full hover:shadow-xl transition-all hover:-translate-y-1 cursor-pointer border-2 hover:border-primary/50">
                      <div className="flex flex-col items-center text-center space-y-4">
                        <div className="w-20 h-20 bg-primary/10 rounded-xl flex items-center justify-center group-hover:bg-primary/20 transition-colors">
                          <FileText className="w-10 h-10 text-primary" />
                        </div>
                        <h3 className="font-semibold text-lg">{test.title}</h3>
                      </div>
                    </Card>
                  </Link>
                ))}
              </div>
            </div>
          )}
        </div>
      </main>
    </div>
  );
};

export default TestList;
