import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Textarea } from "@/components/ui/textarea";
import { Avatar, AvatarFallback, AvatarImage } from "@/components/ui/avatar";
import { Badge } from "@/components/ui/badge";
import { Separator } from "@/components/ui/separator";
import { 
  Edit, Save, X, Camera, Mail, Phone, Calendar, MapPin, Loader2,
  User as UserIcon, Shield, CheckCircle2, XCircle, AlertCircle, Key, Lock
} from 'lucide-react';
import { useState, useEffect } from 'react';
import { useAuth } from '@/components/AuthContext';
import userService, { UserProfile } from '@/services/userService';

const AccountPage = () => {
  const { user } = useAuth();
  const [isEditing, setIsEditing] = useState(false);
  const [isLoading, setIsLoading] = useState(true);
  const [userProfile, setUserProfile] = useState<UserProfile | null>(null);
  const [formData, setFormData] = useState({
    name: '',
    email: '',
    phone: '',
    address: '',
    bio: '',
    role: '',
    joinDate: ''
  });

  useEffect(() => {
    const fetchUserProfile = async () => {
      if (!user?.userId) {
        setIsLoading(false);
        return;
      }

      try {
        setIsLoading(true);
        const profile = await userService.getUserProfile(user.userId);
        setUserProfile(profile);
        
        setFormData({
          name: profile.FullName || profile.Username || '',
          email: profile.Email || '',
          phone: profile.Phone || '',
          address: profile.Address || '',
          bio: profile.Bio || '',
          role: user.role === 'admin' ? 'Quản trị viên' : 'Người dùng',
          joinDate: profile.CreatedAt ? new Date(profile.CreatedAt).toISOString().split('T')[0] : ''
        });
      } catch (error) {
        console.error('Error fetching user profile:', error);
      } finally {
        setIsLoading(false);
      }
    };

    fetchUserProfile();
  }, [user]);

  const handleInputChange = (e: React.ChangeEvent<HTMLInputElement | HTMLTextAreaElement>) => {
    const { name, value } = e.target;
    setFormData(prev => ({
      ...prev,
      [name]: value
    }));
  };

  const handleSave = async () => {
    // TODO: Implement save logic with API call
    setIsEditing(false);
  };

  const getInitials = (name?: string) => {
    if (!name) return 'A';
    return name
      .split(' ')
      .map(n => n[0])
      .join('')
      .toUpperCase()
      .slice(0, 2);
  };

  const getStatusColor = (status?: string) => {
    switch (status) {
      case 'active':
        return 'bg-green-100 text-green-700 border-green-300 dark:bg-green-900/20 dark:text-green-400 dark:border-green-800';
      case 'banned':
        return 'bg-red-100 text-red-700 border-red-300 dark:bg-red-900/20 dark:text-red-400 dark:border-red-800';
      case 'inactive':
        return 'bg-gray-100 text-gray-700 border-gray-300 dark:bg-gray-900/20 dark:text-gray-400 dark:border-gray-800';
      default:
        return 'bg-gray-100 text-gray-700 border-gray-300 dark:bg-gray-900/20 dark:text-gray-400 dark:border-gray-800';
    }
  };

  const getStatusIcon = (status?: string) => {
    switch (status) {
      case 'active':
        return <CheckCircle2 className="w-3 h-3" />;
      case 'banned':
        return <XCircle className="w-3 h-3" />;
      case 'inactive':
        return <AlertCircle className="w-3 h-3" />;
      default:
        return <AlertCircle className="w-3 h-3" />;
    }
  };

  const getStatusText = (status?: string) => {
    switch (status) {
      case 'active':
        return 'Đang hoạt động';
      case 'banned':
        return 'Bị cấm';
      case 'inactive':
        return 'Không hoạt động';
      default:
        return 'Không xác định';
    }
  };

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-96">
        <Loader2 className="w-8 h-8 animate-spin text-blue-600" />
      </div>
    );
  }

  if (!user) {
    return (
      <div className="flex items-center justify-center h-96">
        <p className="text-gray-600 dark:text-gray-400">Vui lòng đăng nhập để xem thông tin tài khoản</p>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* Header */}
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Tài khoản</h1>
          <p className="text-gray-600 dark:text-gray-400 mt-1">
            Quản lý thông tin cá nhân và cài đặt tài khoản
          </p>
        </div>
        <div className="flex gap-2">
          {isEditing ? (
            <>
              <Button 
                variant="outline" 
                onClick={() => setIsEditing(false)}
              >
                <X className="w-4 h-4 mr-2" />
                Hủy
              </Button>
              <Button 
                onClick={handleSave}
                className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600"
              >
                <Save className="w-4 h-4 mr-2" />
                Lưu thay đổi
              </Button>
            </>
          ) : (
            <Button 
              onClick={() => setIsEditing(true)}
              className="bg-blue-600 hover:bg-blue-700 dark:bg-blue-500 dark:hover:bg-blue-600"
            >
              <Edit className="w-4 h-4 mr-2" />
              Chỉnh sửa
            </Button>
          )}
        </div>
      </div>

      <div className="grid gap-6 lg:grid-cols-3">
        {/* Profile Sidebar */}
        <div className="lg:col-span-1">
          <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardContent className="pt-6">
              <div className="flex flex-col items-center space-y-4">
                {/* Avatar */}
                <div className="relative">
                  <Avatar className="w-24 h-24 border-4 border-blue-100 dark:border-blue-900">
                    {userProfile?.AvatarURL ? (
                      <AvatarImage src={userProfile.AvatarURL} alt={formData.name} />
                    ) : null}
                    <AvatarFallback className="bg-blue-600 dark:bg-blue-500 text-white text-2xl font-bold">
                      {getInitials(formData.name || user.username)}
                    </AvatarFallback>
                  </Avatar>
                  {isEditing && (
                    <Button
                      size="sm"
                      variant="outline"
                      className="absolute -bottom-2 -right-2 w-9 h-9 rounded-full p-0 bg-white dark:bg-gray-800 border-2 border-blue-500"
                    >
                      <Camera className="w-4 h-4 text-blue-600 dark:text-blue-400" />
                    </Button>
                  )}
                </div>
                
                {/* User Info */}
                <div className="text-center space-y-2 w-full">
                  <h3 className="text-xl font-bold text-gray-900 dark:text-white">
                    {formData.name || user.username}
                  </h3>
                  <p className="text-sm text-gray-500 dark:text-gray-400">@{user.username}</p>
                  
                  <div className="flex flex-col gap-2 pt-2">
                    <Badge 
                      variant="outline" 
                      className={getStatusColor(userProfile?.Status)}
                    >
                      {getStatusIcon(userProfile?.Status)}
                      <span className="ml-1">{getStatusText(userProfile?.Status)}</span>
                    </Badge>
                    
                    <Badge className="bg-blue-600 dark:bg-blue-500 text-white border-0">
                      <Shield className="w-3 h-3 mr-1" />
                      {formData.role}
                    </Badge>
                  </div>
                </div>

                <Separator />

                {/* Contact Info */}
                <div className="w-full space-y-3">
                  <div className="flex items-center gap-3 text-sm">
                    <Mail className="w-4 h-4 text-gray-400 dark:text-gray-500 flex-shrink-0" />
                    <span className="text-gray-700 dark:text-gray-300 truncate">
                      {formData.email || 'Chưa cập nhật'}
                    </span>
                  </div>
                  <div className="flex items-center gap-3 text-sm">
                    <Phone className="w-4 h-4 text-gray-400 dark:text-gray-500 flex-shrink-0" />
                    <span className="text-gray-700 dark:text-gray-300">
                      {formData.phone || 'Chưa cập nhật'}
                    </span>
                  </div>
                  <div className="flex items-center gap-3 text-sm">
                    <MapPin className="w-4 h-4 text-gray-400 dark:text-gray-500 flex-shrink-0" />
                    <span className="text-gray-700 dark:text-gray-300">
                      {formData.address || 'Chưa cập nhật'}
                    </span>
                  </div>
                  <div className="flex items-center gap-3 text-sm">
                    <Calendar className="w-4 h-4 text-gray-400 dark:text-gray-500 flex-shrink-0" />
                    <span className="text-gray-700 dark:text-gray-300">
                      {formData.joinDate 
                        ? new Date(formData.joinDate).toLocaleDateString('vi-VN')
                        : 'Chưa có thông tin'}
                    </span>
                  </div>
                </div>
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Main Content */}
        <div className="lg:col-span-2 space-y-6">
          {/* Personal Information */}
          <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader className="border-b border-gray-200 dark:border-gray-700">
              <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                <UserIcon className="w-5 h-5 text-blue-600 dark:text-blue-400" />
                Thông tin cá nhân
              </CardTitle>
            </CardHeader>
            <CardContent className="pt-6 space-y-4">
              <div className="grid gap-4 md:grid-cols-2">
                <div className="space-y-2">
                  <Label htmlFor="name" className="text-gray-700 dark:text-gray-300">Họ và tên</Label>
                  <Input
                    id="name"
                    name="name"
                    value={formData.name}
                    onChange={handleInputChange}
                    disabled={!isEditing}
                    className="bg-gray-50 dark:bg-gray-900 border-gray-300 dark:border-gray-600"
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="email" className="text-gray-700 dark:text-gray-300">Email</Label>
                  <Input
                    id="email"
                    name="email"
                    type="email"
                    value={formData.email}
                    onChange={handleInputChange}
                    disabled={!isEditing}
                    className="bg-gray-50 dark:bg-gray-900 border-gray-300 dark:border-gray-600"
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="phone" className="text-gray-700 dark:text-gray-300">Số điện thoại</Label>
                  <Input
                    id="phone"
                    name="phone"
                    value={formData.phone}
                    onChange={handleInputChange}
                    disabled={!isEditing}
                    placeholder="Nhập số điện thoại"
                    className="bg-gray-50 dark:bg-gray-900 border-gray-300 dark:border-gray-600"
                  />
                </div>
                
                <div className="space-y-2">
                  <Label htmlFor="address" className="text-gray-700 dark:text-gray-300">Địa chỉ</Label>
                  <Input
                    id="address"
                    name="address"
                    value={formData.address}
                    onChange={handleInputChange}
                    disabled={!isEditing}
                    placeholder="Nhập địa chỉ"
                    className="bg-gray-50 dark:bg-gray-900 border-gray-300 dark:border-gray-600"
                  />
                </div>
              </div>
              
              <div className="space-y-2">
                <Label htmlFor="bio" className="text-gray-700 dark:text-gray-300">Giới thiệu bản thân</Label>
                <Textarea
                  id="bio"
                  name="bio"
                  value={formData.bio}
                  onChange={handleInputChange}
                  disabled={!isEditing}
                  rows={4}
                  placeholder="Viết vài dòng về bản thân..."
                  className="bg-gray-50 dark:bg-gray-900 border-gray-300 dark:border-gray-600 resize-none"
                />
              </div>
            </CardContent>
          </Card>

          {/* Account Information */}
          <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader className="border-b border-gray-200 dark:border-gray-700">
              <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                <Shield className="w-5 h-5 text-purple-600 dark:text-purple-400" />
                Thông tin tài khoản
              </CardTitle>
            </CardHeader>
            <CardContent className="pt-6">
              <div className="grid gap-4 md:grid-cols-2">
                <div className="p-4 bg-gray-50 dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-700">
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Tên đăng nhập</p>
                  <p className="text-base font-semibold text-gray-900 dark:text-white">@{user.username}</p>
                </div>
                
                <div className="p-4 bg-gray-50 dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-700">
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Vai trò</p>
                  <Badge className="bg-blue-600 dark:bg-blue-500 text-white">
                    {formData.role}
                  </Badge>
                </div>
                
                <div className="p-4 bg-gray-50 dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-700">
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Trạng thái</p>
                  <Badge variant="outline" className={getStatusColor(userProfile?.Status)}>
                    {getStatusIcon(userProfile?.Status)}
                    <span className="ml-1">{getStatusText(userProfile?.Status)}</span>
                  </Badge>
                </div>
                
                <div className="p-4 bg-gray-50 dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-700">
                  <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">Ngày tham gia</p>
                  <p className="text-base font-semibold text-gray-900 dark:text-white">
                    {formData.joinDate 
                      ? new Date(formData.joinDate).toLocaleDateString('vi-VN')
                      : 'N/A'}
                  </p>
                </div>
              </div>
            </CardContent>
          </Card>

          {/* Security Settings */}
          <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader className="border-b border-gray-200 dark:border-gray-700">
              <CardTitle className="flex items-center gap-2 text-gray-900 dark:text-white">
                <Lock className="w-5 h-5 text-red-600 dark:text-red-400" />
                Bảo mật & Quyền riêng tư
              </CardTitle>
            </CardHeader>
            <CardContent className="pt-6 space-y-3">
              <div className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-700 hover:border-blue-300 dark:hover:border-blue-600 transition-colors">
                <div>
                  <h4 className="font-semibold text-gray-900 dark:text-white">Đổi mật khẩu</h4>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Cập nhật mật khẩu để bảo mật tài khoản
                  </p>
                </div>
                <Button variant="outline">
                  <Key className="w-4 h-4 mr-2" />
                  Đổi mật khẩu
                </Button>
              </div>
              
              <div className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-700 hover:border-blue-300 dark:hover:border-blue-600 transition-colors">
                <div>
                  <h4 className="font-semibold text-gray-900 dark:text-white">Xác thực 2 bước</h4>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Thêm lớp bảo mật cho tài khoản của bạn
                  </p>
                </div>
                <Button variant="outline">
                  <Shield className="w-4 h-4 mr-2" />
                  Kích hoạt
                </Button>
              </div>
              
              <div className="flex items-center justify-between p-4 bg-gray-50 dark:bg-gray-900 rounded-lg border border-gray-200 dark:border-gray-700 hover:border-blue-300 dark:hover:border-blue-600 transition-colors">
                <div>
                  <h4 className="font-semibold text-gray-900 dark:text-white">Phiên đăng nhập</h4>
                  <p className="text-sm text-gray-600 dark:text-gray-400">
                    Quản lý các thiết bị đã đăng nhập
                  </p>
                </div>
                <Button variant="outline">
                  Xem chi tiết
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    </div>
  );
};

export default AccountPage;
