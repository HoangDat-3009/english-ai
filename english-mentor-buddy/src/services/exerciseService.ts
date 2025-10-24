import { apiService } from './api';

export interface ExerciseGenerationParams {
  Topic: string;
  AssignmentTypes: number[];
  EnglishLevel: number;
  TotalQuestions: number;
}
export enum AssignmentType {
  MostSuitableWord = 1,
  VerbConjugation = 2,
  ConditionalSentences = 3,
  IndirectSpeech = 4,
  FillTheBlank = 5,
  ReadingComprehension = 6,
  Grammar = 7,
  Collocations = 8,
  SynonymAndAntonym = 9,
  Vocabulary = 10,
  ErrorIdentification = 11,
  WordFormation = 12,
  PassiveVoice = 13,
  RelativeClauses = 14,
  ComparisonSentences = 15,
  Inversion = 16,
  Articles = 17,
  Prepositions = 18,
  Idioms = 19,
  SentenceTransformation = 20,
  PronunciationAndStress = 21,
  ClozeTest = 22,
  SentenceCombination = 23,
  MatchingHeadings = 24,
  DialogueCompletion = 25,
  SentenceOrdering = 26,
  WordMeaningInContext = 27
}

export interface Question {
  Question: string;
  Options: string[];
  RightOptionIndex: number;
  ExplanationInVietnamese: string;
}

export interface ExerciseSet {
  Topic: string;
  Questions: Question[];
  TimeLimit?: number;
}

export interface SubmissionResult {
  score: number;
  totalQuestions: number;
  correctAnswers: number;
  feedback: string;
}

export const exerciseService = {
  // Generate exercise
  generateExercise: async (params: ExerciseGenerationParams): Promise<ExerciseSet> => {
    try {
      // Format request to match the required structure
      const requestBody = {
        Topic: params.Topic,
        AssignmentTypes: params.AssignmentTypes,
        EnglishLevel: params.EnglishLevel,
        TotalQuestions: params.TotalQuestions
      };

      console.log('REQUEST - generateExercise:', JSON.stringify(requestBody, null, 2));

      const response = await fetch(`${apiService.getBaseUrl()}/api/Assignment/Generate`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...apiService.getHeaders()
        },
        body: JSON.stringify(requestBody)
      });

      if (!response.ok) {
        console.error('API Error - Status:', response.status);
        console.error('API Error - Status Text:', response.statusText);
        const errorText = await response.text();
        console.error('API Error - Response:', errorText);
        throw new Error('Failed to generate exercise');
      }

      const jsonData = await response.json();
      console.log('RESPONSE - Raw JSON:', jsonData);

      // Xử lý dữ liệu trả về để đảm bảo định dạng đúng
      let formattedData: ExerciseSet;

      // Kiểm tra nếu dữ liệu trả về là một mảng các câu hỏi
      if (Array.isArray(jsonData)) {
        formattedData = {
          Topic: params.Topic,
          Questions: jsonData.map(q => ({
            Question: q.Question,
            Options: q.Options,
            RightOptionIndex: q.RightOptionIndex,
            ExplanationInVietnamese: q.ExplanationInVietnamese
          }))
        };
      }
      // Kiểm tra nếu dữ liệu trả về có cấu trúc Questions
      else if (jsonData.Questions && Array.isArray(jsonData.Questions)) {
        formattedData = jsonData;
      }
      // Trường hợp dữ liệu trả về là một câu hỏi đơn lẻ
      else {
        formattedData = {
          Topic: params.Topic,
          Questions: [jsonData]
        };
      }

      console.log('RESPONSE - Formatted:', JSON.stringify(formattedData, null, 2));
      return formattedData;
    } catch (error) {
      console.error('Error generating exercise:', error);
      throw error;
    }
  },

  // Submit answers and get results
  submitAnswers: async (exerciseSet: ExerciseSet, answers: Record<number, string>): Promise<SubmissionResult> => {
    try {
      // Format the submission data
      console.log('SUBMISSION DATA:');
      console.log('Exercise Set:', JSON.stringify(exerciseSet, null, 2));
      console.log('User Answers:', JSON.stringify(answers, null, 2));

      // Tính toán kết quả dựa trên câu trả lời và đáp án đúng
      let correctAnswers = 0;

      Object.entries(answers).forEach(([questionIndex, answer]) => {
        const index = parseInt(questionIndex) - 1;
        if (index >= 0 && index < exerciseSet.Questions.length) {
          const question = exerciseSet.Questions[index];
          const correctAnswer = question.Options[question.RightOptionIndex];
          const isCorrect = correctAnswer === answer;

          console.log(`Question ${questionIndex}:`, {
            question: question.Question,
            userAnswer: answer,
            correctAnswer: correctAnswer,
            isCorrect: isCorrect
          });

          if (isCorrect) {
            correctAnswers++;
          }
        }
      });

      const totalQuestions = exerciseSet.Questions.length;
      const score = Math.round((correctAnswers / totalQuestions) * 100);

      // Tạo phản hồi dựa trên kết quả
      let feedback = '';
      if (score >= 90) {
        feedback = 'Xuất sắc! Bạn đã nắm vững kiến thức.';
      } else if (score >= 70) {
        feedback = 'Tốt! Bạn đã hiểu phần lớn nội dung.';
      } else if (score >= 50) {
        feedback = 'Khá! Bạn cần ôn tập thêm một số phần.';
      } else {
        feedback = 'Bạn cần ôn tập lại kiến thức cơ bản.';
      }

      const result = {
        score,
        totalQuestions,
        correctAnswers,
        feedback
      };

      console.log('RESULT - submitAnswers:', JSON.stringify(result, null, 2));

      // Expected result format for reference
      console.log('EXPECTED RESULT FORMAT:');
      console.log(JSON.stringify({
        "score": 80,
        "totalQuestions": 10,
        "correctAnswers": 8,
        "feedback": "Tốt! Bạn đã hiểu phần lớn nội dung."
      }, null, 2));

      return result;
    } catch (error) {
      console.error('Error submitting answers:', error);
      throw error;
    }
  }
};
 // ==================== READING EXERCISES ====================

