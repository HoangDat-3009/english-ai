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
        target: "http://localhost:5283", // Local API endpoint
        changeOrigin: true,
        secure: false,
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