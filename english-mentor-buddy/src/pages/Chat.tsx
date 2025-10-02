import React, { useState, useEffect, useRef } from 'react';
import { MessageSquare, Send, RefreshCw } from 'lucide-react';
import Header from '@/components/Navbar';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { useToast } from '@/hooks/use-toast';
import { chatService, ChatMessage } from '@/services/consultationService';

// Định nghĩa interface Message cho UI
interface Message {
  id: number;
  content: string;
  isUser: boolean;
  timestamp: string;
}

const Consultation: React.FC = () => {
  const { success, error } = useToast();
  const [message, setMessage] = useState('');
  const [messages, setMessages] = useState<Message[]>([]);
  const [isLoading, setIsLoading] = useState(false);
  const messagesEndRef = useRef<HTMLDivElement>(null);

  // Cài đặt người dùng cố định
  const userSettings = {
    username: 'Cong',
    gender: 'Nam',
    age: 19,
    englishLevel: 1,
    enableReasoning: false,
    enableSearching: false
  };

  // Load initial messages
  useEffect(() => {
    const loadInitialMessages = async () => {
      try {
        setIsLoading(true);

        // Load conversation from localStorage
        const savedChatHistory = chatService.loadConversation();

        if (savedChatHistory.length > 0) {
          // Convert saved chat history to our Message format
          const formattedMessages = savedChatHistory.map((chatMsg, index) => ({
            id: index + 1,
            content: chatMsg.Message,
            isUser: chatMsg.FromUser,
            timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
          }));

          setMessages(formattedMessages);
        } else {
          // Set default welcome message
          const welcomeMessage: Message = {
            id: 1,
            content: 'Chào! Mình là PJ1Buddy, trợ lý ảo được thiết kế riêng để hỗ trợ bạn học tiếng Anh nè. 😊\n\nMình luôn cố gắng hỗ trợ bạn tốt nhất, nhưng đôi khi vẫn có thể mắc sai sót, nên bạn nhớ kiểm tra lại những thông tin quan trọng nha!',
            isUser: false,
            timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
          };

          setMessages([welcomeMessage]);

          // Save welcome message to localStorage
          chatService.saveConversation([
            {
              FromUser: false,
              Message: welcomeMessage.content
            }
          ]);
        }
      } catch (err) {
        console.error('Error loading conversation history:', err);
        error('Không thể tải lịch sử trò chuyện', 'Vui lòng thử lại sau');
      } finally {
        setIsLoading(false);
      }
    };

    loadInitialMessages();
  }, [error]);

  // Scroll to bottom when messages change
  useEffect(() => {
    messagesEndRef.current?.scrollIntoView({ behavior: 'smooth' });
  }, [messages]);

  const handleSendMessage = async () => {
    if (!message.trim()) return;

    // Create new user message
    const newUserMessage: Message = {
      id: Date.now(),
      content: message,
      isUser: true,
      timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
    };

    // Add user message to the UI
    setMessages(prev => [...prev, newUserMessage]);

    // Clear input field
    setMessage('');

    // Prepare for API request
    setIsLoading(true);

    try {
      // Convert current message history to format expected by API
      const chatHistory: ChatMessage[] = messages.map(msg => ({
        FromUser: msg.isUser,
        Message: msg.content
      }));

      // Add the new user message to chat history
      chatHistory.push({
        FromUser: true,
        Message: newUserMessage.content
      });

      // Create request object
      const request = {
        ChatHistory: chatHistory,
        Question: newUserMessage.content,
        ImagesAsBase64: null
      };

      // Send request to API
      const response = await chatService.generateAnswer(
        request,
        userSettings.username,
        userSettings.gender,
        userSettings.age,
        userSettings.englishLevel,
        userSettings.enableReasoning,
        userSettings.enableSearching
      );

      // Create bot response message
      const botResponse: Message = {
        id: Date.now() + 1,
        content: response,
        isUser: false,
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
      };

      // Add bot response to UI
      setMessages(prev => [...prev, botResponse]);

      // Save updated conversation to localStorage
      const updatedChatHistory = [
        ...chatHistory,
        { FromUser: false, Message: botResponse.content }
      ];
      chatService.saveConversation(updatedChatHistory);

    } catch (err) {
      console.error('Error sending message:', err);
      error('Không thể gửi tin nhắn', 'Vui lòng thử lại sau');
    } finally {
      setIsLoading(false);
    }
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !e.shiftKey) {
      e.preventDefault();
      handleSendMessage();
    }
  };

  const handleClearConversation = async () => {
    try {
      setIsLoading(true);

      // Clear conversation in localStorage
      chatService.clearConversation();

      // Add new welcome message
      const welcomeMessage: Message = {
        id: Date.now(),
        content: 'Cuộc trò chuyện đã được làm mới. Bạn có thể bắt đầu cuộc trò chuyện mới!',
        isUser: false,
        timestamp: new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' })
      };

      setMessages([welcomeMessage]);

      // Save welcome message to localStorage
      chatService.saveConversation([
        { FromUser: false, Message: welcomeMessage.content }
      ]);

      success('Đã xóa cuộc trò chuyện', 'Cuộc trò chuyện đã được làm mới');
    } catch (err) {
      console.error('Error clearing conversation:', err);
      error('Không thể xóa cuộc trò chuyện', 'Vui lòng thử lại sau');
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="min-h-screen flex flex-col">
      <Header />
      <main className="flex-1 container max-w-screen-xl mx-auto py-4 px-4 flex flex-col">
        <div className="flex-1 bg-white rounded-lg shadow-sm overflow-hidden flex flex-col">
          <div className="border-b p-4 flex items-center justify-between">
            <div className="flex items-center gap-2">
              <div className="w-10 h-10 bg-gradient-to-r from-pink-500 to-rose-600 rounded-lg flex items-center justify-center">
                <MessageSquare size={20} color="white" />
              </div>
              <h2 className="font-semibold text-lg">Tư vấn với AI</h2>
            </div>
            <Button
              variant="outline"
              size="sm"
              className="text-red-500 hover:text-red-600 hover:bg-red-50"
              onClick={handleClearConversation}
              disabled={isLoading}
            >
              <RefreshCw size={16} className="mr-2" />
              Làm mới
            </Button>
          </div>

          <div className="flex-1 overflow-y-auto p-4 space-y-4">
            {messages.map(msg => (
              <div
                key={msg.id}
                className={`max-w-3xl ${msg.isUser ? 'ml-auto' : ''}`}
              >
                <div
                  className={`rounded-2xl p-4 ${msg.isUser
                    ? 'bg-gradient-to-r from-pink-100 to-rose-100 text-right'
                    : 'bg-gradient-to-r from-gray-100 to-gray-50'
                    }`}
                >
                  <p className="whitespace-pre-line">{msg.content}</p>
                </div>
                <div className="text-xs text-gray-500 mt-1">
                  {msg.timestamp}
                </div>
              </div>
            ))}
            {isLoading && (
              <div className="max-w-3xl">
                <div className="bg-gradient-to-r from-gray-100 to-gray-50 rounded-2xl p-4">
                  <div className="flex space-x-2">
                    <div className="w-3 h-3 rounded-full bg-gray-400 animate-bounce"></div>
                    <div className="w-3 h-3 rounded-full bg-gray-400 animate-bounce" style={{ animationDelay: '0.2s' }}></div>
                    <div className="w-3 h-3 rounded-full bg-gray-400 animate-bounce" style={{ animationDelay: '0.4s' }}></div>
                  </div>
                </div>
              </div>
            )}
            <div ref={messagesEndRef} />
          </div>

          <div className="border-t p-4">
            <div className="flex gap-2">
              <div className="flex-1 relative">
                <Input
                  placeholder="Nhập tin nhắn của bạn..."
                  className="pr-10 py-6 rounded-xl"
                  value={message}
                  onChange={(e) => setMessage(e.target.value)}
                  onKeyDown={handleKeyDown}
                  disabled={isLoading}
                />
              </div>
              <Button
                className="bg-gradient-to-r from-pink-500 to-rose-600 hover:from-pink-600 hover:to-rose-700 text-white rounded-xl px-4"
                onClick={handleSendMessage}
                disabled={isLoading || !message.trim()}
              >
                <Send size={20} />
              </Button>
            </div>
          </div>
        </div>
      </main>
    </div>
  );
};

export default Consultation;