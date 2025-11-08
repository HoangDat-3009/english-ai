import { useAuth0 } from '@auth0/auth0-react';
import { useEffect } from 'react';
import { useAuth } from '@/components/AuthContext';
import { useToast } from '@/hooks/use-toast';
import { supabase } from '@/services/supabaseClient';

interface Auth0User {
  sub: string;
  email?: string;
  name?: string;
  picture?: string;
  email_verified?: boolean;
}

/**
 * Custom hook để tích hợp Auth0 với hệ thống authentication local
 * Tự động sync user từ Auth0 vào database và AuthContext
 */
export const useAuth0Integration = () => {
  const { isAuthenticated, user: auth0User, isLoading, error, logout: auth0Logout } = useAuth0();
  const { login, logout: localLogout, user: localUser } = useAuth();
  const { toast } = useToast();

  useEffect(() => {
    const syncAuth0User = async () => {
      if (isAuthenticated && auth0User && !localUser) {
        try {
          const typedAuth0User = auth0User as Auth0User;
          
          // Kiểm tra xem user đã tồn tại trong database chưa
          const { data: existingUser, error: fetchError } = await supabase
            .from('user')
            .select('*')
            .eq('email', typedAuth0User.email || '')
            .single();

          if (fetchError && fetchError.code !== 'PGRST116') {
            // PGRST116 = không tìm thấy row, đó là trường hợp bình thường
            throw fetchError;
          }

          if (existingUser) {
            // User đã tồn tại, login vào hệ thống
            login(existingUser);
            toast({
              title: "Đăng nhập thành công",
              description: `Chào mừng ${existingUser.tendangnhap} quay trở lại!`,
              variant: "default",
            });
          } else {
            // User mới từ Auth0, tạo tài khoản trong database
            const newUser = {
              email: typedAuth0User.email || '',
              tendangnhap: typedAuth0User.name || typedAuth0User.email?.split('@')[0] || 'User',
              password: '', // OAuth users không cần password
              englishlevel: 'beginner', // Mặc định
              ngaytaotaikhoan: new Date().toISOString(),
              auth0_sub: typedAuth0User.sub,
              avatar_url: typedAuth0User.picture,
              email_verified: typedAuth0User.email_verified || false,
            };

            const { data: createdUser, error: createError } = await supabase
              .from('user')
              .insert([newUser])
              .select()
              .single();

            if (createError) {
              throw createError;
            }

            if (createdUser) {
              login(createdUser);
              toast({
                title: "Tài khoản đã được tạo",
                description: `Chào mừng ${createdUser.tendangnhap} đến với EngBuddy!`,
                variant: "default",
              });
            }
          }
        } catch (error) {
          console.error('Error syncing Auth0 user:', error);
          toast({
            title: "Lỗi đồng bộ tài khoản",
            description: "Không thể đồng bộ thông tin tài khoản từ Auth0",
            variant: "destructive",
          });
        }
      }
    };

    syncAuth0User();
  }, [isAuthenticated, auth0User, localUser, login, toast]);

  // Xử lý lỗi Auth0
  useEffect(() => {
    if (error) {
      console.error('Auth0 error:', error);
      toast({
        title: "Lỗi xác thực",
        description: error.message || "Đã xảy ra lỗi trong quá trình xác thực",
        variant: "destructive",
      });
    }
  }, [error, toast]);

  // Hàm logout tích hợp (logout cả Auth0 và local)
  const handleLogout = async () => {
    try {
      localLogout();
      if (isAuthenticated) {
        await auth0Logout({
          logoutParams: {
            returnTo: window.location.origin,
          },
        });
      }
    } catch (error) {
      console.error('Logout error:', error);
      toast({
        title: "Lỗi đăng xuất",
        description: "Không thể đăng xuất hoàn toàn",
        variant: "destructive",
      });
    }
  };

  return {
    isLoading,
    isAuthenticated,
    auth0User,
    localUser,
    handleLogout,
    error,
  };
};
