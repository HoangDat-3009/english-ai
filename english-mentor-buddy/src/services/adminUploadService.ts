// 🔄 ADMIN UPLOAD SERVICE - Kết nối Admin Upload với Reading Exercises
// ✅ Sync admin uploaded files với Reading Exercises page
// 🎯 Features: File upload handling, exercise creation from uploaded content

import { Question, ReadingExercise } from './databaseStatsService';

interface BackendQuestion {
  Id?: number;
  QuestionText?: string;
  question?: string;
  Options?: string[];
  options?: string[];
  correctAnswer?: number;
  Explanation?: string;
  explanation?: string;
}

export interface UploadedFile {
  id: string;
  name: string;
  size: string;
  date: string;
  status: 'uploaded' | 'processing' | 'error' | 'local';
  type: 'reading-passages' | 'reading-questions' | 'audio' | 'document';
  content?: string; // For text-based files
  exercises?: ReadingExercise[]; // Parsed exercises from the file
}

export interface AdminUploadedExercise {
  id: number;
  name: string;
  content: string;
  level: 'Beginner' | 'Intermediate' | 'Advanced';
  type: 'Part 5' | 'Part 6' | 'Part 7';
  sourceType: 'manual';
  questions: Question[];
  uploadedBy: string;
  originalFileName: string;
  createdAt: string;
  updatedAt: string;
}

class AdminUploadService {
  private readonly STORAGE_KEY = 'admin_uploaded_exercises';
  private readonly FILES_KEY = 'admin_uploaded_files';

  // Lưu file upload từ admin
  saveUploadedFile(file: UploadedFile): void {
    const files = this.getUploadedFiles();
    const existingIndex = files.findIndex(f => f.id === file.id);
    
    if (existingIndex >= 0) {
      files[existingIndex] = file;
    } else {
      files.push(file);
    }
    
    localStorage.setItem(this.FILES_KEY, JSON.stringify(files));
  }

  // Lấy tất cả files đã upload
  getUploadedFiles(): UploadedFile[] {
    const stored = localStorage.getItem(this.FILES_KEY);
    return stored ? JSON.parse(stored) : [];
  }

  // Tạo Reading Exercise từ uploaded content - GỬI VỀ API BACKEND
  async createExerciseFromUpload(
    fileName: string,
    content: string,
    level: 'Beginner' | 'Intermediate' | 'Advanced',
    type: 'Part 5' | 'Part 6' | 'Part 7',
    uploadedBy: string = 'Admin'
  ): Promise<ReadingExercise> {
    try {
      // Gọi API tạo exercise với AI questions
      const response = await fetch('http://localhost:5283/api/ReadingExercise/create-with-ai', {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
        },
        body: JSON.stringify({
          name: this.generateExerciseName(fileName, type),
          content,
          level,
          type,
          description: `Exercise created from admin upload: ${fileName}`,
          estimatedMinutes: type === 'Part 5' ? 15 : type === 'Part 6' ? 20 : 30,
          createdBy: uploadedBy,
          questionCount: type === 'Part 5' ? 5 : type === 'Part 6' ? 4 : 6
        })
      });

      if (!response.ok) {
        throw new Error(`API error: ${response.status}`);
      }

      const exercise = await response.json();
      
      // Convert API response to frontend format
      return {
        id: exercise.id || exercise.Id,
        exerciseId: exercise.id || exercise.Id,
        name: exercise.name || exercise.Name,
        content: exercise.content || exercise.Content,
        level: exercise.level || exercise.Level,
        type: exercise.type || exercise.Type,
        sourceType: 'manual',
        questions: (exercise.Questions || exercise.questions || []).map((q: BackendQuestion) => ({
          question: q.QuestionText || q.question,
          options: q.Options || q.options || [],
          correctAnswer: q.correctAnswer ?? -1,
          explanation: q.Explanation || q.explanation
        })),
        createdAt: exercise.CreatedAt || exercise.createdAt || new Date().toISOString(),
        updatedAt: exercise.UpdatedAt || exercise.updatedAt || new Date().toISOString()
      };
    } catch (error) {
      console.error('Error creating exercise via API:', error);
      
      // Fallback to localStorage if API fails
      const exercises = this.getAdminExercises();
      const newId = Math.max(0, ...exercises.map(e => e.id)) + 1000; // Use high ID to avoid conflicts
      
      const questions = this.parseContentToQuestions(content, type);
      
      const exercise: ReadingExercise = {
        id: newId,
        exerciseId: newId,
        name: this.generateExerciseName(fileName, type),
        content,
        level,
        type,
        sourceType: 'manual',
        questions,
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      };
      
      // Lưu exercise vào localStorage as fallback
      exercises.push(exercise);
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(exercises));
      