export interface ReadingQuestion {
  question: string;
  options: string[];
  correctAnswer: number;
  explanation?: string;
}

export interface ReadingExercise {
  id: number; name: string; content: string;
  level: 'Beginner' | 'Intermediate' | 'Advanced';
  type: 'Part 5' | 'Part 6' | 'Part 7';
  questions: ReadingQuestion[]; 
  sourceType: 'uploaded' | 'ai'; 
  timeLimit: number;
}

export interface UserResult {
  id: number; userId: number; exerciseId: number; score: number;
  totalQuestions: number; answers: number[]; dateCompleted: string; attempts: number;
}

// ==================== MOCK DATA ====================
const MOCK_READING_EXERCISES: ReadingExercise[] = [
  {
    id: 1,
    name: "TOEIC Part 5 - Grammar & Vocabulary",
    content: "Part 5 focuses on grammar and vocabulary in workplace contexts. Complete each sentence by choosing the correct word or phrase.",
    level: "Intermediate",
    type: "Part 5",
    sourceType: "uploaded",
    timeLimit: 15,
    questions: [
      {
        question: "The new marketing campaign was _______ successful than the previous one.",
        options: ["more", "most", "much", "many"],
        correctAnswer: 0,
        explanation: "'More' is used for comparative form with 'than'"
      },
      {
        question: "All employees must _______ their ID badges at all times.",
        options: ["wear", "wearing", "wore", "worn"],
        correctAnswer: 0,
        explanation: "Modal verb 'must' is followed by base form of verb"
      },
      {
        question: "The meeting has been _______ until next Friday.",
        options: ["postponed", "postponing", "postpone", "postpones"],
        correctAnswer: 0,
        explanation: "Past participle is used with 'has been' in passive voice"
      }
    ]
  },
  {
    id: 2,
    name: "TOEIC Part 6 - Text Completion",
    content: `Dear Mr. Johnson,

We are pleased to inform you that your application has been ___(1)___. The position will ___(2)___ you to travel frequently to our overseas offices in Tokyo, London, and Sydney.

Your starting salary will be $75,000 per year, and you will be eligible for our comprehensive benefits package, which includes health insurance, dental coverage, and a 401(k) retirement plan.

Please ___(3)___ the attached documents and return them by email before your start date of March 15th. We look forward to welcoming you to our team.

Sincerely,
Sarah Miller
Human Resources Manager`,
    level: "Advanced",
    type: "Part 6",
    sourceType: "uploaded",
    timeLimit: 20,
    questions: [
      {
        question: "Dear Mr. Johnson, We are pleased to inform you that your application has been ___.",
        options: ["accepted", "accepting", "accepts", "acceptance"],
        correctAnswer: 0,
        explanation: "Past participle 'accepted' is used in passive voice"
      },
      {
        question: "The position will ___ you to travel frequently to our overseas offices.",
        options: ["require", "requiring", "required", "requirement"],
        correctAnswer: 0,
        explanation: "Modal 'will' is followed by base form of verb"
      },
      {
        question: "Please ___ the attached documents and return them by email.",
        options: ["review", "reviews", "reviewing", "reviewed"],
        correctAnswer: 0,
        explanation: "Imperative sentence uses base form of verb"
      }
    ]
  },
  {
    id: 3,
    name: "TOEIC Part 7 - Reading Comprehension",
    content: `MEMORANDUM

TO: All Sales Staff
FROM: Jennifer Park, Sales Director  
DATE: October 10, 2025
RE: Q3 Sales Review Meeting

I am pleased to announce that our third quarter sales figures exceeded expectations by 15%. This success is largely due to the hard work and dedication of our entire sales team.

We will be holding a mandatory meeting on Wednesday, October 17th at 2:00 PM in Conference Room B to review our Q3 performance in detail. During this meeting, we will:

• Analyze sales data by region and product line
• Discuss successful strategies that led to increased revenue  
• Set goals for the fourth quarter
• Recognize top-performing team members

Please bring your individual sales reports and be prepared to share insights about your most successful client interactions. All reports must be submitted to me by Wednesday, October 17th at 12:00 PM - two hours before the meeting begins.

Light refreshments will be provided. If you have any questions, please contact me at jpark@company.com.

Thank you for your continued excellence.`,
    level: "Advanced",
    type: "Part 7",
    sourceType: "uploaded",
    timeLimit: 25,
    questions: [
      {
        question: "According to the memo, what is the main purpose of the meeting?",
        options: [
          "To discuss budget cuts",
          "To introduce new software",
          "To review quarterly sales",
          "To plan the holiday party"
        ],
        correctAnswer: 2,
        explanation: "The memo states the meeting is to review Q3 sales performance"
      },
      {
        question: "When is the deadline for submitting the reports?",
        options: [
          "Monday, October 15th",
          "Wednesday, October 17th at 12:00 PM", 
          "Friday, October 19th",
          "Monday, October 22nd"
        ],
        correctAnswer: 1,
        explanation: "The memo clearly states reports are due by Wednesday, October 17th at 12:00 PM"
      }
    ]
  },
  {
    id: 4,
    name: "Business Email - AI Generated",
    content: `Subject: Project Update Request

Hello Team,

I hope this email finds you well. I am writing to request an update on the quarterly marketing project that we discussed in last week's meeting.

As we ___(1)___ towards the end of the quarter, it is important that we ___(2)___ our progress and ensure that we are meeting our established deadlines. The client has expressed interest in seeing preliminary results by the end of this month.

Could each team member please ___(3)___ me with a brief status report by Friday? This will help us prepare for our presentation to the board next week.

Thank you for your attention to this matter.

Best regards,
Alex Chen
Project Manager`,
    level: "Beginner",
    type: "Part 6",
    sourceType: "ai",
    timeLimit: 15,
    questions: [
      {
        question: "As we ___ towards the end of the quarter, it is important...",
        options: ["move", "moving", "moved", "moves"],
        correctAnswer: 0,
        explanation: "Present simple tense with 'we' requires base form"
      },
      {
        question: "...it is important that we ___ our progress...",
        options: ["monitor", "monitoring", "monitored", "monitors"],
        correctAnswer: 0,
        explanation: "Subjunctive mood after 'it is important that' uses base form"
      },
      {
        question: "Could each team member please ___ me with a brief status report?",
        options: ["provide", "providing", "provided", "provides"],
        correctAnswer: 0,
        explanation: "Modal 'could' is followed by base form of verb"
      }
    ]
  },
  {
    id: 5,
    name: "Travel Notice - Reading Comprehension",
    content: `IMPORTANT TRAVEL NOTICE
                                                
Flight Information Update
Golden Airways Flight GA 428
London Heathrow (LHR) → New York JFK

DEPARTURE CHANGE NOTIFICATION

Dear Valued Passengers,

Due to unexpected weather conditions affecting the London area, we regret to inform you that Flight GA 428 scheduled to depart on December 15th at 3:45 PM has been delayed.

NEW DEPARTURE DETAILS:
• Original departure: December 15th, 3:45 PM
• New departure: December 15th, 7:20 PM  
• Gate: B12 (changed from A7)
• Check-in: Now open at counters 15-18
• Boarding: Will begin 30 minutes before departure

COMPENSATION:
All passengers will receive a complimentary meal voucher valued at £15, which can be used at any restaurant in Terminal 5. Vouchers are available at the customer service desk near Gate B12.

For passengers with connecting flights in New York, our staff will assist you in rebooking your connections at no additional charge.

We sincerely apologize for any inconvenience caused and appreciate your patience.

For real-time updates, please check our mobile app or visit our website.

Golden Airways Customer Service`,
    level: "Intermediate",
    type: "Part 7",
    sourceType: "ai",
    timeLimit: 20,
    questions: [
      {
        question: "What is the main reason for the flight delay?",
        options: [
          "Technical problems with the aircraft",
          "Weather conditions in London",
          "Air traffic control issues",
          "Security concerns at the airport"
        ],
        correctAnswer: 1,
        explanation: "The notice clearly states 'Due to unexpected weather conditions affecting the London area'"
      },
      {
        question: "Where can passengers obtain their meal vouchers?",
        options: [
          "At counters 15-18",
          "At Gate A7",
          "At the customer service desk near Gate B12",
          "At any restaurant in Terminal 5"
        ],
        correctAnswer: 2,
        explanation: "The notice states vouchers are available 'at the customer service desk near Gate B12'"
      },
      {
        question: "How much earlier should passengers arrive for boarding?",
        options: [
          "15 minutes before departure",
          "30 minutes before departure",
          "45 minutes before departure",
          "60 minutes before departure"
        ],
        correctAnswer: 1,
        explanation: "The notice states 'Boarding: Will begin 30 minutes before departure'"
      }
    ]
  }
];

