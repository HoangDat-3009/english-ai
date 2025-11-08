import { useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { useAuth0 } from '@auth0/auth0-react';
import { motion } from 'framer-motion';
import { Loader2 } from 'lucide-react';

/**
 * Auth0 Callback Page
 * Trang này xử lý redirect sau khi user đăng nhập thành công từ Auth0
 */
const Auth0Callback: React.FC = () => {
  const navigate = useNavigate();
  const { isAuthenticated, isLoading, error } = useAuth0();

  useEffect(() => {
    // Đợi Auth0 hoàn tất quá trình authentication
    if (!isLoading) {
      if (isAuthenticated) {
        // Đăng nhập thành công, redirect về trang chủ
        navigate('/index', { replace: true });
      } else if (error) {
        // Có lỗi xảy ra, redirect về trang login
        console.error('Auth0 callback error:', error);
        navigate('/login', { replace: true });
      }
    }
  }, [isAuthenticated, isLoading, error, navigate]);

  return (
    <div className="min-h-screen flex flex-col items-center justify-center bg-gradient-to-br from-pink-50 via-rose-50 to-fuchsia-50 dark:from-pink-950 dark:via-rose-950 dark:to-fuchsia-950">
      <motion.div
        initial={{ opacity: 0, scale: 0.9 }}
        animate={{ opacity: 1, scale: 1 }}
        transition={{ duration: 0.3 }}
        className="text-center"
      >
        <div className="mb-6">
          <Loader2 className="h-16 w-16 animate-spin text-primary mx-auto" />
        </div>
        <h1 className="text-2xl font-bold text-gray-800 dark:text-white mb-2">
          Đang xác thực...
        </h1>
        <p className="text-gray-600 dark:text-gray-400">
          Vui lòng đợi trong giây lát
        </p>
        {error && (
          <motion.div
            initial={{ opacity: 0 }}
            animate={{ opacity: 1 }}
            className="mt-4 p-4 bg-red-50 dark:bg-red-900/20 text-red-600 dark:text-red-400 rounded-lg"
          >
            <p className="text-sm">
              Đã xảy ra lỗi: {error.message}
            </p>
            <p className="text-xs mt-2">
              Đang chuyển hướng về trang đăng nhập...
            </p>
          </motion.div>
        )}
      </motion.div>
    </div>
  );
};

export default Auth0Callback;
