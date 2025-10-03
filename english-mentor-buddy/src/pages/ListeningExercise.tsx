import React, { useState, useEffect } from 'react';
import { Volume2, VolumeX, Play, Pause, RotateCcw, Clock, CheckCircle, ArrowLeft } from 'lucide-react';
import Header from '@/components/Navbar';
import { Button } from '@/components/ui/button';
import { Progress } from '@/components/ui/progress';
import { Card, CardContent, CardHeader, CardTitle } from '@/components/ui/card';
import { RadioGroup, RadioGroupItem } from '@/components/ui/radio-group';
import { Label } from '@/components/ui/label';
import { Badge } from '@/components/ui/badge';
import { useToast } from '@/hooks/use-toast';
import { useNavigate } from 'react-router-dom';
import {
  listeningService,
  ListeningExercise,
  ListeningExerciseResult,
  EnglishLevel,
  ListeningQuestionType
} from '@/services/listeningService';

const ListeningExercisePage: React.FC = () => {
  const navigate = useNavigate();
  const { toast } = useToast();
  
  const [exercise, setExercise] = useState<ListeningExercise | null>(null);
  const [currentQuestionIndex, setCurrentQuestionIndex] = useState(0);
  const [answers, setAnswers] = useState<{ [questionId: string]: number }>({});
  const [isLoading, setIsLoading] = useState(false);
  const [isPlaying, setIsPlaying] = useState(false);
  const [startTime] = useState(Date.now());
  const [questionStartTime, setQuestionStartTime] = useState(Date.now());
  const [result, setResult] = useState<ListeningExerciseResult | null>(null);
  const [showResult, setShowResult] = useState(false);

  // Generate a sample exercise for demonstration
  useEffect(() => {
    generateSampleExercise();
  }, []);

  const generateSampleExercise = async () => {
    setIsLoading(true);
    try {
      // For demo purposes, create a sample exercise
      const sampleExercise: ListeningExercise = {
        id: 'sample-1',
        title: 'Travel Adventures',
        topic: 'Travel',
        level: EnglishLevel.B1,
        audioContent: `
Hello everyone! Today I want to share with you my amazing travel adventure from last summer. 
I decided to visit Thailand for two weeks, and it turned out to be one of the most memorable experiences of my life.

I started my journey in Bangkok, the bustling capital city. The streets were incredibly busy, filled with colorful tuk-tuks, 
street food vendors, and friendly locals. The first thing that struck me was the delicious aroma of Thai cuisine everywhere I went. 
I tried pad thai, green curry, and mango sticky rice. Each dish was more flavorful than the last.

After spending three days exploring Bangkok's temples and markets, I traveled north to Chiang Mai. 
This city had a completely different atmosphere - much more peaceful and surrounded by beautiful mountains. 
I participated in a traditional cooking class where I learned to make authentic Thai dishes. 
The instructor was very patient and taught us about the importance of balancing sweet, sour, salty, and spicy flavors.

The highlight of my trip was definitely the elephant sanctuary visit. Unlike tourist attractions that exploit elephants, 
this sanctuary focused on rehabilitation and care. I was able to feed the elephants, walk with them in the forest, 
and even help bathe them in the river. It was incredibly moving to see these majestic creatures in their natural habitat.

On my last few days, I visited the beautiful islands in the south. The beaches had crystal-clear water and white sand. 
I went snorkeling and saw colorful fish and coral reefs. The sunset views were absolutely breathtaking.

This trip taught me so much about Thai culture, cuisine, and the importance of responsible tourism. 
I can't wait to return and explore more of this incredible country.
        `,
        audioUrl: '',
        questions: [
          {
            id: 'q1',
            question: 'How long did the speaker stay in Thailand?',
            options: ['One week', 'Two weeks', 'Three weeks', 'One month'],
            correctOptionIndex: 1,
            explanationInVietnamese: 'Người nói đã ở Thái Lan hai tuần (two weeks) như đã được nhắc đến trong phần đầu.',
            type: ListeningQuestionType.SpecificDetails
          },
          {
            id: 'q2',
            question: 'What was the main difference between Bangkok and Chiang Mai?',
            options: ['The food was different', 'Chiang Mai was more peaceful', 'Bangkok had more temples', 'The weather was different'],
            correctOptionIndex: 1,
            explanationInVietnamese: 'Người nói mô tả Chiang Mai có bầu không khí khác hoàn toàn - yên bình hơn nhiều (much more peaceful).',
            type: ListeningQuestionType.GeneralComprehension
          },
          {
            id: 'q3',
            question: 'What made the elephant sanctuary special according to the speaker?',
            options: ['It was cheaper than other attractions', 'It focused on rehabilitation and care', 'It had more elephants', 'It was located in Bangkok'],
            correctOptionIndex: 1,
            explanationInVietnamese: 'Người nói nhấn mạnh rằng khu bảo tồn này tập trung vào việc phục hồi và chăm sóc voi, không phải khai thác du lịch.',
            type: ListeningQuestionType.Inference
          },
          {
            id: 'q4',
            question: 'How does the speaker feel about the trip overall?',
            options: ['Disappointed', 'Satisfied', 'Very enthusiastic', 'Neutral'],
            correctOptionIndex: 2,
            explanationInVietnamese: 'Người nói thể hiện sự nhiệt tình cao độ, gọi đây là một trong những trải nghiệm đáng nhớ nhất và không thể chờ đợi để quay lại.',
            type: ListeningQuestionType.AttitudeEmotion
          },
          {
            id: 'q5',
            question: 'What does "rehabilitation" mean in the context of the elephant sanctuary?',
            options: ['Training elephants for shows', 'Helping elephants recover and heal', 'Selling elephants to zoos', 'Teaching elephants tricks'],
            correctOptionIndex: 1,
            explanationInVietnamese: '"Rehabilitation" trong ngữ cảnh này có nghĩa là giúp voi hồi phục và chữa lành, không phải khai thác chúng.',
            type: ListeningQuestionType.VocabularyInContext
          }
        ],
        duration: 300,
        createdAt: new Date().toISOString()
      };

      setExercise(sampleExercise);
    } catch (error) {
      toast({
        title: "Lỗi",
        description: "Không thể tạo bài tập nghe. Vui lòng thử lại.",
        variant: "destructive"
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handlePlayAudio = () => {
    if (!exercise) return;

    if (isPlaying) {
      listeningService.stopSpeech();
      setIsPlaying(false);
    } else {
      setIsPlaying(true);
      listeningService.textToSpeech(exercise.audioContent, 'en-US')
        .then(() => {
          setIsPlaying(false);
        })
        .catch((error) => {
          console.error('Speech error:', error);
          setIsPlaying(false);
          toast({
            title: "Lỗi",
            description: "Không thể phát audio. Vui lòng thử lại.",
            variant: "destructive"
          });
        });
    }
  };

  const handleAnswerSelect = (questionId: string, answerIndex: number) => {
    setAnswers(prev => ({
      ...prev,
      [questionId]: answerIndex
    }));
  };

  const handleNextQuestion = () => {
    if (exercise && currentQuestionIndex < exercise.questions.length - 1) {
      setCurrentQuestionIndex(prev => prev + 1);
      setQuestionStartTime(Date.now());
    }
  };

  const handlePreviousQuestion = () => {
    if (currentQuestionIndex > 0) {
      setCurrentQuestionIndex(prev => prev - 1);
      setQuestionStartTime(Date.now());
    }
  };

  const handleSubmit = async () => {
    if (!exercise) return;

    const totalTime = Date.now() - startTime;
    const submissionAnswers = Object.entries(answers).map(([questionId, selectedIndex]) => ({
      questionId,
      selectedOptionIndex: selectedIndex,
      timeSpentOnQuestion: '00:00:30' // Mock time for each question
    }));

    try {
      setIsLoading(true);
      
      // Mock result calculation
      let correctCount = 0;
      const processedAnswers = exercise.questions.map(question => {
        const userAnswer = answers[question.id];
        const isCorrect = userAnswer === question.correctOptionIndex;
        if (isCorrect) correctCount++;
        
        return {
          questionId: question.id,
          selectedOptionIndex: userAnswer ?? -1,
          isCorrect,
          timeSpentOnQuestion: '00:00:30'
        };
      });

      const mockResult: ListeningExerciseResult = {
        exerciseId: exercise.id,
        answers: processedAnswers,
        totalQuestions: exercise.questions.length,
        correctAnswers: correctCount,
        scorePercentage: Math.round((correctCount / exercise.questions.length) * 100),
        timeSpent: new Date(totalTime).toISOString().substr(11, 8),
        completedAt: new Date().toISOString()
      };

      setResult(mockResult);
      setShowResult(true);
    } catch (error) {
      toast({
        title: "Lỗi",
        description: "Không thể nộp bài. Vui lòng thử lại.",
        variant: "destructive"
      });
    } finally {
      setIsLoading(false);
    }
  };

  const handleRestart = () => {
    setAnswers({});
    setCurrentQuestionIndex(0);
    setResult(null);
    setShowResult(false);
    setQuestionStartTime(Date.now());
  };

  if (isLoading) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
        <Header />
        <div className="flex items-center justify-center h-96">
          <div className="text-center">
            <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-indigo-600 mx-auto mb-4"></div>
            <p>Đang tạo bài tập nghe...</p>
          </div>
        </div>
      </div>
    );
  }

  if (showResult && result) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
        <Header />
        <div className="container mx-auto px-4 py-8 max-w-4xl">
          <Card className="mb-8">
            <CardHeader className="text-center">
              <CardTitle className="text-2xl text-green-600">
                <CheckCircle className="inline-block mr-2" />
                Hoàn thành bài tập!
              </CardTitle>
            </CardHeader>
            <CardContent className="text-center">
              <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
                <div className="bg-blue-50 p-4 rounded-lg">
                  <h3 className="font-semibold text-blue-600">Điểm số</h3>
                  <p className="text-3xl font-bold text-blue-700">{result.scorePercentage}%</p>
                </div>
                <div className="bg-green-50 p-4 rounded-lg">
                  <h3 className="font-semibold text-green-600">Câu đúng</h3>
                  <p className="text-3xl font-bold text-green-700">
                    {result.correctAnswers}/{result.totalQuestions}
                  </p>
                </div>
                <div className="bg-purple-50 p-4 rounded-lg">
                  <h3 className="font-semibold text-purple-600">Thời gian</h3>
                  <p className="text-3xl font-bold text-purple-700">{result.timeSpent}</p>
                </div>
              </div>
              
              <div className="flex gap-4 justify-center">
                <Button onClick={handleRestart} variant="outline">
                  <RotateCcw className="mr-2 h-4 w-4" />
                  Làm lại
                </Button>
                <Button onClick={() => navigate('/exercises')}>
                  Bài tập khác
                </Button>
                <Button onClick={() => navigate('/')} variant="outline">
                  <ArrowLeft className="mr-2 h-4 w-4" />
                  Trang chủ
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Detailed Results */}
          <Card>
            <CardHeader>
              <CardTitle>Chi tiết kết quả</CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {exercise?.questions.map((question, index) => {
                  const userAnswer = result.answers.find(a => a.questionId === question.id);
                  const isCorrect = userAnswer?.isCorrect ?? false;
                  
                  return (
                    <div key={question.id} className={`p-4 rounded-lg border ${
                      isCorrect ? 'bg-green-50 border-green-200' : 'bg-red-50 border-red-200'
                    }`}>
                      <div className="flex items-start justify-between mb-2">
                        <h4 className="font-semibold">Câu {index + 1}: {question.question}</h4>
                        {isCorrect ? (
                          <CheckCircle className="h-5 w-5 text-green-600" />
                        ) : (
                          <div className="h-5 w-5 rounded-full bg-red-600"></div>
                        )}
                      </div>
                      
                      <div className="grid grid-cols-1 md:grid-cols-2 gap-2 mb-2">
                        {question.options.map((option, optIndex) => (
                          <div key={optIndex} className={`p-2 rounded text-sm ${
                            optIndex === question.correctOptionIndex 
                              ? 'bg-green-100 text-green-800 font-semibold' 
                              : optIndex === userAnswer?.selectedOptionIndex 
                                ? 'bg-red-100 text-red-800' 
                                : 'bg-gray-100'
                          }`}>
                            {String.fromCharCode(65 + optIndex)}. {option}
                            {optIndex === question.correctOptionIndex && ' ✓'}
                            {optIndex === userAnswer?.selectedOptionIndex && optIndex !== question.correctOptionIndex && ' ✗'}
                          </div>
                        ))}
                      </div>
                      
                      <p className="text-sm text-gray-600 italic">
                        {question.explanationInVietnamese}
                      </p>
                    </div>
                  );
                })}
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  if (!exercise) {
    return (
      <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
        <Header />
        <div className="container mx-auto px-4 py-8 max-w-4xl">
          <div className="text-center">
            <h1 className="text-2xl font-bold mb-4">Không tìm thấy bài tập nghe</h1>
            <Button onClick={() => navigate('/exercises')}>
              <ArrowLeft className="mr-2 h-4 w-4" />
              Quay lại
            </Button>
          </div>
        </div>
      </div>
    );
  }

  const currentQuestion = exercise.questions[currentQuestionIndex];
  const progress = ((currentQuestionIndex + 1) / exercise.questions.length) * 100;
  const allQuestionsAnswered = exercise.questions.every(q => answers[q.id] !== undefined);

  return (
    <div className="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100">
      <Header />
      
      <div className="container mx-auto px-4 py-8 max-w-4xl">
        {/* Header */}
        <div className="mb-8">
          <div className="flex items-center justify-between mb-4">
            <Button
              variant="outline"
              onClick={() => navigate('/exercises')}
            >
              <ArrowLeft className="mr-2 h-4 w-4" />
              Quay lại
            </Button>
            <Badge variant="outline" className="px-3 py-1">
              {exercise.level}
            </Badge>
          </div>
          
          <h1 className="text-3xl font-bold text-gray-800 mb-2">{exercise.title}</h1>
          <p className="text-gray-600 mb-4">Chủ đề: {exercise.topic}</p>
          
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center space-x-4">
              <Clock className="h-4 w-4 text-gray-500" />
              <span className="text-sm text-gray-600">
                Thời gian: {Math.ceil(exercise.duration / 60)} phút
              </span>
            </div>
            <div className="text-sm text-gray-600">
              Câu {currentQuestionIndex + 1} / {exercise.questions.length}
            </div>
          </div>
          
          <Progress value={progress} className="w-full" />
        </div>

        {/* Audio Player */}
        <Card className="mb-8">
          <CardContent className="p-6">
            <div className="flex items-center justify-center space-x-4">
              <Button
                size="lg"
                onClick={handlePlayAudio}
                className="flex items-center space-x-2"
              >
                {isPlaying ? (
                  <>
                    <Pause className="h-5 w-5" />
                    <span>Dừng</span>
                  </>
                ) : (
                  <>
                    <Play className="h-5 w-5" />
                    <span>Phát audio</span>
                  </>
                )}
              </Button>
              
              <div className="flex items-center space-x-2 text-sm text-gray-600">
                <Volume2 className="h-4 w-4" />
                <span>Nhấn để nghe đoạn hội thoại</span>
              </div>
            </div>
          </CardContent>
        </Card>

        {/* Current Question */}
        <Card className="mb-8">
          <CardHeader>
            <CardTitle className="flex items-center justify-between">
              <span>Câu {currentQuestionIndex + 1}</span>
              <Badge variant="secondary">
                {currentQuestion.type === ListeningQuestionType.GeneralComprehension && 'Hiểu tổng quan'}
                {currentQuestion.type === ListeningQuestionType.SpecificDetails && 'Chi tiết cụ thể'}
                {currentQuestion.type === ListeningQuestionType.Inference && 'Suy luận'}
                {currentQuestion.type === ListeningQuestionType.AttitudeEmotion && 'Thái độ/Cảm xúc'}
                {currentQuestion.type === ListeningQuestionType.VocabularyInContext && 'Từ vựng trong ngữ cảnh'}
              </Badge>
            </CardTitle>
          </CardHeader>
          <CardContent>
            <h3 className="text-lg font-semibold mb-4">{currentQuestion.question}</h3>
            
            <RadioGroup
              value={answers[currentQuestion.id]?.toString() || ''}
              onValueChange={(value) => handleAnswerSelect(currentQuestion.id, parseInt(value))}
            >
              {currentQuestion.options.map((option, index) => (
                <div key={index} className="flex items-center space-x-2 p-3 rounded-lg hover:bg-gray-50">
                  <RadioGroupItem value={index.toString()} id={`option-${index}`} />
                  <Label htmlFor={`option-${index}`} className="flex-1 cursor-pointer">
                    <span className="font-medium mr-2">{String.fromCharCode(65 + index)}.</span>
                    {option}
                  </Label>
                </div>
              ))}
            </RadioGroup>
          </CardContent>
        </Card>

        {/* Navigation */}
        <div className="flex items-center justify-between">
          <Button
            variant="outline"
            onClick={handlePreviousQuestion}
            disabled={currentQuestionIndex === 0}
          >
            Câu trước
          </Button>
          
          <div className="flex space-x-2">
            {exercise.questions.map((_, index) => (
              <button
                key={index}
                onClick={() => setCurrentQuestionIndex(index)}
                className={`w-8 h-8 rounded-full text-sm font-medium ${
                  index === currentQuestionIndex
                    ? 'bg-indigo-600 text-white'
                    : answers[exercise.questions[index].id] !== undefined
                    ? 'bg-green-100 text-green-600 border border-green-200'
                    : 'bg-gray-100 text-gray-600 border border-gray-200'
                }`}
              >
                {index + 1}
              </button>
            ))}
          </div>
          
          {currentQuestionIndex === exercise.questions.length - 1 ? (
            <Button
              onClick={handleSubmit}
              disabled={!allQuestionsAnswered || isLoading}
              className="bg-green-600 hover:bg-green-700"
            >
              {isLoading ? 'Đang nộp...' : 'Nộp bài'}
            </Button>
          ) : (
            <Button
              onClick={handleNextQuestion}
              disabled={answers[currentQuestion.id] === undefined}
            >
              Câu tiếp theo
            </Button>
          )}
        </div>
      </div>
    </div>
  );
};

export default ListeningExercisePage;