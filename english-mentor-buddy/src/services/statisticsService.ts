import apiService from './api';

export interface SystemStatistics {
  TotalUsers: number;
  TotalTests: number;
  TotalExercises: number;
  TotalCompletions: number;
}

export interface UsersByRole {
  [role: string]: number;
}

class StatisticsService {
  /**
   * Get system-wide statistics
   */
  async getSystemStatistics(): Promise<SystemStatistics> {
    try {
      const response = await apiService.request<SystemStatistics>('/api/Statistics', {
        method: 'GET',
      });
      return response;
    } catch (error) {
      console.error('Error fetching system statistics:', error);
      throw error;
    }
  }

  /**
   * Get users count by role
   */
  async getUsersByRole(): Promise<UsersByRole> {
    try {
      const response = await apiService.request<UsersByRole>('/api/Statistics/users-by-role', {
        method: 'GET',
      });
      return response;
    } catch (error) {
      console.error('Error fetching users by role:', error);
      throw error;
    }
  }
}

// Export singleton instance
const statisticsService = new StatisticsService();
export default statisticsService;
