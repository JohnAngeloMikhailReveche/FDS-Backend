import React from 'react';
import '../css/ItemFilter.css';

const ItemFilter = ({ categories, activeCategory, onCategoryClick }) => {
  return (
    <div className="category-pill">
      <div className="filter-scroll">
        {categories.map((cat) => (
          <button 
            key={cat.id}
            className={`filter-btn ${activeCategory === cat.id ? 'active' : ''}`}
            onClick={() => onCategoryClick(cat.id)}
          >
            {cat.label}
          </button>
        ))}
      </div>
    </div>
  );
};

export default ItemFilter;