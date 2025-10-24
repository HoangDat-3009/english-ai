import FeatureCard from '@/components/FeatureCard';
import MainLayout from '@/layouts/MainLayout';
import { motion } from 'framer-motion';
import { Book, Globe, GraduationCap, MessageCircle, TrendingUp, Trophy } from 'lucide-react';
import { Link } from 'react-router-dom'; // Import Link from react-router-dom

const features = [
  {
    title: 'Từ điển',
    description: 'Tra cứu từ vựng với định nghĩa chi tiết, ví dụ thực tế và gợi ý sử dụng trong nhiều ngữ cảnh khác nhau.',
    icon: Book,
    path: '/dictionary',
    color: 'text-pink-600',
    bgColor: 'bg-gradient-to-br from-pink-50 to-rose-50 dark:from-pink-950/50 dark:to-rose-950/50',
  },
  {
    title: 'Bài tập',
    description: 'Thiết lập bài tập phù hợp với nhu cầu học tập của bạn với các chủ đề và dạng bài tập đa dạng.',
    icon: GraduationCap,
    path: '/exercises',
    color: 'text-fuchsia-600',
    bgColor: 'bg-gradient-to-br from-fuchsia-50 to-purple-50 dark:from-fuchsia-950/50 dark:to-purple-950/50',
  },
  {
    title: 'Chat với AI',
    description: 'Trò chuyện với trợ lý AI thông minh để được hướng dẫn, giải thích và luyện tập tiếng Anh trong tình huống thực tế.',
    icon: MessageCircle,
    path: '/chat',
    color: 'text-rose-600',
    bgColor: 'bg-gradient-to-br from-rose-50 to-pink-50 dark:from-rose-950/50 dark:to-pink-950/50',
  },
  {
    title: 'Chủ đề hội thoại',
    description: 'Luyện tập tiếng Anh theo các chủ đề cụ thể với AI, hỗ trợ ghi âm và phát âm để cải thiện kỹ năng giao tiếp.',
    icon: Globe,
    path: '/topics',
    color: 'text-slate-600',
    bgColor: 'bg-gradient-to-br from-slate-50 to-gray-50 dark:from-slate-950/50 dark:to-gray-950/50',
  },
  {
    title: 'Tiến độ học tập',
    description: 'Theo dõi quá trình học tập của bạn với biểu đồ chi tiết, thống kê điểm số và các mục tiêu đạt được.',
    icon: TrendingUp,
    path: '/progress',
    color: 'text-blue-600',
    bgColor: 'bg-gradient-to-br from-blue-50 to-cyan-50 dark:from-blue-950/50 dark:to-cyan-950/50',
  },
  {
    title: 'Bảng xếp hạng',
    description: 'Cạnh tranh với những người học khác, xem thứ hạng của bạn và động lực phấn đấu hơn nữa.',
    icon: Trophy,
    path: '/leaderboard',
    color: 'text-yellow-600',
    bgColor: 'bg-gradient-to-br from-yellow-50 to-orange-50 dark:from-yellow-950/50 dark:to-orange-950/50',
  }

];

