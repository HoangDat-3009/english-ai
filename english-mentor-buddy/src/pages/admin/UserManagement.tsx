// 👥 USER MANAGEMENT PAGE - Quản lý toàn diện người dùng như bảng Excel
// ✅ READY FOR GIT: Hệ thống quản lý user tổng hợp với Progress/Leaderboard sync
// 🔄 TODO BACKEND: Tích hợp với .NET API cho user analytics
// 📊 Features: Excel-like table, sorting, filtering, progress tracking, skill analytics
// 🎯 Business Impact: Quản lý tập trung, theo dõi chi tiết từng user

import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle } from "@/components/ui/dialog";
import { Input } from "@/components/ui/input";
import { Label } from "@/components/ui/label";
import { Progress } from "@/components/ui/progress";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "@/components/ui/select";
import { Table, TableBody, TableCell, TableHead, TableHeader, TableRow } from "@/components/ui/table";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "@/components/ui/tabs";
import { Textarea } from "@/components/ui/textarea";
import {
  AlertTriangle,
  ArrowDown,
  ArrowUp,
  ArrowUpDown,
  Award,
  BarChart3,
  BookOpen,
  Calendar,
  CheckCircle,
  Download,
  Edit,
  Eye,
  Filter,
  Plus,
  RefreshCw,
  Search,
  Target,
  Trash2,
  TrendingUp,
  Trophy,
  Users
} from 'lucide-react';
import { useMemo, useState } from 'react';
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts';
import { useAddAchievement, useAdminUsers, useAdminUserStats, useCreateUser, useDeleteUser, useUpdateUserProgress } from '../../hooks/useAdminProgress';
import { AdminUser } from '../../services/adminService';

type SortField = 'username' | 'totalXp' | 'averageScore' | 'exercisesCompleted' | 'level' | 'streakDays' | 'lastActivity';
type SortDirection = 'asc' | 'desc';

