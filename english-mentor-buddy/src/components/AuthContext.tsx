import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

interface User {
    id: number;
    email: string;
    password: string;
    englishlevel: string;
    tendangnhap: string;
    ngaytaotaikhoan: string;
    auth0_sub?: string;
    avatar_url?: string;
    email_verified?: boolean;
}

interface AuthContextType {
    user: User | null;
    login: (userData: User) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

// Tài khoản admin mặc định
const defaultAdminUser: User = {
    id: 1,
    email: 'admin@example.com',
    password: '123456',
    englishlevel: 'advanced',
    tendangnhap: 'Admin',
    ngaytaotaikhoan: new Date().toISOString()
};

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
    const [user, setUser] = useState<User | null>(null);

    useEffect(() => {
        const storedUser = localStorage.getItem('user');
        if (storedUser) {
            setUser(JSON.parse(storedUser));
        }
        // Loại bỏ việc tự động đăng nhập admin mặc định
        // else {
        //     setUser(defaultAdminUser);
        //     localStorage.setItem('user', JSON.stringify(defaultAdminUser));
        // }
    }, []);

    const login = (userData: User) => {
        setUser(userData);
        localStorage.setItem('user', JSON.stringify(userData));
    };

    const logout = () => {
        setUser(null);
        localStorage.removeItem('user');
    };

    return (
        <AuthContext.Provider value={{ user, login, logout }}>
            {children}
        </AuthContext.Provider>
    );
};

export const useAuth = () => {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within an AuthProvider');
    }
    return context;
};