import React from 'react';
import { Card, CardContent, CardHeader, CardTitle, CardDescription } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { motion } from "framer-motion";
import { Users, BookOpen, Target, TrendingUp, Plus, Edit, Trash2, Eye, Clock } from 'lucide-react';

const AdminDashboard = () => {
  // Statistics data matching english-admin
  const stats = [
    { label: "Tổng người dùng", value: "1,284", note: "+32 tuần này" },
    { label: "Bài test", value: "86", note: "12 TOEIC • 24 Listening" },
    { label: "Lượt làm bài", value: "9,420", note: "+8% MoM" },
    { label: "Tỉ lệ hoàn thành", value: "76%", note: "-3%" },
  ];

  // Mock data cho danh sách đề thi
  const tests = [
    {
      id: 1,
      title: "TOEIC 2025 - Đề 01",
      type: "TOEIC Full",
      category: "Listening + Reading",
      level: "Intermediate",
      status: "published",
      createdDate: "2024-03-10",
      attempts: 1234,
      duration: "120 phút"
    },
    {
      id: 2,
      title: "Business English Test",
      type: "Listening",
      category: "Business",
      level: "Advanced",
      status: "draft",
      createdDate: "2024-03-12",
      attempts: 0,
      duration: "45 phút"
    },
    {
      id: 3,
      title: "Grammar Practice Test",
      type: "Reading",
      category: "Grammar",
      level: "Beginner",
      status: "published",
      createdDate: "2024-03-08",
      attempts: 856,
      duration: "60 phút"
    }
  ];

  const getStatusBadge = (status: string) => {
    if (status === 'published') {
      return <Badge variant="default" className="bg-green-500 dark:bg-green-600 text-white">Đã xuất bản</Badge>;
    }
    return <Badge variant="secondary" className="bg-gray-100 dark:bg-gray-700 text-gray-700 dark:text-gray-300">Bản nháp</Badge>;
  };

  const getTypeBadge = (type: string) => {
    const colors = {
      'TOEIC Full': 'bg-blue-500 dark:bg-blue-600',
      'Listening': 'bg-purple-500 dark:bg-purple-600',
      'Reading': 'bg-orange-500 dark:bg-orange-600',
      'Speaking': 'bg-green-500 dark:bg-green-600',
      'Writing': 'bg-red-500 dark:bg-red-600'
    };
    
    return (
      <Badge 
        variant="secondary" 
        className={`${colors[type as keyof typeof colors] || 'bg-gray-500 dark:bg-gray-600'} text-white`}
      >
        {type}
      </Badge>
    );
  };

  const getLevelBadge = (level: string) => {
    const colors = {
      'Beginner': 'bg-green-100 text-green-800 dark:bg-green-900 dark:text-green-300 border-green-200 dark:border-green-800',
      'Intermediate': 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900 dark:text-yellow-300 border-yellow-200 dark:border-yellow-800',
      'Advanced': 'bg-red-100 text-red-800 dark:bg-red-900 dark:text-red-300 border-red-200 dark:border-red-800'
    };
    
    return (
      <Badge 
        variant="outline" 
        className={colors[level as keyof typeof colors]}
      >
        {level}
      </Badge>
    );
  };

  return (
    <div className="space-y-6">
      {/* Dashboard Cards - matching english-admin style */}
      <div className="grid sm:grid-cols-2 xl:grid-cols-4 gap-4">
        {stats.map((s, i) => (
          <motion.div 
            key={s.label} 
            initial={{ opacity: 0, y: 6 }} 
            animate={{ opacity: 1, y: 0 }} 
            transition={{ delay: i * 0.05 }}
          >
            <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
              <CardHeader>
                <CardDescription className="text-gray-600 dark:text-gray-400">{s.label}</CardDescription>
                <CardTitle className="text-3xl text-gray-900 dark:text-white">{s.value}</CardTitle>
                <span className="text-xs text-gray-500 dark:text-gray-400">{s.note}</span>
              </CardHeader>
            </Card>
          </motion.div>
        ))}
      </div>

      {/* Danh sách đề thi */}
      <Card className="rounded-2xl bg-white dark:bg-gray-800 border-gray-200 dark:border-gray-700">
        <CardHeader>
          <div className="flex items-center justify-between">
            <div>
              <CardTitle className="text-gray-900 dark:text-white">Đề / Bài test</CardTitle>
              <CardDescription className="text-gray-600 dark:text-gray-400">Quản lý tất cả các bài kiểm tra và đề thi</CardDescription>
            </div>
            <Button className="rounded-xl bg-blue-600 hover:bg-blue-700 text-white">
              <Plus className="mr-2 h-4 w-4" />
              Tạo đề mới
            </Button>
          </div>
        </CardHeader>
        <CardContent>
          {/* Quick Stats for Tests */}
          <div className="grid gap-4 md:grid-cols-4 mb-6">
            <div className="flex items-center gap-3 p-3 bg-blue-50 dark:bg-blue-900/20 rounded-xl">
              <div className="p-2 bg-blue-100 dark:bg-blue-800 rounded-lg">
                <Eye className="h-4 w-4 text-blue-600 dark:text-blue-400" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Tổng đề thi</p>
                <p className="text-lg font-bold text-gray-900 dark:text-white">86</p>
              </div>
            </div>
            
            <div className="flex items-center gap-3 p-3 bg-green-50 dark:bg-green-900/20 rounded-xl">
              <div className="p-2 bg-green-100 dark:bg-green-800 rounded-lg">
                <Users className="h-4 w-4 text-green-600 dark:text-green-400" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Lượt thi</p>
                <p className="text-lg font-bold text-gray-900 dark:text-white">9,420</p>
              </div>
            </div>
            
            <div className="flex items-center gap-3 p-3 bg-orange-50 dark:bg-orange-900/20 rounded-xl">
              <div className="p-2 bg-orange-100 dark:bg-orange-800 rounded-lg">
                <Clock className="h-4 w-4 text-orange-600 dark:text-orange-400" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Trung bình</p>
                <p className="text-lg font-bold text-gray-900 dark:text-white">78%</p>
              </div>
            </div>
            
            <div className="flex items-center gap-3 p-3 bg-purple-50 dark:bg-purple-900/20 rounded-xl">
              <div className="p-2 bg-purple-100 dark:bg-purple-800 rounded-lg">
                <Plus className="h-4 w-4 text-purple-600 dark:text-purple-400" />
              </div>
              <div>
                <p className="text-sm font-medium text-gray-600 dark:text-gray-400">Mới tuần này</p>
                <p className="text-lg font-bold text-gray-900 dark:text-white">12</p>
              </div>
            </div>
          </div>

          {/* Tests List */}
          <div className="space-y-3">
            <h4 className="font-medium text-gray-900 dark:text-white">Đề thi gần đây</h4>
            {tests.map((test) => (
              <div 
                key={test.id} 
                className="flex items-center justify-between p-4 border border-gray-200 dark:border-gray-700 rounded-xl hover:bg-gray-50 dark:hover:bg-gray-700/50 transition-colors"
              >
                <div className="space-y-2 flex-1">
                  <div className="flex items-center space-x-2">
                    <h3 className="font-medium text-gray-900 dark:text-white">{test.title}</h3>
                    {getTypeBadge(test.type)}
                    {getLevelBadge(test.level)}
                    {getStatusBadge(test.status)}
                  </div>
                  <div className="flex items-center space-x-4 text-xs text-gray-600 dark:text-gray-400">
                    <span>📚 {test.category}</span>
                    <span>⏱️ {test.duration}</span>
                    <span>👥 {test.attempts.toLocaleString()}</span>
                    <span>📅 {new Date(test.createdDate).toLocaleDateString('vi-VN')}</span>
                  </div>
                </div>
                
                <div className="flex space-x-2">
                  <Button variant="outline" size="sm" className="rounded-xl hover:bg-gray-100 dark:hover:bg-gray-700">
                    <Eye className="h-3 w-3" />
                  </Button>
                  <Button variant="outline" size="sm" className="rounded-xl hover:bg-gray-100 dark:hover:bg-gray-700">
                    <Edit className="h-3 w-3" />
                  </Button>
                  <Button variant="outline" size="sm" className="text-red-600 dark:text-red-400 hover:text-red-700 dark:hover:text-red-300 rounded-xl hover:bg-red-50 dark:hover:bg-red-900/20">
                    <Trash2 className="h-3 w-3" />
                  </Button>
                </div>
              </div>
            ))}
            
            <div className="pt-4 border-t border-gray-200 dark:border-gray-700">
              <Button variant="outline" className="w-full rounded-xl hover:bg-gray-100 dark:hover:bg-gray-700">
                Xem tất cả đề thi
              </Button>
            </div>
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default AdminDashboard;