// 1. LẤY BÀI TẬP
const getReadingExercises = async (): Promise<ReadingExercise[]> => {
  try {
    interface ExamApiResponse {
      ExamID: number;
      Name: string;
      Content: string;
      Level: string;
      Type: string;
      Answers: string;
      TimeLimit: number;
    }
    
    const response = await apiService.get<ExamApiResponse[]>('/api/Exam');
    return response
      .filter(exam => ['Part 5', 'Part 6', 'Part 7'].includes(exam.Type))
      .map(exam => ({
        id: exam.ExamID, 
        name: exam.Name, 
        content: exam.Content,
        level: exam.Level as 'Beginner' | 'Intermediate' | 'Advanced',
        type: exam.Type as 'Part 5' | 'Part 6' | 'Part 7',
        questions: JSON.parse(exam.Answers || '[]') as ReadingQuestion[],
        sourceType: 'uploaded', 
        timeLimit: exam.TimeLimit,
      }));
  } catch (error) {
    console.log('API not available, using mock data:', error);
    // Return mock data when API is not available
    return MOCK_READING_EXERCISES;
  }
};

// 2. THEO TYPE
const getReadingExercisesByType = async (type: 'Part 5' | 'Part 6' | 'Part 7'): Promise<ReadingExercise[]> => {
  const all = await getReadingExercises();
  return all.filter(ex => ex.type === type);
};

