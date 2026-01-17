import React, { useState, useEffect } from 'react';
import '../css/AdminNavbar.css'; 
import AdminPopupMenu from './AdminPopupMenu'; 
import { Bell, Menu as MenuIcon } from 'lucide-react';
import logo from '../assets/logo.png'; 

const AdminNavbar = ({ currentPage, navigateTo }) => {
  const [isMenuOpen, setIsMenuOpen] = useState(false);

  // prevents background scrolling when menu is open
  useEffect(() => {
    if (isMenuOpen) {
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => { document.body.style.overflow = 'unset'; };
  }, [isMenuOpen]);

  const toggleMenu = () => setIsMenuOpen(!isMenuOpen);

  return (
    <>
      <nav className="admin-navbar">
        <div className="logo-container" onClick={(e) => navigateTo && navigateTo('dashboard', e)}>
          <img 
            src={logo} 
            alt="Kapebara Admin" 
            className="logo-img"
            onError={(e) => {e.target.style.display='none'; e.target.nextSibling.style.display='block'}} 
          />
          <span className="logo-text" style={{display: 'none'}}>Kapebara</span>
        </div>

        {/* desktop text navi */}
        <div className="nav-links-center hidden-mobile">
          <a 
            href="#" 
            className={`nav-link ${currentPage === 'orders' ? 'active' : ''}`} 
            onClick={(e) => navigateTo('orders', e)}
          >
            Orders
          </a>
          <a 
            href="#" 
            className={`nav-link ${currentPage === 'delivery' ? 'active' : ''}`} 
            onClick={(e) => navigateTo('delivery', e)}
          >
            Delivery
          </a>
        </div>

        {/* right icons */}
        <div className="nav-icons-right">
          {/* notif bell */}
          <button type="button" className="icon-btn" aria-label="Notifications">
            <Bell size={24} fill="currentColor" />
          </button>

          {/* menu button */}
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

    {/* popup */}
      <AdminPopupMenu 
        isOpen={isMenuOpen} 
        onClose={() => setIsMenuOpen(false)} 
        navigateTo={navigateTo}
      />
    </>
  );
};

export default AdminNavbar;