const API_BASE_URL = 'http://localhost:5000/api';

export interface AIReviewStats {
  totalPending: number;
  totalApproved: number;
  totalRejected: number;
  lowConfidence: number;
  avgConfidence: number;
  needsAttention: number;
}

export interface AIGradedSubmission {
  id: number;
  userId: number;
  userName: string;
  userEmail: string;
  exerciseId: number;
  exerciseTitle: string;
  exerciseCode?: string;
  exerciseLevel: string;
  exerciseType: string;
  aiGenerated: boolean;
  originalScore: number;
  finalScore?: number;
  confidenceScore: number;
  reviewStatus: 'pending' | 'approved' | 'rejected' | 'needs_regrade';
  completedAt: string;
  reviewedBy?: number;
  reviewedAt?: string;
  reviewNotes?: string;
  totalQuestions: number;
  userAnswers: string;
}

export const aiReviewService = {
  // Get statistics for dashboard cards
  async getStats(): Promise<AIReviewStats> {
    const response = await fetch(`${API_BASE_URL}/AIReview/stats`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return await response.json();
  },

  // Get list of submissions
  async getSubmissions(
    status?: string,
    confidenceFilter?: string,
    search?: string
  ): Promise<AIGradedSubmission[]> {
    const params = new URLSearchParams();
    if (status && status !== 'all') params.append('status', status);
    if (confidenceFilter && confidenceFilter !== 'all') params.append('confidenceFilter', confidenceFilter);
    if (search) params.append('search', search);

    const url = `${API_BASE_URL}/AIReview/submissions${params.toString() ? '?' + params.toString() : ''}`;
    const response = await fetch(url);
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return await response.json();
  },

  // Get detailed information for a specific submission
  async getSubmissionDetails(id: number) {
    const response = await fetch(`${API_BASE_URL}/AIReview/submissions/${id}/details`);
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    return await response.json();
  },
};
