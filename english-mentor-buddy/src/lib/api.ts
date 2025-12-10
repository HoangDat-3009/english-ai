import { apiService } from '@/services/api';

// Map frontend levels to backend enum values
// Basic (A1-A2) -> Elementary (2)
// Intermediate (B1-B2) -> UpperIntermediate (4)
// Advanced (C1-C2) -> Proficient (6)
const levelMapping: Record<string, number> = {
  "Basic": 2,       // A1-A2 -> Elementary
  "Intermediate": 4, // B1-B2 -> UpperIntermediate
  "Advanced": 6,     // C1-C2 -> Proficient
};

export interface GenerateReviewRequest {
  userLevel: string;
  requirement: string;
  content: string;
}

export const reviewApi = {
  generateReview: async (data: GenerateReviewRequest, provider: 'gemini' | 'openai' = 'gemini'): Promise<string> => {
    try {
      console.log("🚀 Calling Review API with data:", {
        userLevel: data.userLevel,
        mappedLevel: levelMapping[data.userLevel],
        requirementLength: data.requirement.length,
        contentLength: data.content.length,
        provider: provider,
      });

      // Chuẩn bị request body giống backend mong đợi
      const requestBody = {
        UserLevel: levelMapping[data.userLevel] || 2, // Map to backend enum
        Requirement: data.requirement,
        Content: data.content,
      };

      console.log("📤 Request body:", requestBody);

      // Sử dụng fetch giống như Chat page
      const response = await fetch(`${apiService.getBaseUrl()}/api/Review/Generate?provider=${provider}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...apiService.getHeaders()
        },
        body: JSON.stringify(requestBody)
      });

      console.log("📥 Response status:", response.status);

      if (!response.ok) {
        const errorText = await response.text();
        console.error("❌ Response error:", errorText);
        throw new Error(`Server responded with status: ${response.status}`);
      }

      // Lấy response text
      const result = await response.text();
      
      console.log("✅ Response received, length:", result.length);
      console.log("📝 Response preview:", result.substring(0, 200));

      // Kiểm tra nếu response chứa message lỗi "busy"
      if (result.includes("CẢNH BÁO") || result.includes("EngBuddy đang bận")) {
        console.log("⚠️ Backend is busy");
        const busyError = new Error(result) as Error & { isBusyError: boolean };
        busyError.isBusyError = true;
        throw busyError;
      }

      return result;
    } catch (error: unknown) {
      console.error("❌ API Error:", error);
      
      // Re-throw busy error
      if (error instanceof Error && 'isBusyError' in error && (error as { isBusyError: boolean }).isBusyError) {
        throw error;
      }
      
      throw new Error("Không thể kết nối đến server. Vui lòng kiểm tra backend.");
    }
  },
};

// Map level for sentence writing (same as review)
const sentenceLevelMapping: Record<string, number> = {
  "Basic": 2,        // A1-A2 -> Elementary
  "Intermediate": 3, // B1-B2 -> Intermediate
  "Advanced": 5      // C1-C2 -> Advanced
};

export interface GenerateSentencesRequest {
  topic: string;
  level: string;
  sentenceCount: number;
  writingStyle?: string;
}

export interface GenerateSentencesResponse {
  Sentences: Array<{
    Id: number;
    Vietnamese: string;
    CorrectAnswer: string;
    Suggestion?: {
      Vocabulary: Array<{ Word: string; Meaning: string }>;
      Structure: string;
    };
  }>;
}

export const sentenceWritingApi = {
  generateSentences: async (data: GenerateSentencesRequest, provider: 'gemini' | 'openai' = 'gemini'): Promise<GenerateSentencesResponse> => {
    try {
      const requestBody = {
        Topic: data.topic,
        Level: sentenceLevelMapping[data.level] || 3,
        SentenceCount: data.sentenceCount,
        WritingStyle: data.writingStyle || "Communicative",
      };
      
      const url = `${apiService.getBaseUrl()}/api/SentenceWriting/Generate?provider=${provider}`;

      const response = await fetch(url, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...apiService.getHeaders()
        },
        body: JSON.stringify(requestBody)
      });

      if (!response.ok) {
        const errorText = await response.text();
        
        // Handle rate limit (429)
        if (response.status === 429) {
          throw new Error("🕐 API đang bận hoặc hết quota. Vui lòng đợi 1-2 phút và thử lại. Nếu vẫn lỗi, hãy kiểm tra Gemini API key.");
        }
        
        throw new Error(errorText || `Server responded with status: ${response.status}`);
      }

      const result = await response.json();

      // Kiểm tra nếu backend busy
      if (typeof result === 'string' && (result.includes("CẢNH BÁO") || result.includes("EngBuddy đang bận"))) {
        const busyError = new Error(result) as Error & { isBusyError: boolean };
        busyError.isBusyError = true;
        throw busyError;
      }

      return result;
    } catch (error: unknown) {
      console.error("❌ API Error:", error);
      console.error("❌ Error type:", error instanceof Error ? 'Error' : typeof error);
      
      if (error instanceof Error) {
        console.error("❌ Error message:", error.message);
        console.error("❌ Error stack:", error.stack);
      }
      
      // Re-throw busy error
      if (error instanceof Error && 'isBusyError' in error && (error as { isBusyError: boolean }).isBusyError) {
        throw error;
      }
      
      // Check if it's a network error
      if (error instanceof TypeError) {
        console.error("❌ Network error detected");
        throw new Error("Không thể kết nối đến server. Vui lòng kiểm tra:\n1. Backend đã chạy chưa?\n2. URL có đúng không? (https://localhost:5000)\n3. CORS có được config chưa?");
      }
      
      // Re-throw other errors with their original message
      if (error instanceof Error) {
        throw error;
      }
      
      throw new Error("Không thể kết nối đến server. Vui lòng kiểm tra backend.");
    }
  },
};

