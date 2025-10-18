import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import './index.css';
import { AuthProvider } from '@/components/AuthContext.tsx'; // Nhập AuthProvider
import { Toaster } from 'sonner';

// Tạo root và render ứng dụng
createRoot(document.getElementById('root')!).render(
    <AuthProvider>
        <App />
        <Toaster position="top-right" richColors closeButton />
    </AuthProvider>
);