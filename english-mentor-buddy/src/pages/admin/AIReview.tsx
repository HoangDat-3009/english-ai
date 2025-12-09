import React, { useState, useEffect, useCallback } from 'react';
import { 
  CheckCircle, 
  XCircle, 
  AlertTriangle, 
  Eye, 
  RefreshCw,
  Filter,
  Search,
  ChevronDown,
  User,
  Calendar,
  Award,
  FileText,
  TrendingUp,
  Clock
} from 'lucide-react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from '@/components/ui/card';
import { Badge } from '@/components/ui/badge';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Tabs, TabsContent, TabsList, TabsTrigger } from '@/components/ui/tabs';
import { Separator } from '@/components/ui/separator';
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from '@/components/ui/select';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Label } from '@/components/ui/label';
import { Textarea } from '@/components/ui/textarea';
import { useToast } from '@/hooks/use-toast';
import { aiReviewService } from '@/services/aiReviewService';

// Types
interface QuestionOption {
  label: string; // A, B, C, D
  text: string;  // Nội dung đáp án
}

interface QuestionDetail {
  questionNumber: number;
  questionText: string;
  options: QuestionOption[]; // Các lựa chọn A, B, C, D
  userAnswer: string; // Đáp án học viên chọn (A/B/C/D) - KHÔNG ĐỔI
  correctAnswer: string; // Đáp án đúng ban đầu (A/B/C/D) - CỐ ĐỊNH
  teacherAnswer?: string; // Đáp án giáo viên chọn (A/B/C/D) - CÓ THỂ THAY ĐỔI
  teacherExplanation?: string; // Giải thích của giáo viên về sự thay đổi
  isCorrect: boolean;
  points: number;
  aiExplanation?: string;
}

interface AIGradedSubmission {
  id: number;
  userId: number;
  userName: string;
  userEmail: string;
  exerciseId: number;
  exerciseTitle: string;
  exerciseCode?: string; // Mã đề bài
  exerciseLevel: string;
  exerciseType: string; // quiz, multiple_choice, listening_quiz, etc.
  aiGenerated: boolean; // Bài tập do AI tạo hay admin upload
  originalScore: number;
  finalScore?: number;
  confidenceScore: number;
  reviewStatus: 'pending' | 'approved' | 'rejected' | 'needs_regrade';
  completedAt: string;
  reviewedBy?: number;
  reviewedAt?: string;
  reviewNotes?: string;
  totalQuestions: number;
  userAnswers: unknown;
  questions?: QuestionDetail[];
}

interface ReviewStats {
  totalPending: number;
  totalApproved: number;
  totalRejected: number;
  lowConfidence: number;
  avgConfidence: number;
  needsAttention: number;
}

