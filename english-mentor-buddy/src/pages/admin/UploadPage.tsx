import { AdminReadingExercisesManager } from '@/components/admin/AdminReadingExercisesManager';
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from '@/hooks/use-toast';
import { useReadingExercises } from '@/hooks/useReadingExercises';
import { adminUploadService } from '@/services/adminUploadService';
import { CheckCircle, Clock, Eye, FileText, Minus, Plus, Save, Upload } from 'lucide-react';
import { useState } from 'react';

const UploadPage = () => {
  const [testType, setTestType] = useState('');
  const [selectedTestType, setSelectedTestType] = useState('');
  const [questions, setQuestions] = useState([{ id: 1, question: '', options: ['', '', '', ''], correct: 0 }]);
  
  // NEW: 2-Step Process States
  const [createdExerciseId, setCreatedExerciseId] = useState<number | null>(null);
  const [step1Complete, setStep1Complete] = useState(false);
  const [isCreatingPassage, setIsCreatingPassage] = useState(false);
  const [isAddingQuestions, setIsAddingQuestions] = useState(false);
  // uploadMethod removed - only text input now

  
  // Exercise creation states
  const [exerciseTitle, setExerciseTitle] = useState('');
  const [exerciseLevel, setExerciseLevel] = useState<'Beginner' | 'Intermediate' | 'Advanced'>('Intermediate');
  const [exercisePartType, setExercisePartType] = useState<'Part 5' | 'Part 6' | 'Part 7'>('Part 6');
  const [exerciseDescription, setExerciseDescription] = useState('');
  
  // NEW: Part 7 - 2 passages
  const [passage1, setPassage1] = useState('');
  const [passage2, setPassage2] = useState('');
  const [isDualPassage, setIsDualPassage] = useState(false);
  // uploadMethod removed - always use text input
  const uploadMethod: 'text' | 'file' = 'text';

  // Helper function for estimated minutes
  const getEstimatedMinutes = (partType: string): number => {
    switch (partType) {
      case 'Part 5': return 15;
      case 'Part 6': return 20; 
      case 'Part 7': return 30;
      default: return 20;
    }
  };

  const testTypes = [
    { id: 'toeic-full', label: 'TOEIC Full Test', icon: '📝', description: 'Đề thi TOEIC đầy đủ (Listening + Reading)' },
    { id: 'listening', label: 'Listening Test', icon: '🎧', description: 'Bài test kỹ năng nghe' },
    { id: 'reading', label: 'Reading Test', icon: '📖', description: 'Bài test kỹ năng đọc' },
    { id: 'speaking', label: 'Speaking Test', icon: '🗣️', description: 'Bài test kỹ năng nói' },
    { id: 'writing', label: 'Writing Test', icon: '✍️', description: 'Bài test kỹ năng viết' },
    { id: 'vocabulary', label: 'Vocabulary Test', icon: '📚', description: 'Bài test từ vựng' },
    { id: 'grammar', label: 'Grammar Test', icon: '📋', description: 'Bài test ngữ pháp' }
  ];

  // File upload handlers
  const { toast: toastFunc } = useToast();
  const success = (title: string, description: string) => toastFunc({ title, description, variant: 'default' });
  const error = (title: string, description: string) => toastFunc({ title, description, variant: 'destructive' });
  const { refreshExercises } = useReadingExercises();

  // handleUploadFile removed - only text input now
  // no-op stub kept for legacy JSX references (upload UI removed)
  const handleUploadFile = (file: File) => {
    // upload file functionality has been removed - keep stub to avoid TSX compile errors
    console.debug('handleUploadFile called but upload is disabled', file?.name);
  };

  // NEW: Step 1 - Create Passage
  const handleCreatePassage = async () => {
    // Validate basic info
    if (!exerciseTitle.trim()) {
      error('Thiếu thông tin', 'Vui lòng nhập tiêu đề');
      return;
    }

    // Validate passage content based on type
    let finalContent = '';
    if (exercisePartType === 'Part 7' && isDualPassage) {
      if (!passage1.trim() || !passage2.trim()) {
        error('Thiếu thông tin', 'Vui lòng nhập đầy đủ 2 đoạn văn cho Part 7');
        return;
      }
      finalContent = `[PASSAGE 1]\n${passage1}\n\n[PASSAGE 2]\n${passage2}`;
    } else {
      if (!exerciseDescription.trim()) {
        error('Thiếu thông tin', 'Vui lòng nhập nội dung đoạn văn');
        return;
      }
      finalContent = exerciseDescription;
    }

    setIsCreatingPassage(true);
    try {
      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5283'}/api/ReadingExercise/create-passage`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          title: exerciseTitle,
          content: finalContent,
          partType: exercisePartType,
          level: exerciseLevel,
          createdBy: 'Admin'
        })
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Failed to create passage' }));
        throw new Error(errorData.message || `HTTP ${response.status}`);
      }

      const result = await response.json();
      const exerciseId = result.exerciseId;
      
      setCreatedExerciseId(exerciseId);
      setStep1Complete(true);
      
      success('✅ Bước 1 hoàn thành!', `Đoạn văn đã được tạo với ID: ${exerciseId}. Bây giờ hãy thêm câu hỏi!`);
      
      // Auto-generate questions template based on Part Type
      const questionCount = exercisePartType === 'Part 6' ? 4 : 5;
      const newQuestions = Array.from({ length: questionCount }, (_, i) => ({
        id: i + 1,
        question: exercisePartType === 'Part 6' 
          ? `Question ${i + 1} (chọn từ phù hợp cho chỗ trống ${i + 1})`
          : `Question ${i + 1}`,
        options: ['', '', '', ''],
        correct: 0
      }));
      setQuestions(newQuestions);

    } catch (err: unknown) {
      console.error('Create passage failed:', err);
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra';
      error('❌ Tạo đoạn văn thất bại', errorMessage);
    } finally {
      setIsCreatingPassage(false);
    }
  };

  // NEW: Step 2 - Add Questions
  const handleAddQuestions = async () => {
    if (!createdExerciseId) {
      error('Chưa có Exercise ID', 'Vui lòng hoàn thành bước 1 trước');
      return;
    }

    // Validate questions
    const hasEmptyQuestions = questions.some(q => !q.question.trim() || q.options.some(opt => !opt.trim()));
    if (hasEmptyQuestions) {
      error('Câu hỏi chưa đầy đủ', 'Vui lòng điền đầy đủ tất cả câu hỏi và đáp án');
      return;
    }

    setIsAddingQuestions(true);
    try {
      const questionsPayload = questions.map((q, index) => ({
        questionText: q.question,
        optionA: q.options[0],
        optionB: q.options[1],
        optionC: q.options[2],
        optionD: q.options[3],
        correctAnswer: q.correct,
        explanation: `Explanation for question ${index + 1}`,
        orderNumber: index + 1
      }));

      const response = await fetch(`${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5283'}/api/ReadingExercise/${createdExerciseId}/add-questions`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          questions: questionsPayload
        })
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Failed to add questions' }));
        throw new Error(errorData.message || `HTTP ${response.status}`);
      }

      const result = await response.json();
      
      success('🎉 Hoàn thành!', `Bài tập đã được tạo thành công với ${questions.length} câu hỏi! Exercise đã được activate.`);
      
      // Refresh exercises list
      refreshExercises();
      
      // Reset form
      setExerciseTitle('');
      setExerciseDescription('');
      setPassage1('');
      setPassage2('');
      setIsDualPassage(false);
      setQuestions([{ id: 1, question: '', options: ['', '', '', ''], correct: 0 }]);
      setCreatedExerciseId(null);
      setStep1Complete(false);

    } catch (err: unknown) {
      console.error('Add questions failed:', err);
      const errorMessage = err instanceof Error ? err.message : 'Có lỗi xảy ra';
      error('❌ Thêm câu hỏi thất bại', errorMessage);
    } finally {
      setIsAddingQuestions(false);
    }
  };

  // Question management functions
  const updateOption = (questionId: number, optionIndex: number, value: string) => {
    setQuestions(questions.map(q => 
      q.id === questionId 
        ? { ...q, options: q.options.map((opt, idx) => idx === optionIndex ? value : opt) }
        : q
    ));
  };

  // Create Reading Exercise from uploaded content
  const createReadingExerciseFromUpload = async () => {
    if (!exerciseTitle.trim()) {
      error('Tên bài test không được để trống', 'Vui lòng nhập tên bài test.');
      return;
    }

    if (questions.length === 0 || !questions[0].question.trim()) {
      error('Cần ít nhất 1 câu hỏi', 'Vui lòng tạo ít nhất một câu hỏi cho bài test.');
      return;
    }

    try {
      // Determine test type based on selected test type
      let exerciseType: 'Part 5' | 'Part 6' | 'Part 7' = 'Part 7';
      if (selectedTestType === 'grammar' || selectedTestType === 'vocabulary') {
        exerciseType = 'Part 5';
      } else if (selectedTestType === 'reading') {
        exerciseType = 'Part 7';
      }

      // Create content from questions
      const content = questions.map((q, index) => {
        const optionsText = q.options.map((opt, optIdx) => 
          `${String.fromCharCode(65 + optIdx)}) ${opt}`
        ).join('\n');
        
        return `${index + 1}. ${q.question}\n${optionsText}\nCorrect Answer: ${String.fromCharCode(65 + q.correct)}`;
      }).join('\n\n');

      // Create exercise using admin upload service (now async)
      const exercise = await adminUploadService.createExerciseFromUpload(
        exerciseTitle,
        `${exerciseDescription}\n\n${content}`,
        exerciseLevel,
        exerciseType,
        'Admin Manual Input'
      );

      success('Bài test đã được tạo!', `Bài test "${exercise.name}" đã được lưu thành công vào database và sẽ xuất hiện trong trang Reading Exercises.`);
      
      // Refresh Reading Exercises data
      refreshExercises();
      
      // Reset form
      setExerciseTitle('');
      setExerciseDescription('');
      setQuestions([{ id: 1, question: '', options: ['', '', '', ''], correct: 0 }]);
      
    } catch (err) {
      console.error('Error creating reading exercise:', err);
      error('Không thể tạo bài test', 'Có lỗi xảy ra khi tạo bài test. Vui lòng thử lại.');
    }
  };

  // Question management functions
  const addQuestion = () => {
    const newId = Math.max(...questions.map(q => q.id)) + 1;
    setQuestions(prev => [...prev, { id: newId, question: '', options: ['', '', '', ''], correct: 0 }]);
  };

  const removeQuestion = (id: number) => {
    setQuestions(prev => prev.filter(q => q.id !== id));
  };

  const updateQuestion = (id: number, field: string, value: string | number) => {
    setQuestions(prev => prev.map(q => 
      q.id === id ? { ...q, [field]: value } : q
    ));
  };

  const updateQuestionOption = (id: number, optionIndex: number, value: string) => {
    setQuestions(prev => prev.map(q => 
      q.id === id ? { ...q, options: q.options.map((opt, idx) => idx === optionIndex ? value : opt) } : q
    ));
  };

  return (
    <div className="space-y-6">
      {/* Page Header */}
      <div className="space-y-2">
        <h1 className="text-3xl font-bold tracking-tight text-gray-900 dark:text-white">Upload Tests</h1>
        <p className="text-muted-foreground">
          Upload files cho từng part của TOEIC và quản lý đề thi của bạn
        </p>
      </div>

      {/* Header với statistics */}
      <div className="grid gap-4 md:grid-cols-3">
        <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600 dark:text-gray-300">Tổng đề thi</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">86</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">+12 tuần này</p>
              </div>
              <div className="p-3 bg-blue-100 dark:bg-blue-900/30 rounded-full">
                <FileText className="h-6 w-6 text-blue-600 dark:text-blue-400" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600 dark:text-gray-300">File đã tải lên</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">324</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">2.8GB dung lượng</p>
              </div>
              <div className="p-3 bg-green-100 dark:bg-green-900/30 rounded-full">
                <Upload className="h-6 w-6 text-green-600 dark:text-green-400" />
              </div>
            </div>
          </CardContent>
        </Card>

        <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="p-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm font-medium text-gray-600 dark:text-gray-300">Đang xử lý</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">3</p>
                <p className="text-xs text-gray-500 dark:text-gray-400">Ước tính 10 phút</p>
              </div>
              <div className="p-3 bg-orange-100 dark:bg-orange-900/30 rounded-full">
                <Clock className="h-6 w-6 text-orange-600 dark:text-orange-400" />
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      <Tabs defaultValue="upload" className="space-y-4">
        <TabsList className="grid w-full grid-cols-3 rounded-xl bg-gray-100 dark:bg-gray-800 p-1">
          <TabsTrigger value="upload" className="rounded-lg data-[state=active]:bg-white dark:data-[state=active]:bg-gray-700 text-gray-600 dark:text-gray-300">Upload File Hoàn Chỉnh</TabsTrigger>
          <TabsTrigger value="create" className="rounded-lg data-[state=active]:bg-white dark:data-[state=active]:bg-gray-700 text-gray-600 dark:text-gray-300">Tạo đề thi</TabsTrigger>
          <TabsTrigger value="reading" className="rounded-lg data-[state=active]:bg-white dark:data-[state=active]:bg-gray-700 text-gray-600 dark:text-gray-300">Quản lý Reading Tests</TabsTrigger>
        </TabsList>

        <TabsContent value="upload" className="space-y-4">
          {/* NEW: Simplified 2-Step Upload for Part 6/7 Only - TEXT INPUT ONLY */}
          <Card className="rounded-2xl border-2 border-blue-200 bg-gradient-to-br from-blue-50 to-white">
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <span className="bg-blue-500 text-white px-3 py-1 rounded-full text-sm">MỚI</span>
                Tạo bài tập Part 6/7 - Quy trình 2 bước (Nhập text)
              </CardTitle>
              <CardDescription>
                <strong>Bước 1:</strong> Nhập đoạn văn → <strong>Bước 2:</strong> Thêm câu hỏi
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Method Selection Toggle - REMOVED, only text input now */}

              {/* Step 1: Enter Text ONLY */}
              <div className="p-4 bg-white rounded-xl border-2 border-green-300">
                <div className="flex items-center gap-2 mb-4">
                  <span className="bg-green-500 text-white w-8 h-8 rounded-full flex items-center justify-center font-bold">1</span>
                  <h3 className="text-lg font-semibold">
                    Bước 1: Nhập đoạn văn Reading
                  </h3>
                  <Badge variant="secondary">Part 6 & 7</Badge>
                </div>

                <div className="space-y-4">
                  <div className="space-y-2">
                    <Label htmlFor="passageTitle">Tiêu đề bài tập</Label>
                    <Input 
                      id="passageTitle" 
                      placeholder="VD: Company Meeting Notice"
                      value={exerciseTitle}
                      onChange={(e) => setExerciseTitle(e.target.value)}
                      className="border-2"
                    />
                  </div>

                  <div className="grid gap-4 md:grid-cols-2">
                    <div className="space-y-2">
                      <Label htmlFor="passagePartType">Part Type</Label>
                      <Select value={exercisePartType} onValueChange={(value: 'Part 5' | 'Part 6' | 'Part 7') => setExercisePartType(value)}>
                        <SelectTrigger className="border-2">
                          <SelectValue placeholder="Chọn Part" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Part 6">Part 6 - Text Completion (4 câu)</SelectItem>
                          <SelectItem value="Part 7">Part 7 - Reading Comprehension (5-10 câu)</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                    
                    <div className="space-y-2">
                      <Label htmlFor="passageLevel">Cấp độ</Label>
                      <Select value={exerciseLevel} onValueChange={(value: 'Beginner' | 'Intermediate' | 'Advanced') => setExerciseLevel(value)}>
                        <SelectTrigger className="border-2">
                          <SelectValue placeholder="Chọn cấp độ" />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="Beginner">Beginner (A1-A2)</SelectItem>
                          <SelectItem value="Intermediate">Intermediate (B1-B2)</SelectItem>
                          <SelectItem value="Advanced">Advanced (C1-C2)</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>

                  {/* Enter Text - Single or Dual Passages */}
                    <div className="space-y-4">
                      {/* Part 7 - Dual Passage Toggle */}
                      {exercisePartType === 'Part 7' && (
                        <div className="flex items-center space-x-2 p-3 bg-blue-50 border border-blue-200 rounded-lg">
                          <input
                            type="checkbox"
                            id="dualPassage"
                            checked={isDualPassage}
                            onChange={(e) => setIsDualPassage(e.target.checked)}
                            className="h-4 w-4 rounded border-gray-300"
                          />
                          <label htmlFor="dualPassage" className="text-sm font-medium cursor-pointer">
                            📄 Part 7 - Dual Passage (2 đoạn văn liên quan)
                          </label>
                        </div>
                      )}

                      {/* Single Passage (Part 6 OR Part 7 Single) */}
                      {(!isDualPassage || exercisePartType !== 'Part 7') && (
                        <div className="space-y-2">
                          <Label htmlFor="passageContent">Đoạn văn Reading</Label>
                          <Textarea 
                            id="passageContent" 
                            placeholder={exercisePartType === 'Part 6' 
                              ? "Nhập đoạn văn có chỗ trống (___) cho Part 6...\n\nVí dụ:\nDear Team Members,\n\nWe are pleased to announce that our company (1) _____ be moving to a new office..."
                              : "Nhập đoạn văn reading cho Part 7...\n\nVí dụ:\nCompany Meeting Notice\n\nDate: Friday, March 15th\nTime: 2:00 PM - 4:00 PM..."}
                            value={exerciseDescription}
                            onChange={(e) => setExerciseDescription(e.target.value)}
                            rows={12}
                            className="text-sm border-2"
                            style={{ fontFamily: 'system-ui, -apple-system, sans-serif' }}
                          />
                          <p className="text-xs text-gray-500">
                            {exercisePartType === 'Part 6' 
                              ? "💡 Đánh dấu chỗ trống bằng (1) ___, (2) ___, (3) ___, (4) ___"
                              : "💡 Nhập toàn bộ đoạn văn, email, thông báo, hoặc bài báo"}
                          </p>
                        </div>
                      )}

                      {/* Dual Passage (Part 7 Only) */}
                      {isDualPassage && exercisePartType === 'Part 7' && (
                        <div className="space-y-4">
                          <div className="space-y-2">
                            <Label htmlFor="passage1">📄 Đoạn văn 1</Label>
                            <Textarea 
                              id="passage1" 
                              placeholder="Nhập đoạn văn thứ nhất...\n\nVí dụ:\nMemo\nTo: All Staff\nFrom: HR Department\nDate: March 10, 2024\n\nSubject: New Office Policy..."
                              value={passage1}
                              onChange={(e) => setPassage1(e.target.value)}
                              rows={8}
                              className="text-sm border-2 border-blue-300"
                              style={{ fontFamily: 'system-ui, -apple-system, sans-serif' }}
                            />
                          </div>

                          <div className="space-y-2">
                            <Label htmlFor="passage2">📄 Đoạn văn 2 (liên quan đến đoạn 1)</Label>
                            <Textarea 
                              id="passage2" 
                              placeholder="Nhập đoạn văn thứ hai...\n\nVí dụ:\nEmail Response\nFrom: john.smith@company.com\nTo: hr@company.com\nDate: March 11, 2024\n\nDear HR Team,\nRegarding the new policy..."
                              value={passage2}
                              onChange={(e) => setPassage2(e.target.value)}
                              rows={8}
                              className="text-sm border-2 border-blue-300"
                              style={{ fontFamily: 'system-ui, -apple-system, sans-serif' }}
                            />
                          </div>

                          <p className="text-xs text-gray-500 bg-yellow-50 p-2 rounded border border-yellow-200">
                            💡 <strong>Part 7 Dual Passage:</strong> 2 đoạn văn có liên quan (ví dụ: email + reply, memo + announcement, article + review)
                          </p>
                        </div>
                      )}
                    </div>

                  {/* Submit Button */}
                  <Button 
                    className="w-full rounded-xl bg-green-600 hover:bg-green-700"
                    disabled={
                      isCreatingPassage || 
                      step1Complete || 
                      (!isDualPassage && !exerciseDescription.trim()) ||
                      (isDualPassage && (!passage1.trim() || !passage2.trim()))
                    }
                    size="lg"
                    onClick={handleCreatePassage}
                  >
                    {isCreatingPassage ? (
                      <>
                        <Clock className="mr-2 h-5 w-5 animate-spin" />
                        Đang tạo...
                      </>
                    ) : step1Complete ? (
                      <>
                        <CheckCircle className="mr-2 h-5 w-5" />
                        ✅ Đã hoàn thành - Exercise ID: {createdExerciseId}
                      </>
                    ) : (
                      <>
                        <CheckCircle className="mr-2 h-5 w-5" />
                        Tạo đoạn văn (Bước 1)
                      </>
                    )}
                  </Button>
                </div>
              </div>

              {/* Step 2: Add Questions */}
              <div className={`p-4 bg-white rounded-xl border-2 ${step1Complete ? 'border-orange-500' : 'border-orange-300 opacity-60'}`}>
                <div className="flex items-center gap-2 mb-4">
                  <span className={`${step1Complete ? 'bg-orange-500' : 'bg-gray-400'} text-white w-8 h-8 rounded-full flex items-center justify-center font-bold`}>2</span>
                  <h3 className="text-lg font-semibold">Bước 2: Thêm câu hỏi</h3>
                  {step1Complete ? (
                    <Badge variant="default" className="bg-green-500">Sẵn sàng - Exercise ID: {createdExerciseId}</Badge>
                  ) : (
                    <Badge variant="outline">Sau khi hoàn thành bước 1</Badge>
                  )}
                </div>

                {!step1Complete ? (
                  <p className="text-sm text-gray-600 mb-4">
                    Sau khi tạo đoạn văn, bạn sẽ nhận được Exercise ID để thêm câu hỏi.
                  </p>
                ) : (
                  <div className="space-y-4">
                    <p className="text-sm text-green-600 font-medium mb-4">
                      ✅ Bước 1 hoàn thành! Bây giờ hãy thêm {exercisePartType === 'Part 6' ? '4' : '5-10'} câu hỏi:
                    </p>

                    {/* Questions Form */}
                    {questions.map((q, index) => (
                      <div key={q.id} className="space-y-3 p-4 border rounded-xl bg-gray-50">
                        <div className="flex items-center justify-between">
                          <Label className="text-base font-medium flex items-center">
                            <span className="bg-blue-500 text-white px-3 py-1 rounded-full text-sm mr-2">
                              Câu {index + 1}
                            </span>
                          </Label>
                          {questions.length > 1 && (
                            <Button 
                              variant="outline" 
                              size="sm"
                              onClick={() => removeQuestion(q.id)}
                              className="text-red-600 hover:text-red-700 rounded-xl"
                            >
                              <Minus className="h-3 w-3" />
                            </Button>
                          )}
                        </div>
                        
                        <div className="space-y-3">
                          <Input
                            placeholder={exercisePartType === 'Part 6' 
                              ? `Question ${index + 1} (chọn từ phù hợp cho chỗ trống ${index + 1})`
                              : `Question ${index + 1}: What is the main idea?`}
                            value={q.question}
                            onChange={(e) => updateQuestion(q.id, 'question', e.target.value)}
                            className="rounded-xl"
                          />
                          
                          <div className="grid gap-2 md:grid-cols-2">
                            {q.options.map((option, optIndex) => (
                              <div key={optIndex} className="flex items-center space-x-2">
                                <span className="text-sm font-medium w-6">{String.fromCharCode(65 + optIndex)})</span>
                                <Input
                                  placeholder={exercisePartType === 'Part 6' 
                                    ? `${String.fromCharCode(65 + optIndex)}) will/would/should...`
                                    : `Đáp án ${String.fromCharCode(65 + optIndex)}`}
                                  value={option}
                                  onChange={(e) => updateQuestionOption(q.id, optIndex, e.target.value)}
                                  className="rounded-xl flex-1"
                                />
                                <input
                                  type="radio"
                                  name={`correct-${q.id}`}
                                  checked={q.correct === optIndex}
                                  onChange={() => updateQuestion(q.id, 'correct', optIndex)}
                                  className="w-4 h-4"
                                  title="Đáp án đúng"
                                />
                              </div>
                            ))}
                          </div>
                        </div>
                      </div>
                    ))}

                    {/* Add/Remove Question Buttons */}
                    <div className="flex gap-2">
                      <Button 
                        variant="outline" 
                        className="rounded-xl flex-1"
                        onClick={addQuestion}
                      >
                        <Plus className="mr-2 h-4 w-4" />
                        Thêm câu hỏi
                      </Button>
                      {questions.length > 1 && (
                        <Button 
                          variant="outline" 
                          className="rounded-xl text-red-600 hover:text-red-700"
                          onClick={() => removeQuestion(questions[questions.length - 1].id)}
                        >
                          <Minus className="mr-2 h-4 w-4" />
                          Xóa câu cuối
                        </Button>
                      )}
                    </div>

                    {/* Submit Questions Button */}
                    <Button 
                      className="w-full rounded-xl bg-orange-600 hover:bg-orange-700" 
                      size="lg"
                      onClick={handleAddQuestions}
                      disabled={isAddingQuestions || questions.some(q => !q.question.trim() || q.options.some(opt => !opt.trim()))}
                    >
                      {isAddingQuestions ? (
                        <>
                          <Clock className="mr-2 h-5 w-5 animate-spin" />
                          Đang xử lý...
                        </>
                      ) : (
                        <>
                          <CheckCircle className="mr-2 h-5 w-5" />
                          Hoàn thành - Thêm {questions.length} câu hỏi (Bước 2)
                        </>
                      )}
                    </Button>
                  </div>
                )}

                {!step1Complete && (
                  <div className="bg-gray-50 p-4 rounded-lg border">
                    <p className="text-xs text-gray-500 mb-3">Mẫu câu hỏi sẽ nhập ở bước 2:</p>
                    <div className="space-y-2 text-xs">
                      <div className="bg-white p-2 rounded border">
                        <strong>Question 1:</strong> (chọn từ phù hợp cho chỗ trống 1)<br/>
                        A) will<br/>
                        B) would<br/>
                        C) should<br/>
                        D) could<br/>
                        <strong className="text-green-600">✓ Correct: A</strong>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              {/* Info Box */}
              <div className="p-4 bg-gradient-to-r from-purple-50 to-pink-50 rounded-lg border border-purple-200">
                <h4 className="font-medium text-gray-900 mb-2 flex items-center gap-2">
                  ℹ️ Quy trình mới (Không cần file):
                </h4>
                <ul className="text-sm text-gray-600 space-y-1.5">
                  <li>✅ <strong>Bước 1:</strong> Nhập đoạn văn → Nhận Exercise ID</li>
                  <li>✅ <strong>Bước 2:</strong> Dùng Exercise ID thêm 4 câu hỏi (Part 6) hoặc nhiều hơn (Part 7)</li>
                  <li>✅ <strong>Tự động active:</strong> Sau khi thêm câu hỏi, bài tập sẵn sàng cho học viên</li>
                  <li>🚫 <strong>Bỏ Part 5:</strong> Không hỗ trợ upload file cho Part 5 nữa</li>
                  <li>💡 <strong>Đơn giản:</strong> Chỉ nhập text trực tiếp, không cần upload file</li>
                </ul>
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="reading" className="space-y-4">
          <AdminReadingExercisesManager />
        </TabsContent>

        <TabsContent value="create" className="space-y-4">
          <Card className="rounded-2xl">
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Tạo đề thi thủ công</CardTitle>
                  <CardDescription>Tạo câu hỏi và đáp án trực tiếp trên hệ thống</CardDescription>
                </div>
                <Button onClick={addQuestion} className="rounded-xl">
                  <Plus className="mr-2 h-4 w-4" />
                  Thêm câu hỏi
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {questions.map((q) => (
                  <div key={q.id} className="space-y-4 p-4 border rounded-xl">
                    <div className="flex items-center justify-between">
                      <Label className="text-base font-medium flex items-center">
                        <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded-full text-sm mr-2">
                          Câu {q.id}
                        </span>
                      </Label>
                      {questions.length > 1 && (
                        <Button 
                          variant="outline" 
                          size="sm"
                          onClick={() => removeQuestion(q.id)}
                          className="text-red-600 hover:text-red-700 rounded-xl"
                        >
                          <Minus className="h-3 w-3" />
                        </Button>
                      )}
                    </div>
                    
                    <div className="space-y-3">
                      <Textarea
                        placeholder="Nhập câu hỏi..."
                        value={q.question}
                        onChange={(e) => updateQuestion(q.id, 'question', e.target.value)}
                        className="rounded-xl"
                      />
                      
                      <div className="grid gap-2 md:grid-cols-2">
                        {q.options.map((option, optIndex) => (
                          <div key={optIndex} className="flex items-center space-x-2">
                            <span className="text-sm font-medium w-6">{String.fromCharCode(65 + optIndex)})</span>
                            <Input
                              placeholder={`Đáp án ${String.fromCharCode(65 + optIndex)}`}
                              value={option}
                              onChange={(e) => updateQuestionOption(q.id, optIndex, e.target.value)}
                              className="rounded-xl flex-1"
                            />
                            <input
                              type="radio"
                              name={`correct-${q.id}`}
                              checked={q.correct === optIndex}
                              onChange={() => updateQuestion(q.id, 'correct', optIndex)}
                              className="w-4 h-4"
                            />
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="create" className="space-y-4">
          <Card className="rounded-2xl">
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle>Tạo đề thi thủ công</CardTitle>
                  <CardDescription>Tạo câu hỏi và đáp án trực tiếp trên hệ thống</CardDescription>
                </div>
                <Button onClick={addQuestion} className="rounded-xl">
                  <Plus className="mr-2 h-4 w-4" />
                  Thêm câu hỏi
                </Button>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {questions.map((q) => (
                  <div key={q.id} className="space-y-4 p-4 border rounded-xl">
                    <div className="flex items-center justify-between">
                      <Label className="text-base font-medium flex items-center">
                        <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded-full text-sm mr-2">
                          Câu {q.id}
                        </span>
                      </Label>
                      {questions.length > 1 && (
                        <Button 
                          variant="outline" 
                          size="sm"
                          onClick={() => removeQuestion(q.id)}
                          className="text-red-600 hover:text-red-700 rounded-xl"
                        >
                          <Minus className="h-3 w-3" />
                        </Button>
                      )}
                    </div>
                    
                    <div className="space-y-3">
                      <Textarea
                        placeholder="Nhập câu hỏi..."
                        value={q.question}
                        onChange={(e) => updateQuestion(q.id, 'question', e.target.value)}
                        className="rounded-xl"
                      />
                      
                      <div className="grid gap-2 md:grid-cols-2">
                        {q.options.map((option, optIndex) => (
                          <div key={optIndex} className="flex items-center space-x-2">
                            <span className="text-sm font-medium w-6">{String.fromCharCode(65 + optIndex)})</span>
                            <Input
                              placeholder={`Đáp án ${String.fromCharCode(65 + optIndex)}`}
                              value={option}
                              onChange={(e) => updateQuestionOption(q.id, optIndex, e.target.value)}
                              className="rounded-xl flex-1"
                            />
                            <input
                              type="radio"
                              name={`correct-${q.id}`}
                              checked={q.correct === optIndex}
                              onChange={() => updateQuestion(q.id, 'correct', optIndex)}
                              className="w-4 h-4"
                            />
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Action Buttons */}
      <div className="flex justify-end space-x-4">
        <Button variant="outline" className="rounded-xl">
          <Eye className="mr-2 h-4 w-4" />
          Xem trước
        </Button>
        <Button 
          className="rounded-xl"
          onClick={createReadingExerciseFromUpload}
          disabled={!exerciseTitle.trim() || questions.length === 0}
        >
          <Save className="mr-2 h-4 w-4" />
          Lưu bài test và tạo Reading Exercise
        </Button>
      </div>
    </div>
  );
};

export default UploadPage;