const UserManagement = () => {
  // State management
  const [searchQuery, setSearchQuery] = useState('');
  const [levelFilter, setLevelFilter] = useState<string>('all');
  const [statusFilter, setStatusFilter] = useState<string>('all');
  const [sortField, setSortField] = useState<SortField>('totalXp');
  const [sortDirection, setSortDirection] = useState<SortDirection>('desc');
  const [selectedUser, setSelectedUser] = useState<AdminUser | null>(null);
  const [isDetailDialogOpen, setIsDetailDialogOpen] = useState(false);
  const [isEditDialogOpen, setIsEditDialogOpen] = useState(false);
  const [isAddDialogOpen, setIsAddDialogOpen] = useState(false);

  // Hooks
  const { data: users, isLoading, refetch } = useAdminUsers();
  const { data: stats } = useAdminUserStats();
  const updateUserMutation = useUpdateUserProgress();
  const addAchievementMutation = useAddAchievement();
  const createUserMutation = useCreateUser();
  const deleteUserMutation = useDeleteUser();

  // Form states
  const [editForm, setEditForm] = useState({
    listening: 0,
    speaking: 0,
    reading: 0,
    writing: 0,
    studyStreak: 0,
    level: '',
    achievements: ''
  });

  const [newUserForm, setNewUserForm] = useState({
    username: '',
    email: '',
    level: 'Beginner',
    listening: 0,
    speaking: 0,
    reading: 0,
    writing: 0
  });

  // Computed values
  const filteredAndSortedUsers = useMemo(() => {
    if (!users) return [];

    const filtered = users.filter(user => {
      const matchesSearch = user.username.toLowerCase().includes(searchQuery.toLowerCase()) ||
                           user.email.toLowerCase().includes(searchQuery.toLowerCase());
      const matchesLevel = levelFilter === 'all' || user.level === parseInt(levelFilter);
      
      // Status filter based on last activity
      let matchesStatus = true;
      if (statusFilter !== 'all') {
        const lastActiveDate = new Date(user.lastActivity);
        const now = new Date();
        const daysDiff = Math.floor((now.getTime() - lastActiveDate.getTime()) / (1000 * 60 * 60 * 24));
        
        if (statusFilter === 'active' && daysDiff > 7) matchesStatus = false;
        if (statusFilter === 'inactive' && daysDiff <= 7) matchesStatus = false;
      }
      
      return matchesSearch && matchesLevel && matchesStatus;
    });

    // Sort data
    filtered.sort((a, b) => {
      let valueA: string | number = a[sortField] as string | number;
      let valueB: string | number = b[sortField] as string | number;

      // Handle undefined values
      if (valueA === undefined || valueA === null) valueA = '';
      if (valueB === undefined || valueB === null) valueB = '';

      // Handle date fields
      if (sortField === 'lastActivity') { // Fix: use lastActivity instead of lastActive
        valueA = new Date(valueA || new Date()).getTime();
        valueB = new Date(valueB || new Date()).getTime();
      }

      // Handle numeric fields
      if (typeof valueA === 'number' && typeof valueB === 'number') {
        return sortDirection === 'asc' ? valueA - valueB : valueB - valueA;
      }

      // Handle string fields - ensure values are not undefined
      const strA = (valueA || '').toString();
      const strB = (valueB || '').toString();
      const comparison = strA.localeCompare(strB);
      return sortDirection === 'asc' ? comparison : -comparison;
    });

    return filtered;
  }, [users, searchQuery, levelFilter, statusFilter, sortField, sortDirection]);

  // Event handlers
  const handleSort = (field: SortField) => {
    if (sortField === field) {
      setSortDirection(sortDirection === 'asc' ? 'desc' : 'asc');
    } else {
      setSortField(field);
      setSortDirection('desc');
    }
  };

  const handleEditUser = (user: AdminUser) => {
    setSelectedUser(user);
    setEditForm({
      listening: 0, // Reading-only focus
      speaking: 0,
      reading: user.averageScore || 0,
      writing: 0,
      studyStreak: user.streakDays,
      level: user.level.toString(),
      achievements: user.achievements.join(', ')
    });
    setIsEditDialogOpen(true);
  };

  const handleViewDetails = (user: AdminUser) => {
    setSelectedUser(user);
    setIsDetailDialogOpen(true);
  };

  const handleUpdateUser = async () => {
    if (!selectedUser) return;

    try {
      await updateUserMutation.mutateAsync({
        userId: selectedUser.id,
        userData: {
          fullName: selectedUser.fullName,
          email: selectedUser.email,
          level: parseInt(editForm.level) || selectedUser.level,
        }
      });
      setIsEditDialogOpen(false);
      refetch();
    } catch (error) {
      console.error('Error updating user:', error);
    }
  };

  const handleAddUser = async () => {
    try {
      await createUserMutation.mutateAsync({
        username: newUserForm.username,
        fullName: newUserForm.username, // Using username as fullName for now
        email: newUserForm.email,
        level: parseInt(newUserForm.level) || 1
      });
      setIsAddDialogOpen(false);
      setNewUserForm({
        username: '',
        email: '',
        level: 'Beginner',
        listening: 0,
        speaking: 0,
        reading: 0,
        writing: 0
      });
      refetch();
    } catch (error) {
      console.error('Error adding user:', error);
    }
  };

  const handleDeleteUser = async (userId: string) => {
    if (!confirm('Bạn có chắc chắn muốn xóa người dùng này?')) return;
    
    try {
      await deleteUserMutation.mutateAsync(userId);
      refetch();
    } catch (error) {
      console.error('Error deleting user:', error);
    }
  };

  // Helper functions
  const getStatusBadge = (user: AdminUser) => {
    const lastActive = new Date(user.lastActivity);
    const now = new Date();
    const daysDiff = Math.floor((now.getTime() - lastActive.getTime()) / (1000 * 60 * 60 * 24));

    if (daysDiff <= 1) {
      return <Badge variant="default" className="bg-green-100 text-green-800">Hoạt động</Badge>;
    } else if (daysDiff <= 7) {
      return <Badge variant="secondary" className="bg-yellow-100 text-yellow-800">Gần đây</Badge>;
    } else {
      return <Badge variant="outline" className="bg-gray-100 text-gray-600">Không hoạt động</Badge>;
    }
  };

  const getCompletionRate = (user: AdminUser) => {
    // Reading exercises completion rate based on exercisesCompleted
    const totalExercises = 100; // Assume 100 total reading exercises
    return Math.round((user.exercisesCompleted / totalExercises) * 100);
  };

  const getAverageScore = (user: AdminUser) => {
    return Math.round(user.averageScore); // Reading score only
  };

  const getSortIcon = (field: SortField) => {
    if (sortField !== field) return <ArrowUpDown className="h-4 w-4" />;
    return sortDirection === 'asc' ? <ArrowUp className="h-4 w-4" /> : <ArrowDown className="h-4 w-4" />;
  };

  const levels = ['Beginner', 'Elementary', 'Intermediate', 'Upper-Intermediate', 'Advanced', 'Proficient'];

  if (isLoading) {
    return (
      <div className="flex items-center justify-center h-64">
        <div className="text-center">
          <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-blue-600 mx-auto"></div>
          <p className="mt-2 text-gray-600">Đang tải dữ liệu người dùng...</p>
        </div>
      </div>
    );
  }
  return (
    <div className="space-y-6">
      {/* Header với Statistics */}
      <div className="flex flex-col gap-4">
        <div className="flex items-center justify-between">
          <div>
            <h1 className="text-3xl font-bold text-gray-900">📊 Quản lý người dùng</h1>
            <p className="text-gray-600 mt-1">Bảng quản lý tổng hợp như Excel - Theo dõi chi tiết tiến độ từng học viên</p>
          </div>
          <div className="flex gap-2">
            <Button variant="outline" onClick={() => refetch()}>
              <RefreshCw className="h-4 w-4 mr-2" />
              Làm mới
            </Button>
            <Button onClick={() => setIsAddDialogOpen(true)}>
              <Plus className="h-4 w-4 mr-2" />
              Thêm học viên
            </Button>
          </div>
        </div>

        {/* Quick Stats */}
        {stats && (
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card>
              <CardContent className="p-4">
                <div className="flex items-center gap-3">
                  <Users className="h-8 w-8 text-blue-600" />
                  <div>
                    <p className="text-2xl font-bold">{stats.totalUsers}</p>
                    <p className="text-sm text-gray-600">Tổng học viên</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="p-4">
                <div className="flex items-center gap-3">
                  <CheckCircle className="h-8 w-8 text-green-600" />
                  <div>
                    <p className="text-2xl font-bold text-green-600">{stats?.activeUsers || 0}</p>
                    <p className="text-sm text-gray-600">Hoạt động hôm nay</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="p-4">
                <div className="flex items-center gap-3">
                  <Target className="h-8 w-8 text-purple-600" />
                  <div>
                    <p className="text-2xl font-bold text-purple-600">{stats.averageScore}</p>
                    <p className="text-sm text-gray-600">Điểm TB</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="p-4">
                <div className="flex items-center gap-3">
                  <TrendingUp className="h-8 w-8 text-orange-600" />
                  <div>
                    <p className="text-2xl font-bold text-orange-600">{stats?.weeklyNewUsers || 0}</p>
                    <p className="text-sm text-gray-600">Hoạt động tuần</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        )}
      </div>

      {/* Filters và Search */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center gap-2">
            <Filter className="h-5 w-5" />
            Bộ lọc và tìm kiếm
          </CardTitle>
        </CardHeader>
        <CardContent>
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <div className="relative">
              <Search className="absolute left-3 top-3 h-4 w-4 text-gray-400" />
              <Input
                placeholder="Tìm theo tên hoặc email..."
                value={searchQuery}
                onChange={(e) => setSearchQuery(e.target.value)}
                className="pl-10"
              />
            </div>

            <Select value={levelFilter} onValueChange={setLevelFilter}>
              <SelectTrigger>
                <SelectValue placeholder="Trình độ" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả trình độ</SelectItem>
                {levels.map(level => (
                  <SelectItem key={level} value={level}>{level}</SelectItem>
                ))}
              </SelectContent>
            </Select>

            <Select value={statusFilter} onValueChange={setStatusFilter}>
              <SelectTrigger>
                <SelectValue placeholder="Trạng thái" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">Tất cả trạng thái</SelectItem>
                <SelectItem value="active">Đang hoạt động</SelectItem>
                <SelectItem value="inactive">Không hoạt động</SelectItem>
              </SelectContent>
            </Select>

            <Button variant="outline">
              <Download className="h-4 w-4 mr-2" />
              Xuất Excel
            </Button>
          </div>
        </CardContent>
      </Card>

      {/* Main Data Table */}
      <Card>
        <CardHeader>
          <CardTitle className="flex items-center justify-between">
            <span className="flex items-center gap-2">
              <BarChart3 className="h-5 w-5" />
              Bảng dữ liệu tổng hợp ({filteredAndSortedUsers.length} học viên)
            </span>
          </CardTitle>
          <CardDescription>
            Bảng quản lý như Excel - Click vào tiêu đề cột để sắp xếp
          </CardDescription>
        </CardHeader>
        <CardContent>
          <div className="overflow-x-auto">
            <Table>
              <TableHeader>
                <TableRow>
                  <TableHead className="w-[200px]">
                    <Button variant="ghost" onClick={() => handleSort('username')} className="h-auto p-0 font-semibold">
                      Học viên {getSortIcon('username')}
                    </Button>
                  </TableHead>
                  <TableHead>Trình độ</TableHead>
                  <TableHead>Trạng thái</TableHead>
                  <TableHead className="text-center">
                    <Button variant="ghost" onClick={() => handleSort('totalXp')} className="h-auto p-0 font-semibold">
                      Total XP {getSortIcon('totalXp')}
                    </Button>
                  </TableHead>
                  <TableHead className="text-center">
                    <Button variant="ghost" onClick={() => handleSort('averageScore')} className="h-auto p-0 font-semibold">
                      <BookOpen className="h-4 w-4 mr-1" />Reading {getSortIcon('averageScore')}
                    </Button>
                  </TableHead>
                  <TableHead className="text-center">
                    <Button variant="ghost" onClick={() => handleSort('exercisesCompleted')} className="h-auto p-0 font-semibold">
                      <Target className="h-4 w-4 mr-1" />Exercises {getSortIcon('exercisesCompleted')}
                    </Button>
                  </TableHead>
                  <TableHead className="text-center">
                    <Button variant="ghost" onClick={() => handleSort('level')} className="h-auto p-0 font-semibold">
                      Level {getSortIcon('level')}
                    </Button>
                  </TableHead>
                  <TableHead className="text-center">Tiến độ</TableHead>
                  <TableHead className="text-center">
                    <Button variant="ghost" onClick={() => handleSort('streakDays')} className="h-auto p-0 font-semibold">
                      Chuỗi {getSortIcon('streakDays')}
                    </Button>
                  </TableHead>
                  <TableHead className="text-center">
                    <Button variant="ghost" onClick={() => handleSort('streakDays')} className="h-auto p-0 font-semibold">
                      Streak {getSortIcon('streakDays')}
                    </Button>
                  </TableHead>
                  <TableHead className="text-center">
                    <Button variant="ghost" onClick={() => handleSort('lastActivity')} className="h-auto p-0 font-semibold">
                      Last Active {getSortIcon('lastActivity')}
                    </Button>
                  </TableHead>
                  <TableHead className="text-right">Thao tác</TableHead>
                </TableRow>
              </TableHeader>
              <TableBody>
                {filteredAndSortedUsers.map((user) => (
                  <TableRow key={user.id} className="hover:bg-gray-50">
                    <TableCell>
                      <div>
                        <p className="font-medium">{user.username}</p>
                        <p className="text-sm text-gray-600">{user.email}</p>
                      </div>
                    </TableCell>
                    <TableCell>
                      <Badge variant="secondary">{user.level}</Badge>
                    </TableCell>
                    <TableCell>{getStatusBadge(user)}</TableCell>
                    <TableCell className="text-center">
                      <div className="font-bold text-blue-600">{user.totalXp}</div>
                      <div className="text-xs text-gray-500">TB: {getAverageScore(user)}</div>
                    </TableCell>
                    <TableCell className="text-center font-mono text-sm">{user.averageScore}%</TableCell>
                    <TableCell className="text-center font-mono text-sm">{user.exercisesCompleted}</TableCell>
                    <TableCell className="text-center font-mono text-sm">{user.level}</TableCell>
                    <TableCell className="text-center font-mono text-sm">{user.totalXp}</TableCell>
                    <TableCell className="text-center">
                      <div className="font-semibold">{user.exercisesCompleted}</div>
                      <div className="text-xs text-gray-500">bài</div>
                    </TableCell>
                    <TableCell className="text-center">
                      <div className="w-16 mx-auto">
                        <Progress value={getCompletionRate(user)} className="h-2" />
                        <div className="text-xs text-gray-600 mt-1">{getCompletionRate(user)}%</div>
                      </div>
                    </TableCell>
                    <TableCell className="text-center">
                      <div className="flex items-center justify-center gap-1">
                        <Trophy className="h-4 w-4 text-yellow-500" />
                        <span className="font-semibold">{user.streakDays}</span>
                      </div>
                    </TableCell>
                    <TableCell>
                      <div className="space-y-1">
                        {user.achievements.slice(0, 2).map((achievement, idx) => (
                          <Badge key={idx} variant="outline" className="text-xs block">
                            {achievement}
                          </Badge>
                        ))}
                        {user.achievements.length > 2 && (
                          <div className="text-xs text-gray-500">+{user.achievements.length - 2} khác</div>
                        )}
                      </div>
                    </TableCell>
                    <TableCell className="text-center">
                      <div className="text-sm">
                        {new Date(user.lastActivity).toLocaleDateString('vi-VN')}
                      </div>
                      <div className="text-xs text-gray-500">
                        {Math.floor((new Date().getTime() - new Date(user.lastActivity).getTime()) / (1000 * 60 * 60 * 24))} ngày trước
                      </div>
                    </TableCell>
                    <TableCell className="text-right">
                      <div className="flex justify-end gap-1">
                        <Button variant="ghost" size="sm" onClick={() => handleViewDetails(user)}>
                          <Eye className="h-4 w-4" />
                        </Button>
                        <Button variant="ghost" size="sm" onClick={() => handleEditUser(user)}>
                          <Edit className="h-4 w-4" />
                        </Button>
                        <Button 
                          variant="ghost" 
                          size="sm" 
                          onClick={() => handleDeleteUser(user.id)}
                          className="text-red-600 hover:text-red-700"
                        >
                          <Trash2 className="h-4 w-4" />
                        </Button>
                      </div>
                    </TableCell>
                  </TableRow>
                ))}
              </TableBody>
            </Table>
          </div>

          {filteredAndSortedUsers.length === 0 && (
            <div className="text-center py-8">
              <AlertTriangle className="h-12 w-12 text-gray-400 mx-auto mb-4" />
              <p className="text-gray-600">Không tìm thấy học viên nào phù hợp với bộ lọc</p>
            </div>
          )}
        </CardContent>
      </Card>

      {/* User Detail Dialog */}
      <Dialog open={isDetailDialogOpen} onOpenChange={setIsDetailDialogOpen}>
        <DialogContent className="max-w-4xl">
          <DialogHeader>
            <DialogTitle>Chi tiết học viên: {selectedUser?.username}</DialogTitle>
            <DialogDescription>
              Xem chi tiết tiến độ và lịch sử học tập
            </DialogDescription>
          </DialogHeader>
          {selectedUser && (
            <Tabs defaultValue="overview">
              <TabsList>
                <TabsTrigger value="overview">Tổng quan</TabsTrigger>
                <TabsTrigger value="progress">Biểu đồ tiến độ</TabsTrigger>
                <TabsTrigger value="history">Lịch sử</TabsTrigger>
              </TabsList>

              <TabsContent value="overview" className="space-y-4">
                <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
                  <Card>
                    <CardContent className="p-4 text-center">
                      <BookOpen className="h-8 w-8 text-purple-600 mx-auto mb-2" />
                      <p className="text-2xl font-bold">{selectedUser.averageScore}</p>
                      <p className="text-sm text-gray-600">Reading Score</p>
                      <Progress value={selectedUser.averageScore} className="mt-2" />
                    </CardContent>
                  </Card>

                  <Card>
                    <CardContent className="p-4 text-center">
                      <Target className="h-8 w-8 text-green-600 mx-auto mb-2" />
                      <p className="text-2xl font-bold">{selectedUser.exercisesCompleted}</p>
                      <p className="text-sm text-gray-600">Exercises Completed</p>
                      <Progress value={(selectedUser.exercisesCompleted / 100) * 100} className="mt-2" />
                    </CardContent>
                  </Card>

                  <Card>
                    <CardContent className="p-4 text-center">
                      <Trophy className="h-8 w-8 text-orange-600 mx-auto mb-2" />
                      <p className="text-2xl font-bold">{selectedUser.totalXp}</p>
                      <p className="text-sm text-gray-600">Total XP</p>
                      <Progress value={(selectedUser.totalXp / 10000) * 100} className="mt-2" />
                    </CardContent>
                  </Card>
                </div>

                <div className="grid grid-cols-2 gap-4">
                  <Card>
                    <CardHeader>
                      <CardTitle className="text-lg">Thông tin cơ bản</CardTitle>
                    </CardHeader>
                    <CardContent className="space-y-3">
                      <div className="flex justify-between">
                        <span>Email:</span>
                        <span className="font-medium">{selectedUser.email}</span>
                      </div>
                      <div className="flex justify-between">
                        <span>Trình độ:</span>
                        <Badge>{selectedUser.level}</Badge>
                      </div>
                      <div className="flex justify-between">
                        <span>Reading Score:</span>
                        <span className="font-bold text-blue-600">{selectedUser.averageScore}%</span>
                      </div>
                      <div className="flex justify-between">
                        <span>Exercises Done:</span>
                        <span className="font-medium">{selectedUser.exercisesCompleted}</span>
                      </div>
                      <div className="flex justify-between">
                        <span>Study Streak:</span>
                        <span className="font-medium">{selectedUser.streakDays} days</span>
                      </div>
                    </CardContent>
                  </Card>

                  <Card>
                    <CardHeader>
                      <CardTitle className="text-lg">Thành tích đã đạt</CardTitle>
                    </CardHeader>
                    <CardContent>
                      <div className="space-y-2">
                        {selectedUser.achievements.length > 0 ? (
                          selectedUser.achievements.map((achievement, idx) => (
                            <div key={idx} className="flex items-center gap-2">
                              <Award className="h-4 w-4 text-yellow-500" />
                              <span className="text-sm">{achievement}</span>
                            </div>
                          ))
                        ) : (
                          <p className="text-gray-600 text-sm">Chưa có thành tích nào</p>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                </div>
              </TabsContent>

              <TabsContent value="progress">
                <Card>
                  <CardHeader>
                    <CardTitle>Biểu đồ tiến độ 4 kỹ năng</CardTitle>
                  </CardHeader>
                  <CardContent>
                    <ResponsiveContainer width="100%" height={300}>
                      <BarChart data={[
                        { skill: 'Reading Score', score: selectedUser.averageScore, max: 100 },
                        { skill: 'Exercises', score: selectedUser.exercisesCompleted, max: 100 },
                        { skill: 'Level', score: selectedUser.level, max: 10 },
                        { skill: 'Streak Days', score: selectedUser.streakDays, max: 30 }
                      ]}>
                        <CartesianGrid strokeDasharray="3 3" />
                        <XAxis dataKey="skill" />
                        <YAxis />
                        <Tooltip />
                        <Bar dataKey="score" fill="#3b82f6" />
                      </BarChart>
                    </ResponsiveContainer>
                  </CardContent>
                </Card>
              </TabsContent>

              <TabsContent value="history">
                <Card>
                  <CardHeader>
                    <CardTitle>Lịch sử học tập</CardTitle>
                    <CardDescription>
                      Theo dõi quá trình phát triển của học viên
                    </CardDescription>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      <div className="text-center py-8 text-gray-600">
                        <Calendar className="h-12 w-12 mx-auto mb-4 text-gray-400" />
                        <p>Lịch sử chi tiết sẽ được tích hợp khi có backend</p>
                        <p className="text-sm">Hiện tại hiển thị thông tin tổng hợp từ dữ liệu admin</p>
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </TabsContent>
            </Tabs>
          )}
        </DialogContent>
      </Dialog>

      {/* Edit User Dialog */}
      <Dialog open={isEditDialogOpen} onOpenChange={setIsEditDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Chỉnh sửa: {selectedUser?.username}</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Listening</Label>
                <Input
                  type="number"
                  value={editForm.listening}
                  onChange={(e) => setEditForm(prev => ({ ...prev, listening: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <div>
                <Label>Speaking</Label>
                <Input
                  type="number"
                  value={editForm.speaking}
                  onChange={(e) => setEditForm(prev => ({ ...prev, speaking: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <div>
                <Label>Reading</Label>
                <Input
                  type="number"
                  value={editForm.reading}
                  onChange={(e) => setEditForm(prev => ({ ...prev, reading: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <div>
                <Label>Writing</Label>
                <Input
                  type="number"
                  value={editForm.writing}
                  onChange={(e) => setEditForm(prev => ({ ...prev, writing: parseInt(e.target.value) || 0 }))}
                />
              </div>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Chuỗi học tập (ngày)</Label>
                <Input
                  type="number"
                  value={editForm.studyStreak}
                  onChange={(e) => setEditForm(prev => ({ ...prev, studyStreak: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <div>
                <Label>Trình độ</Label>
                <Select value={editForm.level} onValueChange={(value) => setEditForm(prev => ({ ...prev, level: value }))}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    {levels.map(level => (
                      <SelectItem key={level} value={level}>{level}</SelectItem>
                    ))}
                  </SelectContent>
                </Select>
              </div>
            </div>
            <div>
              <Label>Thêm thành tích (ngăn cách bằng dấu phẩy)</Label>
              <Textarea
                value={editForm.achievements}
                onChange={(e) => setEditForm(prev => ({ ...prev, achievements: e.target.value }))}
                placeholder="Ví dụ: Grammar Expert, Listening Pro"
              />
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setIsEditDialogOpen(false)}>
                Hủy
              </Button>
              <Button onClick={handleUpdateUser}>
                Cập nhật
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>

      {/* Add User Dialog */}
      <Dialog open={isAddDialogOpen} onOpenChange={setIsAddDialogOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Thêm học viên mới</DialogTitle>
          </DialogHeader>
          <div className="space-y-4">
            <div>
              <Label>Tên đăng nhập</Label>
              <Input
                value={newUserForm.username}
                onChange={(e) => setNewUserForm(prev => ({ ...prev, username: e.target.value }))}
                placeholder="Nhập tên đăng nhập"
              />
            </div>
            <div>
              <Label>Email</Label>
              <Input
                type="email"
                value={newUserForm.email}
                onChange={(e) => setNewUserForm(prev => ({ ...prev, email: e.target.value }))}
                placeholder="Nhập email"
              />
            </div>
            <div>
              <Label>Trình độ</Label>
              <Select value={newUserForm.level} onValueChange={(value) => setNewUserForm(prev => ({ ...prev, level: value }))}>
                <SelectTrigger>
                  <SelectValue />
                </SelectTrigger>
                <SelectContent>
                  {levels.map(level => (
                    <SelectItem key={level} value={level}>{level}</SelectItem>
                  ))}
                </SelectContent>
              </Select>
            </div>
            <div className="grid grid-cols-2 gap-4">
              <div>
                <Label>Listening</Label>
                <Input
                  type="number"
                  value={newUserForm.listening}
                  onChange={(e) => setNewUserForm(prev => ({ ...prev, listening: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <div>
                <Label>Speaking</Label>
                <Input
                  type="number"
                  value={newUserForm.speaking}
                  onChange={(e) => setNewUserForm(prev => ({ ...prev, speaking: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <div>
                <Label>Reading</Label>
                <Input
                  type="number"
                  value={newUserForm.reading}
                  onChange={(e) => setNewUserForm(prev => ({ ...prev, reading: parseInt(e.target.value) || 0 }))}
                />
              </div>
              <div>
                <Label>Writing</Label>
                <Input
                  type="number"
                  value={newUserForm.writing}
                  onChange={(e) => setNewUserForm(prev => ({ ...prev, writing: parseInt(e.target.value) || 0 }))}
                />
              </div>
            </div>
            <div className="flex justify-end gap-2">
              <Button variant="outline" onClick={() => setIsAddDialogOpen(false)}>
                Hủy
              </Button>
              <Button onClick={handleAddUser}>
                Thêm học viên
              </Button>
            </div>
          </div>
        </DialogContent>
      </Dialog>
    </div>
  );
};

export default UserManagement;