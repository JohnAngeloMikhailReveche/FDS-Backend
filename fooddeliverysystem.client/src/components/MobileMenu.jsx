import React from 'react';
import { X, User, LogOut } from 'lucide-react';
import '../css/MobileMenu.css';

const MobileMenu = ({ isOpen, onClose, navigateTo }) => {
  if (!isOpen) return null;

  return (
    <div className={`mobile-menu-overlay ${isOpen ? 'open' : ''}`} onClick={onClose}>
      <div className="mobile-menu-content" onClick={(e) => e.stopPropagation()}>
        
        <div className="mobile-menu-header">
          <span className="mobile-menu-title">Menu</span>
          <button className="close-btn" onClick={onClose}>
            <X size={28} />
          </button>
        </div>

        <nav className="mobile-nav-links">
          <a href="#" className="mobile-link" onClick={(e) => { navigateTo('home', e); onClose(); }}>
            Home
          </a>
          <a href="#" className="mobile-link" onClick={(e) => { navigateTo('menu', e); onClose(); }}>
            Menu
          </a>
          <a href="#" className="mobile-link" onClick={(e) => { onClose(); }}>
            Your Order
          </a>
        </nav>

        {/* footer */}
        <div className="mobile-menu-footer">
          <button className="mobile-action-btn">
            <User size={20} />
            <span>Profile</span>
          </button>
          <button className="mobile-action-btn logout">
            <LogOut size={20} />
            <span>Log Out</span>
          </button>
        </div>

      </div>
    </div>
  );
};

export default MobileMenu;