import React, { useEffect, useState, useCallback } from 'react';
import { 
  User, 
  Mail, 
  CreditCard, 
  Calendar, 
  Package, 
  FileText, 
  CheckCircle2, 
  Clock, 
  XCircle,
  Infinity as InfinityIcon
} from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Badge } from '@/components/ui/badge';
import { Skeleton } from '@/components/ui/skeleton';
import { Card, CardContent } from '@/components/ui/card';
import { Separator } from '@/components/ui/separator';
import { TransactionDetail, PaymentStatus } from '@/services/transactionService';
import transactionService from '@/services/transactionService';
import { useToast } from '@/hooks/use-toast';

interface TransactionDetailModalProps {
  transactionId: string | null;
  open: boolean;
  onClose: () => void;
}

const TransactionDetailModal: React.FC<TransactionDetailModalProps> = ({
  transactionId,
  open,
  onClose,
}) => {
  const [transaction, setTransaction] = useState<TransactionDetail | null>(null);
  const [loading, setLoading] = useState(false);
  const { toast } = useToast();

  const fetchTransactionDetail = useCallback(async () => {
    if (!transactionId) return;

    setLoading(true);
    try {
      const data = await transactionService.fetchTransactionById(transactionId);
      setTransaction(data);
    } catch (error) {
      console.error('Error fetching transaction detail:', error);
      toast({
        title: 'Lỗi',
        description: 'Không thể tải thông tin giao dịch',
        variant: 'destructive',
      });
      onClose();
    } finally {
      setLoading(false);
    }
  }, [transactionId, toast, onClose]);

  useEffect(() => {
    if (open && transactionId) {
      fetchTransactionDetail();
    }
  }, [open, transactionId, fetchTransactionDetail]);

  const formatCurrency = (amount: number): string => {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND',
    }).format(amount);
  };

  const formatDate = (dateString: string): string => {
    const date = new Date(dateString);
    return new Intl.DateTimeFormat('vi-VN', {
      day: '2-digit',
      month: '2-digit',
      year: 'numeric',
      hour: '2-digit',
      minute: '2-digit',
      second: '2-digit',
    }).format(date);
  };

  const getPackageName = (packageId: string): string => {
    const packages: { [key: string]: string } = {
      '1': 'Premium 1 Tháng',
      '2': 'Premium 3 Tháng',
      '3': 'Premium 12 Tháng',
      '4': 'Premium Vĩnh Viễn',
      '5': 'Dùng Thử 7 Ngày',
    };
    return packages[packageId] || `Gói #${packageId}`;
  };

  const getPaymentMethodDisplay = (method: string | undefined): string => {
    if (!method) return 'Không xác định';
    
    const methods: { [key: string]: string } = {
      'momo': 'MoMo',
      'bank': 'Chuyển khoản ngân hàng',
      'paypal': 'PayPal',
      'credit': 'Thẻ tín dụng',
      'trial': 'Dùng thử',
    };
    return methods[method.toLowerCase()] || method;
  };

  const getStatusConfig = (status: string) => {
    const configs = {
      [PaymentStatus.Completed]: {
        label: 'Đã hoàn thành',
        icon: CheckCircle2,
        className: 'bg-green-50 border-green-200 text-green-700 dark:bg-green-950 dark:border-green-800 dark:text-green-400',
        iconClassName: 'text-green-600 dark:text-green-500',
      },
      [PaymentStatus.Pending]: {
        label: 'Chờ xử lý',
        icon: Clock,
        className: 'bg-yellow-50 border-yellow-200 text-yellow-700 dark:bg-yellow-950 dark:border-yellow-800 dark:text-yellow-400',
        iconClassName: 'text-yellow-600 dark:text-yellow-500',
      },
      [PaymentStatus.Failed]: {
        label: 'Thất bại',
        icon: XCircle,
        className: 'bg-red-50 border-red-200 text-red-700 dark:bg-red-950 dark:border-red-800 dark:text-red-400',
        iconClassName: 'text-red-600 dark:text-red-500',
      },
    };

    return configs[status as PaymentStatus] || {
      label: status,
      icon: Clock,
      className: 'bg-gray-50 border-gray-200 text-gray-700 dark:bg-gray-950 dark:border-gray-800 dark:text-gray-400',
      iconClassName: 'text-gray-600 dark:text-gray-500',
    };
  };

  const InfoItem: React.FC<{
    icon: React.ReactNode;
    label: string;
    value: React.ReactNode;
    className?: string;
  }> = ({ icon, label, value, className = '' }) => (
    <div className={`flex items-start gap-4 ${className}`}>
      <div className="flex-shrink-0 w-10 h-10 rounded-lg bg-primary/10 dark:bg-primary/20 flex items-center justify-center text-primary">
        {icon}
      </div>
      <div className="flex-1 min-w-0">
        <p className="text-xs font-medium text-muted-foreground uppercase tracking-wide mb-1">
          {label}
        </p>
        <div className="font-medium text-foreground break-words">
          {value}
        </div>
      </div>
    </div>
  );

  return (
    <Dialog open={open} onOpenChange={onClose}>
      <DialogContent className="max-w-3xl max-h-[90vh] overflow-y-auto">
        <DialogHeader className="pb-6 space-y-3">
          <DialogTitle className="text-2xl font-bold">
            Chi tiết giao dịch
          </DialogTitle>
          {transaction && (
            <div className="flex items-center gap-2">
              <span className="text-sm text-muted-foreground font-normal">
                Mã giao dịch:
              </span>
              <Badge 
                variant="outline" 
                className="font-mono text-sm px-3 py-1 bg-primary/5 border-primary/20"
              >
                {transaction.Id}
              </Badge>
            </div>
          )}
        </DialogHeader>

        {loading ? (
          <div className="space-y-4 py-4">
            {[...Array(5)].map((_, i) => (
              <Skeleton key={i} className="h-24 w-full rounded-lg" />
            ))}
          </div>
        ) : transaction ? (
          <div className="space-y-6 pb-2">
            {/* Status and Amount Card */}
            <Card className={`border-2 ${getStatusConfig(transaction.Status).className}`}>
              <CardContent className="p-6">
                <div className="flex items-center justify-between gap-6">
                  <div className="flex items-center gap-4">
                    <div className={`flex-shrink-0 ${getStatusConfig(transaction.Status).iconClassName}`}>
                      {React.createElement(getStatusConfig(transaction.Status).icon, { 
                        className: 'w-10 h-10' 
                      })}
                    </div>
                    <div>
                      <p className="text-sm font-medium text-muted-foreground mb-1">
                        Trạng thái
                      </p>
                      <p className="text-xl font-bold">
                        {getStatusConfig(transaction.Status).label}
                      </p>
                    </div>
                  </div>
                  <Separator orientation="vertical" className="h-16" />
                  <div className="text-right">
                    <p className="text-sm font-medium text-muted-foreground mb-1">
                      Số tiền
                    </p>
                    <p className="text-3xl font-bold text-primary">
                      {formatCurrency(transaction.Amount)}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* User Information Card */}
            <Card>
              <CardContent className="p-6">
                <h3 className="text-lg font-semibold mb-6 flex items-center gap-2">
                  <User className="w-5 h-5 text-primary" />
                  Thông tin người dùng
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <InfoItem
                    icon={<User className="w-5 h-5" />}
                    label="Tên người dùng"
                    value={transaction.UserName || 'Chưa cập nhật'}
                  />
                  <InfoItem
                    icon={<Mail className="w-5 h-5" />}
                    label="Email"
                    value={transaction.UserEmail}
                  />
                </div>
              </CardContent>
            </Card>

            {/* Payment Information Card */}
            <Card>
              <CardContent className="p-6">
                <h3 className="text-lg font-semibold mb-6 flex items-center gap-2">
                  <CreditCard className="w-5 h-5 text-primary" />
                  Thông tin thanh toán
                </h3>
                <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                  <InfoItem
                    icon={<CreditCard className="w-5 h-5" />}
                    label="Phương thức thanh toán"
                    value={getPaymentMethodDisplay(transaction.PaymentMethod)}
                  />
                  <InfoItem
                    icon={<Package className="w-5 h-5" />}
                    label="Gói dịch vụ"
                    value={
                      <div className="flex items-center gap-2 flex-wrap">
                        <span>{getPackageName(transaction.PackageId || '')}</span>
                        {transaction.IsLifetime && (
                          <Badge 
                            variant="secondary" 
                            className="bg-purple-100 text-purple-700 dark:bg-purple-900 dark:text-purple-300 border-purple-200 dark:border-purple-800"
                          >
                            <InfinityIcon className="w-3 h-3 mr-1" />
                            Vĩnh viễn
                          </Badge>
                        )}
                      </div>
                    }
                  />
                  <InfoItem
                    icon={<Calendar className="w-5 h-5" />}
                    label="Ngày tạo"
                    value={formatDate(transaction.CreatedAt)}
                  />
                  <InfoItem
                    icon={<Calendar className="w-5 h-5" />}
                    label="Cập nhật lần cuối"
                    value={formatDate(transaction.UpdatedAt)}
                  />
                </div>
              </CardContent>
            </Card>

            {/* Transaction Notes Card */}
            {transaction.TransactionNotes && (
              <Card>
                <CardContent className="p-6">
                  <h3 className="text-lg font-semibold mb-4 flex items-center gap-2">
                    <FileText className="w-5 h-5 text-primary" />
                    Lịch sử & Ghi chú
                  </h3>
                  <div className="bg-muted/50 dark:bg-muted/20 rounded-lg p-4">
                    <p className="text-sm text-foreground whitespace-pre-wrap leading-relaxed">
                      {transaction.TransactionNotes}
                    </p>
                  </div>
                </CardContent>
              </Card>
            )}
          </div>
        ) : null}
      </DialogContent>
    </Dialog>
  );
};

export default TransactionDetailModal;
