import React, { useEffect, useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Badge } from "@/components/ui/badge";
import { ScrollArea } from "@/components/ui/scroll-area";
import { Separator } from "@/components/ui/separator";
import { Alert, AlertDescription } from "@/components/ui/alert";
import { Loader2, History, ArrowRight, User, Calendar, FileText } from 'lucide-react';
import userService, { StatusHistory } from '@/services/userService';

interface UserStatusHistoryDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  userId: number;
  username: string;
}

export const UserStatusHistoryDialog: React.FC<UserStatusHistoryDialogProps> = ({
  open,
  onOpenChange,
  userId,
  username,
}) => {
  const [history, setHistory] = useState<StatusHistory[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const fetchHistory = async () => {
    try {
      setLoading(true);
      setError(null);
      const data = await userService.getUserStatusHistory(userId);
      setHistory(data);
    } catch (err) {
      console.error('Error fetching status history:', err);
      setError('Không thể tải lịch sử thay đổi trạng thái. Vui lòng thử lại sau.');
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    if (open && userId) {
      fetchHistory();
    }
  }, [open, userId]); // eslint-disable-line react-hooks/exhaustive-deps

  const getStatusBadge = (status: string | null) => {
    if (!status) return null;
    
    const statusMap: Record<string, { label: string; className: string }> = {
      'active': { label: 'Hoạt động', className: 'bg-green-100 text-green-800 dark:bg-green-900/30 dark:text-green-400' },
      'inactive': { label: 'Không hoạt động', className: 'bg-yellow-100 text-yellow-800 dark:bg-yellow-900/30 dark:text-yellow-400' },
      'banned': { label: 'Bị cấm', className: 'bg-red-100 text-red-800 dark:bg-red-900/30 dark:text-red-400' }
    };
    
    const statusInfo = statusMap[status] || statusMap['inactive'];
    return (
      <Badge variant="secondary" className={statusInfo.className}>
        {statusInfo.label}
      </Badge>
    );
  };

  const formatDateTime = (dateString: string) => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('vi-VN', {
      year: 'numeric',
      month: '2-digit',
      day: '2-digit',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    }).format(date);
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[700px] max-h-[80vh]">
        <DialogHeader>
          <DialogTitle className="flex items-center gap-2 text-blue-600 dark:text-blue-400">
            <History className="h-5 w-5" />
            Lịch sử thay đổi trạng thái
          </DialogTitle>
          <DialogDescription>
            Xem lịch sử thay đổi trạng thái của tài khoản <strong>{username}</strong>
          </DialogDescription>
        </DialogHeader>

        <ScrollArea className="max-h-[500px] pr-4">
          {loading ? (
            <div className="flex items-center justify-center py-12">
              <Loader2 className="h-8 w-8 animate-spin text-blue-500" />
              <span className="ml-2 text-gray-600 dark:text-gray-400">Đang tải lịch sử...</span>
            </div>
          ) : error ? (
            <Alert variant="destructive" className="my-4">
              <AlertDescription>{error}</AlertDescription>
            </Alert>
          ) : history.length === 0 ? (
            <div className="text-center py-12">
              <History className="mx-auto h-12 w-12 text-gray-400" />
              <p className="mt-2 text-gray-600 dark:text-gray-400">
                Chưa có lịch sử thay đổi trạng thái
              </p>
            </div>
          ) : (
            <div className="space-y-4">
              {history.map((record, index) => (
                <div key={record.HistoryID}>
                  <div className="relative pl-8 pb-8">
                    {/* Timeline dot */}
                    <div className="absolute left-0 top-1 w-4 h-4 bg-blue-500 rounded-full border-4 border-white dark:border-gray-800"></div>
                    
                    {/* Timeline line */}
                    {index < history.length - 1 && (
                      <div className="absolute left-[7px] top-5 w-[2px] h-full bg-gray-300 dark:bg-gray-600"></div>
                    )}
                    
                    {/* Content */}
                    <div className="bg-gray-50 dark:bg-gray-800 rounded-lg p-4 border border-gray-200 dark:border-gray-700">
                      {/* Header: Time + Admin */}
                      <div className="flex items-center justify-between mb-3">
                        <div className="flex items-center gap-2 text-sm text-gray-600 dark:text-gray-400">
                          <Calendar className="h-4 w-4" />
                          {formatDateTime(record.ChangedAt)}
                        </div>
                        {record.ChangedByUsername && (
                          <div className="flex items-center gap-1 text-sm">
                            <User className="h-4 w-4 text-purple-500" />
                            <span className="font-medium text-purple-600 dark:text-purple-400">
                              {record.ChangedByUsername}
                            </span>
                          </div>
                        )}
                      </div>

                      {/* Status Change */}
                      <div className="flex items-center gap-2 mb-3">
                        {record.FromStatus ? (
                          <>
                            {getStatusBadge(record.FromStatus)}
                            <ArrowRight className="h-4 w-4 text-gray-400" />
                            {getStatusBadge(record.ToStatus)}
                          </>
                        ) : (
                          <div className="flex items-center gap-2">
                            <span className="text-sm text-gray-600 dark:text-gray-400">Trạng thái mới:</span>
                            {getStatusBadge(record.ToStatus)}
                          </div>
                        )}
                      </div>

                      {/* Reason */}
                      {record.ReasonCode && (
                        <div className="space-y-2">
                          <Separator className="my-2" />
                          <div className="flex items-start gap-2">
                            <FileText className="h-4 w-4 text-gray-500 mt-0.5" />
                            <div className="flex-1">
                              <div className="flex items-center gap-2 mb-1">
                                <span className="text-sm font-semibold text-gray-700 dark:text-gray-300">
                                  {record.ReasonName || record.ReasonCode}
                                </span>
                              </div>
                              {record.ReasonNote && (
                                <p className="text-sm text-gray-600 dark:text-gray-400 bg-white dark:bg-gray-900 p-2 rounded border border-gray-200 dark:border-gray-700">
                                  {record.ReasonNote}
                                </p>
                              )}
                            </div>
                          </div>
                        </div>
                      )}

                      {/* Expires At (if applicable) */}
                      {record.ExpiresAt && (
                        <div className="mt-2 text-xs text-orange-600 dark:text-orange-400">
                          ⏰ Hết hạn: {formatDateTime(record.ExpiresAt)}
                        </div>
                      )}
                    </div>
                  </div>
                </div>
              ))}
            </div>
          )}
        </ScrollArea>
      </DialogContent>
    </Dialog>
  );
};
