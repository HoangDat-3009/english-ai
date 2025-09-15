import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { componentTagger } from "lovable-tagger";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => ({
  base: mode === 'production' ? '/english-mentor-buddy/' : '/',
  server: {
    host: "::",
    port: 8080,
    proxy: {
      "/api": {
        target: "https://EngBuddy-d39f.onrender.com", // Địa chỉ server backend
        changeOrigin: true, // Thay đổi origin trong header thành target
        secure: false, // Tắt kiểm tra SSL nếu cần (dùng trong dev)
        // rewrite: (path) => path.replace(/^\/api/, '') // Tùy chọn: bỏ prefix /api nếu server không cần
      },
    },
  },
  plugins: [
    react(),
    mode === "development" && componentTagger(),
  ].filter(Boolean),
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
}));