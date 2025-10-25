import React, { useEffect, useState, useCallback, useRef } from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Input } from "@/components/ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "@/components/ui/select";
import { 
  DropdownMenu,
  DropdownMenuContent,
  DropdownMenuItem,
  DropdownMenuLabel,
  DropdownMenuSeparator,
  DropdownMenuTrigger,
} from "@/components/ui/dropdown-menu";
import { MoreHorizontal, Mail, UserCheck, UserX, AlertCircle, RefreshCw, Users as UsersIcon, Shield, GraduationCap, Ban, History, User as UserIcon, TrendingUp, BookOpen, AlertTriangle, BarChart3, Search, Filter } from 'lucide-react';
import { toast } from 'sonner';
import userService, { User, UserStatistics } from '@/services/userService';
import { StatusReasonDialog } from '@/components/StatusReasonDialog';
import { ConfirmStatusDialog } from '@/components/ConfirmStatusDialog';
import { UserStatusHistoryDialog } from '@/components/UserStatusHistoryDialog';
import { UserProfileDialog } from '@/components/UserProfileDialog';

const UserManagement = () => {
  const [activeTab, setActiveTab] = useState('users');
  const [users, setUsers] = useState<User[]>([]);
  const [allUsers, setAllUsers] = useState<User[]>([]); // Store all users for stats
  const [filteredUsers, setFilteredUsers] = useState<User[]>([]); // Filtered users for search
  const [searchQuery, setSearchQuery] = useState(''); // Search query
  const [statusFilter, setStatusFilter] = useState<string>('all'); // Status filter
  const [statistics, setStatistics] = useState<UserStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [filter, setFilter] = useState<string>('student'); // Default to show students only
  const [updatingUserId, setUpdatingUserId] = useState<number | null>(null);
  
  // Status reason dialog state
  const [reasonDialogOpen, setReasonDialogOpen] = useState(false);
  const [pendingStatusWithReason, setPendingStatusWithReason] = useState<{
    userId: number;
    username: string;
    currentStatus: string;
    newStatus: 'active' | 'inactive' | 'banned';
  } | null>(null);
  
  // User status history dialog state
  const [historyDialogOpen, setHistoryDialogOpen] = useState(false);
  const [selectedUserForHistory, setSelectedUserForHistory] = useState<{
    userId: number;
    username: string;
  } | null>(null);
  
  // User profile dialog state
  const [profileDialogOpen, setProfileDialogOpen] = useState(false);
  const [selectedUserForProfile, setSelectedUserForProfile] = useState<number | null>(null);
  
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
      
      // Fetch statistics
      const stats = await userService.getUserStatistics();
      setStatistics(stats);
      
      let data: User[];
      if (filter === 'all') {
        data = await userService.getAllUsers();
      } else {
        data = await userService.getUsersByRole(filter);
      }
      
      setUsers(data);
      setFilteredUsers(data); // Initialize filtered users
      
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

  // Handle search query change
  useEffect(() => {
    let filtered = users;

    // Apply status filter
    if (statusFilter !== 'all') {
      filtered = filtered.filter(user => user.Status === statusFilter);
    }

    // Apply search query
    if (searchQuery.trim()) {
      const query = searchQuery.toLowerCase();
      filtered = filtered.filter(user => {
        const fullName = (user.FullName || '').toLowerCase();
        const username = user.Username.toLowerCase();
        const userId = user.UserID.toString();
        
        return fullName.includes(query) || 
               username.includes(query) || 
               userId.includes(query);
      });
    }

    setFilteredUsers(filtered);
  }, [searchQuery, statusFilter, users]);

  // Handle initial status change click (show reason dialog for ALL status changes)
  const handleStatusChangeClick = (userId: number, username: string, currentStatus: string, newStatus: 'active' | 'inactive' | 'banned') => {
    if (currentStatus === newStatus) return;

    console.log('handleStatusChangeClick called:', { userId, username, currentStatus, newStatus });

    // All status changes now require a reason
    setPendingStatusWithReason({ userId, username, currentStatus, newStatus });
    setReasonDialogOpen(true);
  };

  // Execute the actual status change after confirmation
  const executeStatusChange = useCallback(async (
    userId: number, 
    username: string, 
    newStatus: string, 
    reasonNote?: string,
    reasonCode?: string
  ) => {
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

      // Use provided reasonCode, fallback to 'OTHER' if not provided
      const finalReasonCode = reasonCode || 'OTHER';
      // TODO: Get changedByUserID from auth context (logged-in admin)
      const changedByUserID = 1; // Temporary hardcode

      await userService.updateUserStatus(
        userId, 
        newStatus, 
        finalReasonCode, 
        reasonNote, 
        changedByUserID
      );

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

      // Refresh statistics after status change
      try {
        const stats = await userService.getUserStatistics();
        setStatistics(stats);
      } catch (statErr) {
        console.error('Error refreshing statistics:', statErr);
      }

      // Success - no toast notification, UI updates automatically
      console.log(`✅ Status updated successfully for user ${userId} to ${newStatus}`, {
        reasonCode: finalReasonCode,
        reasonNote
      });
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
  const handleStatusChangeWithReason = useCallback((reasonCode: string, reasonNote: string) => {
    console.log('handleStatusChangeWithReason called:', { pendingStatusWithReason, reasonCode, reasonNote });
    if (pendingStatusWithReason) {
      executeStatusChange(
        pendingStatusWithReason.userId,
        pendingStatusWithReason.username,
        pendingStatusWithReason.newStatus,
        reasonNote, // Pass reasonNote
        reasonCode  // Pass reasonCode
      );
      setPendingStatusWithReason(null);
      setReasonDialogOpen(false); // Close dialog explicitly
    }
  }, [pendingStatusWithReason, executeStatusChange]);

  // Handle open status history dialog
  const handleOpenHistory = (userId: number, username: string) => {
    setSelectedUserForHistory({ userId, username });
    setHistoryDialogOpen(true);
  };

  // Handle open profile dialog
  const handleOpenProfile = (userId: number) => {
    setSelectedUserForProfile(userId);
    setProfileDialogOpen(true);
  };

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
      <div className="grid grid-cols-1 md:grid-cols-3 gap-6">
        {/* Tổng học viên */}
        <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Tổng học viên</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {loading || !statistics ? '...' : statistics.TotalStudents}
                </p>
              </div>
              <UsersIcon className="h-8 w-8 text-blue-500" />
            </div>
          </CardContent>
        </Card>

        {/* Đang hoạt động */}
        <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Đang hoạt động</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {loading || !statistics ? '...' : statistics.ActiveStudents}
                </p>
              </div>
              <UserCheck className="h-8 w-8 text-green-500" />
            </div>
          </CardContent>
        </Card>

        {/* Mới tháng này */}
        <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardContent className="pt-6">
            <div className="flex items-center justify-between">
              <div>
                <p className="text-sm text-gray-600 dark:text-gray-400">Mới tháng này</p>
                <p className="text-2xl font-bold text-gray-900 dark:text-white">
                  {loading || !statistics ? '...' : statistics.NewThisMonth}
                </p>
              </div>
              <TrendingUp className="h-8 w-8 text-purple-500" />
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Tabs for User List and Statistics */}
      <Tabs value={activeTab} onValueChange={setActiveTab} className="w-full">
        <TabsList className="grid w-full max-w-md grid-cols-2 bg-gray-100 dark:bg-gray-800 p-1">
          <TabsTrigger 
            value="users" 
            className="flex items-center gap-2 data-[state=active]:bg-white data-[state=active]:dark:bg-gray-700 data-[state=active]:shadow-md data-[state=active]:font-semibold"
          >
            <UsersIcon className="h-4 w-4" />
            Danh sách học viên
          </TabsTrigger>
          <TabsTrigger 
            value="statistics" 
            className="flex items-center gap-2 data-[state=active]:bg-white data-[state=active]:dark:bg-gray-700 data-[state=active]:shadow-md data-[state=active]:font-semibold"
          >
            <BarChart3 className="h-4 w-4" />
            Biểu đồ thống kê
          </TabsTrigger>
        </TabsList>

        {/* Tab 1: User List */}
        <TabsContent value="users" className="mt-6">
          <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        <CardHeader>
          <div className="flex items-center gap-4">
            <div className="relative flex-1 max-w-md">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Tìm kiếm theo tên, username hoặc ID..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>
            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger className="w-[180px]">
                <Filter className="h-4 w-4 mr-2" />
                <SelectValue placeholder="Lọc theo trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">
                  <div className="flex items-center gap-2">
                    <UsersIcon className="h-4 w-4" />
                    Tất cả
                  </div>
                </SelectItem>
                <SelectItem value="active">
                  <div className="flex items-center gap-2">
                    <UserCheck className="h-4 w-4 text-green-600" />
                    Hoạt động
                  </div>
                </SelectItem>
                <SelectItem value="inactive">
                  <div className="flex items-center gap-2">
                    <UserX className="h-4 w-4 text-yellow-600" />
                    Tạm khóa
                  </div>
                </SelectItem>
                <SelectItem value="banned">
                  <div className="flex items-center gap-2">
                    <Ban className="h-4 w-4 text-red-600" />
                    Bị cấm
                  </div>
                </SelectItem>
              </SelectContent>
            </Select>
          </div>
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
          ) : filteredUsers.length === 0 ? (
            <div className="text-center py-12">
              <UsersIcon className="mx-auto h-12 w-12 text-gray-400" />
              <p className="mt-2 text-gray-600 dark:text-gray-400">
                {searchQuery ? 'Không tìm thấy người dùng phù hợp' : 'Không có người dùng nào'}
              </p>
            </div>
          ) : (
            <div className="space-y-3">
              {filteredUsers.map((user, index) => (
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
                      <p className="font-semibold text-gray-900 dark:text-white">
                        {user.FullName || user.Username}
                      </p>
                      <p className="text-xs text-gray-500 dark:text-gray-400">@{user.Username}</p>
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

                    {/* More Options Dropdown Menu */}
                    <DropdownMenu>
                      <DropdownMenuTrigger asChild>
                        <Button 
                          variant="outline" 
                          size="sm" 
                          className="rounded-md hover:bg-gray-100 dark:hover:bg-gray-700"
                        >
                          <MoreHorizontal className="h-4 w-4" />
                        </Button>
                      </DropdownMenuTrigger>
                      <DropdownMenuContent align="end" className="w-56">
                        <DropdownMenuLabel>Tùy chọn</DropdownMenuLabel>
                        <DropdownMenuSeparator />
                        
                        <DropdownMenuItem 
                          onClick={() => handleOpenProfile(user.UserID)}
                          className="cursor-pointer"
                        >
                          <UserIcon className="mr-2 h-4 w-4" />
                          <span>Xem thông tin chi tiết</span>
                        </DropdownMenuItem>
                        
                        <DropdownMenuItem 
                          onClick={() => handleOpenHistory(user.UserID, user.Username)}
                          className="cursor-pointer"
                        >
                          <History className="mr-2 h-4 w-4" />
                          <span>Lịch sử thay đổi trạng thái</span>
                        </DropdownMenuItem>
                      </DropdownMenuContent>
                    </DropdownMenu>
                  </div>
                </div>
              ))}
            </div>
          )}
        </CardContent>
      </Card>
        </TabsContent>

        {/* Tab 2: Statistics & Charts */}
        <TabsContent value="statistics" className="mt-6">
          {/* Charts Placeholder */}
          <Card className="rounded-xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader>
              <CardTitle className="text-gray-900 dark:text-white">
                Biểu đồ thống kê
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="flex flex-col items-center justify-center py-12 space-y-4">
                <BarChart3 className="h-16 w-16 text-gray-400" />
                <p className="text-gray-600 dark:text-gray-400 text-center">
                  Biểu đồ thống kê đang được phát triển
                </p>
                <p className="text-sm text-gray-500 dark:text-gray-500 text-center max-w-md">
                  Sẽ hiển thị các biểu đồ về xu hướng tăng trưởng học viên, phân bố trạng thái, và hoạt động học tập
                </p>
              </div>
            </CardContent>
          </Card>
        </TabsContent>
      </Tabs>

      {/* Status Reason Dialog - For all status changes */}
      <StatusReasonDialog
        open={reasonDialogOpen}
        onOpenChange={setReasonDialogOpen}
        onConfirm={handleStatusChangeWithReason}
        username={pendingStatusWithReason?.username || ''}
        newStatus={pendingStatusWithReason?.newStatus || 'active'}
      />

      {/* User Status History Dialog */}
      <UserStatusHistoryDialog
        open={historyDialogOpen}
        onOpenChange={setHistoryDialogOpen}
        userId={selectedUserForHistory?.userId || 0}
        username={selectedUserForHistory?.username || ''}
      />

      {/* User Profile Dialog */}
      <UserProfileDialog
        open={profileDialogOpen}
        onOpenChange={setProfileDialogOpen}
        userId={selectedUserForProfile || 0}
      />
    </div>
  );
};

export default UserManagement;