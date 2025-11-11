import { useState } from "react";
import { toast } from "sonner";
import type { ConversationTopic } from "@/types/conversation";

interface AnalyzeSpeechResponse {
  transcript: string;
  pronunciationScore: number;
  fluencyScore: number;
  intonationScore: number;
  accuracyScore: number;
  feedback: string;
  suggestion: string;
}

interface EvaluateAnswerResponse {
  transcript: string;
  feedback: string;
  suggestions: string;
  contentScore: number;
  grammarScore: number;
  vocabularyScore: number;
  pronunciationScore: number;
  fluencyScore: number;
  overallScore: number;
}

export const useSpeakingPractice = () => {
  const [isLoading, setIsLoading] = useState(false);

  const generateDialogue = async (topic: string, purpose: string, level: number): Promise<ConversationTopic | null> => {
    setIsLoading(true);
    try {
      const formData = new FormData();
      formData.append("topic", topic);
      formData.append("purpose", purpose);
      formData.append("level", level.toString());

      const response = await fetch("https://btl-d39f.onrender.com/api/SpeakingAi/GenerateDialogue", {
        method: "POST",
        body: formData,
      });

      if (!response.ok) throw new Error("Failed to generate dialogue");
      
      const data = await response.json();
      return data;
    } catch (error) {
      console.error("Error generating dialogue:", error);
      toast.error("Không thể tạo hội thoại. Vui lòng thử lại.");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  const analyzeSpeech = async (
    topic: string,
    reference: string,
    audioBlob: Blob
  ): Promise<AnalyzeSpeechResponse | null> => {
    setIsLoading(true);
    try {
      const formData = new FormData();
      formData.append("Topic", topic);
      formData.append("Reference", reference);
      formData.append("File", audioBlob, "recording.wav");

      const response = await fetch("https://btl-d39f.onrender.com/api/SpeakingAi/AnalyzeSpeech", {
        method: "POST",
        body: formData,
      });

      if (!response.ok) throw new Error("Failed to analyze speech");
      
      const text = await response.text();
      
      // Parse the response text
      const lines = text.split('\n').map(line => line.trim()).filter(line => line);
      const result: any = {
        transcript: "",
        pronunciationScore: 0,
        fluencyScore: 0,
        intonationScore: 0,
        accuracyScore: 0,
        feedback: "",
        suggestion: ""
      };

      lines.forEach(line => {
        if (line.startsWith("Transcript:")) {
          result.transcript = line.replace("Transcript:", "").trim();
        } else if (line.startsWith("Pronunciation score:")) {
          result.pronunciationScore = parseInt(line.replace("Pronunciation score:", "").trim());
        } else if (line.startsWith("Fluency score:")) {
          result.fluencyScore = parseInt(line.replace("Fluency score:", "").trim());
        } else if (line.startsWith("Intonation score:")) {
          result.intonationScore = parseInt(line.replace("Intonation score:", "").trim());
        } else if (line.startsWith("Accuracy score:")) {
          result.accuracyScore = parseInt(line.replace("Accuracy score:", "").trim());
        } else if (line.startsWith("Feedback:")) {
          result.feedback = line.replace("Feedback:", "").trim();
        } else if (line.startsWith("Suggestion:")) {
          result.suggestion = line.replace("Suggestion:", "").trim();
        }
      });

      return result;
    } catch (error) {
      console.error("Error analyzing speech:", error);
      toast.error("Không thể phân tích giọng nói. Vui lòng thử lại.");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  const evaluateAnswer = async (
    topic: string,
    question: string,
    audioBlob: Blob
  ): Promise<EvaluateAnswerResponse | null> => {
    setIsLoading(true);
    try {
      const formData = new FormData();
      formData.append("Topic", topic);
      formData.append("Question", question);
      formData.append("File", audioBlob, "recording.wav");

      const response = await fetch("https://btl-d39f.onrender.com/api/SpeakingAi/EvaluateAnswer", {
        method: "POST",
        body: formData,
      });

      if (!response.ok) throw new Error("Failed to evaluate answer");
      
      const text = await response.text();
      
      // Parse the response text
      const lines = text.split('\n').map(line => line.trim()).filter(line => line);
      const result: any = {
        transcript: "",
        feedback: "",
        suggestions: "",
        contentScore: 0,
        grammarScore: 0,
        vocabularyScore: 0,
        pronunciationScore: 0,
        fluencyScore: 0,
        overallScore: 0
      };

      lines.forEach(line => {
        if (line.startsWith("Transcript:")) {
          result.transcript = line.replace("Transcript:", "").trim();
        } else if (line.includes("Feedback:")) {
          result.feedback = line.split("Feedback:")[1].trim();
        } else if (line.includes("Suggestions:")) {
          result.suggestions = line.split("Suggestions:")[1].trim();
        } else if (line.includes("Content:")) {
          result.contentScore = parseInt(line.match(/\d+/)?.[0] || "0");
        } else if (line.includes("Grammar:")) {
          result.grammarScore = parseInt(line.match(/\d+/)?.[0] || "0");
        } else if (line.includes("Vocabulary:")) {
          result.vocabularyScore = parseInt(line.match(/\d+/)?.[0] || "0");
        } else if (line.includes("Pronunciation:")) {
          result.pronunciationScore = parseInt(line.match(/\d+/)?.[0] || "0");
        } else if (line.includes("Fluency:")) {
          result.fluencyScore = parseInt(line.match(/\d+/)?.[0] || "0");
        } else if (line.includes("Overall:")) {
          result.overallScore = parseInt(line.match(/\d+/)?.[0] || "0");
        }
      });

      return result;
    } catch (error) {
      console.error("Error evaluating answer:", error);
      toast.error("Không thể đánh giá câu trả lời. Vui lòng thử lại.");
      return null;
    } finally {
      setIsLoading(false);
    }
  };

  return {
    isLoading,
    generateDialogue,
    analyzeSpeech,
    evaluateAnswer,
  };
};
