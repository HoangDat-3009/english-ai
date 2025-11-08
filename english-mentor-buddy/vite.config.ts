import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { componentTagger } from "lovable-tagger";
import basicSsl from '@vitejs/plugin-basic-ssl';

// https://vitejs.dev/config/
// @ts-expect-error - Duplicate node_modules in workspace causing type conflicts
export default defineConfig(({ mode }) => ({
  base: mode === 'production' ? '/english-mentor-buddy/' : '/',
  server: {
    // bind to localhost and use a different port to avoid conflicts with other local services
    host: 'localhost',
    port: 5173,
    // HTTPS is enabled via basicSsl plugin
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
    basicSsl(),
    mode === "development" && componentTagger(),
  ].filter(Boolean),
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
}));