import React from 'react';
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card";
import { Button } from "@/components/ui/button";
import { Badge } from "@/components/ui/badge";
import { Avatar, AvatarFallback } from "@/components/ui/avatar";
import { MoreHorizontal, Mail, UserCheck, UserX } from 'lucide-react';

const UserManagement = () => {
  // Mock data - thay thế bằng API call thực tế
  const users = [
    {
      id: 1,
      name: "Nguyễn Văn A",
      email: "nguyenvana@email.com",
      englishLevel: "Intermediate",
      joinDate: "2024-01-15",
      status: "active",
      lastActive: "2024-03-15"
    },
    {
      id: 2,
      name: "Trần Thị B",
      email: "tranthib@email.com",
      englishLevel: "Beginner",
      joinDate: "2024-02-20",
      status: "active",
      lastActive: "2024-03-14"
    },
    {
      id: 3,
      name: "Lê Văn C",
      email: "levanc@email.com",
      englishLevel: "Advanced",
      joinDate: "2024-03-01",
      status: "inactive",
      lastActive: "2024-03-10"
    }
  ];

  const getStatusBadge = (status: string) => {
    if (status === 'active') {
      return <Badge variant="default" className="bg-green-500">Hoạt động</Badge>;
    }
    return <Badge variant="secondary">Không hoạt động</Badge>;
  };

  const getLevelBadge = (level: string) => {
    const colors = {
      'Beginner': 'bg-blue-500',
      'Intermediate': 'bg-orange-500',
      'Advanced': 'bg-green-500'
    };
    
    return (
      <Badge 
        variant="secondary" 
        className={`${colors[level as keyof typeof colors]} text-white`}
      >
        {level}
      </Badge>
    );
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-center">
        <div>
          <h1 className="text-3xl font-bold">Quản lý người dùng</h1>
          <p className="text-muted-foreground">
            Quản lý tài khoản và thông tin người dùng
          </p>
        </div>
        <Button>
          <Mail className="mr-2 h-4 w-4" />
          Gửi thông báo
        </Button>
      </div>

      <Card>
        <CardHeader>
          <CardTitle>Danh sách người dùng</CardTitle>
        </CardHeader>
        <CardContent>
          <div className="space-y-4">
            {users.map((user) => (
              <div 
                key={user.id} 
                className="flex items-center justify-between p-4 border rounded-lg hover:bg-accent/50 transition-colors"
              >
                <div className="flex items-center space-x-4">
                  <Avatar>
                    <AvatarFallback>
                      {user.name.split(' ').map(n => n[0]).join('')}
                    </AvatarFallback>
                  </Avatar>
                  <div>
                    <p className="font-medium">{user.name}</p>
                    <p className="text-sm text-muted-foreground">{user.email}</p>
                  </div>
                </div>
                
                <div className="flex items-center space-x-4">
                  {getLevelBadge(user.englishLevel)}
                  {getStatusBadge(user.status)}
                  <div className="text-sm text-muted-foreground">
                    <p>Tham gia: {new Date(user.joinDate).toLocaleDateString('vi-VN')}</p>
                    <p>Hoạt động: {new Date(user.lastActive).toLocaleDateString('vi-VN')}</p>
                  </div>
                  <div className="flex space-x-2">
                    <Button variant="outline" size="sm">
                      <UserCheck className="h-4 w-4" />
                    </Button>
                    <Button variant="outline" size="sm">
                      <UserX className="h-4 w-4" />
                    </Button>
                    <Button variant="outline" size="sm">
                      <MoreHorizontal className="h-4 w-4" />
                    </Button>
                  </div>
                </div>
              </div>
            ))}
          </div>
        </CardContent>
      </Card>
    </div>
  );
};

export default UserManagement;