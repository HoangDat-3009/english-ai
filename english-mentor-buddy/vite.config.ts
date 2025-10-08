import { defineConfig } from "vite";
import react from "@vitejs/plugin-react-swc";
import path from "path";
import { componentTagger } from "lovable-tagger";

// https://vitejs.dev/config/
export default defineConfig(({ mode }) => ({
  base: mode === 'production' ? '/english-mentor-buddy/' : '/',
  server: {
    // bind to localhost and use a different port to avoid conflicts with other local services
    host: 'localhost',
    port: 5173,
    // use plain HTTP on localhost (Auth0 treats localhost as secure for dev)
    https: false as any,
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
    mode === "development" && componentTagger(),
  ].filter(Boolean),
  resolve: {
    alias: {
      "@": path.resolve(__dirname, "./src"),
    },
  },
}));