const AIReview: React.FC = () => {
  const { toast } = useToast();
  const [submissions, setSubmissions] = useState<AIGradedSubmission[]>([]);
  const [filteredSubmissions, setFilteredSubmissions] = useState<AIGradedSubmission[]>([]);
  const [stats, setStats] = useState<ReviewStats>({
    totalPending: 0,
    totalApproved: 0,
    totalRejected: 0,
    lowConfidence: 0,
    avgConfidence: 0,
    needsAttention: 0,
  });
  const [loading, setLoading] = useState(false);
  const [searchTerm, setSearchTerm] = useState('');
  const [filterStatus, setFilterStatus] = useState<string>('all');
  const [filterConfidence, setFilterConfidence] = useState<string>('all');
  const [selectedSubmission, setSelectedSubmission] = useState<AIGradedSubmission | null>(null);
  const [reviewDialogOpen, setReviewDialogOpen] = useState(false);
  const [loadingDetails, setLoadingDetails] = useState(false);
  const [reviewForm, setReviewForm] = useState({
    finalScore: 0,
    reviewStatus: 'approved' as 'approved' | 'rejected' | 'needs_regrade',
    reviewNotes: '',
  });
  const [manualScores, setManualScores] = useState<{ [key: number]: number }>({});
  const [teacherExplanations, setTeacherExplanations] = useState<{ [key: number]: string }>({});

  // Fetch submissions callback
  const fetchSubmissions = useCallback(async () => {
    setLoading(true);
    try {
      // Try to fetch from API
      const data = await aiReviewService.getSubmissions(
        filterStatus === 'all' ? undefined : filterStatus,
        filterConfidence === 'all' ? undefined : filterConfidence,
        searchTerm || undefined
      );
      
      setSubmissions(data);
      setFilteredSubmissions(data);

      // Calculate stats from data
      const statsData = {
        totalPending: data.filter(s => s.reviewStatus === 'pending').length,
        totalApproved: data.filter(s => s.reviewStatus === 'approved').length,
        totalRejected: data.filter(s => s.reviewStatus === 'rejected').length,
        lowConfidence: data.filter(s => s.confidenceScore < 0.70).length,
        avgConfidence: data.reduce((acc, s) => acc + s.confidenceScore, 0) / (data.length || 1),
        needsAttention: data.filter(s => s.reviewStatus === 'pending' && s.confidenceScore < 0.70).length,
      };
      setStats(statsData);
    } catch (error) {
      console.error('API unavailable, using mock data:', error);
      
      // Fallback to mock data if API fails
      const mockData: AIGradedSubmission[] = [
        {
          id: 1,
          userId: 2,
          userName: 'John Doe',
          userEmail: 'john@example.com',
          exerciseId: 1,
          exerciseTitle: '[AI] A1 Vocabulary - Animals',
          exerciseCode: 'VOC-A1-001',
          exerciseLevel: 'A1',
          exerciseType: 'quiz',
          aiGenerated: true, // Bài AI tạo - CẦN review
          originalScore: 85,
          confidenceScore: 0.92,
          reviewStatus: 'pending',
          completedAt: new Date().toISOString(),
          totalQuestions: 10,
          userAnswers: {},
        },
        {
          id: 2,
          userId: 3,
          userName: 'Mary Jane',
          userEmail: 'mary@example.com',
          exerciseId: 2,
          exerciseTitle: '[AI] A2 Vocabulary - Jobs',
          exerciseCode: 'VOC-A2-045',
          exerciseLevel: 'A2',
          exerciseType: 'multiple_choice',
          aiGenerated: true, // Bài AI tạo - CẦN review
          originalScore: 65,
          confidenceScore: 0.68,
          reviewStatus: 'pending',
          completedAt: new Date(Date.now() - 3600000).toISOString(),
          totalQuestions: 15,
          userAnswers: {},
        },
        {
          id: 3,
          userId: 4,
          userName: 'Tom Cruise',
          userEmail: 'tom@example.com',
          exerciseId: 3,
          exerciseTitle: '[AI] A1 Grammar - Present',
          exerciseCode: 'GRA-A1-012',
          exerciseLevel: 'A1',
          exerciseType: 'quiz',
          aiGenerated: true, // Bài AI tạo - CẦN review
          originalScore: 90,
          finalScore: 95,
          confidenceScore: 0.88,
          reviewStatus: 'approved',
          completedAt: new Date(Date.now() - 7200000).toISOString(),
          reviewedAt: new Date(Date.now() - 1800000).toISOString(),
          reviewedBy: 1,
          reviewNotes: 'Excellent work, minor adjustment on question 5',
          totalQuestions: 12,
          userAnswers: {},
        },
      ];

      // Fallback mock data
      setSubmissions(mockData);
      setFilteredSubmissions(mockData);
      
      const fallbackStats = {
        totalPending: mockData.filter(s => s.reviewStatus === 'pending').length,
        totalApproved: mockData.filter(s => s.reviewStatus === 'approved').length,
        totalRejected: mockData.filter(s => s.reviewStatus === 'rejected').length,
        lowConfidence: mockData.filter(s => s.confidenceScore < 0.70).length,
        avgConfidence: mockData.reduce((acc, s) => acc + s.confidenceScore, 0) / mockData.length,
        needsAttention: mockData.filter(s => s.reviewStatus === 'pending' && s.confidenceScore < 0.70).length,
      };
      setStats(fallbackStats);
    } finally {
      setLoading(false);
    }
  }, [toast, filterStatus, filterConfidence, searchTerm]);

  const applyFilters = useCallback(() => {
    let filtered = [...submissions];

    // Search filter
    if (searchTerm) {
      filtered = filtered.filter(s => 
        s.userName.toLowerCase().includes(searchTerm.toLowerCase()) ||
        s.userEmail.toLowerCase().includes(searchTerm.toLowerCase()) ||
        s.exerciseTitle.toLowerCase().includes(searchTerm.toLowerCase())
      );
    }

    // Status filter
    if (filterStatus !== 'all') {
      filtered = filtered.filter(s => s.reviewStatus === filterStatus);
    }

    // Confidence filter
    if (filterConfidence !== 'all') {
      if (filterConfidence === 'low') {
        filtered = filtered.filter(s => s.confidenceScore < 0.75);
      } else if (filterConfidence === 'medium') {
        filtered = filtered.filter(s => s.confidenceScore >= 0.75 && s.confidenceScore < 0.9);
      } else if (filterConfidence === 'high') {
        filtered = filtered.filter(s => s.confidenceScore >= 0.9);
      }
    }

    setFilteredSubmissions(filtered);
  }, [submissions, searchTerm, filterStatus, filterConfidence]);

  // Effects
  useEffect(() => {
    fetchSubmissions();
  }, [fetchSubmissions]);

  useEffect(() => {
    applyFilters();
  }, [applyFilters]);

  const handleReviewClick = async (submission: AIGradedSubmission) => {
    setSelectedSubmission(submission);
    setReviewForm({
      finalScore: submission.finalScore || submission.originalScore,
      reviewStatus: 'approved',
      reviewNotes: submission.reviewNotes || '',
    });
    setReviewDialogOpen(true);
    
    // Fetch detailed questions and answers - pass submission directly
    await fetchSubmissionDetails(submission);
  };

  const fetchSubmissionDetails = async (submission: AIGradedSubmission) => {
    setLoadingDetails(true);
    try {
      // Fetch detailed submission data with questions from API
      const details = await aiReviewService.getSubmissionDetails(submission.id);
      
      // Parse JSON strings from API response
      let userAnswersData: any[] = [];
      let questionsData: any[] = [];
      let correctAnswersData: any[] = [];
      
      try {
        userAnswersData = JSON.parse(details.userAnswers || '[]');
        questionsData = JSON.parse(details.questionsJson || '[]');
        correctAnswersData = JSON.parse(details.correctAnswersJson || '[]');
      } catch (e) {
        console.error('Failed to parse JSON data:', e);
      }

      // Create questions array with real data from database
      const questions: QuestionDetail[] = questionsData.map((q: any, index: number) => {
        const questionNumber = q.questionNumber || index + 1;
        
        // Get user answer from user_answers_json
        const userAnswerObj = userAnswersData.find((a: any) => a.questionNumber === questionNumber);
        const userAnswer = userAnswerObj?.selectedAnswer || '';
        
        // Get correct answer from correct_answers_json
        const correctAnswerObj = correctAnswersData.find((a: any) => a.questionNumber === questionNumber);
        const correctAnswer = correctAnswerObj?.correctAnswer || '';
        
        const isCorrect = userAnswer === correctAnswer;
        const pointsPerQuestion = 100 / submission.totalQuestions;
        
        return {
          questionNumber,
          questionText: q.questionText || q.question || q.text || `Câu ${questionNumber}`,
          options: q.options || [
            { label: 'A', text: 'Option A' },
            { label: 'B', text: 'Option B' },
            { label: 'C', text: 'Option C' },
            { label: 'D', text: 'Option D' },
          ],
          userAnswer: userAnswer,
          correctAnswer: correctAnswer,
          isCorrect: isCorrect,
          points: isCorrect ? pointsPerQuestion : 0,
          aiExplanation: isCorrect 
            ? `Học viên đã chọn đáp án ${userAnswer}. Đáp án đúng.`
            : `Học viên đã chọn đáp án ${userAnswer}. Đáp án đúng là ${correctAnswer}.`,
        };
      });

      setSelectedSubmission(prev => prev ? { ...prev, questions } : null);
      
      // Initialize manual scores
      const scores: { [key: number]: number } = {};
      questions.forEach(q => {
        scores[q.questionNumber] = q.points;
      });
      setManualScores(scores);
      
    } catch (error) {
      console.error('Error fetching submission details:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể tải chi tiết câu hỏi',
        variant: 'destructive',
      });
    } finally {
      setLoadingDetails(false);
    }
  };

  const handleManualScoreChange = (questionNumber: number, newScore: number) => {
    setManualScores(prev => ({
      ...prev,
      [questionNumber]: newScore,
    }));
    
    // Recalculate total score
    const totalScore = Object.values({ ...manualScores, [questionNumber]: newScore })
      .reduce((sum, score) => sum + score, 0);
    setReviewForm(prev => ({ ...prev, finalScore: totalScore }));
  };

  const handleTeacherAnswerChange = (questionNumber: number, newTeacherAnswer: string) => {
    // Update selected submission's questions
    setSelectedSubmission(prev => {
      if (!prev || !prev.questions) return prev;
      
      const updatedQuestions = prev.questions.map(q => {
        if (q.questionNumber === questionNumber) {
          // Giáo viên chọn đáp án mới → Đây trở thành đáp án đúng mới
          const newCorrectAnswer = newTeacherAnswer;
          const isNowCorrect = q.userAnswer === newCorrectAnswer; // So với học viên đã chọn
          const newPoints = isNowCorrect ? 10 : 0; // Auto-calculate points
          
          // Update manual scores
          handleManualScoreChange(questionNumber, newPoints);
          
          return {
            ...q,
            teacherAnswer: newTeacherAnswer, // Lưu lại đáp án giáo viên chọn
            isCorrect: isNowCorrect,
          };
        }
        return q;
      });
      
      return { ...prev, questions: updatedQuestions };
    });
  };

  const handleSubmitReview = async () => {
    if (!selectedSubmission) return;

    try {
      // TODO: Replace with actual API call
      // await fetch(`/api/ai-review/${selectedSubmission.id}`, {
      //   method: 'PUT',
      //   body: JSON.stringify(reviewForm),
      // });

      toast({
        title: 'Thành công',
        description: 'Đã lưu kết quả review',
      });

      setReviewDialogOpen(false);
      fetchSubmissions();
    } catch (error) {
      console.error('Error submitting review:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể lưu kết quả review',
        variant: 'destructive',
      });
    }
  };

  const getStatusBadge = (status: string) => {
    const configs = {
      pending: {
        label: 'Chờ review',
        className: 'bg-yellow-100 text-yellow-700 dark:bg-yellow-900 dark:text-yellow-300',
      },
      approved: {
        label: 'Đã duyệt',
        className: 'bg-green-100 text-green-700 dark:bg-green-900 dark:text-green-300',
      },
      rejected: {
        label: 'Từ chối',
        className: 'bg-red-100 text-red-700 dark:bg-red-900 dark:text-red-300',
      },
      needs_regrade: {
        label: 'Cần chấm lại',
        className: 'bg-orange-100 text-orange-700 dark:bg-orange-900 dark:text-orange-300',
      },
    };

    const config = configs[status as keyof typeof configs];
    return (
      <Badge variant="outline" className={config.className}>
        {config.label}
      </Badge>
    );
  };

  const getConfidenceBadge = (score: number) => {
    if (score >= 0.9) {
      return <Badge className="bg-green-100 text-green-700">Cao ({(score * 100).toFixed(0)}%)</Badge>;
    } else if (score >= 0.75) {
      return <Badge className="bg-blue-100 text-blue-700">Trung bình ({(score * 100).toFixed(0)}%)</Badge>;
    } else {
      return <Badge className="bg-red-100 text-red-700">Thấp ({(score * 100).toFixed(0)}%)</Badge>;
    }
  };

  const formatDate = (dateString: string) => {
    return new Date(dateString).toLocaleString('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
    });
  };

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">
            Review AI Grading
          </h1>
          <p className="text-gray-500 dark:text-gray-400 mt-1">
            Giám sát và kiểm tra chất lượng chấm điểm của AI
          </p>
          <div className="flex items-center gap-2 mt-2">
            <Badge variant="outline" className="text-xs">
              <CheckCircle className="w-3 h-3 mr-1" />
              Chỉ bài tập trắc nghiệm do AI tạo
            </Badge>
            <Badge variant="outline" className="text-xs text-muted-foreground">
              <XCircle className="w-3 h-3 mr-1" />
              Không bao gồm bài admin upload
            </Badge>
          </div>
        </div>
        <Button onClick={fetchSubmissions} disabled={loading}>
          <RefreshCw className={`w-4 h-4 mr-2 ${loading ? 'animate-spin' : ''}`} />
          Làm mới
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Chờ Review</CardTitle>
            <Clock className="h-4 w-4 text-yellow-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalPending}</div>
            <p className="text-xs text-muted-foreground">
              {stats.needsAttention} cần chú ý
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Đã Duyệt</CardTitle>
            <CheckCircle className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.totalApproved}</div>
            <p className="text-xs text-muted-foreground">
              Đạt chất lượng
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Độ Tin Cậy TB</CardTitle>
            <TrendingUp className="h-4 w-4 text-blue-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {(stats.avgConfidence * 100).toFixed(1)}%
            </div>
            <p className="text-xs text-muted-foreground">
              {stats.lowConfidence} bài confidence thấp
            </p>
          </CardContent>
        </Card>

        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Cần Xem Lại</CardTitle>
            <AlertTriangle className="h-4 w-4 text-orange-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">{stats.needsAttention}</div>
            <p className="text-xs text-muted-foreground">
              Ưu tiên cao
            </p>
          </CardContent>
        </Card>
      </div>

      {/* Filters */}
      <Card>
        <CardContent className="pt-6">
          <div className="flex flex-col md:flex-row gap-4">
            <div className="flex-1">
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-4 h-4" />
                <Input
                  placeholder="Tìm theo tên, email hoặc bài tập..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="pl-10"
                />
              </div>
            </div>
            <Select value={filterStatus} onValueChange={setFilterStatus}>
              <SelectTrigger className="w-full md:w-[200px]">
                <SelectValue placeholder="Trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả trạng thái</SelectItem>
                <SelectItem value="pending">Chờ review</SelectItem>
                <SelectItem value="approved">Đã duyệt</SelectItem>
                <SelectItem value="rejected">Từ chối</SelectItem>
                <SelectItem value="needs_regrade">Cần chấm lại</SelectItem>
              </SelectContent>
            </Select>
            <Select value={filterConfidence} onValueChange={setFilterConfidence}>
              <SelectTrigger className="w-full md:w-[200px]">
                <SelectValue placeholder="Độ tin cậy" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả</SelectItem>
                <SelectItem value="low">Thấp (&lt; 75%)</SelectItem>
                <SelectItem value="medium">Trung bình (75-90%)</SelectItem>
                <SelectItem value="high">Cao (≥ 90%)</SelectItem>
              </SelectContent>
            </Select>
          </div>
        </CardContent>
      </Card>

      {/* Submissions List */}
      <Card>
        <CardHeader>
          <CardTitle>Danh sách bài chấm ({filteredSubmissions.length})</CardTitle>
          <CardDescription>
            Các bài tập đã được AI chấm điểm và cần review
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {loading ? (
              <div className="text-center py-8 text-gray-500">Đang tải...</div>
            ) : filteredSubmissions.length === 0 ? (
              <div className="text-center py-8 text-gray-500">
                Không tìm thấy bài nào phù hợp
              </div>
            ) : (
              filteredSubmissions.map((submission) => (
                <Card key={submission.id} className="hover:shadow-md transition-shadow">
                  <CardContent className="p-6">
                    <div className="flex items-start justify-between">
                      <div className="flex-1 space-y-3">
                        {/* Header Row */}
                        <div className="flex items-center gap-3 flex-wrap">
                          <h3 className="font-semibold text-lg">
                            {submission.exerciseTitle}
                          </h3>
                          <Badge variant="outline">{submission.exerciseLevel}</Badge>
                          {getStatusBadge(submission.reviewStatus)}
                          {getConfidenceBadge(submission.confidenceScore)}
                        </div>

                        {/* User Info */}
                        <div className="flex items-center gap-6 text-sm text-gray-600 dark:text-gray-400">
                          <div className="flex items-center gap-2">
                            <User className="w-4 h-4" />
                            <span>{submission.userName}</span>
                          </div>
                          <div className="flex items-center gap-2">
                            <Calendar className="w-4 h-4" />
                            <span>{formatDate(submission.completedAt)}</span>
                          </div>
                          <div className="flex items-center gap-2">
                            <FileText className="w-4 h-4" />
                            <span>{submission.totalQuestions} câu</span>
                          </div>
                        </div>

                        {/* Score Info */}
                        <div className="flex items-center gap-4">
                          <div className="flex items-center gap-2">
                            <Award className="w-4 h-4 text-primary" />
                            <span className="font-medium">
                              Điểm AI: {submission.originalScore}
                            </span>
                          </div>
                          {submission.finalScore && (
                            <div className="flex items-center gap-2">
                              <CheckCircle className="w-4 h-4 text-green-600" />
                              <span className="font-medium text-green-600">
                                Điểm cuối: {submission.finalScore}
                              </span>
                            </div>
                          )}
                        </div>

                        {/* Review Notes */}
                        {submission.reviewNotes && (
                          <div className="bg-muted/50 rounded-lg p-3 text-sm">
                            <p className="text-muted-foreground">
                              <strong>Ghi chú:</strong> {submission.reviewNotes}
                            </p>
                          </div>
                        )}
                      </div>

                      {/* Action Button */}
                      <Button
                        variant={submission.reviewStatus === 'pending' ? 'default' : 'outline'}
                        size="sm"
                        onClick={() => handleReviewClick(submission)}
                        className="ml-4"
                      >
                        <Eye className="w-4 h-4 mr-2" />
                        {submission.reviewStatus === 'pending' ? 'Review' : 'Xem lại'}
                      </Button>
                    </div>
                  </CardContent>
                </Card>
              ))
            )}
          </div>
        </CardContent>
      </Card>

      {/* Review Dialog */}
      <Dialog open={reviewDialogOpen} onOpenChange={setReviewDialogOpen}>
        <DialogContent className="max-w-4xl max-h-[90vh] overflow-hidden flex flex-col">
          <DialogHeader>
            <DialogTitle>Review Bài Chấm của AI</DialogTitle>
            <DialogDescription>
              Kiểm tra và điều chỉnh kết quả chấm điểm
            </DialogDescription>
          </DialogHeader>

          {selectedSubmission && (
            <div className="flex-1 overflow-hidden flex flex-col">
              {/* Single Combined View */}
              <div className="flex-1 overflow-y-auto py-4">
                <div className="space-y-6">
                  {/* Header Section */}
                  <div className="flex items-center gap-3 pb-4 border-b">
                    <div className="w-12 h-12 rounded-full bg-gradient-to-br from-blue-500 to-blue-600 flex items-center justify-center text-white font-bold text-lg">
                      {selectedSubmission.userName.charAt(0).toUpperCase()}
                    </div>
                    <div className="flex-1">
                      <h3 className="font-semibold text-lg">{selectedSubmission.userName}</h3>
                      <p className="text-sm text-muted-foreground">{selectedSubmission.userEmail}</p>
                    </div>
                  </div>

                  {/* Info Cards Grid */}
                  <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                    {/* Exercise Info */}
                    <Card className="border-l-4 border-l-blue-500">
                      <CardContent className="p-4">
                        <div className="flex items-start gap-3">
                          <div className="w-10 h-10 rounded-lg bg-blue-100 flex items-center justify-center flex-shrink-0">
                            <FileText className="w-5 h-5 text-blue-600" />
                          </div>
                          <div className="flex-1 min-w-0">
                            <p className="text-xs text-muted-foreground mb-1">Bài tập</p>
                            <p className="font-semibold text-sm truncate">{selectedSubmission.exerciseTitle}</p>
                            {selectedSubmission.exerciseCode && (
                              <p className="text-xs text-muted-foreground font-mono mt-0.5">
                                Mã: {selectedSubmission.exerciseCode}
                              </p>
                            )}
                            <div className="flex items-center gap-2 mt-1">
                              <Badge variant="outline" className="text-xs">{selectedSubmission.exerciseLevel}</Badge>
                              <span className="text-xs text-muted-foreground">
                                {selectedSubmission.totalQuestions} câu
                              </span>
                            </div>
                          </div>
                        </div>
                      </CardContent>
                    </Card>

                    {/* AI Score */}
                    <Card className="border-l-4 border-l-purple-500">
                      <CardContent className="p-4">
                        <div className="flex items-start gap-3">
                          <div className="w-10 h-10 rounded-lg bg-purple-100 flex items-center justify-center flex-shrink-0">
                            <Award className="w-5 h-5 text-purple-600" />
                          </div>
                          <div className="flex-1">
                            <p className="text-xs text-muted-foreground mb-1">Điểm AI chấm</p>
                            <p className="font-bold text-2xl text-purple-600">{selectedSubmission.originalScore}</p>
                            <div className="flex items-center gap-2 mt-1">
                              <div className="flex items-center gap-1">
                                <TrendingUp className="w-3 h-3 text-green-600" />
                                <span className="text-xs font-medium text-green-600">
                                  {(selectedSubmission.confidenceScore * 100).toFixed(0)}%
                                </span>
                              </div>
                              <span className="text-xs text-muted-foreground">độ tin cậy</span>
                            </div>
                          </div>
                        </div>
                      </CardContent>
                    </Card>
                  </div>

                  {/* Questions Section */}
                  <div className="space-y-4 mt-6">
                    {loadingDetails ? (
                      <div className="flex items-center justify-center py-12">
                        <div className="text-center space-y-2">
                          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-primary mx-auto" />
                          <p className="text-sm text-muted-foreground">Đang tải câu hỏi...</p>
                        </div>
                      </div>
                    ) : (
                      <div className="space-y-4">
                        {selectedSubmission.questions?.map((question) => (
                          <Card key={question.questionNumber} className={question.isCorrect ? 'border-green-200' : 'border-red-200'}>
                            <CardContent className="p-4 space-y-3">
                              {/* Question Header */}
                              <div className="flex items-start justify-between">
                                <div className="flex-1">
                                  <div className="flex items-center gap-2 mb-2">
                                    <Badge variant="outline">Câu {question.questionNumber}</Badge>
                                    {question.isCorrect ? (
                                      <Badge className="bg-green-100 text-green-800 hover:bg-green-100">
                                        <CheckCircle className="w-3 h-3 mr-1" />
                                        Đúng
                                      </Badge>
                                    ) : (
                                      <Badge variant="destructive">
                                        <XCircle className="w-3 h-3 mr-1" />
                                        Sai
                                      </Badge>
                                    )}
                                  </div>
                                  <p className="font-medium text-sm">{question.questionText}</p>
                                </div>
                              </div>

                              <Separator />

                              {/* Multiple Choice Options - Interactive */}
                              <div className="space-y-2">
                                <div className="flex items-center justify-between mb-2">
                                  <p className="text-xs font-semibold text-muted-foreground">
                                    Các lựa chọn:
                                  </p>
                                  <p className="text-xs text-amber-600 font-medium italic">
                                    👨‍🏫 Click để chọn đáp án đúng
                                  </p>
                                </div>
                                <div className="grid grid-cols-1 gap-2">
                                  {question.options.map((option) => {
                                    const isUserChoice = option.label === question.userAnswer; // Học viên chọn gì
                                    const isOriginalCorrect = option.label === question.correctAnswer; // Đáp án đúng ban đầu
                                    const isTeacherChoice = option.label === question.teacherAnswer; // Giáo viên chọn gì (đáp án đúng mới)
                                    const finalCorrectAnswer = question.teacherAnswer || question.correctAnswer;
                                    const isFinalCorrect = option.label === finalCorrectAnswer;
                                    
                                    return (
                                      <button
                                        key={option.label}
                                        type="button"
                                        onClick={() => handleTeacherAnswerChange(question.questionNumber, option.label)}
                                        className={`rounded-lg p-3 border-2 transition-all text-left hover:shadow-md ${
                                          isTeacherChoice
                                            ? 'bg-amber-50 border-amber-400 hover:bg-amber-100'
                                            : isFinalCorrect
                                            ? 'bg-green-50 border-green-400 hover:bg-green-100'
                                            : isUserChoice
                                            ? 'bg-purple-50 border-purple-300 hover:bg-purple-100'
                                            : 'bg-gray-50 border-gray-200 hover:bg-gray-100 hover:border-amber-300'
                                        }`}
                                      >
                                        <div className="flex items-start gap-3">
                                          {/* Option Label */}
                                          <div
                                        className={`flex-shrink-0 w-8 h-8 rounded-full flex items-center justify-center font-bold text-sm ${
                                          isTeacherChoice
                                            ? 'bg-amber-500 text-white'
                                            : isFinalCorrect
                                            ? 'bg-green-500 text-white'
                                            : isUserChoice
                                            ? 'bg-purple-500 text-white'
                                            : 'bg-gray-300 text-gray-700'
                                        }`}
                                      >
                                        {option.label}
                                      </div>

                                      {/* Option Text */}
                                      <div className="flex-1">
                                        <p className={`text-sm font-medium ${
                                          isTeacherChoice
                                            ? 'text-amber-700'
                                            : isFinalCorrect
                                            ? 'text-green-700'
                                            : isUserChoice
                                            ? 'text-purple-700'
                                            : 'text-gray-700'
                                        }`}>
                                          {option.text}
                                        </p>
                                        
                                        {/* Indicators */}
                                        <div className="flex gap-2 mt-1 flex-wrap">
                                          {isUserChoice && (
                                            <Badge className="bg-purple-100 text-purple-700 text-xs">
                                              <User className="w-3 h-3 mr-1" />
                                              Học viên chọn
                                            </Badge>
                                          )}
                                          {isOriginalCorrect && (
                                            <Badge className="bg-blue-100 text-blue-700 text-xs">
                                              <CheckCircle className="w-3 h-3 mr-1" />
                                              AI chấm
                                            </Badge>
                                          )}
                                          {isTeacherChoice && (
                                            <Badge className="bg-amber-100 text-amber-700 text-xs font-semibold">
                                              <User className="w-3 h-3 mr-1" />
                                              Giáo viên chọn
                                            </Badge>
                                          )}
                                          {isUserChoice && !isFinalCorrect && (
                                            <Badge variant="destructive" className="text-xs">
                                              <XCircle className="w-3 h-3 mr-1" />
                                              Sai
                                            </Badge>
                                          )}
                                        </div>
                                          </div>
                                        </div>
                                      </button>
                                    );
                                  })}
                                </div>
                              </div>

                              {/* AI Explanation */}
                              {question.aiExplanation && (
                                <div className="bg-muted/50 rounded-lg p-3">
                                  <p className="text-xs font-semibold mb-1 text-muted-foreground">
                                    Giải thích của AI:
                                  </p>
                                  <p className="text-sm">{question.aiExplanation}</p>
                                </div>
                              )}

                                  <Separator />

                              {/* Scoring Section: AI vs Teacher - Auto calculated */}
                              <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                                {/* AI Original Score */}
                                <div className="bg-blue-50 border border-blue-200 rounded-lg p-3">
                                  <div className="flex items-center gap-2 mb-2">
                                    <div className="w-6 h-6 rounded-full bg-blue-500 flex items-center justify-center">
                                      <span className="text-white text-xs font-bold">AI</span>
                                    </div>
                                    <p className="text-xs font-semibold text-muted-foreground">
                                      Chấm ban đầu:
                                    </p>
                                  </div>
                                  <p className="text-2xl font-bold text-blue-700">
                                    {question.points} <span className="text-sm font-normal text-muted-foreground">/ 10 điểm</span>
                                  </p>
                                </div>

                                {/* Teacher Score - Auto calculated */}
                                <div className={`rounded-lg p-3 border ${
                                  question.teacherAnswer
                                    ? 'bg-amber-50 border-amber-400 border-2'
                                    : 'bg-gray-50 border-gray-200'
                                }`}>
                                  <div className="flex items-center gap-2 mb-2">
                                    <div className="w-6 h-6 rounded-full bg-amber-500 flex items-center justify-center">
                                      <User className="w-4 h-4 text-white" />
                                    </div>
                                    <p className="text-xs font-semibold text-muted-foreground">
                                      Giáo viên chấm lại:
                                    </p>
                                  </div>
                                  <p className={`text-2xl font-bold ${
                                    question.teacherAnswer
                                      ? 'text-amber-700'
                                      : 'text-gray-700'
                                  }`}>
                                    {manualScores[question.questionNumber] ?? question.points}{' '}
                                    <span className="text-sm font-normal text-muted-foreground">/ 10 điểm</span>
                                  </p>
                                  {question.teacherAnswer ? (
                                    <div className="mt-1 space-y-1">
                                      <p className="text-xs text-amber-600 font-medium">
                                        Đáp án đúng mới: {question.teacherAnswer}
                                      </p>
                                      <div className="flex items-center gap-1">
                                        <AlertTriangle className="w-3 h-3 text-amber-600" />
                                        <p className="text-xs text-amber-600">
                                          Đã điều chỉnh từ {question.points} điểm
                                        </p>
                                      </div>
                                    </div>
                                  ) : (
                                    <p className="text-xs text-gray-500 mt-1 italic">
                                      👆 Click vào option để chọn đáp án đúng
                                    </p>
                                  )}
                                </div>
                              </div>

                              {/* Teacher Explanation - Optional */}
                              <div className="space-y-2 mt-4">
                                <Label htmlFor={`explanation-${question.questionNumber}`} className="text-sm font-medium flex items-center gap-2">
                                  <AlertTriangle className="w-4 h-4 text-amber-600" />
                                  Giải thích thay đổi (tùy chọn)
                                </Label>
                                <Textarea
                                  id={`explanation-${question.questionNumber}`}
                                  placeholder="Giải thích lý do thay đổi đáp án hoặc điểm số..."
                                  rows={3}
                                  value={teacherExplanations[question.questionNumber] || ''}
                                  onChange={(e) =>
                                    setTeacherExplanations({
                                      ...teacherExplanations,
                                      [question.questionNumber]: e.target.value,
                                    })
                                  }
                                  className="resize-none text-sm"
                                />
                              </div>
                            </CardContent>
                          </Card>
                        ))}

                        {(!selectedSubmission.questions || selectedSubmission.questions.length === 0) && (
                          <div className="text-center py-8 text-muted-foreground">
                            Không có dữ liệu câu hỏi
                          </div>
                        )}
                      </div>
                    )}

                    {/* Review Form Section - Moved to bottom */}
                    <Card className="mt-6">
                      <CardHeader className="pb-3">
                        <CardTitle className="text-base flex items-center gap-2">
                          <CheckCircle className="w-5 h-5 text-primary" />
                          Điều chỉnh kết quả
                        </CardTitle>
                      </CardHeader>
                      <CardContent className="space-y-4">
                        <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                          {/* Final Score - TODO: Auto-calculate after BE integration */}
                          <div className="space-y-2">
                            <Label htmlFor="finalScore" className="text-sm font-medium">
                              Điểm cuối cùng
                            </Label>
                            <Input
                              id="finalScore"
                              type="number"
                              min="0"
                              max="100"
                              value={reviewForm.finalScore}
                              onChange={(e) =>
                                setReviewForm({ ...reviewForm, finalScore: Number(e.target.value) })
                              }
                              className="text-lg font-semibold"
                              placeholder="Tự động tính sau khi kết nối BE"
                            />
                            <p className="text-xs text-muted-foreground">
                              💡 Điểm sẽ được tính tự động dựa trên thay đổi đáp án
                            </p>
                          </div>

                          {/* Status */}
                          <div className="space-y-2">
                            <Label htmlFor="reviewStatus" className="text-sm font-medium">
                              Trạng thái
                            </Label>
                            <Select
                              value={reviewForm.reviewStatus}
                              onValueChange={(value: 'approved' | 'rejected' | 'needs_regrade') =>
                                setReviewForm({ ...reviewForm, reviewStatus: value })
                              }
                            >
                              <SelectTrigger>
                                <SelectValue />
                              </SelectTrigger>
                              <SelectContent>
                                <SelectItem value="approved">
                                  <div className="flex items-center gap-2">
                                    <CheckCircle className="w-4 h-4 text-green-600" />
                                    <span>Duyệt</span>
                                  </div>
                                </SelectItem>
                                <SelectItem value="rejected">
                                  <div className="flex items-center gap-2">
                                    <XCircle className="w-4 h-4 text-red-600" />
                                    <span>Từ chối</span>
                                  </div>
                                </SelectItem>
                                <SelectItem value="needs_regrade">
                                  <div className="flex items-center gap-2">
                                    <AlertTriangle className="w-4 h-4 text-amber-600" />
                                    <span>Cần chấm lại</span>
                                  </div>
                                </SelectItem>
                              </SelectContent>
                            </Select>
                          </div>
                        </div>

                        {/* Notes */}
                        <div className="space-y-2">
                          <Label htmlFor="reviewNotes" className="text-sm font-medium">
                            Ghi chú tổng quan
                          </Label>
                          <Textarea
                            id="reviewNotes"
                            placeholder="Thêm ghi chú về quá trình review..."
                            rows={4}
                            value={reviewForm.reviewNotes}
                            onChange={(e) =>
                              setReviewForm({ ...reviewForm, reviewNotes: e.target.value })
                            }
                            className="resize-none"
                          />
                        </div>
                      </CardContent>
                    </Card>
                  </div>
                </div>
              </div>
            </div>
          )}

          <DialogFooter>
            <Button variant="outline" onClick={() => setReviewDialogOpen(false)}>
              Hủy
            </Button>
            <Button onClick={handleSubmitReview}>Lưu Review</Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default AIReview;
