import React, { useState, useEffect } from 'react';
import ItemList from '../components/ItemList'; 
import MenuHeader from '../components/MenuHeader';

const Menu = ({ products, onViewProduct }) => {
  const [activeCategory, setActiveCategory] = useState('classic');
  const menuItems = products || {};

  const categories = [
    { id: 'classic', label: 'Classic Coffee Series' },
    { id: 'frappe', label: 'Frappe' },
    { id: 'latte', label: 'Latte' },
    { id: 'specialty', label: 'Specialty Drinks' },
    { id: 'baked', label: 'Cupcakes & Baked Goods' },
  ];

  useEffect(() => {
    const handleScroll = () => {
      const headerOffset = 250; 
      const scrollPosition = window.scrollY + headerOffset;
      for (const cat of categories) {
        const section = document.getElementById(cat.id);
        if (section) {
          const { offsetTop, offsetHeight } = section;
          if (scrollPosition >= offsetTop && scrollPosition < offsetTop + offsetHeight) {
            setActiveCategory(cat.id);
          }
        }
      }
    };
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);

  const scrollToSection = (id) => {
    const element = document.getElementById(id);
    if (element) {
      const headerOffset = 180;
      const elementPosition = element.getBoundingClientRect().top;
      const offsetPosition = elementPosition + window.pageYOffset - headerOffset;
      window.scrollTo({ top: offsetPosition, behavior: "smooth" });
      setActiveCategory(id);
    }
  };

  return (
    <div className="menu-page">
      <MenuHeader 
        categories={categories} 
        activeCategory={activeCategory} 
        onCategoryClick={scrollToSection} 
      />

      <main className="menu-content">
        {menuItems.classic && menuItems.classic.length > 0 && (
          <div id="classic" className="menu-section">
            <h2 className="menu-section-title">Classic Coffee</h2>
            <p className="menu-section-subtitle">Smooth cold brews to energize your day.</p>
            <ItemList items={menuItems.classic} onViewProduct={onViewProduct} />
          </div>
        )}

        {menuItems.frappe && menuItems.frappe.length > 0 && (
          <div id="frappe" className="menu-section">
            <h2 className="menu-section-title">Frappe</h2>
            <p className="menu-section-subtitle">Ice blended perfection.</p>
            <ItemList items={menuItems.frappe} onViewProduct={onViewProduct} />
          </div>
        )}

        {menuItems.latte && menuItems.latte.length > 0 && (
          <div id="latte" className="menu-section">
            <h2 className="menu-section-title">Latte</h2>
            <p className="menu-section-subtitle">Creamy and smooth.</p>
            <ItemList items={menuItems.latte} onViewProduct={onViewProduct} />
          </div>
        )}

        {menuItems.specialty && menuItems.specialty.length > 0 && (
          <div id="specialty" className="menu-section">
            <h2 className="menu-section-title">Specialty Drinks</h2>
            <p className="menu-section-subtitle">Something unique for you.</p>
            <ItemList items={menuItems.specialty} onViewProduct={onViewProduct} />
          </div>
        )}

        {menuItems.baked && menuItems.baked.length > 0 && (
          <div id="baked" className="menu-section">
            <h2 className="menu-section-title">Cupcakes & Baked Goods</h2>
            <p className="menu-section-subtitle">Freshly baked everyday.</p>
            <ItemList items={menuItems.baked} onViewProduct={onViewProduct} />
          </div>
        )}
      </main>
    </div>
  );
};

export default Menu;