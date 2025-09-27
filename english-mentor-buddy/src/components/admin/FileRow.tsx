import React from "react";
import { Badge } from "@/components/ui/badge";
import { Button } from "@/components/ui/button";
import { FileText, Trash2, Download } from "lucide-react";

interface FileRowProps {
  fileName: string;
  fileSize: string;
  uploadDate: string;
  status?: 'uploaded' | 'processing' | 'error';
  onDelete?: () => void;
  onDownload?: () => void;
}

export const FileRow = ({ 
  fileName, 
  fileSize, 
  uploadDate, 
  status = 'uploaded',
  onDelete,
  onDownload 
}: FileRowProps) => {
  const getStatusBadge = () => {
    switch (status) {
      case 'uploaded':
        return <Badge variant="default" className="bg-green-500">Đã tải lên</Badge>;
      case 'processing':
        return <Badge variant="secondary">Đang xử lý</Badge>;
      case 'error':
        return <Badge variant="destructive">Lỗi</Badge>;
      default:
        return null;
    }
  };

  return (
    <div className="flex items-center justify-between p-3 bg-gray-50 rounded-lg hover:bg-gray-100 transition-colors">
      <div className="flex items-center space-x-3">
        <FileText className="h-4 w-4 text-gray-500" />
        <div>
          <p className="text-sm font-medium text-gray-900">{fileName}</p>
          <div className="flex items-center space-x-2 text-xs text-gray-500">
            <span>{fileSize}</span>
            <span>•</span>
            <span>{uploadDate}</span>
          </div>
        </div>
        {getStatusBadge()}
      </div>
      
      <div className="flex space-x-1">
        {onDownload && (
          <Button variant="outline" size="sm" onClick={onDownload}>
            <Download className="h-3 w-3" />
          </Button>
        )}
        {onDelete && (
          <Button 
            variant="outline" 
            size="sm" 
            className="text-red-600 hover:text-red-700"
            onClick={onDelete}
          >
            <Trash2 className="h-3 w-3" />
          </Button>
        )}
      </div>
    </div>
  );
};