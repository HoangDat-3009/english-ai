import React, { useState } from 'react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";
import { Button } from "@/components/ui/button";
import { Textarea } from "@/components/ui/textarea";
import { Label } from "@/components/ui/label";

interface StatusReasonDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  onConfirm: (reason: string) => void;
  username: string;
  newStatus: 'active' | 'inactive' | 'banned';
}

const statusConfig = {
  active: {
    title: '🔓 Kích hoạt tài khoản',
    description: 'Bạn đang kích hoạt lại tài khoản',
    reasonLabel: 'Lý do kích hoạt',
    reasonPlaceholder: 'Ví dụ: Đã xác minh thông tin, đã giải quyết vấn đề, yêu cầu từ cấp trên...',
    warningText: 'Tài khoản sẽ có thể đăng nhập và sử dụng hệ thống bình thường.',
    warningColor: 'bg-green-50 dark:bg-green-900/20 border-green-200 dark:border-green-800',
    warningTextColor: 'text-green-800 dark:text-green-200',
    titleColor: 'text-green-600 dark:text-green-400',
    buttonVariant: 'default' as const,
    buttonText: 'Xác nhận kích hoạt',
  },
  inactive: {
    title: '⏸️ Tạm khóa tài khoản',
    description: 'Bạn đang tạm khóa tài khoản',
    reasonLabel: 'Lý do tạm khóa',
    reasonPlaceholder: 'Ví dụ: Vi phạm nhỏ, cần xác minh thông tin, yêu cầu tạm thời...',
    warningText: 'Tài khoản sẽ bị tạm khóa và không thể đăng nhập cho đến khi được kích hoạt lại.',
    warningColor: 'bg-yellow-50 dark:bg-yellow-900/20 border-yellow-200 dark:border-yellow-800',
    warningTextColor: 'text-yellow-800 dark:text-yellow-200',
    titleColor: 'text-yellow-600 dark:text-yellow-400',
    buttonVariant: 'destructive' as const,
    buttonText: 'Xác nhận tạm khóa',
  },
  banned: {
    title: '🚫 Cấm tài khoản vĩnh viễn',
    description: 'Bạn đang cấm vĩnh viễn tài khoản',
    reasonLabel: 'Lý do cấm vĩnh viễn',
    reasonPlaceholder: 'Ví dụ: Vi phạm nghiêm trọng, spam, lừa đảo, hành vi không phù hợp...',
    warningText: 'Hành động này KHÔNG THỂ HOÀN TÁC. Tài khoản sẽ bị khóa vĩnh viễn và không thể kích hoạt lại.',
    warningColor: 'bg-red-50 dark:bg-red-900/20 border-red-200 dark:border-red-800',
    warningTextColor: 'text-red-800 dark:text-red-200',
    titleColor: 'text-red-600 dark:text-red-400',
    buttonVariant: 'destructive' as const,
    buttonText: 'Xác nhận cấm vĩnh viễn',
  },
};

export const StatusReasonDialog: React.FC<StatusReasonDialogProps> = ({
  open,
  onOpenChange,
  onConfirm,
  username,
  newStatus,
}) => {
  const [reason, setReason] = useState('');
  const config = statusConfig[newStatus];

  const handleConfirm = () => {
    if (reason.trim()) {
      const trimmedReason = reason.trim();
      setReason(''); // Reset immediately
      onConfirm(trimmedReason);
    }
  };

  const handleCancel = () => {
    setReason('');
    onOpenChange(false);
  };

  // Reset reason when dialog closes
  React.useEffect(() => {
    if (!open) {
      setReason('');
    }
  }, [open]);

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent className="sm:max-w-[500px]">
        <DialogHeader>
          <DialogTitle className={config.titleColor}>
            {config.title}
          </DialogTitle>
          <DialogDescription>
            {config.description} <strong>"{username}"</strong>.
            <br />
            Vui lòng ghi rõ lý do để lưu vào hệ thống.
          </DialogDescription>
        </DialogHeader>

        <div className="grid gap-4 py-4">
          <div className="grid gap-2">
            <Label htmlFor="reason" className="text-left font-semibold">
              {config.reasonLabel} <span className="text-red-500">*</span>
            </Label>
            <Textarea
              id="reason"
              placeholder={config.reasonPlaceholder}
              value={reason}
              onChange={(e) => setReason(e.target.value)}
              className="min-h-[100px] resize-none"
              maxLength={500}
            />
            <p className="text-xs text-gray-500 dark:text-gray-400 text-right">
              {reason.length}/500 ký tự
            </p>
          </div>

          <div className={`${config.warningColor} border rounded-lg p-3`}>
            <p className={`text-sm ${config.warningTextColor}`}>
              <strong>Lưu ý:</strong> {config.warningText}
            </p>
          </div>
        </div>

        <DialogFooter>
          <Button
            variant="outline"
            onClick={handleCancel}
            className="rounded-lg"
          >
            Hủy
          </Button>
          <Button
            variant={config.buttonVariant}
            onClick={handleConfirm}
            disabled={!reason.trim()}
            className="rounded-lg"
          >
            {config.buttonText}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
