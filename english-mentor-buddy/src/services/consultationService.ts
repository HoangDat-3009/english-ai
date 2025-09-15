import { apiService } from './api';

export interface ChatMessage {
  FromUser: boolean;
  Message: string;
}

export interface ChatRequest {
  ChatHistory: ChatMessage[];
  Question: string;
  ImagesAsBase64: string[] | null;
}

export interface ChatResponse {
  data: string;
  message: string;
  status: number;
  success: boolean;
}

export const chatService = {
  // Send a message to the chatbot and get a response
  generateAnswer: async (
    request: ChatRequest,
    username: string,
    gender: string,
    age: number,
    englishLevel: number,
    enableReasoning: boolean = false,
    enableSearching: boolean = false
  ): Promise<string> => {
    try {
      // Construct the query parameters
      const params = new URLSearchParams({
        username,
        gender,
        age: age.toString(),
        englishLevel: englishLevel.toString(),
        enableReasoning: enableReasoning.toString(),
        enableSearching: enableSearching.toString()
      });

      // Get the raw response
      const response = await fetch(`${apiService.getBaseUrl()}/api/Chatbot/GenerateAnswer?${params.toString()}`, {
        method: 'POST',
        headers: {
          'Content-Type': 'application/json',
          ...apiService.getHeaders()
        },
        body: JSON.stringify(request)
      });

      if (!response.ok) {
        throw new Error(`Server responded with status: ${response.status}`);
      }

      // Handle the response
      const contentType = response.headers.get('content-type');

      if (contentType && contentType.includes('application/json')) {
        // If it's JSON, parse it
        const jsonData = await response.json();
        return jsonData.data || jsonData;
      } else {
        // If it's text, return it directly
        return await response.text();
      }
    } catch (error) {
      console.error('Error generating answer:', error);
      throw error;
    }
  },

  // Save conversation to localStorage for persistence
  saveConversation: (messages: ChatMessage[]): void => {
    try {
      localStorage.setItem('chat_history', JSON.stringify(messages));
    } catch (error) {
      console.error('Error saving conversation to localStorage:', error);
    }
  },

  // Load conversation from localStorage
  loadConversation: (): ChatMessage[] => {
    try {
      const savedMessages = localStorage.getItem('chat_history');
      return savedMessages ? JSON.parse(savedMessages) : [];
    } catch (error) {
      console.error('Error loading conversation from localStorage:', error);
      return [];
    }
  },

  // Clear conversation from localStorage
  clearConversation: (): void => {
    try {
      localStorage.removeItem('chat_history');
    } catch (error) {
      console.error('Error clearing conversation from localStorage:', error);
    }
  }
};