// 🗄️ DATABASE STATS SERVICE - Kết nối .NET API với fallback to mock data
// ✅ READY FOR GIT: Hoàn thành với comprehensive mock data + API integration
// 🔄 TODO DEPLOY: Cập nhật API_BASE_URL khi deploy .NET backend lên production
// 🛡️ Features: Auto fallback, error handling, comprehensive TOEIC exercises
// 📊 Mock Data: 7 complete TOEIC reading exercises, full user stats, leaderboard

// Real API service kết nối với MySQL database
import { apiService } from './api';

// Types matching với database schema
export interface UserStats {
  id: string;
  userId: string;
  username: string;
  fullName?: string;
  level: number;
  totalXp: number;
  weeklyXp: number;
  monthlyXp: number;
  streakDays: number;
  lastActivity: string;
  exercisesCompleted: number;
  lessonsCompleted: number;
  wordsLearned: number;
  readingScore: number;
  listeningScore: number;
  grammarScore: number;
  vocabularyScore: number;
  achievements: string[];
  updatedAt: string;
  rank: number;
  weeklyRank: number;
  monthlyRank: number;
}

export interface LeaderboardEntry {
  id: string;
  userId: string;
  username: string;
  avatar?: string;
  fullName?: string;
  level: number;
  totalXp: number;
  weeklyXp: number;
  monthlyXp: number;
  rank: number;
  weeklyRank: number;
  monthlyRank: number;
  streakDays: number;
  badge?: string;
  lastUpdated?: string;
}

export interface ProgressData {
  userId: string;
  currentLevel: number;
  currentXp: number;
  xpToNextLevel: number;
  progressPercentage: number;
  weeklyGoal: number;
  weeklyProgress: number;
  monthlyGoal: number;
  monthlyProgress: number;
  streakDays: number;
  exercisesThisWeek: number;
  lessonsThisMonth: number;
  wordsLearnedTotal: number;
  recentAchievements: string[];
}

export interface ReadingExercise {
  id: number; // Add id field for frontend usage
  exerciseId: number;
  name: string;
  content: string;
  level: 'Beginner' | 'Intermediate' | 'Advanced';
  type: 'Part 5' | 'Part 6' | 'Part 7';
  sourceType: 'manual' | 'ai';
  topic?: string;
  questions: Question[];
  timeLimit?: number; // Optional time limit
  createdAt: string;
  updatedAt: string;
}

export interface Question {
  question: string;
  options: string[];
  correctAnswer: number;
  explanation?: string;
}

export interface UserResult {
  resultId: number;
  userId: number;
  exerciseId: number;
  answers: number[];
  score: number;
  totalQuestions: number;
  timeSpent?: number;
  completedAt: string;
}

export type TimeFilter = 'all' | 'weekly' | 'monthly' | 'today';

class DatabaseStatsService {
  // Get user statistics từ .NET API
  async getUserStats(userId: string): Promise<UserStats> {
    try {
      const response = await apiService.get<UserStats>(`/api/User/${userId}/stats`);
      return response;
    } catch (error) {
      console.error('Error fetching user stats:', error);
      // Fallback to mock data nếu API fail
      return this.getMockUserStats(userId);
    }
  }

  // Get leaderboard với time filtering từ .NET API
  async getLeaderboard(timeFilter: TimeFilter = 'all', limit: number = 50): Promise<LeaderboardEntry[]> {
    try {
      // .NET API endpoint với query parameters
      const queryParams = new URLSearchParams({
        timeFilter,
        limit: limit.toString()
      });
      const response = await apiService.get<LeaderboardEntry[]>(`/api/Leaderboard?${queryParams}`);
      return response;
    } catch (error) {
      console.error('Error fetching leaderboard:', error);
      // Fallback to mock data
      return this.getMockLeaderboard(timeFilter);
    }
  }

  // Get progress data từ .NET API
  async getProgressData(userId: string): Promise<ProgressData> {
    try {
      const response = await apiService.get<ProgressData>(`/api/User/${userId}/progress`);
      return response;
    } catch (error) {
      console.error('Error fetching progress data:', error);
      // Fallback to mock data
      return this.getMockProgressData(userId);
    }
  }



