import React, { useEffect, useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Separator } from "@/components/ui/separator";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { 
  Loader2, 
  User as UserIcon, 
  Mail, 
  Phone, 
  Shield, 
  GraduationCap, 
  Users,
  CheckCircle2,
  XCircle,
  Ban as BanIcon,
  Calendar
} from 'lucide-react';
import userService, { UserProfile } from '@/services/userService';

interface UserProfileDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  userId: number;
}

export const UserProfileDialog: React.FC<UserProfileDialogProps> = ({
  open,
  onOpenChange,
  userId,
}) => {
  const [user, setUser] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchUserProfile = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await userService.getUserProfile(userId);
      setUser(data);
    } catch (err) {
      console.error('Error fetching user profile:', err);
      setError('Không thể tải thông tin người dùng. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (open && userId) {
      fetchUserProfile();
    }
  }, [open, userId]); // eslint-disable-line react-hooks/exhaustive-deps

  const getInitials = (username: string) => {
    const words = username.split(' ');
    if (words.length >= 2) {
      return words[0][0] + words[words.length - 1][0];
    }
    return username.substring(0, 2).toUpperCase();
  };

  const getRoleInfo = (role: string) => {
    const roleMap: Record<string, { label: string; className: string; icon: typeof Shield; description: string }> = {
      'admin': { 
        label: 'Quản trị viên', 
        className: 'bg-purple-100 text-purple-800 dark:bg-purple-900/30 dark:text-purple-400',
        icon: Shield,
        description: 'Có toàn quyền quản lý hệ thống'
      },
      'teacher': { 
        label: 'Giáo viên', 
        className: 'bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-400',
        icon: GraduationCap,
        description: 'Có thể tạo và quản lý nội dung học tập'
      },
      'student': { 
        label: 'Học viên', 
        className: 'bg-orange-100 text-orange-800 dark:bg-orange-900/30 dark:text-orange-400',
        icon: Users,
        description: 'Người dùng học tập trên hệ thống'
      }
    };
    
    return roleMap[role] || roleMap['student'];
  };

  const getStatusInfo = (status: string) => {
    const statusMap: Record<string, { 
      label: string; 
      className: string; 
      icon: typeof CheckCircle2;
      description: string;
    }> = {
      'active': { 
        label: 'Đang hoạt động', 
        className: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400',
        icon: CheckCircle2,
        description: 'Tài khoản có thể đăng nhập và sử dụng bình thường'
      },
      'inactive': { 
        label: 'Tạm khóa', 
        className: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400',
        icon: XCircle,
        description: 'Tài khoản tạm thời không thể đăng nhập'
      },
      'banned': { 
        label: 'Bị cấm vĩnh viễn', 
        className: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400',
        icon: BanIcon,
        description: 'Tài khoản bị khóa vĩnh viễn và không thể kích hoạt lại'
      }
    };
    
    return statusMap[status] || statusMap['inactive'];
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px] max-h-[85vh] p-0">
        <DialogHeader className="px-6 pt-6 pb-4">
          <DialogTitle className="flex items-center gap-2 text-blue-600 dark:text-blue-400">
            <UserIcon className="h-5 w-5" />
            Thông tin người dùng
          </DialogTitle>
          <DialogDescription>
            Chi tiết thông tin tài khoản và trạng thái
          </DialogDescription>
        </DialogHeader>

        {loading ? (
          <div className="flex items-center justify-center py-12">
            <Loader2 className="h-8 w-8 animate-spin text-blue-500" />
            <span className="ml-2 text-gray-600 dark:text-gray-400">Đang tải thông tin...</span>
          </div>
        ) : error ? (
          <Alert variant="destructive" className="mx-6 my-4">
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        ) : user ? (
          <ScrollArea className="max-h-[calc(85vh-120px)] px-6 pb-6">
            <div className="space-y-4">
            {/* Avatar + Basic Info */}
            <div className="flex items-center gap-3">
              <Avatar className="h-16 w-16 flex-shrink-0">
                <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-500 text-white text-xl">
                  {getInitials(user.FullName || user.Username)}
                </AvatarFallback>
              </Avatar>
              
              <div className="flex-1 min-w-0">
                <h3 className="text-lg font-bold text-gray-900 dark:text-white truncate">
                  {user.FullName || user.Username}
                </h3>
                <p className="text-xs text-gray-500 dark:text-gray-400 mb-2 truncate">
                  @{user.Username}
                </p>
                <Badge className={getRoleInfo(user.Role).className}>
                  {getRoleInfo(user.Role).label}
                </Badge>
              </div>
            </div>

            <Separator />

            {/* Role Section */}
            <div className="space-y-2">
              <h4 className="text-xs font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-1.5">
                <Shield className="h-3.5 w-3.5" />
                Vai trò
              </h4>
              <div className="bg-gray-50 dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
                {(() => {
                  const roleInfo = getRoleInfo(user.Role);
                  const RoleIcon = roleInfo.icon;
                  return (
                    <div className="flex items-center gap-2">
                      <div className="p-1.5 bg-white dark:bg-gray-900 rounded-md flex-shrink-0">
                        <RoleIcon className="h-4 w-4 text-blue-500" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <Badge variant="secondary" className={`${roleInfo.className} text-xs`}>
                          {roleInfo.label}
                        </Badge>
                        <p className="text-xs text-gray-600 dark:text-gray-400 mt-0.5 line-clamp-1">
                          {roleInfo.description}
                        </p>
                      </div>
                    </div>
                  );
                })()}
              </div>
            </div>

            {/* Status Section */}
            <div className="space-y-2">
              <h4 className="text-xs font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-1.5">
                <Calendar className="h-3.5 w-3.5" />
                Trạng thái tài khoản
              </h4>
              <div className="bg-gray-50 dark:bg-gray-800 rounded-lg p-3 border border-gray-200 dark:border-gray-700">
                {(() => {
                  const statusInfo = getStatusInfo(user.Status);
                  const StatusIcon = statusInfo.icon;
                  return (
                    <div className="flex items-center gap-2">
                      <div className="p-1.5 bg-white dark:bg-gray-900 rounded-md flex-shrink-0">
                        <StatusIcon className="h-4 w-4 text-green-500" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <Badge variant="secondary" className={`${statusInfo.className} text-xs`}>
                          {statusInfo.label}
                        </Badge>
                        <p className="text-xs text-gray-600 dark:text-gray-400 mt-0.5 line-clamp-1">
                          {statusInfo.description}
                        </p>
                      </div>
                    </div>
                  );
                })()}
              </div>
            </div>

            <Separator />

            {/* Personal Information */}
            {(user.DOB || user.Address) && (
              <>
                <div className="space-y-2">
                  <h4 className="text-xs font-semibold text-gray-700 dark:text-gray-300">
                    Thông tin cá nhân
                  </h4>
                  
                  {user.DOB && (
                    <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                      <div className="p-1.5 bg-purple-100 dark:bg-purple-900/30 rounded-md flex-shrink-0">
                        <Calendar className="h-3.5 w-3.5 text-purple-600 dark:text-purple-400" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-gray-500 dark:text-gray-400">Ngày sinh</p>
                        <p className="text-xs font-medium text-gray-900 dark:text-white truncate">
                          {new Date(user.DOB).toLocaleDateString('vi-VN', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric'
                          })}
                        </p>
                      </div>
                    </div>
                  )}

                  {user.Address && (
                    <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                      <div className="p-1.5 bg-orange-100 dark:bg-orange-900/30 rounded-md flex-shrink-0">
                        <Mail className="h-3.5 w-3.5 text-orange-600 dark:text-orange-400" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-gray-500 dark:text-gray-400">Địa chỉ</p>
                        <p className="text-xs font-medium text-gray-900 dark:text-white truncate">
                          {user.Address}
                        </p>
                      </div>
                    </div>
                  )}
                </div>
                <Separator className="my-3" />
              </>
            )}

            {/* Learning Information */}
            {(user.PreferredLevel || user.LearningGoal) && (
              <>
                <div className="space-y-2">
                  <h4 className="text-xs font-semibold text-gray-700 dark:text-gray-300 flex items-center gap-1.5">
                    <GraduationCap className="h-3.5 w-3.5" />
                    Thông tin học tập
                  </h4>
                  
                  {user.PreferredLevel && user.PreferredLevel !== '-' && (
                    <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                      <div className="p-1.5 bg-blue-100 dark:bg-blue-900/30 rounded-md flex-shrink-0">
                        <Shield className="h-3.5 w-3.5 text-blue-600 dark:text-blue-400" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-gray-500 dark:text-gray-400">Trình độ</p>
                        <p className="text-xs font-medium text-gray-900 dark:text-white truncate">
                          {user.PreferredLevel}
                        </p>
                      </div>
                    </div>
                  )}

                  {user.LearningGoal && (
                    <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                      <div className="p-1.5 bg-green-100 dark:bg-green-900/30 rounded-md flex-shrink-0">
                        <Users className="h-3.5 w-3.5 text-green-600 dark:text-green-400" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-gray-500 dark:text-gray-400">Mục tiêu học tập</p>
                        <p className="text-xs font-medium text-gray-900 dark:text-white line-clamp-2">
                          {user.LearningGoal}
                        </p>
                      </div>
                    </div>
                  )}
                </div>
                <Separator className="my-3" />
              </>
            )}

            {/* Contact Information */}
            <div className="space-y-2">
              <h4 className="text-xs font-semibold text-gray-700 dark:text-gray-300">
                Thông tin liên hệ
              </h4>
              
              {/* Email */}
              <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                <div className="p-1.5 bg-blue-100 dark:bg-blue-900/30 rounded-md flex-shrink-0">
                  <Mail className="h-3.5 w-3.5 text-blue-600 dark:text-blue-400" />
                </div>
                <div className="flex-1 min-w-0">
                  <p className="text-xs text-gray-500 dark:text-gray-400">Email</p>
                  <p className="text-xs font-medium text-gray-900 dark:text-white truncate">
                    {user.Email}
                  </p>
                </div>
              </div>

              {/* Phone */}
              {user.Phone ? (
                <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                  <div className="p-1.5 bg-green-100 dark:bg-green-900/30 rounded-md flex-shrink-0">
                    <Phone className="h-3.5 w-3.5 text-green-600 dark:text-green-400" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs text-gray-500 dark:text-gray-400">Số điện thoại</p>
                    <p className="text-xs font-medium text-gray-900 dark:text-white truncate">
                      {user.Phone}
                    </p>
                  </div>
                </div>
              ) : (
                <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg border border-dashed border-gray-300 dark:border-gray-600">
                  <div className="p-1.5 bg-gray-200 dark:bg-gray-700 rounded-md flex-shrink-0">
                    <Phone className="h-3.5 w-3.5 text-gray-400" />
                  </div>
                  <div className="flex-1 min-w-0">
                    <p className="text-xs text-gray-500 dark:text-gray-400">Số điện thoại</p>
                    <p className="text-xs text-gray-400 dark:text-gray-500 italic">
                      Chưa cập nhật
                    </p>
                  </div>
                </div>
              )}
            </div>

            {/* Account Information */}
            {(user.CreatedAt || user.Timezone) && (
              <>
                <Separator className="my-3" />
                <div className="space-y-2">
                  <h4 className="text-xs font-semibold text-gray-700 dark:text-gray-300">
                    Thông tin tài khoản
                  </h4>
                  
                  {user.CreatedAt && (
                    <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                      <div className="p-1.5 bg-indigo-100 dark:bg-indigo-900/30 rounded-md flex-shrink-0">
                        <Calendar className="h-3.5 w-3.5 text-indigo-600 dark:text-indigo-400" />
                      </div>
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-gray-500 dark:text-gray-400">Ngày tạo tài khoản</p>
                        <p className="text-xs font-medium text-gray-900 dark:text-white truncate">
                          {new Date(user.CreatedAt).toLocaleDateString('vi-VN', {
                            year: 'numeric',
                            month: 'long',
                            day: 'numeric',
                            hour: '2-digit',
                            minute: '2-digit'
                          })}
                        </p>
                      </div>
                    </div>
                  )}

                  {user.Timezone && (
                    <div className="flex items-center gap-2 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                      <div className="flex-1 min-w-0">
                        <p className="text-xs text-gray-500 dark:text-gray-400">Múi giờ</p>
                        <p className="text-xs font-medium text-gray-900 dark:text-white truncate">
                          {user.Timezone}
                        </p>
                      </div>
                    </div>
                  )}

                  <div className="grid grid-cols-2 gap-2">
                    <div className="flex items-center gap-1.5 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                      <div className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${user.EmailNotify ? 'bg-green-500' : 'bg-gray-400'}`}></div>
                      <span className="text-xs text-gray-600 dark:text-gray-400 truncate">
                        {user.EmailNotify ? 'Nhận email' : 'Tắt email'}
                      </span>
                    </div>
                    <div className="flex items-center gap-1.5 p-2 bg-gray-50 dark:bg-gray-800 rounded-lg">
                      <div className={`w-1.5 h-1.5 rounded-full flex-shrink-0 ${user.DarkMode ? 'bg-blue-500' : 'bg-gray-400'}`}></div>
                      <span className="text-xs text-gray-600 dark:text-gray-400 truncate">
                        {user.DarkMode ? 'Dark mode' : 'Light mode'}
                      </span>
                    </div>
                  </div>
                </div>
              </>
            )}
            </div>
          </ScrollArea>
        ) : null}
      </DialogContent>
    </Dialog>
  );
};
