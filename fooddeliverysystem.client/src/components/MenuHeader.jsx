import React from 'react';
import '../css/MenuHeader.css'; 
import ItemFilter from './ItemFilter';
import SearchBar from './SearchBar';
import AddItemButton from './AddItemButton'; 

const MenuHeader = ({ 
  categories, 
  activeCategory, 
  onCategoryClick, 
  isAdmin,      
  onAddItem     
}) => {
  
  return (
    <div className="menu-controls-wrapper">
      <div className="menu-controls-container">
        
        <div className="menu-left-section">
           <ItemFilter 
            categories={categories} 
            activeCategory={activeCategory} 
            onCategoryClick={onCategoryClick} 
          />
        </div>

        <div className="menu-actions-right">
          <div className="search-wrapper">
             <SearchBar placeholder="Search" />
          </div>
          
          {isAdmin && (
            <AddItemButton onClick={onAddItem} />
          )}
        </div>

      </div>
    </div>
  );
};

export default MenuHeader;