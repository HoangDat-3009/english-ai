import { Card } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { ArrowLeft, Copy, CheckCheck } from "lucide-react";
import ReactMarkdown from "react-markdown";
import { useState } from "react";
import { toast } from "sonner";

interface ReviewResultProps {
  review: string;
  onBack: () => void;
}

export const ReviewResult = ({ review, onBack }: ReviewResultProps) => {
  const [copied, setCopied] = useState(false);

  const handleCopy = async () => {
    await navigator.clipboard.writeText(review);
    setCopied(true);
    toast.success("Đã sao chép nhận xét");
    setTimeout(() => setCopied(false), 2000);
  };

  return (
    <div className="space-y-4">
      <div className="flex items-center justify-between">
        <Button
          variant="ghost"
          onClick={onBack}
          className="gap-2 hover:bg-accent"
        >
          <ArrowLeft className="w-4 h-4" />
          Quay lại
        </Button>
        <Button
          variant="outline"
          onClick={handleCopy}
          className="gap-2 border-border hover:bg-accent"
        >
          {copied ? (
            <>
              <CheckCheck className="w-4 h-4" />
              Đã sao chép
            </>
          ) : (
            <>
              <Copy className="w-4 h-4" />
              Sao chép
            </>
          )}
        </Button>
      </div>

      <Card className="p-6 shadow-soft">
        <div className="prose prose-pink max-w-none">
          <ReactMarkdown
            components={{
              h1: ({ children }) => (
                <h1 className="text-2xl font-bold text-foreground mb-4 pb-2 border-b border-border">
                  {children}
                </h1>
              ),
              h2: ({ children }) => (
                <h2 className="text-xl font-bold text-foreground mt-6 mb-3">
                  {children}
                </h2>
              ),
              h3: ({ children }) => (
                <h3 className="text-lg font-semibold text-foreground mt-4 mb-2">
                  {children}
                </h3>
              ),
              p: ({ children }) => (
                <p className="text-foreground leading-relaxed mb-4">
                  {children}
                </p>
              ),
              ul: ({ children }) => (
                <ul className="list-disc list-inside space-y-2 mb-4 text-foreground">
                  {children}
                </ul>
              ),
              li: ({ children }) => (
                <li className="leading-relaxed">
                  {children}
                </li>
              ),
              strong: ({ children }) => (
                <strong className="font-semibold text-primary">
                  {children}
                </strong>
              ),
              blockquote: ({ children }) => (
                <blockquote className="border-l-4 border-primary pl-4 py-2 my-4 bg-accent/50 rounded-r">
                  {children}
                </blockquote>
              ),
            }}
          >
            {review}
          </ReactMarkdown>
        </div>
      </Card>
    </div>
  );
};
