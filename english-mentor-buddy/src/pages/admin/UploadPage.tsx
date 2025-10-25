import { FileRow } from '@/components/admin/FileRow';
import { SectionBox } from '@/components/admin/SectionBox';
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";
import { useToast } from '@/hooks/use-toast';
import { apiService } from '@/services/api';
import { CheckCircle, Clock, Eye, FileText, LucideIcon, Minus, Plus, Save, Upload, X } from 'lucide-react';
import { useRef, useState } from 'react';

const UploadPage = () => {
  const [testType, setTestType] = useState('');
  const [selectedTestType, setSelectedTestType] = useState('');
  const [questions, setQuestions] = useState([{ id: 1, question: '', options: ['', '', '', ''], correct: 0 }]);
  interface UploadedFileItem { id?: string; name: string; size: string; date: string; status?: 'uploaded' | 'processing' | 'error' | 'local' }
  const [uploadedFiles, setUploadedFiles] = useState<UploadedFileItem[]>([
    { name: 'TOEIC_Part1_Audio.mp3', size: '2.4MB', date: '2024-03-10', status: 'uploaded' },
    { name: 'Reading_Questions.pdf', size: '1.8MB', date: '2024-03-09', status: 'uploaded' }
  ]);
  const [fallbackFilesMap, setFallbackFilesMap] = useState<Record<string, File[]>>({});

  // File upload states
  const [uploadProgress, setUploadProgress] = useState<{[key: string]: number | undefined}>({});
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
  const { success, error } = useToast();

  const handleFileSelect = (uploadType: string, files: FileList | null) => {
    if (!files) return;

    const fileArray = Array.from(files);
    setSelectedFiles(prev => ({
      ...prev,
      [uploadType]: fileArray
    }));

    // Start real upload using apiService with progress callback
    const fd = new FormData();
    fileArray.forEach((f) => fd.append('files', f));
    fd.append('testType', selectedTestType || uploadType);

    setUploadProgress(prev => ({ ...prev, [uploadType]: 0 }));

    apiService.postFormDataWithProgress('/api/admin/upload', fd, (pct) => {
      setUploadProgress(prev => ({ ...prev, [uploadType]: pct }));
    }).then((res: { files?: Array<{ originalName?: string; storedName?: string; size?: number }> }) => {
      setUploadProgress(prev => ({ ...prev, [uploadType]: 100 }));
      success('Upload thành công', `Đã tải lên ${fileArray.length} file.`);
      // If server returns metadata, add to uploadedFiles list
      if (res?.files && Array.isArray(res.files)) {
        const newFiles = res.files.map((f) => ({
          name: f.originalName || f.storedName,
          size: formatFileSize(f.size || 0),
          date: new Date().toISOString().split('T')[0]
        }));
        setUploadedFiles(prev => [...newFiles, ...prev]);
      }
    }).catch((err) => {
      console.error('Upload failed', err);
      setUploadProgress(prev => ({ ...prev, [uploadType]: undefined }));
      error('Upload thất bại', err?.message || 'Có lỗi xảy ra khi tải file lên');
      // Fallback: lưu local để quản lý tạm thời khi backend chưa sẵn sàng
      try {
        const fallbackId = `local-${Date.now()}`;
        setFallbackFilesMap(prev => ({ ...prev, [fallbackId]: fileArray }));
        const fallbackFiles = fileArray.map((f) => ({
          id: fallbackId,
          name: f.name,
          size: formatFileSize(f.size || 0),
          date: new Date().toISOString().split('T')[0],
          status: 'local' as const
        }));
        setUploadedFiles(prev => [...fallbackFiles, ...prev]);
        error('Đã lưu tạm file cục bộ', 'File đã được lưu tạm trong trang quản lý.');
      } catch (fallbackError) {
        console.warn('Could not save fallback files:', fallbackError);
      }
    });
  };

  // Retry upload for a fallback/local entry
  const retryUpload = (id?: string) => {
    if (!id) return;
    const files = fallbackFilesMap[id];
    if (!files || files.length === 0) return;

    const fd = new FormData();
    files.forEach(f => fd.append('files', f));

    // mark progress
    setUploadProgress(prev => ({ ...prev, [id]: 0 }));

    apiService.postFormDataWithProgress('/api/admin/upload', fd, (pct) => {
      setUploadProgress(prev => ({ ...prev, [id]: pct }));
    }).then((res: { files?: Array<{ originalName?: string; storedName?: string; size?: number }> }) => {
      // update uploadedFiles entry status -> uploaded
      setUploadedFiles(prev => prev.map(it => it.id === id ? { ...it, status: 'uploaded' } : it));
      // remove fallback map entry
      setFallbackFilesMap(prev => {
        const copy = { ...prev };
        delete copy[id];
        return copy;
      });
      success('Upload thành công', 'Đã tải lại file thành công.');
    }).catch((err) => {
      console.error('Retry upload failed', err);
      error('Upload lại thất bại', err?.message || 'Không thể tải lại file');
      setUploadProgress(prev => ({ ...prev, [id]: undefined }));
    });
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

          {selectedTestType && (
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
                <CardTitle>Writing Test - Upload Files</CardTitle>
                <CardDescription>Upload đề bài và rubric cho bài test viết</CardDescription>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid gap-4 md:grid-cols-2">
                    <FileUploadArea
                      uploadType="writing-prompts"
                      title="✍️ Writing Prompts"
                      description="Kéo thả file đề bài hoặc click để chọn"
                      acceptedTypes=".pdf,.docx,.json"
                      maxSize="10MB"
                      icon={FileText}
                      borderColor="border-red-300"
                      iconColor="text-red-400"
                      required={true}
                    />

                    <FileUploadArea
                      uploadType="writing-rubric"
                      title="📊 Scoring Rubric"
                      description="Kéa thả file rubric hoặc click để chọn"
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
                  <div key={q.id}>
                    <SectionBox className="space-y-4 relative">
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
                  </div>
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
                    <div key={file.id ?? index}>
                      <FileRow
                        fileName={file.name}
                        fileSize={file.size}
                        uploadDate={file.date}
                        status={file.status as 'uploaded' | 'processing' | 'error' | 'local'}
                        onDelete={() => {
                          setUploadedFiles(uploadedFiles.filter((_, i) => i !== index));
                          if (file.id) {
                            setFallbackFilesMap(prev => { const c = { ...prev }; delete c[file.id!]; return c; });
                          }
                        }}
                        onDownload={() => {
                          // Handle download
                        }}
                        onRetry={file.id ? () => retryUpload(file.id) : undefined}
                      />
                    </div>
                  ))}
                </div>
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
        <Button className="rounded-xl">
          <Save className="mr-2 h-4 w-4" />
          Lưu bài test
        </Button>
      </div>
    </div>
  );
};

export default UploadPage;