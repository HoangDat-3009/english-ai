import React, { createContext, useContext, useState, useEffect, ReactNode } from 'react';

// New User interface matching backend UserDto
export interface User {
    userId: number;
    email: string;
    username?: string;
    fullName?: string;
    avatar?: string;
    role: string;
    status: string;
    emailVerified: boolean;
}

interface AuthContextType {
    user: User | null;
    login: (userData: User) => void;
    logout: () => void;
}

const AuthContext = createContext<AuthContextType | undefined>(undefined);

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