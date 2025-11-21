import { apiService } from './api';

export interface User {
  id: number;
  username: string;
  email: string;
  fullName: string;
  level: string;
  studyStreak: number;
  totalStudyTime: number;
  totalXp: number;
  createdAt: string;
  lastActiveAt: string;
  avatar?: string;
  isActive: boolean;
}

export interface LoginRequest {
  username: string;
  password: string;
}

export interface RegisterRequest {
  username: string;
  email: string;
  password: string;
  fullName: string;
  level?: string;
}

export interface UpdateProfileRequest {
  fullName?: string;
  email?: string;
  level?: string;
  password?: string;
  avatar?: string;
}

export const authService = {
  // Login user
  login: async (request: LoginRequest): Promise<User> => {
    try {
      const response = await apiService.post<User>('/api/Auth/login', request);
      return response;
    } catch (error) {
      console.error('Login error:', error);
      throw error;
    }
  },

  // Register new user
  register: async (request: RegisterRequest): Promise<User> => {
    try {
      const response = await apiService.post<User>('/api/Auth/register', request);
      return response;
    } catch (error) {
      console.error('Registration error:', error);
      throw error;
    }
  },

  // Update user profile
  updateProfile: async (userId: number, request: UpdateProfileRequest): Promise<User> => {
    try {
      const response = await apiService.put<User>(
        `/api/Auth/update-profile?userId=${userId}`,
        request
      );
      return response;
    } catch (error) {
      console.error('Update profile error:', error);
      throw error;
    }
  },
};

