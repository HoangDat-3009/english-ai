import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { HashRouter, Routes, Route } from "react-router-dom"; // Loại bỏ BrowserRouter không dùng
import { AnimatePresence } from "framer-motion";
import { ThemeProvider } from "./components/ThemeProvider";
import Index from "./pages/Index";
import Dictionary from "./pages/Dictionary";
import DictionaryResult from "./pages/DictionaryResult";
import Exercises from "./pages/Exercises";
import Listening from "./pages/Listening";
import Speaking from "./pages/Speaking";
import Chat from "./pages/Chat";
import EnglishTopicCards from "./pages/EnglishTopicCards";
import NotFound from "./pages/NotFound";
import Login from "./pages/Login";
import LoginAlt from "./pages/LoginAlt";
import Register from "./pages/Register";
import ProtectedRoute from "./components/ProtectedRoute"; // Nhập ProtectedRoute

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
              <Route path="/listening" element={<Listening />} />
              <Route path="/speaking" element={<Speaking />} />
              <Route path="/chat" element={<Chat />} />
              <Route path="/topics" element={<EnglishTopicCards />} />

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