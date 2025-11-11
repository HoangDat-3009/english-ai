import { Link, useLocation } from "react-router-dom";
import { BookOpen, TrendingUp, Headphones, FileText, BookMarked, User } from "lucide-react";
import { cn } from "@/lib/utils";

const Sidebar = () => {
  const location = useLocation();

  const menuItems = [
    { icon: BookOpen, label: "BÀI HỌC", path: "/lesson-list" },
    { icon: TrendingUp, label: "TIẾN ĐỘ", path: "/progress" },
    { icon: Headphones, label: "LUYỆN NGHE", path: "/listening-practice" },
    { icon: FileText, label: "LUYỆN ĐỀ", path: "/test-list" },
    { icon: BookMarked, label: "NGỮ PHÁP", path: "/grammar-practice" },
    { icon: BookOpen, label: "TỪ VỰNG", path: "/vocabulary-topics" },
    { icon: User, label: "HỒ SƠ", path: "/profile" },
  ];

  return (
    <aside className="w-64 min-h-screen bg-card border-r border-border fixed left-0 top-0 flex flex-col">
      <div className="p-6">
        <Link to="/" className="flex items-center gap-2">
          <div className="w-10 h-10 bg-gradient-to-br from-primary to-purple rounded-lg flex items-center justify-center">
            <BookOpen className="w-6 h-6 text-white" />
          </div>
          <span className="text-xl font-bold bg-gradient-to-r from-primary to-purple bg-clip-text text-transparent">
            EngBuddy
          </span>
        </Link>
      </div>

      <nav className="flex-1 px-3">
        {menuItems.map((item) => {
          const Icon = item.icon;
          const isActive = location.pathname === item.path;
          
          return (
            <Link
              key={item.path}
              to={item.path}
              className={cn(
                "flex items-center gap-3 px-4 py-3 mb-1 rounded-lg transition-all",
                isActive
                  ? "bg-primary/10 text-primary font-semibold"
                  : "text-muted-foreground hover:bg-accent hover:text-accent-foreground"
              )}
            >
              <Icon className="w-5 h-5" />
              <span className="text-sm">{item.label}</span>
            </Link>
          );
        })}
      </nav>
    </aside>
  );
};

export default Sidebar;
