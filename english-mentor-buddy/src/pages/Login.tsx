import React, { useState } from 'react';
import { motion } from 'framer-motion';
import { useNavigate, Link } from 'react-router-dom';
import { LockKeyhole, User } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { useToast } from '@/hooks/use-toast';
import { useTheme } from '@/components/ThemeProvider';
import { supabase } from '@/services/supabaseClient';
import { useAuth } from '@/components/AuthContext';

const Login: React.FC = () => {
    const navigate = useNavigate();
    const { toast } = useToast();
    const { theme } = useTheme();
    const { login } = useAuth();
    const [isLoading, setIsLoading] = useState(false);
    const [formData, setFormData] = useState({
        username: '',
        password: '',
        rememberMe: false,
    });

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
        const { name, value } = e.target;
        setFormData(prev => ({
            ...prev,
            [name]: value,
        }));
    };

    const handleCheckboxChange = (checked: boolean) => {
        setFormData(prev => ({
            ...prev,
            rememberMe: checked,
        }));
    };

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        if (!formData.username.trim() || !formData.password.trim()) {
            toast({
                title: "Thông tin không hợp lệ",
                description: "Vui lòng nhập tên đăng nhập và mật khẩu",
                variant: "destructive",
            });
            return;
        }

        try {
            setIsLoading(true);

            const { data, error } = await supabase
                .from('user')
                .select('*')
                .eq('tendangnhap', formData.username)
                .eq('password', formData.password) // So sánh mật khẩu thô
                .single();

            if (error || !data) {
                throw new Error('Thông tin đăng nhập không đúng');
            }

            login(data);

            toast({
                title: "Đăng nhập thành công",
                description: `Chào mừng ${data.tendangnhap} quay trở lại!`,
                variant: "default",
            });

            navigate('/index');

        } catch (error) {
            console.error('Login error:', error);
            toast({
                title: "Đăng nhập thất bại",
                description: "Tên đăng nhập hoặc mật khẩu không đúng",
                variant: "destructive",
            });
        } finally {
            setIsLoading(false);
        }
    };

    return (
        <div className={`min-h-screen flex flex-col items-center justify-center bg-gradient-to-b from-blue-50 to-blue-100 dark:from-gray-900 dark:to-gray-800 px-4 py-8 ${theme === 'dark' ? 'dark' : ''}`}>
            <motion.div
                initial={{ opacity: 0, y: 20 }}
                animate={{ opacity: 1, y: 0 }}
                transition={{ duration: 0.5 }}
                className="w-full max-w-md"
            >
                <div className="flex flex-col items-center mb-8">
                    <Link to="/" className="flex items-center gap-2 mb-6">
                        <span className="text-3xl font-bold bg-gradient-to-r from-primary via-dictionary to-exercises bg-clip-text text-transparent">EngBuddy</span>
                    </Link>
                    <h1 className="text-3xl font-bold dark:text-white mb-2">Đăng nhập</h1>
                    <p className="text-gray-600 dark:text-gray-400 text-center">
                        Đăng nhập để tiếp tục hành trình học tiếng Anh của bạn
                    </p>
                </div>
                <motion.div
                    initial={{ opacity: 0, y: 20 }}
                    animate={{ opacity: 1, y: 0 }}
                    transition={{ duration: 0.5, delay: 0.1 }}
                    className="bg-white dark:bg-gray-800 rounded-xl shadow-lg p-6 md:p-8"
                >
                    <form onSubmit={handleSubmit} className="space-y-5">
                        <div className="space-y-2">
                            <Label htmlFor="username" className="text-gray-700 dark:text-gray-300">
                                Tên đăng nhập
                            </Label>
                            <div className="relative">
                                <User className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                                <Input
                                    id="username"
                                    name="username"
                                    type="text"
                                    placeholder="Nhập tên đăng nhập"
                                    className="pl-10 py-6 rounded-xl text-base dark:bg-gray-700 dark:text-white dark:border-gray-600"
                                    value={formData.username}
                                    onChange={handleInputChange}
                                    disabled={isLoading}
                                />
                            </div>
                        </div>
                        <div className="space-y-2">
                            <div className="flex justify-between">
                                <Label htmlFor="password" className="text-gray-700 dark:text-gray-300">
                                    Mật khẩu
                                </Label>
                                <Link to="/forgot-password" className="text-sm text-primary hover:underline">
                                    Quên mật khẩu?
                                </Link>
                            </div>
                            <div className="relative">
                                <LockKeyhole className="absolute left-3 top-1/2 -translate-y-1/2 h-5 w-5 text-gray-400" />
                                <Input
                                    id="password"
                                    name="password"
                                    type="password"
                                    placeholder="Nhập mật khẩu"
                                    className="pl-10 py-6 rounded-xl text-base dark:bg-gray-700 dark:text-white dark:border-gray-600"
                                    value={formData.password}
                                    onChange={handleInputChange}
                                    disabled={isLoading}
                                />
                            </div>
                        </div>
                        <div className="flex items-center space-x-2">
                            <Checkbox
                                id="rememberMe"
                                checked={formData.rememberMe}
                                onCheckedChange={handleCheckboxChange}
                                disabled={isLoading}
                            />
                            <Label
                                htmlFor="rememberMe"
                                className="text-sm text-gray-600 dark:text-gray-400 cursor-pointer"
                            >
                                Ghi nhớ đăng nhập
                            </Label>
                        </div>
                        <Button
                            type="submit"
                            className="w-full py-6 text-lg font-medium bg-primary hover:bg-primary/90"
                            disabled={isLoading}
                        >
                            {isLoading ? (
                                <>
                                    <span className="animate-spin mr-2">
                                        <svg className="h-5 w-5" viewBox="0 0 24 24">
                                            <circle
                                                className="opacity-25"
                                                cx="12"
                                                cy="12"
                                                r="10"
                                                stroke="currentColor"
                                                strokeWidth="4"
                                                fill="none"
                                            />
                                            <path
                                                className="opacity-75"
                                                fill="currentColor"
                                                d="M4 12a8 8 0 018-8V0C5.373 0 0 5.373 0 12h4zm2 5.291A7.962 7.962 0 014 12H0c0 3.042 1.135 5.824 3 7.938l3-2.647z"
                                            />
                                        </svg>
                                    </span>
                                    Đang đăng nhập...
                                </>
                            ) : 'Đăng nhập'}
                        </Button>
                    </form>
                    <div className="mt-5 text-center">
                        <p className="text-gray-600 dark:text-gray-400">
                            Chưa có tài khoản?{' '}
                            <Link to="/register" className="text-primary font-medium hover:underline">
                                Đăng ký ngay
                            </Link>
                        </p>
                    </div>
                </motion.div>
                <div className="mt-8 text-center text-sm text-gray-500 dark:text-gray-400">
                    <p>© {new Date().getFullYear()} EngBuddy. All rights reserved.</p>
                </div>
            </motion.div>
        </div>
    );
};

export default Login;