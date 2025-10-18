// Base API service for making HTTP requests
class ApiService {
  private baseUrl: string;

  constructor(baseUrl: string = '') {
    // Use empty string for relative URLs (Vite proxy will handle routing)
    this.baseUrl = baseUrl;
    console.log('Base URL:', this.baseUrl || 'Using Vite proxy (relative URLs)');
  }

  // Get the base URL
  getBaseUrl(): string {
    return this.baseUrl;
  }

  // Get headers
  getHeaders(): HeadersInit {
    return {
      'Content-Type': 'application/json',
    };
  }

  async request<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    try {
      const url = `${this.baseUrl}${endpoint}`;

      // Default headers
      const headers: HeadersInit = {
        'Content-Type': 'application/json',
        ...options.headers,
      };

      const response = await fetch(url, {
        ...options,
        headers,
      });

      // Check if the response is successful
      if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        throw new Error(
          errorData?.message || `Server responded with status: ${response.status}`
        );
      }

      // For 204 No Content responses, return empty object
      if (response.status === 204) {
        return {} as T;
      }

      return await response.json();
    } catch (error) {
      console.error('API request failed:', error);
      throw error;
    }
  }

  // GET request
  get<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'GET' });
  }

  // POST request
  post<T>(endpoint: string, data: unknown, options: RequestInit = {}): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'POST',
      body: JSON.stringify(data),
    });
  }

  // PUT request
  put<T>(endpoint: string, data: unknown, options: RequestInit = {}): Promise<T> {
    return this.request<T>(endpoint, {
      ...options,
      method: 'PUT',
      body: JSON.stringify(data),
    });
  }

  // DELETE request
  delete<T>(endpoint: string, options: RequestInit = {}): Promise<T> {
    return this.request<T>(endpoint, { ...options, method: 'DELETE' });
  }

  // POST request với FormData (để upload ảnh)
  async postFormData<T>(endpoint: string, formData: FormData): Promise<T> {
    try {
      const url = `${this.baseUrl}${endpoint}`;

      const response = await fetch(url, {
        method: 'POST',
        // Không đặt Content-Type khi sử dụng FormData vì trình duyệt sẽ tự xử lý
        body: formData,
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => null);
        throw new Error(
          errorData?.message || `Server responded with status: ${response.status}`
        );
      }

      if (response.status === 204) {
        return {} as T;
      }

      return await response.json();
    } catch (error) {
      console.error('API form data request failed:', error);
      throw error;
    }
  }
}

// Create a new instance with empty baseUrl (use Vite proxy)
export const apiService = new ApiService("");

export default apiService;