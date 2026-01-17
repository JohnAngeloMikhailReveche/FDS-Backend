import React, { useState } from 'react';
import { useAuth } from '../context/AuthContext';
import '../css/PopupMenu.css';

const PopupMenu = ({ isOpen, onClose, navigateTo, user: passedUser }) => {
    const { isAuthenticated, user: contextUser, logout } = useAuth();
    const [isClosing, setIsClosing] = useState(false);

    const user = passedUser || contextUser;

    if (!isOpen && !isClosing) return null;

    const handleItemClick = (page) => {
        if (navigateTo) navigateTo(page);
        onClose();
    };

    const handleLogout = () => {
        logout();
        if (navigateTo) navigateTo('home');
        onClose();
    };

    const handleClose = () => {
        setIsClosing(true);
        setTimeout(() => {
            setIsClosing(false);
            onClose();
        }, 300);
    };

    const userRole = user?.role?.toLowerCase();
    const isAdmin = userRole === 'admin' || userRole === 'superadmin';

    return (
        <div
            className={`popup-overlay ${isOpen && !isClosing ? 'open' : 'closing'}`}
            onClick={handleClose}
        >
            <div
                className={`popup-content ${isOpen && !isClosing ? 'open' : 'closing'}`}
                onClick={(e) => e.stopPropagation()}
            >
                <ul className="popup-list">
                    {isAuthenticated ? (
                        <>
                            <li style={{
                                fontWeight: 'normal',
                                color: '#888',
                                fontSize: '0.9rem',
                                padding: '10px 0',
                                cursor: 'default'
                            }}>
                                {user?.fullName} ({user?.role})
                            </li>

                            {/* ===== ADMIN MENU ===== */}
                            {isAdmin ? (
                                <>
                                    <li onClick={() => handleItemClick('profile')}>
                                        Profile
                                    </li>
                                    <li onClick={() => handleItemClick('admin')}>
                                        Admin Mode
                                    </li>

                                    <li className="logout-item" onClick={handleLogout}>
                                        Log Out
                                    </li>
                                </>
                            ) : (
                                <>
                                    <li onClick={() => handleItemClick('profile')}>Profile</li>
                                    <li onClick={() => handleItemClick('menu')}>Menu</li>
                                    <li onClick={() => handleItemClick('orders')}>Orders</li>
                                    <li onClick={() => handleItemClick('wallet')}>Wallet</li>
                                    <li onClick={() => handleItemClick('about')}>About KapeBara</li>
                                    <li className="logout-item" onClick={handleLogout}>Log Out</li>
                                </>
                            )}
                        </>
                    ) : (
                        <>
                            <li onClick={() => handleItemClick('login')}>Log In</li>
                            <li onClick={() => handleItemClick('register')}>Sign Up</li>
                            <li onClick={() => handleItemClick('menu')}>Menu</li>
                            <li onClick={() => handleItemClick('about')}>About KapeBara</li>
                        </>
                    )}
                </ul>
            </div>
        </div>
    );
};

export default PopupMenu;