const Index = () => {
  return (
    <MainLayout>
      {/* Hero Section */}
      <section className="pt-4 pb-20 md:pt-10 md:pb-32 overflow-hidden bg-gradient-to-br from-pink-50 via-rose-50 to-fuchsia-50 dark:from-pink-950 dark:via-rose-950 dark:to-fuchsia-950">
        <div className="container px-4 md:px-6">
          <div className="flex flex-col items-center text-center space-y-4 md:space-y-8">
            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, ease: "easeOut" }}
              className="space-y-2"
            >
              <div className="flex items-center justify-center gap-2 mb-4">
                <div className="p-2 rounded-full bg-gradient-to-r from-pink-400 to-rose-500 shadow-lg shadow-pink-200/50">
                  <Globe className="h-6 w-6 text-white" />
                </div>
                <span className="text-3xl md:text-4xl font-bold bg-gradient-to-r from-pink-600 via-rose-600 to-fuchsia-600 bg-clip-text text-transparent">
                  EngBuddy
                </span>
                <span className="hidden sm:inline-block text-sm text-pink-600 dark:text-pink-400 font-medium">
                  | Nền tảng học tiếng Anh thông minh
                </span>
              </div>

              <h1 className="text-3xl md:text-5xl lg:text-6xl font-bold tracking-tighter text-slate-900 dark:text-slate-100">
                Nâng cao kỹ năng tiếng Anh với{" "}
                <span className="bg-gradient-to-r from-pink-600 via-rose-600 to-fuchsia-600 bg-clip-text text-transparent inline-block">
                  EngBuddy
                </span>
              </h1>
              <p className="mx-auto max-w-[700px] text-slate-600 dark:text-slate-300 md:text-xl mt-4">
                Công cụ học tiếng Anh thông minh giúp bạn tra từ, tạo bài tập và luyện tập với AI chỉ trong một nền tảng.
              </p>
            </motion.div>

            <motion.div
              initial={{ opacity: 0, y: 20 }}
              animate={{ opacity: 1, y: 0 }}
              transition={{ duration: 0.7, delay: 0.3, ease: "easeOut" }}
              className="flex flex-wrap gap-4 justify-center"
            >
              <Link
                to="/dictionary"
                className="group inline-flex h-12 items-center justify-center rounded-xl bg-gradient-to-r from-pink-500 to-rose-600 px-8 text-sm font-medium text-white shadow-lg shadow-pink-200/50 transition-all duration-300 hover:shadow-xl hover:scale-105 hover:shadow-pink-300/50 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-pink-500"
              >
                <span className="mr-2">✨</span>
                Bắt đầu ngay
                <span className="ml-2 transition-transform duration-300 group-hover:translate-x-1">→</span>
              </Link>
              <Link
                to="/exercises"
                className="group inline-flex h-12 items-center justify-center rounded-xl border border-pink-200 dark:border-pink-700 bg-white/90 dark:bg-slate-800/90 backdrop-blur-sm px-8 text-sm font-medium text-pink-700 dark:text-pink-200 shadow-lg transition-all duration-300 hover:shadow-xl hover:scale-105 hover:bg-pink-50 dark:hover:bg-pink-900/20 focus-visible:outline-none focus-visible:ring-2 focus-visible:ring-pink-500"
              >
                <span className="mr-2">🚀</span>
                Khám phá tính năng
                <span className="ml-2 transition-transform duration-300 group-hover:translate-x-1">→</span>
              </Link>
            </motion.div>
          </div>
        </div>
      </section>

      {/* Wave Separator */}
      <div className="relative h-16 md:h-24 overflow-hidden">
        <svg
          className="absolute bottom-0 w-full h-full text-rose-100 dark:text-rose-900"
          viewBox="0 0 1440 320"
          fill="none"
          xmlns="http://www.w3.org/2000/svg"
          preserveAspectRatio="none"
        >
          <path
            fillRule="evenodd"
            clipRule="evenodd"
            d="M0 320H1440V0C1344 80 1104 160 720 160C336 160 96 80 0 0V320Z"
            fill="currentColor"
          />
        </svg>
      </div>

      {/* Features Section */}
      <section id="features" className="py-12 md:py-20 bg-gradient-to-b from-rose-100 via-pink-50 to-white dark:from-rose-900 dark:via-pink-800 dark:to-slate-900">
        <div className="container px-4 md:px-6">
          <div className="flex flex-col items-center justify-center space-y-4 text-center mb-12">
            <div className="space-y-2">
              <div className="inline-block rounded-full bg-gradient-to-r from-pink-100 to-rose-100 dark:from-pink-900 dark:to-rose-900 px-4 py-2 text-sm font-medium text-pink-700 dark:text-pink-300 shadow-lg">
                ✨ Tính năng chính
              </div>
              <h2 className="text-3xl font-bold tracking-tighter md:text-4xl/tight bg-gradient-to-r from-slate-900 to-pink-700 dark:from-slate-100 dark:to-pink-300 bg-clip-text text-transparent">
                Mọi công cụ bạn cần để thành thạo tiếng Anh
              </h2>
              <p className="mx-auto max-w-[700px] text-slate-600 dark:text-slate-300 md:text-lg">
                EngBuddy kết hợp các tính năng thiết yếu giúp việc học tiếng Anh trở nên hiệu quả và thú vị hơn.
              </p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6 mt-8">
            {features.map((feature, index) => (
              <FeatureCard
                key={feature.title}
                {...feature}
                delay={index}
              />
            ))}
          </div>
        </div>
      </section>
    </MainLayout>
  );
};

export default Index;