// 3. AI GENERATION WITH BETTER FALLBACK
const generateReadingExercise = async (
  topic: string, 
  level: 'Beginner' | 'Intermediate' | 'Advanced', 
  type: 'Part 5' | 'Part 6' | 'Part 7'
): Promise<ReadingExercise> => {
  try {
    interface AiResponse {
      id?: number;
      name?: string;
      content?: string;
      questions?: ReadingQuestion[];
      timeLimit?: number;
    }
    
    const response = await apiService.post<AiResponse>('/api/Exam/generate', { 
      topic, level, type 
    });
    
    return {
      id: response.id || Date.now(),
      name: response.name || `${type} - ${topic}`,
      content: response.content || '',
      level,
      type,
      questions: response.questions || [],
      sourceType: 'ai',
      timeLimit: response.timeLimit || 10,
    };
  } catch (error) {
    console.log('AI generation failed, creating mock exercise:', error);
    // Generate a mock exercise with the requested parameters and proper reading passage
    let mockContent = '';
    let mockQuestions: ReadingQuestion[] = [];

    if (type === 'Part 5') {
      mockContent = `Part 5 practice questions about ${topic}. Complete each sentence with the correct word or phrase.`;
      mockQuestions = [
        {
          question: `The ${topic} industry has been _______ rapidly in recent years.`,
          options: ["growing", "grown", "grows", "growth"],
          correctAnswer: 0,
          explanation: `Present perfect continuous with 'has been' requires -ing form`
        },
        {
          question: `All employees working in ${topic} must _______ safety regulations.`,
          options: ["follow", "following", "followed", "follows"],
          correctAnswer: 0,
          explanation: `Modal 'must' is followed by base form of verb`
        }
      ];
    } else if (type === 'Part 6') {
      mockContent = `Dear Valued Customer,

We are writing to inform you about exciting developments in ${topic}. Our company has been ___(1)___ in this field for over a decade, and we are pleased to announce new services.

Starting next month, we will ___(2)___ enhanced features that will improve your experience. These updates represent our commitment to innovation and customer satisfaction.

We encourage you to ___(3)___ our website for more information about these upcoming changes. Thank you for your continued trust in our services.

Sincerely,
Customer Service Team`;
      
      mockQuestions = [
        {
          question: `Our company has been ___ in this field for over a decade.`,
          options: ["working", "worked", "works", "work"],
          correctAnswer: 0,
          explanation: `Present perfect continuous requires 'has been' + -ing form`
        },
        {
          question: `Starting next month, we will ___ enhanced features.`,
          options: ["introduce", "introducing", "introduced", "introduces"],
          correctAnswer: 0,
          explanation: `Future tense 'will' is followed by base form`
        },
        {
          question: `We encourage you to ___ our website for more information.`,
          options: ["visit", "visiting", "visited", "visits"],
          correctAnswer: 0,
          explanation: `'Encourage to' is followed by base form of verb`
        }
      ];
    } else { // Part 7
      mockContent = `ANNOUNCEMENT: ${topic.toUpperCase()} UPDATE

TO: All Staff Members
FROM: Management Team
DATE: ${new Date().toLocaleDateString()}
RE: Important Changes to ${topic} Procedures

We are pleased to announce significant improvements to our ${topic} operations. These changes will take effect starting next month and are designed to enhance efficiency and service quality.

KEY UPDATES:
• New ${topic} protocols will be implemented
• Staff training sessions are scheduled for next week
• Updated equipment will be installed by month-end
• Customer service hours will be extended

All team members are required to attend a mandatory briefing session on Friday at 2:00 PM in the main conference room. Please bring your employee handbook and be prepared to ask questions.

For additional information, please contact the HR department at extension 1234.

Thank you for your cooperation during this transition period.`;

      mockQuestions = [
        {
          question: `When will the new changes take effect?`,
          options: [
            "This week",
            "Next month", 
            "Next Friday",
            "By month-end"
          ],
          correctAnswer: 1,
          explanation: `The announcement states changes will take effect 'starting next month'`
        },
        {
          question: `What time is the mandatory briefing session?`,
          options: [
            "1:00 PM on Friday",
            "2:00 PM on Friday",
            "2:00 PM next week", 
            "1234 PM on Friday"
          ],
          correctAnswer: 1,
          explanation: `The text clearly states 'Friday at 2:00 PM in the main conference room'`
        }
      ];
    }

    return {
      id: Date.now(),
      name: `${type} - ${topic} (AI Generated)`,
      content: mockContent,
      level,
      type,
      questions: mockQuestions,
      sourceType: 'ai',
      timeLimit: type === 'Part 5' ? 10 : type === 'Part 6' ? 15 : 20,
    };
  }
};

