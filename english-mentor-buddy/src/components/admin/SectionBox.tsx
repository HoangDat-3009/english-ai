import React from "react";

interface SectionBoxProps {
  className?: string;
  children: React.ReactNode;
}

export const SectionBox = ({ className = "", children }: SectionBoxProps) => {
  return (
    <div className={`p-6 bg-white border border-gray-200 rounded-lg ${className}`}>
      {children}
    </div>
  );
};