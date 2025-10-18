import apiService from './api';

export interface User {
  UserID: number;
  Username: string;
  Email: string;
  Phone?: string;
  Role: string; // 'admin', 'student', 'teacher'
  Status: string; // 'active', 'inactive', 'banned'
}

class UserService {
  /**
   * Get all users
   */
  async getAllUsers(): Promise<User[]> {
    try {
      const response = await apiService.request<User[]>('/api/Users', {
        method: 'GET',
      });
      return response;
    } catch (error) {
      console.error('Error fetching users:', error);
      throw error;
    }
  }

  /**
   * Get a specific user by ID
   */
  async getUserById(id: number): Promise<User> {
    try {
      const response = await apiService.request<User>(`/api/Users/${id}`, {
        method: 'GET',
      });
      return response;
    } catch (error) {
      console.error(`Error fetching user ${id}:`, error);
      throw error;
    }
  }

  /**
   * Get users by role
   */
  async getUsersByRole(role: string): Promise<User[]> {
    try {
      const response = await apiService.request<User[]>(`/api/Users/role/${role}`, {
        method: 'GET',
      });
      return response;
    } catch (error) {
      console.error(`Error fetching users by role ${role}:`, error);
      throw error;
    }
  }

  /**
   * Update user status
   */
  async updateUserStatus(userId: number, newStatus: string): Promise<{ message: string; userId: number; newStatus: string }> {
    try {
      const response = await apiService.request<{ message: string; userId: number; newStatus: string }>(
        `/api/Users/${userId}/status`,
        {
          method: 'PATCH',
          body: JSON.stringify({ status: newStatus }),
        }
      );
      return response;
    } catch (error) {
      console.error(`Error updating user ${userId} status:`, error);
      throw error;
    }
  }
}

// Export singleton instance
const userService = new UserService();
export default userService;