// 4. SUBMIT WITH MOCK SUPPORT
const submitReadingResult = async (userId: number, examId: number, answers: number[]): Promise<UserResult> => {
  try {
    interface CompletionResponse {
      CompletionID: number;
    }
    
    const response = await apiService.post<CompletionResponse>('/api/Completion', { 
      UserID: userId, 
      ExamID: examId, 
      Answers: JSON.stringify(answers) 
    });
    
    const exercise = (await getReadingExercises()).find(e => e.id === examId);
    const score = answers.reduce((acc, ans, i) => 
      ans === exercise?.questions[i]?.correctAnswer ? acc + 1 : acc, 0
    );
    
    return { 
      id: response.CompletionID, 
      userId, 
      exerciseId: examId, 
      score, 
      totalQuestions: answers.length, 
      answers, 
      dateCompleted: new Date().toISOString(), 
      attempts: 1 
    };
  } catch (error) {
    console.log('Submit API failed, calculating score locally:', error);
    // Calculate score locally when API is not available
    const exercise = MOCK_READING_EXERCISES.find(e => e.id === examId);
    const score = answers.reduce((acc, ans, i) => 
      ans === exercise?.questions[i]?.correctAnswer ? acc + 1 : acc, 0
    );
    
    return { 
      id: Date.now(), 
      userId, 
      exerciseId: examId, 
      score, 
      totalQuestions: answers.length, 
      answers, 
      dateCompleted: new Date().toISOString(), 
      attempts: 1 
    };
  }
};

export const readingService = {
  getReadingExercises, 
  getReadingExercisesByType, 
  generateReadingExercise, 
  submitReadingResult
};
