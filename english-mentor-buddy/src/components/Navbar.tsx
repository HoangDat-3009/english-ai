import { Link, useLocation } from 'react-router-dom';
import { motion } from 'framer-motion';
import { Book, GraduationCap, MessageCircle, User, Sun, Moon, Globe, Settings, LogOut, UserCircle, Pencil } from 'lucide-react';
import { cn } from '@/lib/utils';
import { useTheme } from './ThemeProvider';
import { Button } from './ui/button';
import {
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
  DropdownMenuLabel,
} from './ui/dropdown-menu';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
  DialogFooter,
} from './ui/dialog';
import {
  Sheet,
  SheetContent,
  SheetDescription,
  SheetHeader,
  SheetTitle,
  SheetTrigger,
  SheetFooter,
} from './ui/sheet';
import { useAuth } from './AuthContext';
import { useNavigate } from 'react-router-dom';
import { useState } from 'react';
import { Input } from './ui/input';
import { Label } from './ui/label';
import { useToast } from '@/hooks/use-toast';
import { useAuth0Integration } from '@/hooks/useAuth0Integration';

const Navbar = () => {
  const location = useLocation();
  const { theme, toggleTheme } = useTheme();
  const { user, login, logout } = useAuth();
  const { handleLogout: auth0Logout } = useAuth0Integration();
  const navigate = useNavigate();
  const { toast } = useToast();

  const [isEditing, setIsEditing] = useState(false);
  const [formData, setFormData] = useState({
    tendangnhap: user?.tendangnhap || '',
    email: user?.email || '',
    englishlevel: user?.englishlevel || '',
    password: user?.password || '', // Hiển thị mật khẩu hiện tại
  });

  const navItems = [
    { name: 'Dictionary', path: '/dictionary', icon: Book, color: 'text-pink-600' },
    { name: 'Exercises', path: '/exercises', icon: GraduationCap, color: 'text-fuchsia-600' },
    { name: 'Writing', path: '/writing-mode', icon: Pencil, color: 'text-emerald-600' },
    { name: 'AI Chat', path: '/chat', icon: MessageCircle, color: 'text-rose-600' },
    { name: 'Topics', path: '/topics', icon: Globe, color: 'text-pink-500' },
  ];

  const handleLogout = async () => {
    await auth0Logout();
    navigate('/');
  };

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value,
    }));
  };

  const handleEditToggle = () => {
    setIsEditing(true);
    setFormData({
      tendangnhap: user?.tendangnhap || '',
      email: user?.email || '',
      englishlevel: user?.englishlevel || '',
      password: user?.password || '', // Giữ mật khẩu hiện tại
    });
  };

  const handleSave = async () => {
    if (!user) return;

    try {
      const { data, error } = await supabase
        .from('user')
        .update({
          tendangnhap: formData.tendangnhap,
          email: formData.email,
          englishlevel: formData.englishlevel,
          password: formData.password, // Cập nhật mật khẩu thô
        })
        .eq('id', user.id)
        .select()
        .single();

      if (error) throw error;

      // Cập nhật user trong Context
      login({ ...user, ...data });

      toast({
        title: "Cập nhật thành công",
        description: "Thông tin cá nhân đã được lưu.",
        variant: "default",
      });

      setIsEditing(false);
    } catch (error) {
      console.error('Update error:', error);
      toast({
        title: "Cập nhật thất bại",
        description: "Có lỗi xảy ra khi lưu thông tin. Vui lòng kiểm tra lại.",
        variant: "destructive",
      });
    }
  };



  return (
    <header className="sticky top-0 z-50 w-full border-b border-border/40 backdrop-blur-lg bg-background/80 dark:bg-background/80">
      <div className="container flex h-16 items-center justify-between">
        <Link to="/index" className="flex items-center gap-2">
          <motion.div
            initial={{ scale: 0.9, opacity: 0 }}
            animate={{ scale: 1, opacity: 1 }}
            transition={{ duration: 0.5 }}
            className="flex items-center gap-2"
          >
            <div className="p-1.5 rounded-lg bg-gradient-to-r from-pink-500 to-rose-600">
              <Globe className="h-6 w-6 text-white" />
            </div>
            <span className="text-2xl font-bold bg-gradient-to-r from-pink-600 to-rose-600 bg-clip-text text-transparent">
              EngBuddy
            </span>
            <span className="hidden sm:inline-block text-sm text-muted-foreground">
              | Nền tảng học tiếng Anh thông minh
            </span>
          </motion.div>
        </Link>
        <nav className="hidden md:flex items-center gap-6">
          {navItems.map((item) => {
            const isActive = location.pathname === item.path;
            return (
              <Link
                key={item.path}
                to={item.path}
                className={cn(
                  "flex items-center gap-1.5 font-medium transition-colors hover:text-foreground relative py-2",
                  isActive ? "text-foreground" : "text-muted-foreground"
                )}
              >
                <item.icon className={cn("w-4 h-4", item.color)} />
                <span>{item.name}</span>
                {isActive && (
                  <motion.div
                    layoutId="navbar-indicator"
                    className="absolute -bottom-[1px] left-0 right-0 h-[2px] bg-primary"
                    initial={{ opacity: 0 }}
                    animate={{ opacity: 1 }}
                    transition={{ duration: 0.3 }}
                  />
                )}
              </Link>
            );
          })}
        </nav>

        

        <div className="flex items-center gap-4">
          <Link to="/pricing">
            <Button 
              variant={location.pathname === "/pricing" ? "default" : "ghost"}
              size="sm"
            >
              Premium
            </Button>
          </Link>

          <Button variant="ghost" size="icon" onClick={toggleTheme} className="rounded-full">
            {theme === 'dark' ? <Sun className="h-5 w-5" /> : <Moon className="h-5 w-5" />}
            <span className="sr-only">Toggle theme</span>
          </Button>

          <DropdownMenu>
            <DropdownMenuTrigger asChild>
              <Button variant="outline" size="icon" className="rounded-full border-border">
                <User className="h-5 w-5" />
                <span className="sr-only">Account</span>
              </Button>
            </DropdownMenuTrigger>
            <DropdownMenuContent align="end" className="w-56">
              <DropdownMenuLabel className="font-medium">
                {user ? `Xin chào, ${user.tendangnhap}` : 'Khách'}
              </DropdownMenuLabel>
              <DropdownMenuSeparator />

              {user ? (
                <>
                  <Dialog>
                    <DialogTrigger asChild>
                      <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                        <UserCircle className="mr-2 h-4 w-4" />
                        <span>Thông tin cá nhân</span>
                      </DropdownMenuItem>
                    </DialogTrigger>
                    <DialogContent className="sm:max-w-[425px]">
                      <DialogHeader>
                        <DialogTitle>Thông tin cá nhân</DialogTitle>
                        <DialogDescription>
                          Xem và cập nhật thông tin tài khoản của bạn
                        </DialogDescription>
                      </DialogHeader>
                      <div className="py-4 space-y-4">
                        <div className="flex items-center justify-center mb-6">
                          <div className="relative">
                            <div className="w-24 h-24 rounded-full bg-muted flex items-center justify-center overflow-hidden border-2 border-primary">
                              <UserCircle className="w-20 h-20 text-muted-foreground" />
                            </div>
                            <Button
                              size="sm"
                              variant="outline"
                              className="absolute bottom-0 right-0 rounded-full size-8 p-0"
                            >
                              <Settings className="h-4 w-4" />
                            </Button>
                          </div>
                        </div>

                        {isEditing ? (
                          <>
                            <div className="space-y-2">
                              <Label htmlFor="tendangnhap" className="font-medium text-sm">Tên đăng nhập</Label>
                              <Input
                                id="tendangnhap"
                                name="tendangnhap"
                                value={formData.tendangnhap}
                                onChange={handleInputChange}
                                className="w-full"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label htmlFor="email" className="font-medium text-sm">Email</Label>
                              <Input
                                id="email"
                                name="email"
                                value={formData.email}
                                onChange={handleInputChange}
                                className="w-full"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label htmlFor="englishlevel" className="font-medium text-sm">Cấp độ</Label>
                              <Input
                                id="englishlevel"
                                name="englishlevel"
                                value={formData.englishlevel}
                                onChange={handleInputChange}
                                className="w-full"
                              />
                            </div>
                            <div className="space-y-2">
                              <Label htmlFor="password" className="font-medium text-sm">Mật khẩu</Label>
                              <Input
                                id="password"
                                name="password"
                                type="password"
                                value={formData.password}
                                onChange={handleInputChange}
                                className="w-full"
                                placeholder="Nhập mật khẩu mới"
                              />
                            </div>
                            <div className="space-y-2">
                              <h4 className="font-medium text-sm">Ngày tham gia</h4>
                              <p className="text-muted-foreground">
                                {user?.ngaytaotaikhoan ? new Date(user.ngaytaotaikhoan).toLocaleDateString() : 'Chưa có'}
                              </p>
                            </div>
                          </>
                        ) : (
                          <>
                            <div className="space-y-2">
                              <h4 className="font-medium text-sm">Tên đăng nhập</h4>
                              <p className="text-muted-foreground">{user?.tendangnhap || 'Chưa đăng nhập'}</p>
                            </div>
                            <div className="space-y-2">
                              <h4 className="font-medium text-sm">Email</h4>
                              <p className="text-muted-foreground">{user?.email || 'Chưa có'}</p>
                            </div>
                            <div className="space-y-2">
                              <h4 className="font-medium text-sm">Cấp độ</h4>
                              <p className="text-muted-foreground">{user?.englishlevel || 'Chưa có'}</p>
                            </div>
                            <div className="space-y-2">
                              <h4 className="font-medium text-sm">Ngày tham gia</h4>
                              <p className="text-muted-foreground">
                                {user?.ngaytaotaikhoan ? new Date(user.ngaytaotaikhoan).toLocaleDateString() : 'Chưa có'}
                              </p>
                            </div>
                          </>
                        )}
                      </div>
                      <DialogFooter>
                        {isEditing ? (
                          <>
                            <Button variant="outline" onClick={() => setIsEditing(false)}>
                              Hủy
                            </Button>
                            <Button onClick={handleSave}>Lưu thay đổi</Button>
                          </>
                        ) : (
                          <Button variant="outline" onClick={handleEditToggle}>
                            Chỉnh sửa
                          </Button>
                        )}
                      </DialogFooter>
                    </DialogContent>
                  </Dialog>

                  <Sheet>
                    <SheetTrigger asChild>
                      <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                        <Settings className="mr-2 h-4 w-4" />
                        <span>Cài đặt</span>
                      </DropdownMenuItem>
                    </SheetTrigger>
                    <SheetContent>
                      <SheetHeader>
                        <SheetTitle>Cài đặt</SheetTitle>
                        <SheetDescription>
                          Tùy chỉnh các thiết lập cho ứng dụng
                        </SheetDescription>
                      </SheetHeader>
                      <div className="py-4 space-y-6">
                        <div className="space-y-4">
                          <h3 className="font-medium">Giao diện</h3>
                          <div className="flex items-center justify-between">
                            <span className="text-sm">Chế độ tối</span>
                            <Button variant="outline" size="sm" onClick={toggleTheme}>
                              {theme === 'dark' ? <Sun className="h-4 w-4 mr-1" /> : <Moon className="h-4 w-4 mr-1" />}
                              {theme === 'dark' ? 'Sáng' : 'Tối'}
                            </Button>
                          </div>
                        </div>
                      </div>
                      <SheetFooter>
                        <Button variant="outline">Hủy</Button>
                        <Button>Lưu thay đổi</Button>
                      </SheetFooter>
                    </SheetContent>
                  </Sheet>

                  {/* Admin access - chỉ hiển thị cho admin */}
                  {(user.email === 'admin@example.com' || 
                    user.tendangnhap === 'Admin' ||
                    user.email?.includes('admin') ||
                    user.id === 1) && (
                    <>
                      <DropdownMenuSeparator />
                      <DropdownMenuItem onClick={() => navigate('/admin')}>
                        <Settings className="mr-2 h-4 w-4" />
                        <span>Quản trị hệ thống</span>
                      </DropdownMenuItem>
                    </>
                  )}

                  <DropdownMenuSeparator />
                  <DropdownMenuItem onClick={handleLogout}>
                    <LogOut className="mr-2 h-4 w-4" />
                    <span>Đăng xuất</span>
                  </DropdownMenuItem>
                </>
              ) : (
                <>
                  <DropdownMenuItem onClick={() => navigate('/login')}>
                    <User className="mr-2 h-4 w-4" />
                    <span>Đăng nhập</span>
                  </DropdownMenuItem>
                  <DropdownMenuItem onClick={() => navigate('/register')}>
                    <UserCircle className="mr-2 h-4 w-4" />
                    <span>Đăng ký</span>
                  </DropdownMenuItem>
                  <DropdownMenuSeparator />
                  <Sheet>
                    <SheetTrigger asChild>
                      <DropdownMenuItem onSelect={(e) => e.preventDefault()}>
                        <Settings className="mr-2 h-4 w-4" />
                        <span>Cài đặt</span>
                      </DropdownMenuItem>
                    </SheetTrigger>
                    <SheetContent>
                      <SheetHeader>
                        <SheetTitle>Cài đặt</SheetTitle>
                        <SheetDescription>
                          Tùy chỉnh các thiết lập cho ứng dụng
                        </SheetDescription>
                      </SheetHeader>
                      <div className="py-4 space-y-6">
                        <div className="space-y-4">
                          <h3 className="font-medium">Giao diện</h3>
                          <div className="flex items-center justify-between">
                            <span className="text-sm">Chế độ tối</span>
                            <Button variant="outline" size="sm" onClick={toggleTheme}>
                              {theme === 'dark' ? <Sun className="h-4 w-4 mr-1" /> : <Moon className="h-4 w-4 mr-1" />}
                              {theme === 'dark' ? 'Sáng' : 'Tối'}
                            </Button>
                          </div>
                        </div>
                      </div>
                      <SheetFooter>
                        <Button variant="outline">Hủy</Button>
                        <Button>Lưu thay đổi</Button>
                      </SheetFooter>
                    </SheetContent>
                  </Sheet>
                </>
              )}
            </DropdownMenuContent>
          </DropdownMenu>
        </div>

        <div className="md:hidden flex items-center gap-4">
          {navItems.map((item) => (
            <Link
              key={item.path}
              to={item.path}
              className={cn(
                "flex items-center justify-center",
                location.pathname === item.path ? item.color : "text-muted-foreground"
              )}
            >
              <item.icon className="w-5 h-5" />
            </Link>
          ))}
        </div>
      </div>
    </header>
  );
};

export default Navbar;