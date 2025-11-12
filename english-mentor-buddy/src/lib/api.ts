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
  generateReview: async (data: GenerateReviewRequest): Promise<string> => {
    try {
      console.log("🚀 Calling Review API with data:", {
        userLevel: data.userLevel,
        mappedLevel: levelMapping[data.userLevel],
        requirementLength: data.requirement.length,
        contentLength: data.content.length,
      });

      // Chuẩn bị request body giống backend mong đợi
      const requestBody = {
        UserLevel: levelMapping[data.userLevel] || 2, // Map to backend enum
        Requirement: data.requirement,
        Content: data.content,
      };

      console.log("📤 Request body:", requestBody);

      // Sử dụng fetch giống như Chat page
      const response = await fetch(`${apiService.getBaseUrl()}/api/Review/Generate`, {
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
