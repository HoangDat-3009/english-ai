import { useState, useEffect } from "react";
import { format } from "date-fns";
import { vi } from "date-fns/locale";
import {
  Table,
  TableBody,
  TableCell,
  TableHead,
  TableHeader,
  TableRow,
} from "@/components/ui/table";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Skeleton } from "@/components/ui/skeleton";
import { ScrollArea } from "@/components/ui/scroll-area";
import { useToast } from "@/hooks/use-toast";
import { aiReviewService, type AIGradedSubmission } from "@/services/aiReviewService";
import { Eye, CheckCircle, XCircle, Clock, AlertCircle, ClipboardCheck, RefreshCw, Check, X, ChevronDown, Search, Filter } from "lucide-react";
import { Input } from "@/components/ui/input";

// Review status type
type ReviewStatus = 'pending' | 'approved' | 'rejected' | 'needs_regrade' | 'reviewing' | 'needs_revision';
import { cn } from "@/lib/utils";
import {
  Collapsible,
  CollapsibleContent,
  CollapsibleTrigger,
} from "@/components/ui/collapsible";

// Types based on API response
interface Submission {
  id: number;
  userId: number;
  userName: string;
  exerciseTitle: string;
  exerciseType: string;
  originalScore: number;
  finalScore?: number;
  maxScore: number;
  completedAt: string;
  reviewStatus: ReviewStatus;
  reviewNotes?: string;
  confidenceScore: number;
}

interface QuestionDetail {
  id: number;
  questionText: string;
  options: { label: string; text: string; isCorrect: boolean }[];
  correctAnswer: string;
  userAnswer: string;
  teacherAnswer: string; // For teacher's re-evaluation
  points: number;
  maxPoints: number;
  originalPoints: number; // Store original AI-graded points
  explanation?: string;
}

interface SubmissionDetail {
  id: number;
  userId: number;
  userName: string;
  exerciseTitle: string;
  exerciseType: string;
  totalScore: number;
  maxScore: number;
  submittedAt: string;
  reviewStatus: ReviewStatus;
  reviewNotes?: string;
  questions: QuestionDetail[];
}

const reviewStatusConfig: Record<ReviewStatus, { label: string; color: string; icon: React.ElementType }> = {
  pending: { label: "Chờ xử lý", color: "bg-yellow-100 text-yellow-800", icon: Clock },
  reviewing: { label: "Đang xem", color: "bg-blue-100 text-blue-800", icon: Eye },
  approved: { label: "Đã duyệt", color: "bg-green-100 text-green-800", icon: CheckCircle },
  rejected: { label: "Từ chối", color: "bg-red-100 text-red-800", icon: XCircle },
  needs_revision: { label: "Cần xem lại", color: "bg-orange-100 text-orange-800", icon: AlertCircle },
  needs_regrade: { label: "Cần chấm lại", color: "bg-purple-100 text-purple-800", icon: AlertCircle },
};

// Hàm tính điểm dựa trên đáp án
// So sánh đáp án học viên với đáp án giáo viên chọn là đúng
const calculatePoints = (
  userAnswer: string,      // Đáp án học viên đã chọn
  teacherAnswer: string,   // Đáp án GV xác nhận là đúng
  maxPoints: number
): number => {
  // Nếu học viên chọn đúng đáp án mà GV xác nhận → được điểm
  if (userAnswer === teacherAnswer) {
    return maxPoints;
  }
  return 0;
};

