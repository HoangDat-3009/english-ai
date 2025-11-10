import React, { useEffect, useState } from 'react';
import { Card, CardContent } from "@/components/ui/card";
import { motion } from "framer-motion";
import { Users, AlertCircle, DollarSign, CreditCard } from 'lucide-react';
import statisticsService, { SystemStatistics } from '@/services/statisticsService';
import { Alert, AlertDescription } from "@/components/ui/alert";
import UserGrowthChart from '@/components/charts/UserGrowthChart';
import RevenuePaymentChart from '@/components/charts/RevenuePaymentChart';

const AdminDashboard = () => {
  // State for statistics from API
  const [statistics, setStatistics] = useState<SystemStatistics | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Fetch statistics from API
  useEffect(() => {
    const fetchStatistics = async () => {
      try {
        setLoading(true);
        setError(null);
        const data = await statisticsService.getSystemStatistics();
        setStatistics(data);
      } catch (err) {
        console.error('Error fetching statistics:', err);
        setError('Không thể tải dữ liệu thống kê. Vui lòng thử lại sau.');
      } finally {
        setLoading(false);
      }
    };

    fetchStatistics();
  }, []);

  // Format number with comma separator
  const formatNumber = (num: number): string => {
    return num.toLocaleString('vi-VN');
  };

  // Format currency to millions VND
  const formatCurrency = (amount: number): string => {
    const millions = amount / 1000000;
    return `${millions.toFixed(1)}M VNĐ`;
  };

  // Statistics data from API
  const stats = statistics ? [
    { 
      label: "Tổng học viên", 
      value: formatNumber(statistics.TotalUsers), 
      note: "Học viên trong hệ thống",
      icon: Users,
      color: "text-blue-600 dark:text-blue-400",
      bgColor: "bg-blue-50 dark:bg-blue-900/20"
    },
    { 
      label: "Mới tháng này", 
      value: formatNumber(statistics.NewUsersThisMonth), 
      note: "Người dùng mới",
      icon: Users,
      color: "text-orange-600 dark:text-orange-400",
      bgColor: "bg-orange-50 dark:bg-orange-900/20"
    },
    { 
      label: "Tổng doanh thu", 
      value: formatCurrency(statistics.TotalRevenue), 
      note: "Tổng doanh thu",
      icon: DollarSign,
      color: "text-purple-600 dark:text-purple-400",
      bgColor: "bg-purple-50 dark:bg-purple-900/20"
    },
    { 
      label: "Doanh thu tháng", 
      value: formatCurrency(statistics.RevenueThisMonth), 
      note: "Doanh thu tháng này",
      icon: DollarSign,
      color: "text-green-600 dark:text-green-400",
      bgColor: "bg-green-50 dark:bg-green-900/20"
    },
    { 
      label: "Thanh toán chờ", 
      value: formatNumber(statistics.PendingPayments), 
      note: "Chờ xử lý",
      icon: CreditCard,
      color: "text-yellow-600 dark:text-yellow-400",
      bgColor: "bg-yellow-50 dark:bg-yellow-900/20"
    },
  ] : [];

  return (
    <div className="space-y-6">
      {/* Error Alert */}
      {error && (
        <Alert variant="destructive" className="rounded-xl">
          <AlertCircle className="h-4 w-4" />
          <AlertDescription>{error}</AlertDescription>
        </Alert>
      )}

      {/* Dashboard Cards - with real data from API */}
      <div className="grid sm:grid-cols-2 xl:grid-cols-5 gap-4">
        {loading ? (
          // Loading skeleton
          Array.from({ length: 5 }).map((_, i) => (
            <Card key={i} className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700 animate-pulse">
              <CardContent className="p-6">
                <div className="h-4 bg-gray-200 dark:bg-gray-700 rounded w-1/2 mb-2"></div>
                <div className="h-8 bg-gray-200 dark:bg-gray-700 rounded w-3/4 mb-2"></div>
              </CardContent>
            </Card>
          ))
        ) : stats.length > 0 ? (
          stats.map((s, i) => {
            const Icon = s.icon;
            return (
              <motion.div 
                key={s.label} 
                initial={{ opacity: 0, y: 6 }} 
                animate={{ opacity: 1, y: 0 }} 
                transition={{ delay: i * 0.05 }}
              >
                <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700 hover:shadow-lg transition-shadow">
                  <CardContent className="p-6">
                    <div className="flex items-center justify-between">
                      <div className="flex-1">
                        <p className="text-sm text-gray-600 dark:text-gray-400 mb-1">{s.label}</p>
                        <h3 className="text-2xl font-bold text-gray-900 dark:text-white mb-1">{s.value}</h3>
                        <p className="text-xs text-gray-500 dark:text-gray-400">{s.note}</p>
                      </div>
                      <div className={`p-3 rounded-lg ${s.bgColor} flex-shrink-0`}>
                        <Icon className={`h-6 w-6 ${s.color}`} />
                      </div>
                    </div>
                  </CardContent>
                </Card>
              </motion.div>
            );
          })
        ) : (
          <div className="col-span-5 text-center text-gray-500 dark:text-gray-400">
            Không có dữ liệu thống kê
          </div>
        )}
      </div>

      {/* User Growth Chart */}
      <UserGrowthChart />

      {/* Revenue & Payment Chart */}
      <RevenuePaymentChart />
    </div>
  );
};

export default AdminDashboard;