  // Get reading exercises từ .NET API (bao gồm admin uploaded + AI generated)
  async getReadingExercises(
    level?: 'Beginner' | 'Intermediate' | 'Advanced',
    type?: 'Part 5' | 'Part 6' | 'Part 7',
    sourceType?: 'uploaded' | 'ai'
  ): Promise<ReadingExercise[]> {
    try {
      // Build query parameters cho filtering
      const queryParams = new URLSearchParams();
      if (level) queryParams.append('level', level);
      if (type) queryParams.append('type', type);
      if (sourceType) queryParams.append('sourceType', sourceType);
      
      const queryString = queryParams.toString();
      const endpoint = queryString ? `/api/ReadingExercise?${queryString}` : '/api/ReadingExercise';
      
      interface DBReadingExercise {
        exerciseId: number;
        name: string;
        content: string;
        level: 'Beginner' | 'Intermediate' | 'Advanced';
        type: 'Part 5' | 'Part 6' | 'Part 7';
        sourceType: 'manual' | 'ai';
        topic?: string;
        questions: string | Question[];
        createdAt: string;
        updatedAt: string;
        createdBy?: string; // Admin who uploaded
      }
      
      interface BackendExercise {
        id?: number;
        exerciseId?: number;
        Name?: string; // Backend uses capital N
        name?: string; // Frontend uses lowercase n
        Content?: string; // Backend uses capital C
        content?: string; // Frontend uses lowercase c
        Level?: 'Beginner' | 'Intermediate' | 'Advanced'; // Backend capital L
        level?: 'Beginner' | 'Intermediate' | 'Advanced'; // Frontend lowercase l
        Type?: 'Part 5' | 'Part 6' | 'Part 7'; // Backend capital T
        type?: 'Part 5' | 'Part 6' | 'Part 7'; // Frontend lowercase t
        SourceType?: 'manual' | 'ai'; // Backend capital S
        sourceType?: 'manual' | 'ai'; // Frontend lowercase s
        topic?: string;
        createdAt?: string;
        updatedAt?: string;
        createdBy?: string;
      }

      // Define backend question structure
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

      interface BackendExerciseWithQuestions extends BackendExercise {
        Questions?: BackendQuestion[];
        questions?: BackendQuestion[];
      }

      const response = await apiService.get<BackendExerciseWithQuestions[]>(endpoint);
      return response.map((exercise) => {
        // Convert backend questions to frontend format
        const backendQuestions = exercise.Questions || exercise.questions || [];
        const convertedQuestions: Question[] = backendQuestions.map((q) => ({
          question: q.QuestionText || q.question || 'Question not available',
          options: q.Options || q.options || [],
          correctAnswer: q.correctAnswer ?? -1,
          explanation: q.Explanation || q.explanation
        }));

        // DEBUG: Log sourceType from backend
        console.log('🔍 Exercise from backend:', {
          name: exercise.Name || exercise.name,
          backendSourceType: exercise.SourceType,
          lowercaseSourceType: exercise.sourceType,
          finalSourceType: (exercise.SourceType || exercise.sourceType || 'manual')
        });

        return {
          ...exercise,
          id: exercise.id || exercise.exerciseId || 0,
          exerciseId: exercise.id || exercise.exerciseId || 0,
          name: exercise.Name || exercise.name || 'Untitled Exercise',
          content: exercise.Content || exercise.content || 'No content available',
          level: exercise.Level || exercise.level || 'Beginner',
          type: exercise.Type || exercise.type || 'Part 7',
          sourceType: (exercise.SourceType || exercise.sourceType || 'manual') as 'manual' | 'ai',
          questions: convertedQuestions,
          createdAt: exercise.createdAt || new Date().toISOString(),
          updatedAt: exercise.updatedAt || new Date().toISOString()
        };
      });
    } catch (error) {
      console.error('Error fetching reading exercises:', error);
      return this.getMockReadingExercises();
    }
  }

  // Generate AI reading exercise - Backend sẽ call Gemini API
  async generateReadingExercise(
    topic: string, 
    level: 'Beginner' | 'Intermediate' | 'Advanced',
    type: 'Part 5' | 'Part 6' | 'Part 7'
  ): Promise<ReadingExercise> {
    try {
      interface AIGenerateRequest {
        topic: string;
        level: 'Beginner' | 'Intermediate' | 'Advanced';
        type: 'Part 5' | 'Part 6' | 'Part 7';
        userId?: string; // Track who generated
      }

      interface GenerateResponse {
        exerciseId: number;
        name: string;
        content: string;
        level: 'Beginner' | 'Intermediate' | 'Advanced';
        type: 'Part 5' | 'Part 6' | 'Part 7';
        sourceType: 'ai';
        topic: string;
        questions: string | Question[];
        createdAt: string;
        updatedAt: string;
        generatedBy: string;
      }
      
      // .NET Controller sẽ call Gemini API và return exercise
      const response = await apiService.post<GenerateResponse>('/api/ReadingExercise/create-with-ai', {
        name: `${type}: ${topic} - ${level} Level`,
        content: `Practice ${type.toLowerCase()} exercises focused on ${topic.toLowerCase()} at ${level.toLowerCase()} level.`,
        level,
        type, 
        description: `AI-generated ${type} exercise about ${topic}`,
        estimatedMinutes: type === 'Part 5' ? 15 : type === 'Part 6' ? 20 : 30,
        createdBy: 'AI Generator',
        questionCount: 5
      });
      
      return {
        ...response,
        id: response.exerciseId, // Map exerciseId to id
        questions: typeof response.questions === 'string' 
          ? JSON.parse(response.questions) 
          : response.questions
      };
    } catch (error) {
      console.error('Error generating AI reading exercise:', error);
      throw error;
    }
  }

