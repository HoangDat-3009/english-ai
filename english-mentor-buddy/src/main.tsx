import { createRoot } from 'react-dom/client';
import App from './App.tsx';
import './index.css';
import { AuthProvider } from '@/components/AuthContext.tsx'; // Nhập AuthProvider
import { Auth0Provider } from '@auth0/auth0-react';
import authConfig from '@/config/auth0';

// Tạo root và render ứng dụng
createRoot(document.getElementById('root')!).render(
    <Auth0Provider
        domain={authConfig.domain}
        clientId={authConfig.clientId}
        authorizationParams={{ redirect_uri: authConfig.redirectUri }}
    >
        <AuthProvider>
            <App />
        </AuthProvider>
    </Auth0Provider>
);