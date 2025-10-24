import { Toaster as Sonner } from "@/components/ui/sonner";
import { Toaster } from "@/components/ui/toaster";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { AnimatePresence } from "framer-motion";
import { HashRouter, Route, Routes } from "react-router-dom"; // Loại bỏ BrowserRouter không dùng
import DataConnectionTest from "./components/DataConnectionTest";
import DataSyncTest from "./components/DataSyncTest";
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
              <Route path="/login" element={<Login />} />
              <Route path="/register" element={<Register />} />

              {/* Các routes chính - không còn bắt buộc đăng nhập */}
              <Route path="/index" element={<Index />} />
              <Route path="/dictionary" element={<Dictionary />} />
              <Route path="/dictionary-result" element={<DictionaryResult />} />
              <Route path="/exercises" element={<Exercises />} />
              <Route path="/exercises/reading" element={<ReadingExercises />} />
              <Route path="/chat" element={<Chat />} />
              <Route path="/topics" element={<EnglishTopicCards />} />
              <Route path="/progress" element={<Progress />} />
              <Route path="/leaderboard" element={<Leaderboard />} />
              
              {/* Test routes for data synchronization */}
              <Route path="/data-sync-test" element={<DataSyncTest />} />
              <Route path="/data-connection-test" element={<DataConnectionTest />} />

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