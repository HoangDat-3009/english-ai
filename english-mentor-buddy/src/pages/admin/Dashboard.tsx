import { Badge } from "@/components/ui/badge";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "@/components/ui/card";
import { adminService, RecentActivity, SystemHealth, SystemStatistics, TopUser } from '@/services/adminService';
import { motion } from "framer-motion";
import { Activity, AlertCircle, BookOpen, Target, TrendingUp, Users } from 'lucide-react';
import { useEffect, useState } from 'react';

const AdminDashboard = () => {
  // 🔄 STATE MANAGEMENT for backend data
  const [systemStats, setSystemStats] = useState<SystemStatistics | null>(null);
  const [recentActivities, setRecentActivities] = useState<RecentActivity[]>([]);
  const [topUsers, setTopUsers] = useState<TopUser[]>([]);
  const [systemHealth, setSystemHealth] = useState<SystemHealth | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // 🚀 LOAD DATA from AdminController API
  useEffect(() => {
    const loadDashboardData = async () => {
      try {
        setLoading(true);
        setError(null);

        // Load all dashboard data from AdminController
        const dashboardData = await adminService.getDashboardData();
        
        // Handle different response structures from backend
        if (dashboardData.systemStats) {
          setSystemStats(dashboardData.systemStats);
        }
        
        if (Array.isArray(dashboardData.recentActivities)) {
          setRecentActivities(dashboardData.recentActivities);
        }
        
        if (Array.isArray(dashboardData.topUsers)) {
          setTopUsers(dashboardData.topUsers);
        }
        
        if (dashboardData.systemHealth) {
          setSystemHealth(dashboardData.systemHealth);
        }

      } catch (err) {
        console.error('Failed to load dashboard data:', err);
        setError('Không thể tải dữ liệu dashboard. Đang sử dụng dữ liệu mẫu.');
        
        // Fallback to mock data
        const mockStats = await adminService.getSystemStats();
        const mockActivities = await adminService.getRecentActivities();
        const mockTopUsers = await adminService.getTopUsers();
        const mockHealth = await adminService.getSystemHealth();
        
        setSystemStats(mockStats);
        setRecentActivities(mockActivities);
        setTopUsers(mockTopUsers);
        setSystemHealth(mockHealth);
      } finally {
        setLoading(false);
      }
    };

    loadDashboardData();
  }, []);

  // 📊 COMPUTED STATISTICS for display
  const stats = (systemStats && (systemStats.totalUsers || systemStats.TotalUsers)) ? [
    { 
      label: "Tổng người dùng", 
      value: (systemStats.totalUsers || systemStats.TotalUsers || 0).toLocaleString(), 
      note: `+${systemStats.weeklyNewUsers || systemStats.ActiveUsersThisWeek || 0} tuần này`,
      icon: Users,
      color: "blue"
    },
    { 
      label: "Bài test", 
      value: (systemStats.totalExercises || systemStats.TotalExercises || 0).toString(), 
      note: `${systemStats.activeUsers || systemStats.ActiveUsers || 0} người dùng hoạt động`,
      icon: BookOpen,
      color: "green"
    },
    { 
      label: "Lượt làm bài", 
      value: (systemStats.totalResults || systemStats.TotalSubmissions || 0).toLocaleString(), 
      note: `Trung bình ${(systemStats.averageScore || systemStats.AverageScore || 0).toFixed(1)}%`,
      icon: Target,
      color: "purple"
    },
    { 
      label: "Tỉ lệ hoàn thành", 
      value: `${(systemStats.completionRate || systemStats.CompletionRate || 0).toFixed(1)}%`, 
      note: `${systemStats.monthlyActiveUsers || systemStats.ActiveUsersThisWeek || 0} hoạt động/tháng`,
      icon: TrendingUp,
      color: "orange"
    },
  ] : [];

  // 🔄 LOADING STATE
  if (loading) {
    return (
      <div className="space-y-6">
        <div className="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">
          {[1, 2, 3, 4].map((i) => (
            <Card key={i} className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
              <CardHeader className="animate-pulse">
                <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-3/4"></div>
                <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded w-1/2"></div>
                <div className="h-3 bg-gray-200 dark:bg-gray-700 rounded w-2/3"></div>
              </CardHeader>
            </Card>
          ))}
        </div>
        <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
          <CardHeader className="animate-pulse">
            <div className="h-6 bg-gray-200 dark:bg-gray-700 rounded w-1/4"></div>
            <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/3"></div>
          </CardHeader>
          <CardContent className="animate-pulse space-y-4">
            {[1, 2, 3].map((i) => (
              <div key={i} className="h-16 bg-gray-200 dark:bg-gray-700 rounded"></div>
            ))}
          </CardContent>
        </Card>
      </div>
    );
  }

  return (
    <div className="space-y-6">
      {/* 🚨 ERROR ALERT */}
      {error && (
        <motion.div
          initial={{ opacity: 0, y: -10 }}
          animate={{ opacity: 1, y: 0 }}
        >
          <Card className="rounded-2xl border-orange-200 dark:border-orange-800 bg-orange-50 dark:bg-orange-900/20">
            <CardContent className="p-4">
              <div className="flex items-center gap-3">
                <AlertCircle className="h-5 w-5 text-orange-600 dark:text-orange-400" />
                <p className="text-sm text-orange-800 dark:text-orange-300">{error}</p>
              </div>
            </CardContent>
          </Card>
        </motion.div>
      )}

      {/* 📊 DASHBOARD STATS CARDS - Enhanced with backend data */}
      <div className="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">
        {stats.map((s, i) => {
          const IconComponent = s.icon;
          const colorClasses = {
            blue: "bg-blue-50 dark:bg-blue-900/20 text-blue-600 dark:text-blue-400",
            green: "bg-green-50 dark:bg-green-900/20 text-green-600 dark:text-green-400", 
            purple: "bg-purple-50 dark:bg-purple-900/20 text-purple-600 dark:text-purple-400",
            orange: "bg-orange-50 dark:bg-orange-900/20 text-orange-600 dark:text-orange-400"
          };

          return (
            <motion.div 
              key={s.label} 
              initial={{ opacity: 0, y: 6 }} 
              animate={{ opacity: 1, y: 0 }} 
              transition={{ delay: i * 0.05 }}
            >
              <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700 hover:shadow-lg transition-shadow">
                <CardHeader className="pb-3">
                  <div className="flex items-center justify-between">
                    <CardDescription className="text-gray-600 dark:text-gray-400">{s.label}</CardDescription>
                    <div className={`p-2 rounded-lg ${colorClasses[s.color as keyof typeof colorClasses]}`}>
                      <IconComponent className="h-4 w-4" />
                    </div>
                  </div>
                  <CardTitle className="text-3xl text-gray-900 dark:text-white">{s.value}</CardTitle>
                  <span className="text-xs text-gray-500 dark:text-gray-400">{s.note}</span>
                </CardHeader>
              </Card>
            </motion.div>
          );
        })}
      </div>

      {/* 🔥 RECENT ACTIVITIES SECTION */}
      <div className="grid lg:grid-cols-3 gap-6">
        {/* Recent Activities */}
        <div className="lg:col-span-2">
          <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader>
              <div className="flex items-center justify-between">
                <div>
                  <CardTitle className="text-gray-900 dark:text-white flex items-center gap-2">
                    <Activity className="h-5 w-5" />
                    Hoạt động gần đây
                  </CardTitle>
                  <CardDescription className="text-gray-600 dark:text-gray-400">Theo dõi hoạt động người dùng realtime</CardDescription>
                </div>
              </div>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {(recentActivities || []).slice(0, 5).map((activity) => (
                  <div key={activity.id} className="flex items-center gap-4 p-3 bg-gray-50 dark:bg-gray-700/50 rounded-xl">
                    <div className="w-10 h-10 bg-blue-100 dark:bg-blue-800 rounded-full flex items-center justify-center">
                      <span className="text-blue-600 dark:text-blue-400 font-medium text-sm">
                        {activity.username.charAt(0).toUpperCase()}
                      </span>
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 dark:text-white">
                        {activity.username}
                      </p>
                      <p className="text-sm text-gray-600 dark:text-gray-400 truncate">
                        {activity.action}
                      </p>
                      {activity.details && (
                        <p className="text-xs text-gray-500 dark:text-gray-500">
                          {activity.details}
                        </p>
                      )}
                    </div>
                    <div className="text-xs text-gray-400 dark:text-gray-500">
                      {new Date(activity.timestamp).toLocaleTimeString('vi-VN', {
                        hour: '2-digit',
                        minute: '2-digit'
                      })}
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>
        </div>

        {/* Top Users & System Health */}
        <div className="space-y-6">
          {/* Top Users */}
          <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader>
              <CardTitle className="text-gray-900 dark:text-white flex items-center gap-2">
                <TrendingUp className="h-5 w-5" />
                Top Performers
              </CardTitle>
            </CardHeader>
            <CardContent>
              <div className="space-y-3">
                {(topUsers || []).slice(0, 5).map((user, index) => (
                  <div key={user.userId} className="flex items-center gap-3">
                    <div className={`w-6 h-6 rounded-full flex items-center justify-center text-xs font-bold ${
                      index === 0 ? 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300' :
                      index === 1 ? 'bg-gray-100 text-gray-800 dark:bg-gray-700 dark:text-gray-300' :
                      index === 2 ? 'bg-orange-100 text-orange-800 dark:bg-orange-900 dark:text-orange-300' :
                      'bg-blue-100 text-blue-800 dark:bg-blue-900 dark:text-blue-300'
                    }`}>
                      {index + 1}
                    </div>
                    <div className="flex-1 min-w-0">
                      <p className="text-sm font-medium text-gray-900 dark:text-white truncate">
                        {user.fullName || user.username}
                      </p>
                      <p className="text-xs text-gray-600 dark:text-gray-400">
                        Level {user.level || 0} • {(user.totalXp || user.TotalXP || 0).toLocaleString()} XP
                      </p>
                    </div>
                    <div className="text-xs text-gray-500 dark:text-gray-400">
                      {(user.averageScore || user.AverageScore || 0).toFixed(1)}%
                    </div>
                  </div>
                ))}
              </div>
            </CardContent>
          </Card>

          {/* System Health */}
          <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
            <CardHeader>
              <CardTitle className="text-gray-900 dark:text-white flex items-center gap-2">
                <div className={`w-3 h-3 rounded-full ${
                  systemHealth?.status === 'Healthy' ? 'bg-green-500' :
                  systemHealth?.status === 'Warning' ? 'bg-yellow-500' : 'bg-red-500'
                }`}></div>
                System Status
              </CardTitle>
            </CardHeader>
            <CardContent>
              {systemHealth && (
                <div className="space-y-3">
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600 dark:text-gray-400">Status</span>
                    <Badge variant={systemHealth.status === 'Healthy' ? 'default' : 'secondary'} 
                           className={systemHealth.status === 'Healthy' ? 'bg-green-500 dark:bg-green-600' : ''}>
                      {systemHealth.status}
                    </Badge>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600 dark:text-gray-400">Uptime</span>
                    <span className="text-gray-900 dark:text-white">{systemHealth.uptime}</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600 dark:text-gray-400">Memory</span>
                    <span className="text-gray-900 dark:text-white">{(systemHealth.memoryUsage || systemHealth.MemoryUsagePercent || 0).toFixed(1)}%</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600 dark:text-gray-400">CPU</span>
                    <span className="text-gray-900 dark:text-white">{(systemHealth.cpuUsage || systemHealth.CpuUsagePercent || 0).toFixed(1)}%</span>
                  </div>
                  <div className="flex justify-between text-sm">
                    <span className="text-gray-600 dark:text-gray-400">Connections</span>
                    <span className="text-gray-900 dark:text-white">{systemHealth.activeConnections || systemHealth.ActiveSessions || 0}</span>
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        </div>
      </div>


    </div>
  );
};

export default AdminDashboard;