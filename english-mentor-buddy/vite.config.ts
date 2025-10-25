// ⚡ VITE CONFIG - Build configuration cho development & production
// ✅ READY FOR GIT: Configured for GitHub Pages deployment
// 🚀 TODO DEPLOY: Cập nhật base path khi deploy lên server khác (không phải GitHub Pages)
// 🔧 Features: React SWC, path aliases, proxy setup, production optimization

import react from "@vitejs/plugin-react-swc";
import { componentTagger } from "lovable-tagger";
import path from "path";
import { defineConfig } from "vite";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => ({
  base: mode === 'production' ? '/english-mentor-buddy/' : '/', // ⚠️ CHANGE for different hosting
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