export default function AIReview() {
  const [submissions, setSubmissions] = useState<Submission[]>([]);
  const [selectedSubmission, setSelectedSubmission] = useState<SubmissionDetail | null>(null);
  const [isDialogOpen, setIsDialogOpen] = useState(false);
  const [loading, setLoading] = useState(true);
  const [detailLoading, setDetailLoading] = useState(false);
  const [submitting, setSubmitting] = useState(false);
  const [reviewNotes, setReviewNotes] = useState("");
  const [reviewStatus, setReviewStatus] = useState<ReviewStatus>("pending");
  const [expandedQuestions, setExpandedQuestions] = useState<Set<number>>(new Set());
  const [searchQuery, setSearchQuery] = useState("");
  const [statusFilter, setStatusFilter] = useState<ReviewStatus | "all">("all");
  const { toast } = useToast();

  // Filtered submissions based on search and status filter
  const filteredSubmissions = submissions.filter((submission) => {
    const matchesSearch = searchQuery === "" || 
      submission.userName.toLowerCase().includes(searchQuery.toLowerCase()) ||
      submission.exerciseTitle.toLowerCase().includes(searchQuery.toLowerCase());
    const matchesStatus = statusFilter === "all" || submission.reviewStatus === statusFilter;
    return matchesSearch && matchesStatus;
  });

  // Fetch submissions from API
  const fetchSubmissions = async () => {
    setLoading(true);
    try {
      const data = await aiReviewService.getSubmissions();
      // Transform API data to component format
      const transformedData: Submission[] = data.map((item) => ({
        id: item.id,
        userId: item.userId,
        userName: item.userName,
        exerciseTitle: item.exerciseTitle,
        exerciseType: item.exerciseType,
        originalScore: item.originalScore,
        finalScore: item.finalScore,
        maxScore: 100, // Total max score is 100
        completedAt: item.completedAt,
        reviewStatus: item.reviewStatus as ReviewStatus,
        reviewNotes: item.reviewNotes,
        confidenceScore: item.confidenceScore,
      }));
      setSubmissions(transformedData);
    } catch (error) {
      console.error("Error fetching submissions:", error);
      toast({
        title: "Lỗi",
        description: "Không thể tải danh sách bài nộp",
        variant: "destructive",
      });
      // Fallback to mock data for development
      setSubmissions([
        {
          id: 1,
          userId: 101,
          userName: "Nguyễn Văn A",
          exerciseTitle: "Grammar Test - Unit 5",
          exerciseType: "multiple_choice",
          originalScore: 85,
          maxScore: 100,
          completedAt: new Date().toISOString(),
          reviewStatus: "pending",
          confidenceScore: 0.85,
        },
        {
          id: 2,
          userId: 102,
          userName: "Trần Thị B",
          exerciseTitle: "Vocabulary Quiz - Animals",
          exerciseType: "multiple_choice",
          originalScore: 70,
          maxScore: 100,
          completedAt: new Date(Date.now() - 86400000).toISOString(),
          reviewStatus: "reviewing",
          confidenceScore: 0.75,
        },
        {
          id: 3,
          userId: 103,
          userName: "Lê Văn C",
          exerciseTitle: "Reading Comprehension",
          exerciseType: "multiple_choice",
          originalScore: 90,
          maxScore: 100,
          completedAt: new Date(Date.now() - 172800000).toISOString(),
          reviewStatus: "approved",
          confidenceScore: 0.92,
        },
      ]);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchSubmissions();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  // Fetch submission details
  const fetchSubmissionDetails = async (submissionId: number) => {
    setDetailLoading(true);
    try {
      const data = await aiReviewService.getSubmissionDetails(submissionId);
      
      // Parse JSON strings from API
      let questionsData: Array<{
        questionNumber?: number;
        questionText?: string;
        question?: string;
        options: Array<string | { label: string; text: string }>;
        correct_answer?: string;
        correctAnswer?: string;
        explanation?: string;
      }> = [];
      let userAnswersData: Array<{
        questionNumber: number;
        selectedAnswer: string;
      }> = [];
      let correctAnswersData: Array<string | { questionNumber: number; correctAnswer: string }> = [];

      try {
        if (data.questionsJson) {
          questionsData = JSON.parse(data.questionsJson);
        }
        if (data.userAnswers) {
          userAnswersData = JSON.parse(data.userAnswers);
        }
        if (data.correctAnswersJson) {
          correctAnswersData = JSON.parse(data.correctAnswersJson);
        }
      } catch (parseError) {
        console.error("Error parsing JSON:", parseError);
      }

      // Find the submission from list to get user info
      const submissionInfo = submissions.find(s => s.id === submissionId);

      // Transform questions from API format to component format
      const transformedQuestions: QuestionDetail[] = questionsData.map((q, idx) => {
        const questionNumber = q.questionNumber || idx + 1;
        const userAnswerObj = userAnswersData.find(ua => ua.questionNumber === questionNumber);
        const userAnswer = userAnswerObj?.selectedAnswer || "";
        
        // Get correct answer - can be string or object
        let correctAnswer = "";
        if (Array.isArray(correctAnswersData) && correctAnswersData.length > idx) {
          const ca = correctAnswersData[idx];
          if (typeof ca === 'string') {
            correctAnswer = ca;
          } else if (ca && typeof ca === 'object' && 'correctAnswer' in ca) {
            correctAnswer = ca.correctAnswer;
          }
        }
        // Fallback to question's own correct_answer
        if (!correctAnswer) {
          correctAnswer = q.correct_answer || q.correctAnswer || "";
        }
        
        const maxPoints = 10; // 10 points per question (total 100 for 10 questions)
        const isCorrect = userAnswer === correctAnswer;
        const points = isCorrect ? maxPoints : 0;

        // Parse options - can be array of strings or array of objects
        const options = (q.options || []).map((opt, optIdx) => {
          const label = String.fromCharCode(65 + optIdx); // A, B, C, D
          let text = "";
          if (typeof opt === 'string') {
            text = opt;
          } else if (opt && typeof opt === 'object') {
            text = (opt as { text?: string }).text || String(opt);
          }
          return {
            label,
            text,
            isCorrect: label === correctAnswer,
          };
        });

        return {
          id: questionNumber,
          questionText: q.questionText || q.question || `Câu hỏi ${questionNumber}`,
          options,
          correctAnswer,
          userAnswer,
          teacherAnswer: userAnswer, // Initially set to user's answer, will be overwritten if adjustments exist
          points,
          maxPoints,
          originalPoints: points,
          explanation: q.explanation,
        };
      });

      // Parse reviewNotes to get saved adjustments - use data from API details directly
      let parsedNotes = "";
      let savedAdjustments: Array<{
        questionNumber: number;
        newCorrectAnswer: string;
        teacherExplanation: string;
        newPoints: number;
      }> = [];
      
      try {
        // Use reviewNotes from API details response (more reliable than list)
        const reviewNotesRaw = data.reviewNotes || submissionInfo?.reviewNotes || "";
        if (reviewNotesRaw && reviewNotesRaw.startsWith('{')) {
          const parsed = JSON.parse(reviewNotesRaw);
          parsedNotes = parsed.notes || "";
          savedAdjustments = parsed.adjustments || [];
        } else {
          parsedNotes = reviewNotesRaw;
        }
      } catch {
        parsedNotes = data.reviewNotes || submissionInfo?.reviewNotes || "";
      }

      // Apply saved adjustments to questions
      const questionsWithAdjustments = transformedQuestions.map((q) => {
        const adjustment = savedAdjustments.find(adj => adj.questionNumber === q.id);
        if (adjustment) {
          return {
            ...q,
            teacherAnswer: adjustment.newCorrectAnswer,
            points: adjustment.newPoints,
            explanation: adjustment.teacherExplanation || q.explanation,
          };
        }
        return q;
      });

      // Calculate total score from adjusted questions
      const totalScore = questionsWithAdjustments.reduce((sum, q) => sum + q.points, 0);
      const maxScore = questionsWithAdjustments.length * 10; // 10 points per question = 100 total

      const transformedData: SubmissionDetail = {
        id: data.id,
        userId: data.userId || submissionInfo?.userId || 0,
        userName: data.userName || submissionInfo?.userName || "Unknown",
        exerciseTitle: data.exerciseTitle || submissionInfo?.exerciseTitle || "Unknown Exercise",
        exerciseType: data.exerciseType || submissionInfo?.exerciseType || "multiple_choice",
        totalScore,
        maxScore,
        submittedAt: submissionInfo?.completedAt || new Date().toISOString(),
        reviewStatus: (data.reviewStatus || submissionInfo?.reviewStatus || "pending") as ReviewStatus,
        reviewNotes: parsedNotes,
        questions: questionsWithAdjustments,
      };

      setSelectedSubmission(transformedData);
      setReviewNotes(transformedData.reviewNotes || "");
      setReviewStatus(transformedData.reviewStatus);
      // Expand all questions by default for review
      setExpandedQuestions(new Set(questionsWithAdjustments.map((_, idx) => idx)));
    } catch (error) {
      console.error("Error fetching submission details:", error);
      toast({
        title: "Lỗi",
        description: "Không thể tải chi tiết bài nộp",
        variant: "destructive",
      });
      // Fallback to mock data
      const mockDetail: SubmissionDetail = {
        id: submissionId,
        userId: 101,
        userName: "Nguyễn Văn A",
        exerciseTitle: "Grammar Test - Unit 5",
        exerciseType: "multiple_choice",
        totalScore: 85,
        maxScore: 100,
        submittedAt: new Date().toISOString(),
        reviewStatus: "pending",
        reviewNotes: "",
        questions: [
          {
            id: 1,
            questionText: "Choose the correct form of the verb: She ___ to school every day.",
            options: [
              { label: "A", text: "go", isCorrect: false },
              { label: "B", text: "goes", isCorrect: true },
              { label: "C", text: "going", isCorrect: false },
              { label: "D", text: "gone", isCorrect: false },
            ],
            correctAnswer: "B",
            userAnswer: "B",
            teacherAnswer: "B",
            points: 10,
            maxPoints: 10,
            originalPoints: 10,
            explanation: "Với chủ ngữ ngôi thứ 3 số ít (she), động từ phải thêm -s hoặc -es.",
          },
          {
            id: 2,
            questionText: "Which sentence is correct?",
            options: [
              { label: "A", text: "He don't like coffee.", isCorrect: false },
              { label: "B", text: "He doesn't likes coffee.", isCorrect: false },
              { label: "C", text: "He doesn't like coffee.", isCorrect: true },
              { label: "D", text: "He not like coffee.", isCorrect: false },
            ],
            correctAnswer: "C",
            userAnswer: "B",
            teacherAnswer: "B",
            points: 0,
            maxPoints: 10,
            originalPoints: 0,
            explanation: "Câu phủ định với ngôi thứ 3 số ít dùng 'doesn't + V(nguyên thể)'.",
          },
          {
            id: 3,
            questionText: "Fill in the blank: I ___ (have) breakfast at 7 AM.",
            options: [
              { label: "A", text: "have", isCorrect: true },
              { label: "B", text: "has", isCorrect: false },
              { label: "C", text: "having", isCorrect: false },
              { label: "D", text: "had", isCorrect: false },
            ],
            correctAnswer: "A",
            userAnswer: "A",
            teacherAnswer: "A",
            points: 10,
            maxPoints: 10,
            originalPoints: 10,
          },
          {
            id: 4,
            questionText: "Choose the correct answer: They ___ football every weekend.",
            options: [
              { label: "A", text: "plays", isCorrect: false },
              { label: "B", text: "play", isCorrect: true },
              { label: "C", text: "playing", isCorrect: false },
              { label: "D", text: "played", isCorrect: false },
            ],
            correctAnswer: "B",
            userAnswer: "B",
            teacherAnswer: "B",
            points: 10,
            maxPoints: 10,
            originalPoints: 10,
            explanation: "Với chủ ngữ số nhiều (they), động từ giữ nguyên dạng nguyên thể.",
          },
          {
            id: 5,
            questionText: "Select the grammatically correct sentence:",
            options: [
              { label: "A", text: "She can sings well.", isCorrect: false },
              { label: "B", text: "She can sing well.", isCorrect: true },
              { label: "C", text: "She cans sing well.", isCorrect: false },
              { label: "D", text: "She can to sing well.", isCorrect: false },
            ],
            correctAnswer: "B",
            userAnswer: "A",
            teacherAnswer: "A",
            points: 0,
            maxPoints: 10,
            originalPoints: 0,
            explanation: "Sau động từ khuyết thiếu 'can', động từ chính giữ nguyên dạng nguyên thể.",
          },
        ],
      };
      setSelectedSubmission(mockDetail);
      setReviewNotes(mockDetail.reviewNotes || "");
      setReviewStatus(mockDetail.reviewStatus);
      setExpandedQuestions(new Set(mockDetail.questions.map((_, idx) => idx)));
    } finally {
      setDetailLoading(false);
    }
  };

  // Handle teacher answer change - GV chọn đáp án đúng mới
  const handleTeacherAnswerChange = (questionIndex: number, newCorrectAnswer: string) => {
    if (!selectedSubmission) return;

    const updatedQuestions = selectedSubmission.questions.map((q, idx) => {
      if (idx === questionIndex) {
        // So sánh đáp án học viên với đáp án mới mà GV chọn là đúng
        const newPoints = calculatePoints(q.userAnswer, newCorrectAnswer, q.maxPoints);
        return {
          ...q,
          teacherAnswer: newCorrectAnswer, // Đáp án GV xác nhận là đúng
          points: newPoints,
        };
      }
      return q;
    });

    // Calculate new total score
    const newTotalScore = updatedQuestions.reduce((sum, q) => sum + q.points, 0);

    setSelectedSubmission({
      ...selectedSubmission,
      questions: updatedQuestions,
      totalScore: newTotalScore,
    });
  };

  // Toggle question expansion
  const toggleQuestion = (index: number) => {
    const newExpanded = new Set(expandedQuestions);
    if (newExpanded.has(index)) {
      newExpanded.delete(index);
    } else {
      newExpanded.add(index);
    }
    setExpandedQuestions(newExpanded);
  };

  // Handle review submission
  const handleSubmitReview = async () => {
    if (!selectedSubmission) return;

    setSubmitting(true);
    try {
      // Prepare question adjustments
      const questionAdjustments = selectedSubmission.questions.map((q, idx) => ({
        questionNumber: idx + 1,
        newCorrectAnswer: q.teacherAnswer,
        teacherExplanation: q.explanation || '',
        newPoints: q.points,
        isCorrect: q.teacherAnswer === q.correctAnswer,
      }));

      // Combine notes and adjustments into a single JSON string for storage
      const reviewNotesWithAdjustments = JSON.stringify({
        notes: reviewNotes,
        adjustments: questionAdjustments,
      });

      await aiReviewService.updateReview(selectedSubmission.id, {
        submissionId: selectedSubmission.id,
        finalScore: selectedSubmission.totalScore,
        reviewStatus: reviewStatus,
        reviewNotes: reviewNotesWithAdjustments,
        questionAdjustments,
      });

      toast({
        title: "Thành công",
        description: "Đã lưu đánh giá thành công",
      });

      // Update local state immediately for better UX
      setSubmissions(
        submissions.map((s) =>
          s.id === selectedSubmission.id
            ? { 
                ...s, 
                reviewStatus, 
                finalScore: selectedSubmission.totalScore, 
                originalScore: s.originalScore, // Keep original score
                reviewNotes 
              }
            : s
        )
      );

      setIsDialogOpen(false);
      
      // Refresh data from server to ensure sync
      fetchSubmissions();
    } catch (error) {
      console.error("Error submitting review:", error);
      toast({
        title: "Lỗi",
        description: "Không thể lưu đánh giá",
        variant: "destructive",
      });
    } finally {
      setSubmitting(false);
    }
  };

  const handleViewDetails = (submission: Submission) => {
    setIsDialogOpen(true);
    fetchSubmissionDetails(submission.id);
  };

  const getStatusBadge = (status: ReviewStatus) => {
    const config = reviewStatusConfig[status] || reviewStatusConfig.pending;
    const Icon = config.icon;
    return (
      <Badge className={cn("gap-1", config.color)}>
        <Icon className="h-3 w-3" />
        {config.label}
      </Badge>
    );
  };

  // Loading skeleton
  if (loading) {
    return (
      <div className="space-y-6">
        <div className="flex items-center justify-between">
          <Skeleton className="h-8 w-64" />
          <Skeleton className="h-10 w-32" />
        </div>
        <Card>
          <CardContent className="pt-6">
            <div className="space-y-4">
              {[1, 2, 3, 4, 5].map((i) => (
                <Skeleton key={i} className="h-16 w-full" />
              ))}
            </div>
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex items-center justify-between">
        <div>
          <h1 className="text-2xl font-bold tracking-tight flex items-center gap-2">
            <ClipboardCheck className="h-6 w-6" />
            Đánh giá AI
          </h1>
          <p className="text-muted-foreground">
            Xem xét và điều chỉnh điểm số bài tập được AI chấm
          </p>
        </div>
        <Button onClick={fetchSubmissions} variant="outline" className="gap-2">
          <RefreshCw className="h-4 w-4" />
          Làm mới
        </Button>
      </div>

      {/* Stats Cards */}
      <div className="grid gap-4 md:grid-cols-4">
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Chờ xử lý</CardTitle>
            <Clock className="h-4 w-4 text-yellow-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {submissions.filter((s) => s.reviewStatus === "pending").length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Đang xem</CardTitle>
            <Eye className="h-4 w-4 text-blue-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {submissions.filter((s) => s.reviewStatus === "reviewing").length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Đã duyệt</CardTitle>
            <CheckCircle className="h-4 w-4 text-green-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {submissions.filter((s) => s.reviewStatus === "approved").length}
            </div>
          </CardContent>
        </Card>
        <Card>
          <CardHeader className="flex flex-row items-center justify-between space-y-0 pb-2">
            <CardTitle className="text-sm font-medium">Cần xem lại</CardTitle>
            <AlertCircle className="h-4 w-4 text-orange-600" />
          </CardHeader>
          <CardContent>
            <div className="text-2xl font-bold">
              {submissions.filter((s) => s.reviewStatus === "needs_revision").length}
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Submissions Table */}
      <Card>
        <CardHeader>
          <CardTitle>Danh sách bài nộp</CardTitle>
        </CardHeader>
        <CardContent>
          {/* Search and Filter */}
          <div className="flex flex-col sm:flex-row gap-4 mb-4">
            <div className="relative flex-1">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Tìm kiếm theo tên học viên hoặc bài tập..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select
              value={statusFilter}
              onValueChange={(value) => setStatusFilter(value as ReviewStatus | "all")}
            >
              <SelectTrigger className="w-full sm:w-[200px]">
                <Filter className="h-4 w-4 mr-2" />
                <SelectValue placeholder="Lọc theo trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả trạng thái</SelectItem>
                <SelectItem value="pending">Chờ xử lý</SelectItem>
                <SelectItem value="reviewing">Đang xem</SelectItem>
                <SelectItem value="approved">Đã duyệt</SelectItem>
                <SelectItem value="rejected">Từ chối</SelectItem>
                <SelectItem value="needs_revision">Cần xem lại</SelectItem>
                <SelectItem value="needs_regrade">Cần chấm lại</SelectItem>
              </SelectContent>
            </Select>
          </div>
          <Table>
            <TableHeader>
              <TableRow>
                <TableHead>Học viên</TableHead>
                <TableHead>Bài tập</TableHead>
                <TableHead>Điểm</TableHead>
                <TableHead>Thời gian nộp</TableHead>
                <TableHead>Trạng thái</TableHead>
                <TableHead className="text-right">Thao tác</TableHead>
              </TableRow>
            </TableHeader>
            <TableBody>
              {filteredSubmissions.length === 0 ? (
                <TableRow>
                  <TableCell colSpan={6} className="text-center py-8">
                    <p className="text-muted-foreground">
                      {submissions.length === 0 
                        ? "Không có bài nộp nào" 
                        : "Không tìm thấy bài nộp phù hợp"}
                    </p>
                  </TableCell>
                </TableRow>
              ) : (
                filteredSubmissions.map((submission) => (
                  <TableRow key={submission.id}>
                    <TableCell className="font-medium">{submission.userName}</TableCell>
                    <TableCell>
                      <div>
                        <p className="font-medium">{submission.exerciseTitle}</p>
                        <p className="text-sm text-muted-foreground">{submission.exerciseType}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <span className="font-semibold">
                        {submission.finalScore ?? submission.originalScore}/{submission.maxScore}
                      </span>
                    </TableCell>
                    <TableCell>
                      {format(new Date(submission.completedAt), "dd/MM/yyyy HH:mm", { locale: vi })}
                    </TableCell>
                    <TableCell>{getStatusBadge(submission.reviewStatus)}</TableCell>
                    <TableCell className="text-right">
                      <Button
                        variant="outline"
                        size="sm"
                        onClick={() => handleViewDetails(submission)}
                        className="gap-1"
                      >
                        <Eye className="h-4 w-4" />
                        Xem chi tiết
                      </Button>
                    </TableCell>
                  </TableRow>
                ))
              )}
            </TableBody>
          </Table>
        </CardContent>
      </Card>

      {/* Review Dialog */}
      <Dialog open={isDialogOpen} onOpenChange={setIsDialogOpen}>
        <DialogContent className="max-w-4xl max-h-[90vh]">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <ClipboardCheck className="h-5 w-5" />
              Chi tiết bài nộp
            </DialogTitle>
            <DialogDescription>
              Xem và đánh giá chi tiết bài làm của học viên
            </DialogDescription>
          </DialogHeader>

          {detailLoading ? (
            <div className="space-y-4 py-4">
              <Skeleton className="h-20 w-full" />
              <Skeleton className="h-32 w-full" />
              <Skeleton className="h-32 w-full" />
            </div>
          ) : selectedSubmission ? (
            <ScrollArea className="max-h-[60vh] pr-4">
              <div className="space-y-6">
                {/* Submission Info */}
                <div className="grid grid-cols-2 md:grid-cols-4 gap-4 p-4 bg-muted rounded-lg">
                  <div>
                    <p className="text-sm text-muted-foreground">Học viên</p>
                    <p className="font-medium">{selectedSubmission.userName}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Bài tập</p>
                    <p className="font-medium">{selectedSubmission.exerciseTitle}</p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Thời gian nộp</p>
                    <p className="font-medium">
                      {format(new Date(selectedSubmission.submittedAt), "dd/MM/yyyy HH:mm", {
                        locale: vi,
                      })}
                    </p>
                  </div>
                  <div>
                    <p className="text-sm text-muted-foreground">Điểm hiện tại</p>
                    <p className="font-medium text-lg">
                      {selectedSubmission.totalScore.toFixed(1)}/{selectedSubmission.maxScore}
                    </p>
                  </div>
                </div>

                {/* Questions Review */}
                <div className="space-y-3">
                  <h3 className="font-semibold text-lg">Chi tiết các câu hỏi</h3>
                  <p className="text-sm text-muted-foreground mb-4">
                    Nhấn vào đáp án để thay đổi. Điểm sẽ được tính tự động.
                  </p>

                  {selectedSubmission.questions.map((question, index) => {
                    // Học viên có đúng không = so sánh userAnswer với teacherAnswer (đáp án GV chọn là đúng)
                    const isStudentCorrect = question.userAnswer === question.teacherAnswer;
                    
                    return (
                    <Collapsible
                      key={question.id}
                      open={expandedQuestions.has(index)}
                      onOpenChange={() => toggleQuestion(index)}
                    >
                      <Card
                        className={cn(
                          "transition-colors",
                          isStudentCorrect
                            ? "border-green-200 bg-green-50/30"
                            : "border-red-200 bg-red-50/30"
                        )}
                      >
                        <CollapsibleTrigger className="w-full">
                          <CardHeader className="py-3">
                            <div className="flex items-center justify-between">
                              <div className="flex items-center gap-3">
                                <span
                                  className={cn(
                                    "flex h-8 w-8 items-center justify-center rounded-full text-sm font-bold",
                                    isStudentCorrect
                                      ? "bg-green-100 text-green-700"
                                      : "bg-red-100 text-red-700"
                                  )}
                                >
                                  {index + 1}
                                </span>
                                <span className="text-sm font-medium text-left line-clamp-1 flex-1">
                                  {question.questionText}
                                </span>
                              </div>
                              <div className="flex items-center gap-3">
                                <span className="text-sm font-semibold">
                                  {question.points.toFixed(1)}/{question.maxPoints}
                                </span>
                                {isStudentCorrect ? (
                                  <Check className="h-5 w-5 text-green-600" />
                                ) : (
                                  <X className="h-5 w-5 text-red-600" />
                                )}
                                <ChevronDown
                                  className={cn(
                                    "h-4 w-4 transition-transform",
                                    expandedQuestions.has(index) && "rotate-180"
                                  )}
                                />
                              </div>
                            </div>
                          </CardHeader>
                        </CollapsibleTrigger>

                        <CollapsibleContent>
                          <CardContent className="pt-0 pb-4">
                            {/* Question Text */}
                            <p className="mb-4 text-sm">{question.questionText}</p>
                            
                            {/* Instruction */}
                            <p className="text-xs text-muted-foreground mb-3">
                              👆 Click vào đáp án để chọn đáp án đúng. Điểm sẽ tự động tính dựa trên câu trả lời của học viên.
                            </p>

                            {/* Answer Options - Clickable */}
                            <div className="grid grid-cols-1 md:grid-cols-2 gap-2 mb-4">
                              {question.options.map((option) => {
                                const isTeacherSelected = question.teacherAnswer === option.label; // GV chọn là đáp án đúng
                                const isUserAnswer = question.userAnswer === option.label; // Học viên đã chọn

                                return (
                                  <button
                                    key={option.label}
                                    type="button"
                                    onClick={() => handleTeacherAnswerChange(index, option.label)}
                                    className={cn(
                                      "flex items-center gap-3 p-3 rounded-lg border-2 text-left transition-all hover:shadow-md",
                                      isTeacherSelected
                                        ? "border-purple-500 bg-purple-50 ring-2 ring-purple-300"
                                        : "border-gray-200 hover:border-gray-300 bg-white"
                                    )}
                                  >
                                    <span
                                      className={cn(
                                        "flex h-8 w-8 shrink-0 items-center justify-center rounded-full text-sm font-bold",
                                        isTeacherSelected
                                          ? "bg-purple-500 text-white"
                                          : "bg-gray-100 text-gray-700"
                                      )}
                                    >
                                      {option.label}
                                    </span>
                                    <span className="flex-1 text-sm">{option.text}</span>
                                    <div className="flex items-center gap-2">
                                      {isUserAnswer && (
                                        <Badge variant="outline" className={cn(
                                          "text-xs",
                                          isTeacherSelected 
                                            ? "bg-green-50 border-green-300 text-green-700"
                                            : "bg-red-50 border-red-300 text-red-700"
                                        )}>
                                          {isTeacherSelected ? "✓ Học viên đúng" : "✗ Học viên sai"}
                                        </Badge>
                                      )}
                                      {isTeacherSelected && (
                                        <Badge className="text-xs bg-purple-100 text-purple-700 border-purple-300">
                                          Đáp án đúng
                                        </Badge>
                                      )}
                                    </div>
                                  </button>
                                );
                              })}
                            </div>

                            {/* Explanation */}
                            {question.explanation && (
                              <div className="p-3 bg-blue-50 rounded-lg border border-blue-100">
                                <p className="text-sm text-blue-800">
                                  <strong>Giải thích:</strong> {question.explanation}
                                </p>
                              </div>
                            )}
                          </CardContent>
                        </CollapsibleContent>
                      </Card>
                    </Collapsible>
                  );
                  })}
                </div>

                {/* Score Summary */}
                <div className="p-4 bg-gradient-to-r from-primary/10 to-primary/5 rounded-lg border">
                  <div className="flex items-center justify-between">
                    <div>
                      <p className="text-sm font-medium text-muted-foreground">Tổng điểm</p>
                      <p className="text-3xl font-bold text-primary">
                        {selectedSubmission.totalScore.toFixed(1)}/{selectedSubmission.maxScore}
                      </p>
                    </div>
                    <div className="text-right">
                      <p className="text-sm text-muted-foreground">
                        Số câu đúng: {selectedSubmission.questions.filter(q => q.userAnswer === q.teacherAnswer).length}/{selectedSubmission.questions.length}
                      </p>
                    </div>
                  </div>
                </div>

                {/* Review Form */}
                <div className="space-y-4 border-t pt-4">
                  <div className="grid gap-4 md:grid-cols-2">
                    <div className="space-y-2">
                      <Label>Trạng thái đánh giá</Label>
                      <Select
                        value={reviewStatus}
                        onValueChange={(value) => setReviewStatus(value as ReviewStatus)}
                      >
                        <SelectTrigger>
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          {Object.entries(reviewStatusConfig).map(([key, config]) => (
                            <SelectItem key={key} value={key}>
                              <div className="flex items-center gap-2">
                                <config.icon className="h-4 w-4" />
                                {config.label}
                              </div>
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  <div className="space-y-2">
                    <Label>Ghi chú đánh giá</Label>
                    <Textarea
                      value={reviewNotes}
                      onChange={(e) => setReviewNotes(e.target.value)}
                      placeholder="Nhập ghi chú đánh giá cho học viên..."
                      rows={3}
                    />
                  </div>
                </div>
              </div>
            </ScrollArea>
          ) : null}

          <DialogFooter>
            <Button variant="outline" onClick={() => setIsDialogOpen(false)}>
              Đóng
            </Button>
            <Button onClick={handleSubmitReview} disabled={submitting || detailLoading}>
              {submitting ? "Đang lưu..." : "Lưu đánh giá"}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
