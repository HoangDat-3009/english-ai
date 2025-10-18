import React, { useEffect, useState, useCallback, useRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { MoreHorizontal, Mail, UserCheck, UserX, AlertCircle, RefreshCw, Users as UsersIcon, Shield, GraduationCap, Ban } from 'lucide-react';
import { toast } from 'sonner';
import userService, { User } from '@/services/userService';
import { StatusReasonDialog } from '@/components/StatusReasonDialog';
import { ConfirmStatusDialog } from '@/components/ConfirmStatusDialog';

const UserManagement = () => {
  const [users, setUsers] = useState<User[]>([]);
  const [allUsers, setAllUsers] = useState<User[]>([]); // Store all users for stats
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<string>('all'); // 'all', 'admin', 'student', 'teacher'
  const [updatingUserId, setUpdatingUserId] = useState<number | null>(null);
  
  // Status reason dialog state
  const [reasonDialogOpen, setReasonDialogOpen] = useState(false);
  const [pendingStatusWithReason, setPendingStatusWithReason] = useState<{
    userId: number;
    username: string;
    currentStatus: string;
    newStatus: 'active' | 'inactive' | 'banned';
  } | null>(null);
  
  // Confirm dialog state (not used anymore, keeping for backward compatibility)
  const [confirmDialogOpen, setConfirmDialogOpen] = useState(false);
  const [pendingStatusChange, setPendingStatusChange] = useState<{
    userId: number;
    username: string;
    currentStatus: string;
    newStatus: string;
  } | null>(null);

  // Prevent duplicate toasts with ref
  const isExecutingRef = useRef(false); // Prevent concurrent executions

  // Fetch users from API
  const fetchUsers = async () => {
    try {
      setLoading(true);
      setError(null);
      
      let data: User[];
      if (filter === 'all') {
        data = await userService.getAllUsers();
      } else {
        data = await userService.getUsersByRole(filter);
      }
      
      setUsers(data);
      
      // Always fetch all users for stats calculation
      if (filter !== 'all') {
        const allData = await userService.getAllUsers();
        setAllUsers(allData);
      } else {
        setAllUsers(data);
      }
    } catch (err) {
      console.error('Error fetching users:', err);
      setError('Không thể tải danh sách người dùng. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUsers();
  }, [filter]); // eslint-disable-line react-hooks/exhaustive-deps

  // Handle initial status change click (show reason dialog for ALL status changes)
  const handleStatusChangeClick = (userId: number, username: string, currentStatus: string, newStatus: 'active' | 'inactive' | 'banned') => {
    if (currentStatus === newStatus) return;

    console.log('handleStatusChangeClick called:', { userId, username, currentStatus, newStatus });

    // All status changes now require a reason
    setPendingStatusWithReason({ userId, username, currentStatus, newStatus });
    setReasonDialogOpen(true);
  };

  // Execute the actual status change after confirmation
  const executeStatusChange = useCallback(async (userId: number, username: string, newStatus: string, reason?: string) => {
    // Prevent concurrent executions
    if (isExecutingRef.current) {
      console.log('Already executing, skipping...');
      return;
    }

    const statusLabels: Record<string, string> = {
      'active': 'Kích hoạt',
      'inactive': 'Tạm khóa',
      'banned': 'Cấm vĩnh viễn'
    };

    try {
      isExecutingRef.current = true;
      setUpdatingUserId(userId);
      setError(null);

      await userService.updateUserStatus(userId, newStatus);

      // Update local state for both filtered users and all users
      setUsers(prevUsers =>
        prevUsers.map(u =>
          u.UserID === userId ? { ...u, Status: newStatus } : u
        )
      );
      
      setAllUsers(prevAllUsers =>
        prevAllUsers.map(u =>
          u.UserID === userId ? { ...u, Status: newStatus } : u
        )
      );

      // Success - no toast notification, UI updates automatically
      console.log(`✅ Status updated successfully for user ${userId} to ${newStatus}${reason ? ` with reason: ${reason}` : ''}`);
    } catch (err) {
      console.error('Error updating user status:', err);
      setError('Không thể cập nhật trạng thái. Vui lòng thử lại sau.');
      
      // Show error toast with unique ID
      toast.error('❌ Không thể cập nhật trạng thái', {
        id: `status-error-${userId}`,
        description: 'Vui lòng thử lại sau hoặc liên hệ quản trị viên.',
        duration: 4000,
      });
    } finally {
      setUpdatingUserId(null);
      isExecutingRef.current = false;
    }
  }, []); // Empty deps - function is stable

  // Handle confirm from confirm dialog
  const handleConfirmStatusChange = useCallback(() => {
    console.log('handleConfirmStatusChange called:', pendingStatusChange);
    if (pendingStatusChange) {
      executeStatusChange(
        pendingStatusChange.userId,
        pendingStatusChange.username,
        pendingStatusChange.newStatus
      );
      setPendingStatusChange(null);
      setConfirmDialogOpen(false); // Close dialog explicitly
    }
  }, [pendingStatusChange, executeStatusChange]);

  // Handle status change with reason
  const handleStatusChangeWithReason = useCallback((reason: string) => {
    console.log('handleStatusChangeWithReason called:', { pendingStatusWithReason, reason });
    if (pendingStatusWithReason) {
      executeStatusChange(
        pendingStatusWithReason.userId,
        pendingStatusWithReason.username,
        pendingStatusWithReason.newStatus,
        reason
      );
      setPendingStatusWithReason(null);
      setReasonDialogOpen(false); // Close dialog explicitly
    }
  }, [pendingStatusWithReason, executeStatusChange]);

  const getStatusBadge = (status: string) => {
    const statusMap: Record<string, { label: string; className: string }> = {
      'active': { label: 'Hoạt động', className: 'bg-green-500 dark:bg-green-600 text-white' },
      'inactive': { label: 'Không hoạt động', className: 'bg-gray-500 dark:bg-gray-600 text-white' },
      'banned': { label: 'Bị cấm', className: 'bg-red-500 dark:bg-red-600 text-white' }
    };
    
    const statusInfo = statusMap[status] || statusMap['inactive'];
    return (
      <Badge variant="default" className={statusInfo.className}>
        {statusInfo.label}
      </Badge>
    );
  };

  const getRoleBadge = (role: string) => {
    const roleMap: Record<string, { label: string; className: string; icon: typeof Shield }> = {
      'admin': { 
        label: 'Quản trị viên', 
        className: 'bg-purple-500 dark:bg-purple-600 text-white',
        icon: Shield
      },
      'teacher': { 
        label: 'Giáo viên', 
        className: 'bg-blue-500 dark:bg-blue-600 text-white',
        icon: GraduationCap
      },
      'student': { 
        label: 'Học viên', 
        className: 'bg-orange-500 dark:bg-orange-600 text-white',
        icon: UsersIcon
      }
    };
    
    const roleInfo = roleMap[role] || roleMap['student'];
    const Icon = roleInfo.icon;
    
    return (
      <Badge variant="secondary" className={roleInfo.className}>
        <Icon className="h-3 w-3 mr-1" />
        {roleInfo.label}
      </Badge>
    );
  };

  const getInitials = (username: string) => {
    const words = username.split(' ');
    if (words.length >= 2) {
      return words[0][0] + words[words.length - 1][0];
    }
    return username.substring(0, 2).toUpperCase();
  };

  // Calculate stats from ALL users, not filtered users
  const userStats = {
    total: allUsers.length,
    admin: allUsers.filter(u => u.Role === 'admin').length,
    teacher: allUsers.filter(u => u.Role === 'teacher').length,
    student: allUsers.filter(u => u.Role === 'student').length,
    active: allUsers.filter(u => u.Status === 'active').length,
  };

  return (
    <div className="space-y-6">
      {/* Error Alert */}
      {error && (
        <Alert variant="destructive" className="rounded-xl">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold text-gray-900 dark:text-white">Quản lý người dùng</h1>
          <p className="text-gray-600 dark:text-gray-400">
            Quản lý tài khoản và thông tin người dùng
          </p>
        </div>
        <div className="flex gap-2">
          <Button onClick={fetchUsers} variant="outline" disabled={loading}>
            <RefreshCw className={`mr-2 h-4 w-4 ${loading ? 'animate-spin' : ''}`} />
            Làm mới
          </Button>
          <Button>
            <Mail className="mr-2 h-4 w-4" />
            Gửi thông báo
          </Button>
        </div>
      </div>

      {/* Statistics Cards */}
      <div className="grid grid-cols-2 md:grid-cols-5 gap-4">
        <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Tổng người dùng</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {loading ? '...' : userStats.total}
                </p>
              </div>
              <UsersIcon className="h-8 w-8 text-blue-500" />
            </div>
          </CardContent>
        </Card>

        <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Quản trị viên</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {loading ? '...' : userStats.admin}
                </p>
              </div>
              <Shield className="h-8 w-8 text-purple-500" />
            </div>
          </CardContent>
        </Card>

        <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Giáo viên</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {loading ? '...' : userStats.teacher}
                </p>
              </div>
              <GraduationCap className="h-8 w-8 text-blue-500" />
            </div>
          </CardContent>
        </Card>

        <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Học viên</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {loading ? '...' : userStats.student}
                </p>
              </div>
              <UsersIcon className="h-8 w-8 text-orange-500" />
            </div>
          </CardContent>
        </Card>

        <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Đang hoạt động</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {loading ? '...' : userStats.active}
                </p>
              </div>
              <UserCheck className="h-8 w-8 text-green-500" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Filter Buttons */}
      <div className="flex gap-2">
        <Button 
          variant={filter === 'all' ? 'default' : 'outline'}
          onClick={() => setFilter('all')}
          className="rounded-xl"
        >
          Tất cả ({userStats.total})
        </Button>
        <Button 
          variant={filter === 'admin' ? 'default' : 'outline'}
          onClick={() => setFilter('admin')}
          className="rounded-xl"
        >
          <Shield className="h-4 w-4 mr-1" />
          Admin ({userStats.admin})
        </Button>
        <Button 
          variant={filter === 'teacher' ? 'default' : 'outline'}
          onClick={() => setFilter('teacher')}
          className="rounded-xl"
        >
          <GraduationCap className="h-4 w-4 mr-1" />
          Giáo viên ({userStats.teacher})
        </Button>
        <Button 
          variant={filter === 'student' ? 'default' : 'outline'}
          onClick={() => setFilter('student')}
          className="rounded-xl"
        >
          <UsersIcon className="h-4 w-4 mr-1" />
          Học viên ({userStats.student})
        </Button>
      </div>

      <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        <CardHeader>
          <CardTitle className="text-gray-900 dark:text-white">
            Danh sách người dùng
          </CardTitle>
        </CardHeader>
        <CardContent>
          {loading ? (
            // Loading skeleton
            <div className="space-y-4">
              {Array.from({ length: 3 }).map((_, i) => (
                <div key={i} className="flex items-center justify-between p-4 border border-gray-200 dark:border-gray-700 rounded-xl animate-pulse">
                  <div className="flex items-center space-x-4">
                    <div className="w-10 h-10 bg-gray-200 dark:bg-gray-700 rounded-full"></div>
                    <div className="space-y-2">
                      <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-32"></div>
                      <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-48"></div>
                    </div>
                  </div>
                </div>
              ))}
            </div>
          ) : users.length === 0 ? (
            <div className="text-center py-12">
              <UsersIcon className="mx-auto h-12 w-12 text-gray-400" />
              <p className="mt-2 text-gray-600 dark:text-gray-400">
                Không có người dùng nào
              </p>
            </div>
          ) : (
            <div className="space-y-3">
              {users.map((user, index) => (
                <div 
                  key={user.UserID} 
                  className="flex items-center justify-between p-4 border border-gray-200 dark:border-gray-700 rounded-xl hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
                >
                  {/* Left side: STT + ID Badge + Avatar + Info */}
                  <div className="flex items-center space-x-4">
                    {/* STT (Số thứ tự) */}
                    <div className="flex-shrink-0 w-8 h-8 flex items-center justify-center bg-blue-100 dark:bg-blue-900/30 text-blue-600 dark:text-blue-400 rounded-full font-semibold text-sm">
                      {index + 1}
                    </div>
                    
                    {/* ID Badge */}
                    <div className="flex-shrink-0">
                      <Badge variant="outline" className="text-xs font-mono px-2 py-1">
                        ID: {user.UserID}
                      </Badge>
                    </div>
                    
                    {/* Avatar */}
                    <Avatar className="h-10 w-10">
                      <AvatarFallback className="bg-gradient-to-br from-blue-500 to-purple-500 text-white">
                        {getInitials(user.Username)}
                      </AvatarFallback>
                    </Avatar>
                    
                    {/* User Info */}
                    <div>
                      <div className="flex items-center gap-2">
                        <p className="font-medium text-gray-900 dark:text-white">{user.Username}</p>
                        {getRoleBadge(user.Role)}
                      </div>
                      <p className="text-sm text-gray-600 dark:text-gray-400">
                        <Mail className="inline h-3 w-3 mr-1" />
                        {user.Email}
                      </p>
                      {user.Phone && (
                        <p className="text-xs text-gray-500 dark:text-gray-500">
                          📞 {user.Phone}
                        </p>
                      )}
                    </div>
                  </div>
                  
                  {/* Right side: Status Badge + Action Buttons */}
                  <div className="flex items-center space-x-3">
                    {/* Current Status Badge */}
                    <div className="mr-2">
                      {getStatusBadge(user.Status)}
                    </div>
                    
                    {/* Status Action Buttons */}
                    <div className="flex items-center gap-1 p-1 bg-gray-100 dark:bg-gray-800 rounded-lg">
                      {/* Active Button */}
                      <Button 
                        variant={user.Status === 'active' ? 'default' : 'ghost'}
                        size="sm" 
                        className={`h-8 px-3 rounded-md ${
                          user.Status === 'active' 
                            ? 'bg-green-500 hover:bg-green-600 text-white' 
                            : 'hover:bg-green-50 dark:hover:bg-green-900/20'
                        } ${user.Status === 'banned' ? 'opacity-50 cursor-not-allowed' : ''}`}
                        title={user.Status === 'banned' ? 'Không thể kích hoạt tài khoản đã bị cấm vĩnh viễn' : 'Kích hoạt tài khoản'}
                        onClick={() => handleStatusChangeClick(user.UserID, user.Username, user.Status, 'active')}
                        disabled={updatingUserId === user.UserID || user.Status === 'banned'}
                      >
                        {updatingUserId === user.UserID && user.Status !== 'active' ? (
                          <RefreshCw className="h-4 w-4 animate-spin" />
                        ) : (
                          <UserCheck className={`h-4 w-4 ${user.Status === 'active' ? '' : 'text-green-600 dark:text-green-400'}`} />
                        )}
                      </Button>

                      {/* Inactive Button */}
                      <Button 
                        variant={user.Status === 'inactive' ? 'default' : 'ghost'}
                        size="sm" 
                        className={`h-8 px-3 rounded-md ${
                          user.Status === 'inactive' 
                            ? 'bg-yellow-500 hover:bg-yellow-600 text-white' 
                            : 'hover:bg-yellow-50 dark:hover:bg-yellow-900/20'
                        } ${user.Status === 'banned' ? 'opacity-50 cursor-not-allowed' : ''}`}
                        title={user.Status === 'banned' ? 'Không thể tạm khóa tài khoản đã bị cấm vĩnh viễn' : 'Tạm khóa tài khoản'}
                        onClick={() => handleStatusChangeClick(user.UserID, user.Username, user.Status, 'inactive')}
                        disabled={updatingUserId === user.UserID || user.Status === 'banned'}
                      >
                        {updatingUserId === user.UserID && user.Status !== 'inactive' ? (
                          <RefreshCw className="h-4 w-4 animate-spin" />
                        ) : (
                          <UserX className={`h-4 w-4 ${user.Status === 'inactive' ? '' : 'text-yellow-600 dark:text-yellow-400'}`} />
                        )}
                      </Button>

                      {/* Banned Button */}
                      <Button 
                        variant={user.Status === 'banned' ? 'default' : 'ghost'}
                        size="sm" 
                        className={`h-8 px-3 rounded-md ${
                          user.Status === 'banned' 
                            ? 'bg-red-500 hover:bg-red-600 text-white' 
                            : 'hover:bg-red-50 dark:hover:bg-red-900/20'
                        }`}
                        title="Cấm vĩnh viễn"
                        onClick={() => handleStatusChangeClick(user.UserID, user.Username, user.Status, 'banned')}
                        disabled={updatingUserId === user.UserID}
                      >
                        {updatingUserId === user.UserID && user.Status !== 'banned' ? (
                          <RefreshCw className="h-4 w-4 animate-spin" />
                        ) : (
                          <Ban className={`h-4 w-4 ${user.Status === 'banned' ? '' : 'text-red-600 dark:text-red-400'}`} />
                        )}
                      </Button>
                    </div>

                    {/* More Options Button */}
                    <Button 
                      variant="outline" 
                      size="sm" 
                      className="rounded-md hover:bg-gray-100 dark:hover:bg-gray-700"
                      title="Thêm tùy chọn"
                    >
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>

      {/* Status Reason Dialog - For all status changes */}
      <StatusReasonDialog
        open={reasonDialogOpen}
        onOpenChange={setReasonDialogOpen}
        onConfirm={handleStatusChangeWithReason}
        username={pendingStatusWithReason?.username || ''}
        newStatus={pendingStatusWithReason?.newStatus || 'active'}
      />
    </div>
  );
};

export default UserManagement;