  // Submit reading exercise result to .NET API
  async submitReadingResult(userId: number, exerciseId: number, answers: number[]): Promise<UserResult> {
    try {
      interface SubmitResultRequest {
        userId: number;
        exerciseId: number;
        answers: number[];
        timeSpent?: number; // Optional time tracking
        completedAt: string;
      }

      const response = await apiService.post<UserResult>('/api/ReadingExercise/submit-result', {
        userId,
        exerciseId,
        answers,
        completedAt: new Date().toISOString()
      });
      return response;
    } catch (error) {
      console.error('Error submitting reading result:', error);
      throw error;
    }
  }

  // Update user XP after completing exercise - .NET API
  async updateUserXp(userId: string, xpGained: number): Promise<UserStats> {
    try {
      const response = await apiService.post<UserStats>(`/api/User/${userId}/update-xp`, {
        xpGained,
        source: 'reading_exercise' // Track XP source
      });
      return response;
    } catch (error) {
      console.error('Error updating user XP:', error);
      throw error;
    }
  }

  // MOCK DATA FALLBACKS (giữ nguyên mock data hiện tại)
  private getMockUserStats(userId: string): UserStats {
    return {
      id: '6',
      userId: userId,
      username: 'englishlearner01',
      fullName: 'Bạn',
      level: 7,
      totalXp: 8200,
      weeklyXp: 620,
      monthlyXp: 1850,
      streakDays: 7,
      lastActivity: new Date().toISOString(),
      exercisesCompleted: 164,
      lessonsCompleted: 41,
      wordsLearned: 820,
      readingScore: 85,
      listeningScore: 78,
      grammarScore: 92,
      vocabularyScore: 89,
      achievements: ['First Steps', 'Week Warrior', 'Grammar Master'],
      updatedAt: new Date().toISOString(),
      rank: 4,
      weeklyRank: 5,
      monthlyRank: 6,
    };
  }

  private getMockLeaderboard(timeFilter: TimeFilter): LeaderboardEntry[] {
    const mockUsers = [
      {
        id: '1',
        userId: 'user1',
        username: 'NguyenVanA',
        fullName: 'Nguyễn Văn A',
        avatar: 'https://images.unsplash.com/photo-1494790108755-2616b612b56c?w=100&h=100&fit=crop&crop=face',
        level: 12,
        totalXp: 15750,
        weeklyXp: 890,
        monthlyXp: 3240,
        rank: 1,
        weeklyRank: 2,
        monthlyRank: 1,
        streakDays: 23,
        badge: '🏆'
      },
      {
        id: '2',
        userId: 'user2',
        username: 'TranThiB',
        fullName: 'Trần Thị B',
        avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=100&h=100&fit=crop&crop=face',
        level: 11,
        totalXp: 14200,
        weeklyXp: 920,
        monthlyXp: 2980,
        rank: 2,
        weeklyRank: 1,
        monthlyRank: 2,
        streakDays: 18,
        badge: '🥈'
      },
      {
        id: '6',
        userId: 'current',
        username: 'englishlearner01',
        fullName: 'Bạn',
        level: 7,
        totalXp: 8200,
        weeklyXp: 620,
        monthlyXp: 1850,
        rank: 4,
        weeklyRank: 5,
        monthlyRank: 6,
        streakDays: 7,
        badge: '🚀'
      }
    ];

    // Apply time-based sorting
    switch (timeFilter) {
      case 'weekly':
        return mockUsers.sort((a, b) => b.weeklyXp - a.weeklyXp)
          .map((user, index) => ({ ...user, rank: index + 1 }));
      case 'monthly':
        return mockUsers.sort((a, b) => b.monthlyXp - a.monthlyXp)
          .map((user, index) => ({ ...user, rank: index + 1 }));
      default:
        return mockUsers;
    }
  }

  private getMockProgressData(userId: string): ProgressData {
    return {
      userId: userId,
      currentLevel: 7,
      currentXp: 200,
      xpToNextLevel: 800,
      progressPercentage: 20,
      weeklyGoal: 1000,
      weeklyProgress: 620,
      monthlyGoal: 4000,
      monthlyProgress: 1850,
      streakDays: 7,
      exercisesThisWeek: 12,
      lessonsThisMonth: 9,
      wordsLearnedTotal: 820,
      recentAchievements: ['First Steps', 'Week Warrior', 'Grammar Master'],
    };
  }

