import { Toaster } from "@/components/ui/toaster";
import { Toaster as Sonner } from "@/components/ui/sonner";
import { TooltipProvider } from "@/components/ui/tooltip";
import { QueryClient, QueryClientProvider } from "@tanstack/react-query";
import { BrowserRouter, Routes, Route, HashRouter } from "react-router-dom";
import Index from "./pages/Index";
import ListeningPractice from "./pages/ListeningPractice";
import ListeningPracticeLesson from "./pages/ListeningPracticeLesson";
import LessonList from "./pages/LessonList";
import CreateLesson from "./pages/CreateLesson";
import ConversationPractice from "./pages/ConversationPractice";
import WritingPractice from "./pages/WritingPractice";
import VocabularyGroup from "./pages/VocabularyGroup";

import FlashcardGroups from "./pages/FlashcardGroups";
import FlashcardPracticeSelection from "./pages/FlashcardPracticeSelection";
import FlashcardPractice from "./pages/FlashcardPractice";
import VocabularyTest from "./pages/VocabularyTest";
import GuessWord from "./pages/GuessWord";
import SentencePractice from "./pages/SentencePractice";
import GrammarPractice from "./pages/GrammarPractice";
import TestList from "./pages/TestList";
import TestConfiguration from "./pages/TestConfiguration";
import TestExam from "./pages/TestExam";
import NotFound from "./pages/NotFound";

const queryClient = new QueryClient();

const App = () => (
  <QueryClientProvider client={queryClient}>
    <TooltipProvider>
      <Toaster />
      <Sonner />
      <HashRouter>
        <Routes>
          <Route path="/" element={<Index />} />
          <Route path="/listening-practice" element={<ListeningPractice />} />
          <Route path="/listening-practice-lesson" element={<ListeningPracticeLesson />} />
          <Route path="/lesson-list" element={<LessonList />} />
          <Route path="/create-lesson" element={<CreateLesson />} />
          <Route path="/conversation-practice" element={<ConversationPractice />} />
          <Route path="/writing-practice" element={<WritingPractice />} />

          <Route path="/vocabulary-group/:topicId" element={<VocabularyGroup />} />
          <Route path="/flashcard-groups" element={<FlashcardGroups />} />
          <Route path="/flashcard-selection" element={<FlashcardPracticeSelection />} />
          <Route path="/flashcard-practice" element={<FlashcardPractice />} />
          <Route path="/vocabulary-test" element={<VocabularyTest />} />
          <Route path="/guess-word" element={<GuessWord />} />
          <Route path="/sentence-practice" element={<SentencePractice />} />
          <Route path="/grammar-practice" element={<GrammarPractice />} />
          <Route path="/test-list" element={<TestList />} />
          <Route path="/test-config/:testId" element={<TestConfiguration />} />
          <Route path="/test-exam/:testId" element={<TestExam />} />
          {/* ADD ALL CUSTOM ROUTES ABOVE THE CATCH-ALL "*" ROUTE */}
          <Route path="*" element={<NotFound />} />
        </Routes>
      </HashRouter>
    </TooltipProvider>
  </QueryClientProvider>
);

export default App;
