import React, { useState, useRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Badge } from "@/components/ui/badge";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Upload, FileText, Plus, Minus, Save, Eye, Download, Clock, Users, Folder, Settings, X, CheckCircle, LucideIcon, MessageSquare } from 'lucide-react';
import { SectionBox } from '@/components/admin/SectionBox';
import { FileRow } from '@/components/admin/FileRow';
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogFooter } from "@/components/ui/dialog";
import { toast } from 'sonner';
import writingExerciseService, { CreateWritingExerciseRequest, SentenceQuestion, WritingExercise } from '@/services/writingExerciseService';

const UploadPage = () => {
  const [testType, setTestType] = useState('');
  const [selectedTestType, setSelectedTestType] = useState('');
  const [questions, setQuestions] = useState([{ id: 1, question: '', options: ['', '', '', ''], correct: 0 }]);
  const [uploadedFiles, setUploadedFiles] = useState([
    { name: 'TOEIC_Part1_Audio.mp3', size: '2.4MB', date: '2024-03-10' },
    { name: 'Reading_Questions.pdf', size: '1.8MB', date: '2024-03-09' }
  ]);

  // Writing exercise states
  const [writingDialogOpen, setWritingDialogOpen] = useState(false);
  const [writingType, setWritingType] = useState<'writing_essay' | 'writing_sentence'>('writing_essay');
  const [writingTitle, setWritingTitle] = useState('');
  const [writingTopic, setWritingTopic] = useState('');
  const [writingTimeLimit, setWritingTimeLimit] = useState(30);
  const [writingDescription, setWritingDescription] = useState('');
  const [writingLevel, setWritingLevel] = useState('A1');
  const [writingQuestions, setWritingQuestions] = useState<SentenceQuestion[]>([]);
  const [exercises, setExercises] = useState<WritingExercise[]>([]);
  const [loadingExercises, setLoadingExercises] = useState(false);
  const [editingExerciseId, setEditingExerciseId] = useState<number | null>(null);

  // File upload states
  const [uploadProgress, setUploadProgress] = useState<{[key: string]: number}>({});
  const [selectedFiles, setSelectedFiles] = useState<{[key: string]: File[]}>({});
  const fileInputRefs = useRef<{[key: string]: HTMLInputElement | null}>({});

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
  const handleFileSelect = (uploadType: string, files: FileList | null) => {
    if (!files) return;
    
    const fileArray = Array.from(files);
    setSelectedFiles(prev => ({
      ...prev,
      [uploadType]: fileArray
    }));

    // Simulate upload progress
    setUploadProgress(prev => ({ ...prev, [uploadType]: 0 }));
    
    const interval = setInterval(() => {
      setUploadProgress(prev => {
        const currentProgress = prev[uploadType] || 0;
        if (currentProgress >= 100) {
          clearInterval(interval);
          return prev;
        }
        return { ...prev, [uploadType]: currentProgress + 10 };
      });
    }, 200);
  };

  const handleFileRemove = (uploadType: string, fileIndex: number) => {
    setSelectedFiles(prev => ({
      ...prev,
      [uploadType]: prev[uploadType]?.filter((_, index) => index !== fileIndex) || []
    }));
  };

  const formatFileSize = (bytes: number) => {
    if (bytes === 0) return '0 Bytes';
    const k = 1024;
    const sizes = ['Bytes', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
  };

  // Load writing exercises
  React.useEffect(() => {
    if (selectedTestType === 'writing') {
      loadWritingExercises();
    }
  }, [selectedTestType]);

  const loadWritingExercises = async () => {
    try {
      setLoadingExercises(true);
      const data = await writingExerciseService.getWritingExercises();
      setExercises(data);
    } catch (error) {
      console.error('Error loading exercises:', error);
      toast.error('Không thể tải danh sách bài tập');
    } finally {
      setLoadingExercises(false);
    }
  };

  // Writing exercise functions
  const openWritingDialog = (type: 'writing_essay' | 'writing_sentence', exercise?: WritingExercise) => {
    if (exercise) {
      // Edit mode
      setEditingExerciseId(exercise.id);
      setWritingType(exercise.type);
      setWritingTitle(exercise.title);
      setWritingTopic(exercise.category || '');
      setWritingTimeLimit(exercise.timeLimit || 30);
      setWritingDescription(exercise.description || '');
      setWritingLevel(exercise.level || 'A1');
      
      if (exercise.type === 'writing_sentence') {
        const parsedQuestions = JSON.parse(exercise.questionsJson || '[]');
        setWritingQuestions(parsedQuestions);
      } else {
        setWritingQuestions([]);
      }
    } else {
      // Create mode
      setEditingExerciseId(null);
      setWritingType(type);
      setWritingTitle('');
      setWritingTopic('');
      setWritingTimeLimit(30);
      setWritingDescription('');
      setWritingLevel('A1');
      
      if (type === 'writing_sentence') {
        const emptyQuestions: SentenceQuestion[] = Array.from({ length: 5 }, (_, i) => ({
          questionOrder: i + 1,
          vietnamesePrompt: '',
          correctAnswer: '',
          vocabularyHint: '',
          grammarHint: ''
        }));
        setWritingQuestions(emptyQuestions);
      } else {
        setWritingQuestions([]);
      }
    }
    
    setWritingDialogOpen(true);
  };

  const updateWritingQuestion = (index: number, field: keyof SentenceQuestion, value: string) => {
    const newQuestions = [...writingQuestions];
    newQuestions[index] = { ...newQuestions[index], [field]: value };
    setWritingQuestions(newQuestions);
  };

  const handleDeleteExercise = async (id: number) => {
    if (!confirm('Bạn có chắc chắn muốn xóa bài tập này?')) return;
    
    try {
      await writingExerciseService.deleteWritingExercise(id);
      toast.success('Xóa bài tập thành công!');
      loadWritingExercises();
    } catch (error) {
      console.error('Error deleting exercise:', error);
      toast.error('Có lỗi xảy ra khi xóa bài tập');
    }
  };

  const handleWritingSubmit = async () => {
    if (!writingTitle.trim()) {
      toast.error('Vui lòng nhập tên bài test');
      return;
    }

    if (!writingTopic.trim()) {
      toast.error('Vui lòng nhập chủ đề');
      return;
    }

    if (writingType === 'writing_sentence') {
      const invalidQuestion = writingQuestions.find(q => 
        !q.vietnamesePrompt.trim() || !q.correctAnswer.trim()
      );
      if (invalidQuestion) {
        toast.error('Vui lòng nhập đầy đủ đề bài và đáp án cho tất cả câu hỏi');
        return;
      }
    }

    try {
      const requestData: CreateWritingExerciseRequest = {
        title: writingTitle,
        content: writingDescription,
        type: writingType,
        category: writingTopic,
        timeLimit: writingTimeLimit,
        estimatedMinutes: writingTimeLimit,
        description: writingDescription,
        level: writingLevel,
        questionsJson: writingType === 'writing_sentence' 
          ? JSON.stringify(writingQuestions)
          : '[]',
        correctAnswersJson: writingType === 'writing_sentence'
          ? JSON.stringify(writingQuestions.map(q => q.correctAnswer))
          : '[]'
      };

      if (editingExerciseId) {
        await writingExerciseService.updateWritingExercise(editingExerciseId, requestData);
        toast.success('Cập nhật bài tập thành công!');
      } else {
        await writingExerciseService.createWritingExercise(requestData);
        toast.success('Tạo bài tập thành công!');
      }
      
      setWritingDialogOpen(false);
      loadWritingExercises();
    } catch (error) {
      console.error('Error saving exercise:', error);
      toast.error('Có lỗi xảy ra khi lưu bài tập');
    }
  };

  // Create file upload component
  const FileUploadArea = ({ 
    uploadType, 
    title, 
    description, 
    acceptedTypes, 
    maxSize, 
    icon: Icon,
    borderColor = "border-gray-300",
    iconColor = "text-gray-400",
    required = false
  }: {
    uploadType: string;
    title: string;
    description: string;
    acceptedTypes: string;
    maxSize: string;
    icon: LucideIcon;
    borderColor?: string;
    iconColor?: string;
    required?: boolean;
  }) => {
    const files = selectedFiles[uploadType] || [];
    const progress = uploadProgress[uploadType];
    const isUploading = progress !== undefined && progress < 100;
    const isCompleted = progress === 100;

    return (
      <SectionBox>
        <h3 className="text-lg font-medium mb-4 flex items-center">
          <Icon className="mr-2 h-5 w-5" />
          {title}
          <Badge variant={required ? "secondary" : "outline"} className="ml-2">
            {required ? "Required" : "Optional"}
          </Badge>
        </h3>
        
        <div className="space-y-3">
          <div 
            className={`border-2 border-dashed ${borderColor} rounded-lg p-6 text-center transition-colors hover:bg-gray-50 cursor-pointer`}
            onClick={() => fileInputRefs.current[uploadType]?.click()}
            onDragOver={(e) => {
              e.preventDefault();
              e.currentTarget.classList.add('border-blue-400', 'bg-blue-50');
            }}
            onDragLeave={(e) => {
              e.currentTarget.classList.remove('border-blue-400', 'bg-blue-50');
            }}
            onDrop={(e) => {
              e.preventDefault();
              e.currentTarget.classList.remove('border-blue-400', 'bg-blue-50');
              handleFileSelect(uploadType, e.dataTransfer.files);
            }}
          >
            <input
              ref={(ref) => fileInputRefs.current[uploadType] = ref}
              type="file"
              multiple
              accept={acceptedTypes}
              onChange={(e) => handleFileSelect(uploadType, e.target.files)}
              className="hidden"
            />
            
            {isCompleted ? (
              <CheckCircle className={`mx-auto h-12 w-12 text-green-500`} />
            ) : (
              <Icon className={`mx-auto h-12 w-12 ${iconColor}`} />
            )}
            
            <p className="mt-2 text-sm text-gray-600">
              {isCompleted ? 'Upload thành công!' : description}
            </p>
            <p className="text-xs text-gray-500 mt-1">{acceptedTypes} (tối đa {maxSize})</p>
            
            {isUploading && (
              <div className="mt-3">
                <div className="w-full bg-gray-200 rounded-full h-2">
                  <div 
                    className="bg-blue-600 h-2 rounded-full transition-all duration-300" 
                    style={{ width: `${progress}%` }}
                  ></div>
                </div>
                <p className="text-xs text-gray-500 mt-1">{progress}% hoàn thành</p>
              </div>
            )}
          </div>

          {/* Display selected files */}
          {files.length > 0 && (
            <div className="space-y-2">
              <h4 className="text-sm font-medium text-gray-700">File đã chọn:</h4>
              {files.map((file, index) => (
                <div key={index} className="flex items-center justify-between p-2 bg-gray-50 rounded-lg">
                  <div className="flex items-center space-x-2">
                    <FileText className="h-4 w-4 text-gray-500" />
                    <div>
                      <p className="text-sm font-medium">{file.name}</p>
                      <p className="text-xs text-gray-500">{formatFileSize(file.size)}</p>
                    </div>
                  </div>
                  <Button
                    variant="outline"
                    size="sm"
                    onClick={(e) => {
                      e.stopPropagation();
                      handleFileRemove(uploadType, index);
                    }}
                    className="text-red-600 hover:text-red-700"
                  >
                    <X className="h-3 w-3" />
                  </Button>
                </div>
              ))}
            </div>
          )}
        </div>
      </SectionBox>
    );
  };

  const addQuestion = () => {
    const newId = Math.max(...questions.map(q => q.id), 0) + 1; // Generate incremental ID
    const newQuestion = { 
      id: newId,
      question: '', 
      options: ['', '', '', ''], 
      correct: 0 
    };
    setQuestions([newQuestion, ...questions]); // Add to beginning of array
  };

  const removeQuestion = (id: number) => {
    setQuestions(questions.filter(q => q.id !== id));
  };

  const updateQuestion = (id: number, field: string, value: string | number) => {
    setQuestions(questions.map(q => 
      q.id === id ? { ...q, [field]: value } : q
    ));
  };

  const updateOption = (questionId: number, optionIndex: number, value: string) => {
    setQuestions(questions.map(q => 
      q.id === questionId 
        ? { ...q, options: q.options.map((opt, idx) => idx === optionIndex ? value : opt) }
        : q
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
          <TabsTrigger value="upload" className="rounded-lg data-[state=active]:bg-white dark:data-[state=active]:bg-gray-700 text-gray-600 dark:text-gray-300">Tải lên file</TabsTrigger>
          <TabsTrigger value="create" className="rounded-lg data-[state=active]:bg-white dark:data-[state=active]:bg-gray-700 text-gray-600 dark:text-gray-300">Tạo đề thi</TabsTrigger>
          <TabsTrigger value="manage" className="rounded-lg data-[state=active]:bg-white dark:data-[state=active]:bg-gray-700 text-gray-600 dark:text-gray-300">Quản lý file</TabsTrigger>
        </TabsList>

        <TabsContent value="upload" className="space-y-4">
          {/* Test Type Selection */}
          <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader>
              <CardTitle className="text-gray-900 dark:text-white">Chọn loại bài test</CardTitle>
              <CardDescription className="text-gray-600 dark:text-gray-300">Chọn loại bài test bạn muốn tạo</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid gap-3 md:grid-cols-2 lg:grid-cols-3">
                {testTypes.map((type) => (
                  <div
                    key={type.id}
                    className={`p-4 border rounded-xl cursor-pointer transition-all hover:shadow-md ${
                      selectedTestType === type.id 
                        ? 'border-primary bg-primary/5 dark:bg-primary/10' 
                        : 'border-gray-200 dark:border-gray-600 bg-white dark:bg-gray-800 hover:bg-gray-50 dark:hover:bg-gray-750'
                    }`}
                    onClick={() => setSelectedTestType(type.id)}
                  >
                    <div className="flex items-center space-x-3">
                      <span className="text-2xl">{type.icon}</span>
                      <div>
                        <p className="font-medium text-gray-900 dark:text-white">{type.label}</p>
                        <p className="text-sm text-gray-600 dark:text-gray-400">{type.description}</p>
                      </div>
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {selectedTestType && selectedTestType !== 'writing' && (
            <Card className="rounded-2xl">
              <CardHeader>
                <CardTitle>Thông tin bài test</CardTitle>
                <CardDescription>Nhập thông tin chi tiết cho bài test</CardDescription>
              </CardHeader>
              <CardContent className="space-y-4">
                <div className="grid gap-4 md:grid-cols-2">
                  <div className="space-y-2">
                    <Label htmlFor="title">Tên bài test</Label>
                    <Input id="title" placeholder="VD: TOEIC 2025 - Đề 01" />
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="level">Cấp độ</Label>
                    <Select>
                      <SelectTrigger>
                        <SelectValue placeholder="Chọn cấp độ" />
                      </SelectTrigger>
                      <SelectContent>
                        <SelectItem value="beginner">Beginner</SelectItem>
                        <SelectItem value="intermediate">Intermediate</SelectItem>
                        <SelectItem value="advanced">Advanced</SelectItem>
                      </SelectContent>
                    </Select>
                  </div>
                </div>

                <div className="space-y-2">
                  <Label htmlFor="description">Mô tả</Label>
                  <Textarea id="description" placeholder="Mô tả chi tiết về bài test..." />
                </div>

                <div className="grid gap-4 md:grid-cols-3">
                  <div className="space-y-2">
                    <Label htmlFor="duration">Thời gian (phút)</Label>
                    <Input id="duration" type="number" placeholder="120" />
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="questions">Số câu hỏi</Label>
                    <Input id="questions" type="number" placeholder="200" />
                  </div>
                  
                  <div className="space-y-2">
                    <Label htmlFor="points">Điểm tối đa</Label>
                    <Input id="points" type="number" placeholder="990" />
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {/* Dynamic Upload Sections based on Test Type */}
          {selectedTestType === 'toeic-full' && (
            <Card className="rounded-2xl">
              <CardHeader>
                <CardTitle>TOEIC Full Test - Upload Files</CardTitle>
                <CardDescription>Upload files cho từng part của TOEIC</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-6">
                  {/* Part 1-4 Listening */}
                  <div className="grid gap-4 md:grid-cols-2">
                    <FileUploadArea
                      uploadType="toeic-audio"
                      title="🎧 Part 1-4 Audio"
                      description="Kéo thả file audio hoặc click để chọn"
                      acceptedTypes="audio/*,.mp3,.wav,.m4a"
                      maxSize="100MB"
                      icon={Upload}
                      borderColor="border-blue-300"
                      iconColor="text-blue-400"
                      required={true}
                    />

                    <FileUploadArea
                      uploadType="toeic-reading"
                      title="📖 Part 5-7 Passage"
                      description="Kéo thả file reading hoặc click để chọn"
                      acceptedTypes=".pdf,.docx,.json,.txt"
                      maxSize="20MB"
                      icon={FileText}
                      borderColor="border-green-300"
                      iconColor="text-green-400"
                      required={true}
                    />
                  </div>

                  {/* Answer Keys */}
                  <div className="grid gap-4 md:grid-cols-2">
                    <FileUploadArea
                      uploadType="toeic-answer-key"
                      title="📋 Answer Key (csv/json)"
                      description="Kéo thả file đáp án hoặc click để chọn"
                      acceptedTypes=".csv,.json"
                      maxSize="5MB"
                      icon={FileText}
                      borderColor="border-purple-300"
                      iconColor="text-purple-400"
                      required={true}
                    />

                    <FileUploadArea
                      uploadType="toeic-questions"
                      title="📑 Bảng câu hỏi (csv/json)"
                      description="Kéo thả file câu hỏi hoặc click để chọn"
                      acceptedTypes=".csv,.json"
                      maxSize="10MB"
                      icon={FileText}
                      borderColor="border-orange-300"
                      iconColor="text-orange-400"
                      required={false}
                    />
                  </div>

                  <div className="bg-gray-50 p-4 rounded-lg">
                    <h4 className="font-medium text-gray-900 mb-2">� Hướng dẫn upload TOEIC Full Test:</h4>
                    <ul className="text-sm text-gray-600 space-y-1">
                      <li>✓ Part 1-4 Audio: File âm thanh cho phần Listening (200 câu đầu)</li>
                      <li>✓ Part 5-7 Reading: Đoạn văn và câu hỏi cho phần Reading (200 câu cuối)</li>
                      <li>✓ Answer Key: File CSV/JSON chứa đáp án từ câu 1-200</li>
                      <li>✓ Question Bank: Chi tiết câu hỏi và các lựa chọn A, B, C, D</li>
                    </ul>
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {selectedTestType === 'listening' && (
            <Card className="rounded-2xl">
              <CardHeader>
                <CardTitle>Listening Test - Upload Files</CardTitle>
                <CardDescription>Upload file âm thanh và câu hỏi cho bài test nghe</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid gap-4 md:grid-cols-2">
                    <FileUploadArea
                      uploadType="listening-audio"
                      title="🎧 Audio Files"
                      description="Kéo thả file audio hoặc click để chọn"
                      acceptedTypes="audio/*,.mp3,.wav,.m4a"
                      maxSize="100MB"
                      icon={Upload}
                      borderColor="border-blue-300"
                      iconColor="text-blue-400"
                      required={true}
                    />

                    <FileUploadArea
                      uploadType="listening-questions"
                      title="📝 Questions & Answer Key"
                      description="Kéo thả file câu hỏi hoặc click để chọn"
                      acceptedTypes=".json,.csv,.pdf"
                      maxSize="10MB"
                      icon={FileText}
                      borderColor="border-green-300"
                      iconColor="text-green-400"
                      required={true}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {selectedTestType === 'reading' && (
            <Card className="rounded-2xl">
              <CardHeader>
                <CardTitle>Reading Test - Upload Files</CardTitle>
                <CardDescription>Upload đoạn văn và câu hỏi cho bài test đọc</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid gap-4 md:grid-cols-2">
                    <FileUploadArea
                      uploadType="reading-passages"
                      title="📖 Reading Passages"
                      description="Kéo thả file đoạn văn hoặc click để chọn"
                      acceptedTypes=".pdf,.docx,.txt"
                      maxSize="20MB"
                      icon={FileText}
                      borderColor="border-blue-300"
                      iconColor="text-blue-400"
                      required={true}
                    />

                    <FileUploadArea
                      uploadType="reading-questions"
                      title="❓ Questions & Answer Key"
                      description="Kéo thả file câu hỏi hoặc click để chọn"
                      acceptedTypes=".json,.csv"
                      maxSize="5MB"
                      icon={FileText}
                      borderColor="border-green-300"
                      iconColor="text-green-400"
                      required={true}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {selectedTestType === 'speaking' && (
            <Card className="rounded-2xl">
              <CardHeader>
                <CardTitle>Speaking Test - Upload Files</CardTitle>
                <CardDescription>Upload prompt và rubric cho bài test nói</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid gap-4 md:grid-cols-2">
                    <FileUploadArea
                      uploadType="speaking-prompts"
                      title="🗣️ Speaking Prompts"
                      description="Kéo thả file đề bài hoặc click để chọn"
                      acceptedTypes=".pdf,.docx,.json"
                      maxSize="10MB"
                      icon={FileText}
                      borderColor="border-orange-300"
                      iconColor="text-orange-400"
                      required={true}
                    />

                    <FileUploadArea
                      uploadType="speaking-rubric"
                      title="📊 Scoring Rubric"
                      description="Kéo thả file rubric hoặc click để chọn"
                      acceptedTypes=".pdf,.docx"
                      maxSize="5MB"
                      icon={FileText}
                      borderColor="border-purple-300"
                      iconColor="text-purple-400"
                      required={false}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {selectedTestType === 'writing' && (
            <Card className="rounded-2xl">
              <CardHeader>
                <CardTitle>Writing Test - Quản lý bài tập viết</CardTitle>
                <CardDescription>Tạo và quản lý bài tập viết đoạn văn và viết theo câu cho học viên</CardDescription>
              </CardHeader>
              <CardContent className="space-y-6">
                {/* Create buttons */}
                <div className="flex gap-3">
                  <Button onClick={() => openWritingDialog('writing_essay')} className="flex-1 h-24 flex-col gap-2">
                    <FileText className="w-8 h-8" />
                    <span className="text-base font-medium">Tạo bài viết đoạn văn</span>
                    <span className="text-xs opacity-80">Essay writing</span>
                  </Button>
                  <Button onClick={() => openWritingDialog('writing_sentence')} variant="outline" className="flex-1 h-24 flex-col gap-2">
                    <MessageSquare className="w-8 h-8" />
                    <span className="text-base font-medium">Tạo bài viết câu</span>
                    <span className="text-xs opacity-80">Sentence translation</span>
                  </Button>
                </div>

                {/* Exercise list */}
                <div className="border-t pt-6">
                  <h3 className="text-lg font-semibold mb-4">Danh sách bài tập đã tạo</h3>
                  {loadingExercises ? (
                    <div className="text-center py-8 text-muted-foreground">Đang tải...</div>
                  ) : exercises.length === 0 ? (
                    <div className="text-center py-8 text-muted-foreground">Chưa có bài tập nào</div>
                  ) : (
                    <div className="space-y-3">
                      {exercises.map((exercise) => (
                        <div key={exercise.id} className="flex items-center justify-between p-4 border rounded-lg hover:bg-gray-50 dark:hover:bg-gray-800">
                          <div className="flex items-center gap-3 flex-1">
                            {exercise.type === 'writing_essay' ? (
                              <FileText className="w-5 h-5 text-green-600" />
                            ) : (
                              <MessageSquare className="w-5 h-5 text-orange-600" />
                            )}
                            <div className="flex-1">
                              <div className="flex items-center gap-2">
                                <h4 className="font-medium">{exercise.title}</h4>
                                {exercise.level && (
                                  <Badge variant="outline" className="text-xs">{exercise.level}</Badge>
                                )}
                              </div>
                              <p className="text-sm text-muted-foreground">
                                {exercise.category} • {exercise.timeLimit} phút
                                {exercise.type === 'writing_sentence' && (
                                  <> • {JSON.parse(exercise.questionsJson || '[]').length} câu</>
                                )}
                              </p>
                            </div>
                          </div>
                          <div className="flex gap-2">
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => openWritingDialog(exercise.type, exercise)}
                            >
                              Sửa
                            </Button>
                            <Button
                              variant="outline"
                              size="sm"
                              onClick={() => handleDeleteExercise(exercise.id)}
                              className="text-red-600 hover:text-red-700"
                            >
                              Xóa
                            </Button>
                          </div>
                        </div>
                      ))}
                    </div>
                  )}
                </div>
              </CardContent>
            </Card>
          )}

          {selectedTestType === 'vocabulary' && (
            <Card className="rounded-2xl">
              <CardHeader>
                <CardTitle>Vocabulary Test - Upload Files</CardTitle>
                <CardDescription>Upload danh sách từ vựng và câu hỏi</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid gap-4 md:grid-cols-2">
                    <FileUploadArea
                      uploadType="vocabulary-list"
                      title="📚 Vocabulary List"
                      description="Kéo thả file từ vựng hoặc click để chọn"
                      acceptedTypes=".csv,.json,.xlsx"
                      maxSize="5MB"
                      icon={FileText}
                      borderColor="border-blue-300"
                      iconColor="text-blue-400"
                      required={true}
                    />

                    <FileUploadArea
                      uploadType="vocabulary-audio"
                      title="🎧 Audio Pronunciation"
                      description="Kéo thả file phát âm hoặc click để chọn"
                      acceptedTypes=".zip,audio/*"
                      maxSize="100MB"
                      icon={Upload}
                      borderColor="border-green-300"
                      iconColor="text-green-400"
                      required={false}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          )}

          {selectedTestType === 'grammar' && (
            <Card className="rounded-2xl">
              <CardHeader>
                <CardTitle>Grammar Test - Upload Files</CardTitle>
                <CardDescription>Upload câu hỏi và bài tập ngữ pháp</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid gap-4 md:grid-cols-2">
                    <FileUploadArea
                      uploadType="grammar-questions"
                      title="📋 Grammar Questions"
                      description="Kéo thả file câu hỏi hoặc click để chọn"
                      acceptedTypes=".json,.csv,.pdf"
                      maxSize="10MB"
                      icon={FileText}
                      borderColor="border-indigo-300"
                      iconColor="text-indigo-400"
                      required={true}
                    />

                    <FileUploadArea
                      uploadType="grammar-rules"
                      title="💡 Grammar Rules"
                      description="Kéo thả file quy tắc hoặc click để chọn"
                      acceptedTypes=".pdf,.docx"
                      maxSize="5MB"
                      icon={FileText}
                      borderColor="border-yellow-300"
                      iconColor="text-yellow-400"
                      required={false}
                    />
                  </div>
                </div>
              </CardContent>
            </Card>
          )}
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
              <div className="space-y-6">
                {questions.map((q) => (
                  <SectionBox key={q.id} className="space-y-4 relative">
                    <div className="flex items-center justify-between">
                      <Label className="text-base font-medium flex items-center">
                        <span className="bg-blue-100 text-blue-800 px-2 py-1 rounded-full text-sm mr-2">
                          Câu {q.id}
                        </span>
                        {q.id === Math.max(...questions.map(qu => qu.id)) && questions.length > 1 && (
                          <Badge variant="secondary" className="ml-2 text-xs">Mới nhất</Badge>
                        )}
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
                    
                    <Textarea
                      placeholder="Nhập câu hỏi..."
                      value={q.question}
                      onChange={(e) => updateQuestion(q.id, 'question', e.target.value)}
                    />
                    
                    <div className="grid gap-2 md:grid-cols-2">
                      {q.options.map((option, optIndex) => (
                        <div key={optIndex} className="flex items-center space-x-2">
                          <input
                            type="radio"
                            name={`correct-${q.id}`}
                            checked={q.correct === optIndex}
                            onChange={() => updateQuestion(q.id, 'correct', optIndex)}
                            className="text-primary"
                          />
                          <Input
                            placeholder={`Đáp án ${String.fromCharCode(65 + optIndex)}`}
                            value={option}
                            onChange={(e) => updateOption(q.id, optIndex, e.target.value)}
                          />
                        </div>
                      ))}
                    </div>
                  </SectionBox>
                ))}
              </div>
            </CardContent>
          </Card>
        </TabsContent>

        <TabsContent value="manage" className="space-y-4">
          <Card className="rounded-2xl">
            <CardHeader>
              <CardTitle>Quản lý file đã tải lên</CardTitle>
              <CardDescription>Xem và quản lý tất cả file đã tải lên hệ thống</CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {/* Filter và Search */}
                <div className="flex gap-4">
                  <div className="flex-1">
                    <Input placeholder="Tìm kiếm file..." />
                  </div>
                  <Select defaultValue="all">
                    <SelectTrigger className="w-48">
                      <SelectValue />
                    </SelectTrigger>
                    <SelectContent>
                      <SelectItem value="all">Tất cả file</SelectItem>
                      <SelectItem value="audio">File âm thanh</SelectItem>
                      <SelectItem value="document">Tài liệu</SelectItem>
                      <SelectItem value="image">Hình ảnh</SelectItem>
                    </SelectContent>
                  </Select>
                </div>

                {/* Files List */}
                <div className="space-y-2">
                  {uploadedFiles.map((file, index) => (
                    <FileRow
                      key={index}
                      fileName={file.name}
                      fileSize={file.size}
                      uploadDate={file.date}
                      onDelete={() => {
                        setUploadedFiles(uploadedFiles.filter((_, i) => i !== index));
                      }}
                      onDownload={() => {
                        // Handle download
                      }}
                    />
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Writing Exercise Dialog */}
      <Dialog open={writingDialogOpen} onOpenChange={setWritingDialogOpen}>
        <DialogContent className="max-w-3xl max-h-[80vh] overflow-y-auto">
          <DialogHeader>
            <DialogTitle>
              {editingExerciseId ? 'Chỉnh sửa' : 'Tạo'} {writingType === 'writing_essay' ? 'bài viết đoạn văn' : 'bài viết câu'}
            </DialogTitle>
            <DialogDescription>
              {writingType === 'writing_essay' 
                ? 'Học viên sẽ viết văn theo chủ đề bạn đưa ra'
                : 'Học viên sẽ dịch câu tiếng Việt sang tiếng Anh'}
            </DialogDescription>
          </DialogHeader>

          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Tên bài test *</Label>
                <Input
                  value={writingTitle}
                  onChange={(e) => setWritingTitle(e.target.value)}
                  placeholder="VD: Bài viết về gia đình"
                />
              </div>
              <div>
                <Label>Chủ đề *</Label>
                <Input
                  value={writingTopic}
                  onChange={(e) => setWritingTopic(e.target.value)}
                  placeholder="VD: Family"
                />
              </div>
            </div>

            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Thời gian (phút) *</Label>
                <Input
                  type="number"
                  min={5}
                  value={writingTimeLimit}
                  onChange={(e) => setWritingTimeLimit(Number(e.target.value))}
                />
              </div>
              <div>
                <Label>Level</Label>
                <Select value={writingLevel} onValueChange={setWritingLevel}>
                  <SelectTrigger>
                    <SelectValue placeholder="Chọn level" />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="A1">A1</SelectItem>
                    <SelectItem value="A2">A2</SelectItem>
                    <SelectItem value="B1">B1</SelectItem>
                    <SelectItem value="B2">B2</SelectItem>
                    <SelectItem value="C1">C1</SelectItem>
                  </SelectContent>
                </Select>
              </div>
            </div>

            <div>
              <Label>Mô tả</Label>
              <Textarea
                value={writingDescription}
                onChange={(e) => setWritingDescription(e.target.value)}
                placeholder="Mô tả bài tập..."
                rows={3}
              />
            </div>

            {writingType === 'writing_sentence' && (
              <div className="space-y-3 border-t pt-4">
                <h4 className="font-medium">Danh sách câu hỏi (5 câu):</h4>
                {writingQuestions.map((q, index) => (
                  <Card key={index} className="p-3">
                    <div className="space-y-2">
                      <Label className="font-semibold">Câu {index + 1}</Label>
                      <div>
                        <Label className="text-xs">Đề bài (tiếng Việt) *</Label>
                        <Input
                          value={q.vietnamesePrompt}
                          onChange={(e) => updateWritingQuestion(index, 'vietnamesePrompt', e.target.value)}
                          placeholder="VD: Tôi yêu gia đình"
                        />
                      </div>
                      <div>
                        <Label className="text-xs">Đáp án (tiếng Anh) *</Label>
                        <Input
                          value={q.correctAnswer}
                          onChange={(e) => updateWritingQuestion(index, 'correctAnswer', e.target.value)}
                          placeholder="VD: I love my family"
                        />
                      </div>
                      <div className="grid grid-cols-2 gap-2">
                        <div>
                          <Label className="text-xs">Gợi ý từ vựng</Label>
                          <Input
                            value={q.vocabularyHint || ''}
                            onChange={(e) => updateWritingQuestion(index, 'vocabularyHint', e.target.value)}
                            placeholder="love, family"
                          />
                        </div>
                        <div>
                          <Label className="text-xs">Gợi ý ngữ pháp</Label>
                          <Input
                            value={q.grammarHint || ''}
                            onChange={(e) => updateWritingQuestion(index, 'grammarHint', e.target.value)}
                            placeholder="Present simple"
                          />
                        </div>
                      </div>
                    </div>
                  </Card>
                ))}
              </div>
            )}
          </div>

          <DialogFooter>
            <Button variant="outline" onClick={() => setWritingDialogOpen(false)}>
              Hủy
            </Button>
            <Button onClick={handleWritingSubmit}>
              {editingExerciseId ? 'Cập nhật' : 'Tạo bài tập'}
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default UploadPage;