  private getMockReadingExercises(): ReadingExercise[] {
    return [
      // PART 5 - INCOMPLETE SENTENCES (30 câu) - ADMIN UPLOADED
      {
        id: 1,
        exerciseId: 1,
        name: 'Part 5: Grammar & Vocabulary - Beginner Level',
        content: 'Complete the sentences by choosing the best answer for each blank.',
        level: 'Beginner',
        type: 'Part 5',
        sourceType: 'manual',
        topic: 'Grammar & Vocabulary',
        timeLimit: 900, // 15 minutes
        questions: [
          {
            question: 'The company will _____ a new product next month.',
            options: ['launch', 'launches', 'launching', 'launched'],
            correctAnswer: 0,
            explanation: 'Future tense requires the base form of the verb after "will".'
          },
          {
            question: 'All employees _____ attend the mandatory training session.',
            options: ['can', 'must', 'might', 'could'],
            correctAnswer: 1,
            explanation: 'Mandatory means required, so "must" is the correct modal verb.'
          },
          {
            question: 'The report was submitted _____ the deadline.',
            options: ['before', 'after', 'during', 'since'],
            correctAnswer: 0,
            explanation: 'Context suggests the report was submitted in advance of the deadline.'
          },
          {
            question: 'Ms. Johnson is _____ for managing the marketing department.',
            options: ['responsible', 'responsibility', 'responsibly', 'respond'],
            correctAnswer: 0,
            explanation: 'Need an adjective after "is". "Responsible for" is the correct phrase.'
          },
          {
            question: 'The meeting has been _____ to next Tuesday.',
            options: ['postponed', 'advanced', 'canceled', 'scheduled'],
            correctAnswer: 0,
            explanation: 'Moving a meeting to a later date means it has been postponed.'
          }
        ],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // PART 5 - AI GENERATED
      {
        id: 2,
        exerciseId: 2,
        name: 'Part 5: Business Grammar - AI Generated',
        content: 'Complete each sentence with the most appropriate word or phrase.',
        level: 'Intermediate',
        type: 'Part 5',
        sourceType: 'ai',
        topic: 'Business English',
        timeLimit: 1200, // 20 minutes
        questions: [
          {
            question: 'The quarterly sales report _____ impressive growth in all regions.',
            options: ['shows', 'showing', 'shown', 'show'],
            correctAnswer: 0,
            explanation: 'Third person singular subject requires "shows".'
          },
          {
            question: 'Due to _____ demand, we are expanding our production capacity.',
            options: ['increase', 'increased', 'increasing', 'increasingly'],
            correctAnswer: 1,
            explanation: 'Need a past participle to modify "demand".'
          },
          {
            question: 'The new policy will be _____ effective immediately.',
            options: ['become', 'becomes', 'becoming', 'became'],
            correctAnswer: 0,
            explanation: 'After "will be", use the base form of the verb.'
          },
          {
            question: 'We appreciate your _____ in this matter.',
            options: ['cooperate', 'cooperation', 'cooperative', 'cooperating'],
            correctAnswer: 1,
            explanation: 'Need a noun after "your". "Cooperation" is the correct form.'
          },
          {
            question: 'The contract must be signed _____ both parties agree.',
            options: ['before', 'after', 'unless', 'although'],
            correctAnswer: 0,
            explanation: 'Logic suggests signing happens before agreement is reached.'
          }
        ],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // PART 6 - TEXT COMPLETION (16 câu) - ADMIN UPLOADED
      {
        id: 3,
        exerciseId: 3,
        name: 'Part 6: Email Correspondence - Complete Text',
        content: `From: sarah.martinez@techcorp.com
To: all-staff@techcorp.com
Subject: New Office Security Procedures

Dear Team,

I am writing to inform you about the new security procedures that will be (1) _____ starting Monday, November 1st. These changes have been made to enhance the safety and security of our workplace.

First, all employees will need to use their ID cards to (2) _____ the building. The front desk will no longer provide manual entry after 6:00 PM on weekdays and all day on weekends.

(3) _____, visitors must be registered in advance through our online system. Walk-in visitors will not be permitted without prior authorization from a department head.

We understand these changes may cause some initial inconvenience, but we believe they are necessary for everyone's safety. (4) _____ you have any questions or concerns, please don't hesitate to contact the HR department.

Thank you for your cooperation.

Best regards,
Sarah Martinez
Security Manager`,
        level: 'Intermediate',
        type: 'Part 6',
        sourceType: 'manual',
        topic: 'Workplace Security',
        timeLimit: 800, // 13 minutes
        questions: [
          {
            question: '(1)',
            options: ['implemented', 'implement', 'implementing', 'implementation'],
            correctAnswer: 0,
            explanation: 'Passive voice requires past participle "implemented".'
          },
          {
            question: '(2)',
            options: ['enter', 'entrance', 'entering', 'entered'],
            correctAnswer: 0,
            explanation: 'Need infinitive form after "to".'
          },
          {
            question: '(3)',
            options: ['However', 'Additionally', 'Therefore', 'Nevertheless'],
            correctAnswer: 1,
            explanation: 'Adding another requirement, so "Additionally" fits best.'
          },
          {
            question: '(4)',
            options: ['If', 'Unless', 'Although', 'Because'],
            correctAnswer: 0,
            explanation: 'Conditional statement requires "If".'
          }
        ],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // PART 6 - AI GENERATED
      {
        id: 4,
        exerciseId: 4,
        name: 'Part 6: Business Memo - AI Generated',
        content: `MEMORANDUM

TO: All Department Heads
FROM: Jennifer Liu, Operations Director
DATE: October 25, 2025
RE: Quarterly Budget Review Process

This memo outlines the procedures for the upcoming quarterly budget review, which will (1) _____ place from November 15-30, 2025.

Each department must submit their budget proposals by November 10th. Late submissions will not be (2) _____ under any circumstances. Please ensure that all supporting documentation is included with your proposal.

The review committee will (3) _____ of the CFO, Operations Director, and two department representatives. We will evaluate each proposal based on strategic alignment, cost-effectiveness, and potential return on investment.

(4) _____ the review process, departments may be asked to provide additional information or clarification. We encourage proactive communication throughout this period.

Results will be announced by December 5th, with budget allocations taking effect January 1st, 2026.`,
        level: 'Advanced',
        type: 'Part 6',
        sourceType: 'ai',
        topic: 'Budget Management',
        timeLimit: 900, // 15 minutes
        questions: [
          {
            question: '(1)',
            options: ['take', 'taking', 'taken', 'takes'],
            correctAnswer: 0,
            explanation: 'Future tense "will take place" is the correct phrase.'
          },
          {
            question: '(2)',
            options: ['accept', 'accepted', 'accepting', 'acceptance'],
            correctAnswer: 1,
            explanation: 'Passive voice requires past participle "accepted".'
          },
          {
            question: '(3)',
            options: ['consist', 'consists', 'consisting', 'consisted'],
            correctAnswer: 0,
            explanation: 'After "will", use base form "consist".'
          },
          {
            question: '(4)',
            options: ['During', 'Before', 'After', 'Despite'],
            correctAnswer: 0,
            explanation: 'The action happens within the review period, so "During" is correct.'
          }
        ],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // PART 7 - READING COMPREHENSION (54 câu) - ADMIN UPLOADED
      {
        id: 5,
        exerciseId: 5,
        name: 'Part 7: Business Correspondence & Documents',
        content: `EMAIL CHAIN

Email 1:
From: mark.thompson@globaltech.com
To: lisa.chen@creativesolutions.net
Date: October 20, 2025
Subject: Partnership Proposal

Dear Ms. Chen,

I hope this message finds you well. I am writing on behalf of Global Tech Solutions to explore a potential partnership opportunity with Creative Solutions.

We have been following your company's innovative work in digital marketing and believe there could be significant synergies between our organizations. Global Tech specializes in software development for small and medium enterprises, while Creative Solutions excels in marketing strategy and brand development.

We would like to propose a meeting to discuss how we might collaborate on upcoming projects. Our CEO, Patricia Williams, will be visiting your city next month and would be available to meet with your team.

Please let me know if you would be interested in exploring this opportunity further.

Best regards,
Mark Thompson
Business Development Manager
Global Tech Solutions

Email 2:
From: lisa.chen@creativesolutions.net
To: mark.thompson@globaltech.com
Date: October 22, 2025
Subject: RE: Partnership Proposal

Dear Mr. Thompson,

Thank you for reaching out to us. We are indeed interested in learning more about potential collaboration opportunities with Global Tech Solutions.

I have reviewed your company's portfolio and am impressed by your software development capabilities. A partnership could be mutually beneficial, especially for our clients who need both technical solutions and marketing support.

I would be happy to arrange a meeting with Ms. Williams during her visit. Could you please provide some potential dates and times? Our leadership team includes myself (CEO), David Park (CTO), and Maria Rodriguez (Head of Strategy). We would all be available to participate in the discussion.

Additionally, it would be helpful if you could share some specific examples of the types of projects you envision us collaborating on.

Looking forward to your response.

Best regards,
Lisa Chen
CEO, Creative Solutions

Email 3:
From: mark.thompson@globaltech.com
To: lisa.chen@creativesolutions.net
Date: October 24, 2025
Subject: RE: Partnership Proposal - Meeting Details

Dear Ms. Chen,

Excellent! I'm pleased to hear of your interest in moving forward with discussions.

Ms. Williams will be available on November 8th, 9th, or 10th. She prefers morning meetings, so any time between 9:00 AM and 11:30 AM would work well. The meeting location is flexible - we could meet at your offices, ours, or a neutral location such as the Riverside Conference Center.

Regarding potential collaboration projects, here are three areas we've identified:

1. E-commerce Platform Development: We could develop custom online stores while you handle the digital marketing campaigns and brand positioning.

2. Mobile App Solutions: Our team creates mobile applications for businesses while your team manages the launch strategy and user acquisition.

3. Digital Transformation Consulting: Combined service offering where we handle the technical implementation and you manage change management and employee training.

Please let me know which date works best for your team, and we can finalize the logistics.

Best regards,
Mark Thompson`,
        level: 'Advanced',
        type: 'Part 7',
        sourceType: 'manual',
        topic: 'Business Partnership',
        timeLimit: 2700, // 45 minutes
        questions: [
          {
            question: 'What is the main purpose of the first email?',
            options: ['To schedule a meeting', 'To propose a business partnership', 'To request information', 'To confirm an appointment'],
            correctAnswer: 1,
            explanation: 'Mark Thompson explicitly states he is writing to "explore a potential partnership opportunity".'
          },
          {
            question: 'What does Global Tech Solutions specialize in?',
            options: ['Digital marketing', 'Brand development', 'Software development for SMEs', 'Conference management'],
            correctAnswer: 2,
            explanation: 'The email states "Global Tech specializes in software development for small and medium enterprises".'
          },
          {
            question: 'Who is Patricia Williams?',
            options: ['Business Development Manager at Global Tech', 'CEO of Global Tech', 'CEO of Creative Solutions', 'Head of Strategy at Creative Solutions'],
            correctAnswer: 1,
            explanation: 'Mark refers to "Our CEO, Patricia Williams".'
          },
          {
            question: 'How does Lisa Chen respond to the partnership proposal?',
            options: ['She rejects it immediately', 'She requests more time to consider', 'She shows interest and agrees to meet', 'She suggests alternative arrangements'],
            correctAnswer: 2,
            explanation: 'Lisa states "We are indeed interested" and agrees to arrange a meeting.'
          },
          {
            question: 'Which dates is Patricia Williams available for the meeting?',
            options: ['November 6th, 7th, or 8th', 'November 8th, 9th, or 10th', 'November 9th, 10th, or 11th', 'November 10th, 11th, or 12th'],
            correctAnswer: 1,
            explanation: 'Mark states "Ms. Williams will be available on November 8th, 9th, or 10th".'
          },
          {
            question: 'What time preference does Patricia Williams have for meetings?',
            options: ['Afternoon meetings', 'Evening meetings', 'Morning meetings', 'No preference stated'],
            correctAnswer: 2,
            explanation: 'Mark mentions "She prefers morning meetings".'
          },
          {
            question: 'How many potential collaboration areas does Mark mention?',
            options: ['Two', 'Three', 'Four', 'Five'],
            correctAnswer: 1,
            explanation: 'Mark lists three specific areas: E-commerce Platform Development, Mobile App Solutions, and Digital Transformation Consulting.'
          },
          {
            question: 'What is NOT mentioned as a potential meeting location?',
            options: ['Creative Solutions offices', 'Global Tech offices', 'Riverside Conference Center', 'Downtown Business District'],
            correctAnswer: 3,
            explanation: 'Mark mentions the first three options but not the Downtown Business District.'
          }
        ],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // PART 7 - AI GENERATED
      {
        id: 6,
        exerciseId: 6,
        name: 'Part 7: Travel & Tourism Article - AI Generated',
        content: `SUSTAINABLE TOURISM: A GROWING TREND

The tourism industry is experiencing a significant shift toward sustainable practices as travelers become increasingly environmentally conscious. This transformation is not just a trend but a necessary evolution to preserve our planet's natural and cultural heritage for future generations.

Sustainable tourism, also known as eco-tourism, focuses on minimizing the negative impact of travel on the environment and local communities while maximizing the benefits for both visitors and destinations. This approach encompasses various aspects, from choosing eco-friendly accommodations to supporting local businesses and respecting cultural traditions.

According to recent studies, 73% of travelers are willing to pay more for sustainable travel options, representing a 15% increase from five years ago. This growing demand has prompted hotels, airlines, and tour operators to implement green initiatives and obtain sustainability certifications.

Hotels are leading the way with innovative programs such as solar power systems, water conservation measures, and waste reduction strategies. Many properties now offer guests the option to reuse towels and linens, participate in local conservation projects, and enjoy organic, locally-sourced meals.

Airlines are investing in more fuel-efficient aircraft and exploring alternative fuels like biofuels and hydrogen. Some carriers have eliminated single-use plastics from their services and introduced carbon offset programs allowing passengers to compensate for their flight emissions.

Tour operators are developing smaller group sizes, hiring local guides, and creating itineraries that showcase authentic cultural experiences while providing economic benefits to communities. These practices help preserve local traditions and create sustainable income sources for residents.

The benefits of sustainable tourism extend beyond environmental protection. It promotes cultural exchange, supports local economies, and creates jobs in rural and remote areas that might otherwise lack economic opportunities. Additionally, sustainable tourism helps preserve historical sites and natural landmarks that attract visitors from around the world.

However, implementing sustainable tourism practices comes with challenges. Higher initial costs, the need for staff training, and changing long-established practices require significant investment and commitment. Some destinations also struggle with balancing tourism growth and environmental preservation.

Despite these challenges, the future of tourism lies in sustainability. Governments worldwide are implementing policies to promote responsible travel, and international organizations are providing guidelines and support for sustainable tourism development.

Travelers can contribute to this movement by making conscious choices: selecting certified sustainable accommodations, using public transportation, buying from local vendors, and respecting natural environments and cultural sites. Small individual actions collectively create substantial positive impact on destinations worldwide.`,
        level: 'Advanced',
        type: 'Part 7',
        sourceType: 'ai',
        topic: 'Sustainable Tourism',
        timeLimit: 2400, // 40 minutes
        questions: [
          {
            question: 'What percentage of travelers are willing to pay more for sustainable travel options?',
            options: ['58%', '68%', '73%', '83%'],
            correctAnswer: 2,
            explanation: 'The text states "73% of travelers are willing to pay more for sustainable travel options".'
          },
          {
            question: 'How much has the demand for sustainable travel increased over five years?',
            options: ['10%', '15%', '20%', '25%'],
            correctAnswer: 1,
            explanation: 'The text mentions "representing a 15% increase from five years ago".'
          },
          {
            question: 'Which of the following is NOT mentioned as a hotel sustainability initiative?',
            options: ['Solar power systems', 'Water conservation measures', 'Plastic recycling programs', 'Waste reduction strategies'],
            correctAnswer: 2,
            explanation: 'Plastic recycling programs are not specifically mentioned in the hotel initiatives section.'
          },
          {
            question: 'What are airlines exploring as alternative fuels?',
            options: ['Electric and solar power', 'Biofuels and hydrogen', 'Wind and water power', 'Nuclear and geothermal energy'],
            correctAnswer: 1,
            explanation: 'The text mentions "exploring alternative fuels like biofuels and hydrogen".'
          },
          {
            question: 'According to the article, what is a challenge of implementing sustainable tourism?',
            options: ['Lack of traveler interest', 'Higher initial costs', 'Government restrictions', 'Limited destinations'],
            correctAnswer: 1,
            explanation: 'The text lists "Higher initial costs" as one of the challenges mentioned.'
          },
          {
            question: 'What does sustainable tourism help preserve?',
            options: ['Only natural landmarks', 'Only historical sites', 'Both historical sites and natural landmarks', 'Only cultural traditions'],
            correctAnswer: 2,
            explanation: 'The text states it "helps preserve historical sites and natural landmarks".'
          },
          {
            question: 'How can travelers contribute to sustainable tourism?',
            options: ['By traveling more frequently', 'By choosing luxury accommodations', 'By selecting certified sustainable accommodations', 'By avoiding public transportation'],
            correctAnswer: 2,
            explanation: 'The text suggests "selecting certified sustainable accommodations" as one way to contribute.'
          },
          {
            question: 'What is the main message of the article?',
            options: ['Tourism should be avoided to protect the environment', 'Sustainable tourism is expensive and impractical', 'The future of tourism lies in sustainability', 'Traditional tourism is better than eco-tourism'],
            correctAnswer: 2,
            explanation: 'The article concludes that "the future of tourism lies in sustainability".'
          }
        ],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      },

      // PART 7 - COMPLEX DOCUMENT - ADMIN UPLOADED
      {
        id: 7,
        exerciseId: 7,
        name: 'Part 7: Company Policy & Benefits Guide',
        content: `EMPLOYEE HANDBOOK - SECTION 4: BENEFITS AND POLICIES

4.1 VACATION POLICY

All full-time employees are eligible for paid vacation time based on their length of service:
• 0-2 years: 15 days annually
• 3-5 years: 20 days annually
• 6+ years: 25 days annually

Vacation requests must be submitted at least two weeks in advance through the HR portal. Requests for vacation during peak business periods (December 15-31, March 1-15) require supervisor approval and may be limited based on departmental needs.

Unused vacation days may be carried over to the following year, up to a maximum of 5 days. Any additional unused days will be forfeited unless approved by HR for exceptional circumstances.

4.2 HEALTH INSURANCE

The company provides comprehensive health insurance coverage for all eligible employees and their dependents. Coverage begins on the first day of the month following 60 days of employment.

Premium costs are shared between the company and employee as follows:
• Employee only: Company pays 80%, employee pays 20%
• Employee + spouse: Company pays 70%, employee pays 30%
• Employee + children: Company pays 70%, employee pays 30%
• Family coverage: Company pays 60%, employee pays 40%

Annual open enrollment occurs every November, with changes taking effect January 1st. Employees may make changes outside of open enrollment only for qualifying life events (marriage, birth, divorce, etc.).

4.3 PROFESSIONAL DEVELOPMENT

The company supports employee growth through various professional development opportunities:

Training Budget: Each employee receives an annual training budget of $2,000 for courses, conferences, certifications, or workshops related to their role.

Tuition Reimbursement: Employees may receive up to $5,000 annually for job-related degree programs. Reimbursement requires maintaining a B average and remaining with the company for at least two years after program completion.

Internal Training: Monthly lunch-and-learn sessions cover topics such as leadership, technology updates, and industry trends. Attendance is encouraged but not mandatory.

4.4 REMOTE WORK POLICY

Effective January 1, 2025, the company has implemented a flexible remote work policy:

Eligibility: Employees who have been with the company for at least 6 months and have satisfactory performance reviews may apply for remote work arrangements.

Options:
• Fully remote: Work from home 5 days per week
• Hybrid: Work from home 2-3 days per week
• Flexible: Occasional remote work as needed

Equipment: The company provides necessary equipment including laptop, monitor, and ergonomic accessories. Employees are responsible for maintaining a suitable workspace and reliable internet connection.

Performance Standards: Remote employees must maintain the same productivity and communication standards as office-based employees. Regular check-ins with supervisors are required weekly.

4.5 EMPLOYEE ASSISTANCE PROGRAM (EAP)

The company offers a confidential Employee Assistance Program providing:
• 24/7 counseling services for personal and work-related issues
• Financial planning assistance
• Legal consultation services
• Work-life balance resources
• Stress management workshops

Services are provided at no cost to employees and their immediate family members. All consultations are strictly confidential and do not affect employment status or performance evaluations.`,
        level: 'Intermediate',
        type: 'Part 7',
        sourceType: 'manual',
        topic: 'Company Policies',
        timeLimit: 3000, // 50 minutes
        questions: [
          {
            question: 'How many vacation days do employees with 4 years of service receive annually?',
            options: ['15 days', '20 days', '25 days', '30 days'],
            correctAnswer: 1,
            explanation: 'Employees with 3-5 years of service receive 20 days annually.'
          },
          {
            question: 'How far in advance must vacation requests be submitted?',
            options: ['One week', 'Two weeks', 'Three weeks', 'One month'],
            correctAnswer: 1,
            explanation: 'The policy states "at least two weeks in advance".'
          },
          {
            question: 'What is the maximum number of unused vacation days that can be carried over?',
            options: ['3 days', '5 days', '7 days', '10 days'],
            correctAnswer: 1,
            explanation: 'The text states "up to a maximum of 5 days".'
          },
          {
            question: 'When does health insurance coverage begin for new employees?',
            options: ['Immediately upon hiring', 'After 30 days', 'First day of the month after 60 days', 'After 90 days'],
            correctAnswer: 2,
            explanation: 'Coverage begins "on the first day of the month following 60 days of employment".'
          },
          {
            question: 'How much does the company pay for employee-only health insurance premiums?',
            options: ['60%', '70%', '80%', '100%'],
            correctAnswer: 2,
            explanation: 'For employee only coverage, "Company pays 80%, employee pays 20%".'
          },
          {
            question: 'What is the annual training budget per employee?',
            options: ['$1,500', '$2,000', '$2,500', '$3,000'],
            correctAnswer: 1,
            explanation: 'The text states "an annual training budget of $2,000".'
          },
          {
            question: 'What GPA must be maintained for tuition reimbursement?',
            options: ['A average', 'B average', 'C average', 'Pass/Fail'],
            correctAnswer: 1,
            explanation: 'Reimbursement requires "maintaining a B average".'
          },
          {
            question: 'How long must an employee stay with the company after receiving tuition reimbursement?',
            options: ['One year', 'Two years', 'Three years', 'Five years'],
            correctAnswer: 1,
            explanation: 'Employees must remain "for at least two years after program completion".'
          },
          {
            question: 'What is required to be eligible for remote work?',
            options: ['3 months employment', '6 months employment and satisfactory reviews', '1 year employment', 'Manager approval only'],
            correctAnswer: 1,
            explanation: 'Employees need "at least 6 months and have satisfactory performance reviews".'
          },
          {
            question: 'Which service is NOT mentioned as part of the Employee Assistance Program?',
            options: ['24/7 counseling services', 'Financial planning assistance', 'Career coaching', 'Legal consultation services'],
            correctAnswer: 2,
            explanation: 'Career coaching is not listed among the EAP services mentioned.'
          }
        ],
        createdAt: new Date().toISOString(),
        updatedAt: new Date().toISOString()
      }
    ];
  }
}

// Export singleton instance
export const databaseStatsService = new DatabaseStatsService();

// Keep existing statsService for backward compatibility
export const statsService = databaseStatsService;