      return exercise;
    }
  }

  // Lấy tất cả exercises từ admin
  getAdminExercises(): ReadingExercise[] {
    const stored = localStorage.getItem(this.STORAGE_KEY);
    return stored ? JSON.parse(stored) : [];
  }

  // Xóa exercise
  deleteExercise(exerciseId: number): boolean {
    const exercises = this.getAdminExercises();
    const filtered = exercises.filter(e => e.id !== exerciseId);
    
    if (filtered.length < exercises.length) {
      localStorage.setItem(this.STORAGE_KEY, JSON.stringify(filtered));
      return true;
    }
    return false;
  }

  // Parse nội dung thành questions (simplified version)
  private parseContentToQuestions(content: string, type: 'Part 5' | 'Part 6' | 'Part 7'): Question[] {
    // Đây là parser đơn giản - trong thực tế sẽ phức tạp hơn
    const questions: Question[] = [];
    
    if (type === 'Part 5') {
      // Parse grammar questions
      const lines = content.split('\n').filter(line => line.trim());
      let currentQuestion: Partial<Question> = {};
      
      lines.forEach((line, index) => {
        if (line.match(/^\d+\./)) {
          // New question
          if (currentQuestion.question && currentQuestion.options) {
            questions.push(currentQuestion as Question);
          }
          currentQuestion = {
            question: line.replace(/^\d+\.\s*/, ''),
            options: [],
            correctAnswer: 0,
            explanation: 'Admin uploaded question'
          };
        } else if (line.match(/^[A-D]\)/)) {
          // Option
          if (currentQuestion.options) {
            currentQuestion.options.push(line.replace(/^[A-D]\)\s*/, ''));
          }
        } else if (line.toLowerCase().includes('answer:')) {
          // Answer
          const answerMatch = line.match(/answer:\s*([A-D])/i);
          if (answerMatch && currentQuestion) {
            currentQuestion.correctAnswer = answerMatch[1].charCodeAt(0) - 65; // A=0, B=1, etc.
          }
        }
      });
      
      // Add last question
      if (currentQuestion.question && currentQuestion.options) {
        questions.push(currentQuestion as Question);
      }
    } else {
      // For Part 6 and 7, create sample questions
      for (let i = 1; i <= (type === 'Part 6' ? 4 : 8); i++) {
        questions.push({
          question: `Question ${i} based on the uploaded content.`,
          options: ['Option A', 'Option B', 'Option C', 'Option D'],
          correctAnswer: 0,
          explanation: 'This question was generated from uploaded content.'
        });
      }
    }
    
    return questions.length > 0 ? questions : [{
      question: 'Sample question from uploaded content.',
      options: ['Option A', 'Option B', 'Option C', 'Option D'],
      correctAnswer: 0,
      explanation: 'Generated from uploaded file.'
    }];
  }

  // Generate tên exercise từ filename
  private generateExerciseName(fileName: string, type: string): string {
    const baseName = fileName.replace(/\.[^/.]+$/, ''); // Remove extension
    return `${type}: ${baseName} (Admin Upload)`;
  }

  // Tích hợp với Reading Exercises - merge admin uploads với mock data
  getAllReadingExercises(): ReadingExercise[] {
    const adminExercises = this.getAdminExercises();
    
    // Mock data từ databaseStatsService
    const mockExercises: ReadingExercise[] = [
      {
        id: 100,
        exerciseId: 100,
        name: 'Part 5: Grammar & Vocabulary - Beginner (Mock)',
        content: 'Complete the sentences by choosing the best answer.',
        level: 'Beginner',
        type: 'Part 5',
        sourceType: 'manual',
        questions: [
          {
            question: 'The company will _____ a new product next month.',
            options: ['launch', 'launches', 'launching', 'launched'],
            correctAnswer: 0,
            explanation: 'Future tense requires base form after "will".'
          }
        ],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }
    ];
    
    // Combine admin + mock, with admin exercises having priority
    return [...adminExercises, ...mockExercises];
  }

  // Import exercise từ JSON/CSV format
  async importExerciseFromJSON(jsonContent: string): Promise<ReadingExercise[]> {
    try {
      const data = JSON.parse(jsonContent);
      const exercises: ReadingExercise[] = [];
      
      if (Array.isArray(data)) {
        for (const item of data) {
          if (item.name && item.content && item.questions) {
            const exercise = await this.createExerciseFromUpload(
              item.name || `Imported Exercise ${data.indexOf(item) + 1}`,
              item.content,
              item.level || 'Intermediate',
              item.type || 'Part 7',
              'Admin Import'
            );
            exercises.push(exercise);
          }
        }
      }
      
      return exercises;
    } catch (error) {
      console.error('Error importing exercise from JSON:', error);
      return [];
    }
  }

  // Clear all admin data (for testing)
  clearAll(): void {
    localStorage.removeItem(this.STORAGE_KEY);
    localStorage.removeItem(this.FILES_KEY);
  }

  // Get statistics
  getUploadStats() {
    const exercises = this.getAdminExercises();
    const files = this.getUploadedFiles();
    
    return {
      totalExercises: exercises.length,
      totalFiles: files.length,
      byLevel: {
        Beginner: exercises.filter(e => e.level === 'Beginner').length,
        Intermediate: exercises.filter(e => e.level === 'Intermediate').length,
        Advanced: exercises.filter(e => e.level === 'Advanced').length,
      },
      byType: {
        'Part 5': exercises.filter(e => e.type === 'Part 5').length,
        'Part 6': exercises.filter(e => e.type === 'Part 6').length,
        'Part 7': exercises.filter(e => e.type === 'Part 7').length,
      },
      recentUploads: exercises.slice(-5).map(e => ({
        name: e.name,
        createdAt: e.createdAt
      }))
    };
  }
}

export const adminUploadService = new AdminUploadService();