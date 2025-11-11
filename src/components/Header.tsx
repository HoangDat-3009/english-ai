import { Globe, Moon, Sun, User } from "lucide-react";
import { Button } from "./ui/button";
import { useNavigate } from "react-router-dom";

export const Header = () => {
  const navigate = useNavigate();

  return (
    <header className="sticky top-0 z-50 w-full border-b bg-background/95 backdrop-blur supports-[backdrop-filter]:bg-background/60">
      <div className="container flex h-16 items-center justify-between px-4">
        {/* Logo */}
        <button 
          onClick={() => navigate("/")}
          className="flex items-center gap-2 hover:opacity-80 transition-opacity"
        >
          <div className="flex items-center justify-center w-10 h-10 rounded-xl bg-primary">
            <Globe className="w-6 h-6 text-primary-foreground" />
          </div>
          <div className="flex flex-col items-start">
            <span className="text-xl font-bold text-primary">EngBuddy</span>
            <span className="text-xs text-muted-foreground hidden sm:block">
              Nền tảng học tiếng Anh thông minh
            </span>
          </div>
        </button>

        {/* Right actions */}
        <div className="flex items-center gap-2">
          <Button variant="ghost" size="icon" className="rounded-full">
            <Sun className="h-5 w-5 rotate-0 scale-100 transition-all dark:-rotate-90 dark:scale-0" />
            <Moon className="absolute h-5 w-5 rotate-90 scale-0 transition-all dark:rotate-0 dark:scale-100" />
            <span className="sr-only">Toggle theme</span>
          </Button>
          <Button variant="ghost" size="icon" className="rounded-full">
            <User className="h-5 w-5" />
            <span className="sr-only">User account</span>
          </Button>
        </div>
      </div>
    </header>
  );
};
