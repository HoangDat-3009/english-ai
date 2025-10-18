import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route } from "react-router-dom";
import { AnimatePresence } from "framer-motion";
import { ThemeProvider } from "./components/ThemeProvider";
import Index from "./pages/Index";
import Dictionary from "./pages/Dictionary";
import DictionaryResult from "./pages/DictionaryResult";
import Exercises from "./pages/Exercises";
import Chat from "./pages/Chat";
import EnglishTopicCards from "./pages/EnglishTopicCards";
import NotFound from "./pages/NotFound";
import Login from "./pages/Login";
import LoginAlt from "./pages/LoginAlt";
import Register from "./pages/Register";
import Auth0Callback from "./pages/Auth0Callback";
import ProtectedRoute from "./components/ProtectedRoute"; // Nhập ProtectedRoute

// Admin imports
import AdminLayout from "./layouts/admin/AdminLayout.tsx";
import AdminProtectedRoute from "./components/admin/AdminProtectedRoute";
import AdminDashboard from "./pages/admin/Dashboard";
import UserManagement from "./pages/admin/UserManagement";
import ContentManagement from "./pages/admin/ContentManagement";
import AdminSettings from "./pages/admin/Settings";
import TestsPage from "./pages/admin/TestsPage";
import UploadPage from "./pages/admin/UploadPage";
import AccountPage from "./pages/admin/AccountPage";
import ProfilePage from "./pages/admin/ProfilePage";
import TestStatistics from "./pages/TestStatistics";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <ThemeProvider>
      <TooltipProvider>
        <Toaster />
        <Sonner />
        <BrowserRouter>
          <AnimatePresence mode="wait">
            <Routes>
              {/* Route mặc định - chuyển thẳng tới trang chính */}
              <Route path="/" element={<Index />} />

              {/* Public routes */}
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />
              <Route path="/callback" element={<Auth0Callback />} />

              {/* Các routes chính - không còn bắt buộc đăng nhập */}
              <Route path="/index" element={<Index />} />
              <Route path="/dictionary" element={<Dictionary />} />
              <Route path="/dictionary-result" element={<DictionaryResult />} />
              <Route path="/exercises" element={<Exercises />} />
              <Route path="/chat" element={<Chat />} />
              <Route path="/topics" element={<EnglishTopicCards />} />
              <Route path="/test-stats" element={<TestStatistics />} />

              {/* Admin routes */}
              <Route path="/admin" element={
                <AdminProtectedRoute>
                  <AdminLayout />
                </AdminProtectedRoute>
              }>
                <Route index element={<AdminDashboard />} />
                <Route path="upload" element={<UploadPage />} />
                <Route path="users" element={<UserManagement />} />
                <Route path="account" element={<AccountPage />} />
                <Route path="profile" element={<ProfilePage />} />
                <Route path="content" element={<ContentManagement />} />
                <Route path="settings" element={<AdminSettings />} />
                {/* Có thể thêm các admin routes khác ở đây */}
              </Route>

              {/* Route cho trang không tìm thấy */}
              <Route path="*" element={<NotFound />} />
            </Routes>
          </AnimatePresence>
        </BrowserRouter>
      </TooltipProvider>
    </ThemeProvider>
  </QueryClientProvider>
);

export default App;