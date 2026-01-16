// src/context/AuthContext.jsx
import { createContext, useState, useContext } from 'react';

const AuthContext = createContext(null);

export function AuthProvider({ children }) {
    const [authState, setAuthState] = useState({
        token: null,
        user: null,
        isAuthenticated: false
    });

    const login = (token, user) => {
        setAuthState({
            token,
            user,
            isAuthenticated: true
        });
    };

    const logout = () => {
        setAuthState({
            token: null,
            user: null,
            isAuthenticated: false
        });
    };

    const value = {
        ...authState,
        login,
        logout
    };

    return (
        <AuthContext.Provider value={value}>
            {children}
        </AuthContext.Provider>
    );
}

export function useAuth() {
    const context = useContext(AuthContext);
    if (!context) {
        throw new Error('useAuth must be used within AuthProvider');
    }
    return context;
}