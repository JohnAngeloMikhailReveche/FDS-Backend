import React, { useEffect, useState } from 'react';
import { useAuth } from '../context/AuthContext';
import '../css/AdminPopupMenu.css';

const AdminPopupMenu = ({ isOpen, onClose, navigateTo }) => {
    const [shouldRender, setShouldRender] = useState(false);
    const { user } = useAuth();

    useEffect(() => {
        if (isOpen) {
            setShouldRender(true);
        } else {
            const timer = setTimeout(() => setShouldRender(false), 300);
            return () => clearTimeout(timer);
        }
    }, [isOpen]);

    if (!shouldRender) return null;

    const handleItemClick = (page) => {
        if (navigateTo) navigateTo(page);
        onClose();
    };

    const userRole = user?.role?.toLowerCase();
    const isSuperAdmin = userRole === 'superadmin';

    return (
        <div
            className={`admin-popup-overlay ${isOpen ? 'open' : 'closing'}`}
            onClick={onClose}
        >
            <div
                className={`admin-popup-content ${isOpen ? 'open' : 'closing'}`}
                onClick={(e) => e.stopPropagation()}
            >
                <ul className="admin-popup-list">
                    <li onClick={() => handleItemClick('manage_users')}>Manage Users</li>

                    {/* SuperAdmin only */}
                    {isSuperAdmin && (
                        <li onClick={() => handleItemClick('create_admin')}>
                            Create Admin Account
                        </li>
                    )}

                    <li onClick={() => handleItemClick('create_rider')}>
                        Create Rider Account
                    </li>

                    {/* For mobile only */}
                    <li className="mobile-only" onClick={() => handleItemClick('orders')}>Orders</li>
                    <li className="mobile-only" onClick={() => handleItemClick('delivery')}>Delivery</li>

                    <li onClick={() => handleItemClick('manage-history')}>Manage History</li>
                    <li onClick={() => handleItemClick('transaction-history')}>Transaction History</li>

                    <li className="logout-item" onClick={() => handleItemClick('logout')}>Log Out</li>
                </ul>
            </div>
        </div>
    );
};

export default AdminPopupMenu;