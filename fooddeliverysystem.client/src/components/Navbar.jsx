import React, { useState, useEffect } from 'react';
import '../css/Navbar.css';
import PopupMenu from './PopupMenu';
import { ShoppingBag, Bell, Menu as MenuIcon } from 'lucide-react';
import logo from '../assets/logo.png';

const Navbar = ({ currentPage, navigateTo, user }) => {
    const [isMenuOpen, setIsMenuOpen] = useState(false);

    useEffect(() => {
        if (isMenuOpen) {
            document.body.style.overflow = 'hidden';
        } else {
            document.body.style.overflow = 'unset';
        }
        return () => { document.body.style.overflow = 'unset'; };
    }, [isMenuOpen]);

    const toggleMenu = () => setIsMenuOpen(!isMenuOpen);

    // Check if user is admin/superadmin AND currently on admin page
    const isAdminMode = user && (user.role === 'admin' || user.role === 'superadmin') && currentPage === 'admin';

    return (
        <>
            <nav className="navbar">
                {/* logo */}
                <div className="logo-container" onClick={(e) => navigateTo && navigateTo('home', e)}>
                    <img
                        src={logo}
                        alt="Kapebara"
                        className="logo-img"
                        onError={(e) => { e.target.style.display = 'none'; e.target.nextSibling.style.display = 'block' }}
                    />
                    <span className="logo-text" style={{ display: 'none' }}>Kapebara</span>
                </div>

                {/* text navi desktop */}
                <div className="nav-links-center hidden-mobile">
                    {isAdminMode ? (
                        // admin mode - show admin navigation (only when on admin page)
                        <>
                            <a href="#" className="nav-link" onClick={(e) => navigateTo('home', e)}>Home</a>
                            <a href="#" className="nav-link" onClick={(e) => navigateTo('admin', e)}>Admin Menu</a>
                            <a href="#" className="nav-link" onClick={(e) => navigateTo('orders', e)}>Orders</a>
                            <a href="#" className="nav-link" onClick={(e) => navigateTo('delivery', e)}>Delivery</a>
                        </>
                    ) : (
                        // user mode - show regular navigation (default for everyone including admins on non-admin pages)
                        <>
                            <a href="#" className="nav-link" onClick={(e) => navigateTo('home', e)}>Home</a>
                            <a href="#" className="nav-link" onClick={(e) => navigateTo('menu', e)}>Menu</a>
                            <a href="#" className="nav-link" onClick={(e) => navigateTo('order', e)}>Your Order</a>
                        </>
                    )}
                </div>

                {/* right icons */}
                <div className="nav-icons-right">
                    <button type="button" className="icon-btn" aria-label="Cart">
                        <ShoppingBag size={24} />
                    </button>

                    <button type="button" className="icon-btn" aria-label="Notifications">
                        <Bell size={24} fill="currentColor" />
                    </button>

                    {/* menu btn */}
                    <button
                        type="button"
                        className="menu-btn"
                        onClick={toggleMenu}
                        aria-label="Menu"
                    >
                        <MenuIcon size={28} />
                    </button>
                </div>
            </nav>

            <PopupMenu
                isOpen={isMenuOpen}
                onClose={() => setIsMenuOpen(false)}
                navigateTo={navigateTo}
                currentPage={currentPage}
                user={user}
            />
        </>
    );
};

export default Navbar;