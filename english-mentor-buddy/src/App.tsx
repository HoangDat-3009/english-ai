import { Toaster as Sonner } from "@/components/ui/sonner";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AnimatePresence } from "framer-motion";
import { HashRouter, Route, Routes } from "react-router-dom"; // Loại bỏ BrowserRouter không dùng
import { ThemeProvider } from "./components/ThemeProvider";
import Chat from "./pages/Chat";
import Dictionary from "./pages/Dictionary";
import DictionaryResult from "./pages/DictionaryResult";
import EnglishTopicCards from "./pages/EnglishTopicCards";
import Exercises from "./pages/Exercises";
import Index from "./pages/Index";
import Leaderboard from "./pages/Leaderboard";
import Login from "./pages/Login";
import NotFound from "./pages/NotFound";
import Progress from "./pages/Progress";
import ReadingExercises from "./pages/ReadingExercises";
import Register from "./pages/Register";

// Admin imports
import AdminProtectedRoute from "./components/admin/AdminProtectedRoute";
import AdminLayout from "./layouts/admin/AdminLayout.tsx";
import AccountPage from "./pages/admin/AccountPage";
import ContentManagement from "./pages/admin/ContentManagement";
import AdminDashboard from "./pages/admin/Dashboard";
import ProfilePage from "./pages/admin/ProfilePage";
import AdminSettings from "./pages/admin/Settings";
import UploadPage from "./pages/admin/UploadPage";
import UserManagement from "./pages/admin/UserManagement";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <ThemeProvider>
      <TooltipProvider>
        <Toaster />
        <Sonner />
        <HashRouter>
          <AnimatePresence mode="wait">
            <Routes>
              {/* Route mặc định - chuyển thẳng tới trang chính */}
              <Route path="/" element={<Index />} />

              {/* Public routes */}
              <Route path="/" element={<Login />} />
              <Route path="/register" element={<Register />} />

              {/* Các routes chính - không còn bắt buộc đăng nhập */}
              <Route path="/index" element={<Index />} />
              <Route path="/dictionary" element={<Dictionary />} />
              <Route path="/dictionary-result" element={<DictionaryResult />} />
              <Route path="/exercises" element={<Exercises />} />
              <Route path="/chat" element={<Chat />} />
              <Route path="/topics" element={<EnglishTopicCards />} />
              <Route path="/progress" element={<Progress />} />
              <Route path="/leaderboard" element={<Leaderboard />} />
              <Route path="/reading-exercises" element={<ReadingExercises />} />

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
        </HashRouter>
      </TooltipProvider>
    </ThemeProvider>
  </QueryClientProvider>
);

export default App;