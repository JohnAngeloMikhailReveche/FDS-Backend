import React from 'react';
import ProductCard from './ProductCard';
import '../css/ItemList.css'; 

const ItemList = ({ items, onViewProduct, onEdit, isAdmin }) => {
  if (!items || items.length === 0) {
    return (
      <div style={{ width: '100%', padding: '20px', textAlign: 'center', color: '#888' }}>
        <p>No items available in this category.</p>
      </div>
    );
  }

  return (
    <div className="item-list-grid">
      {items.map(item => (
        <ProductCard 
          key={item.id} 
          item={item} 
          onView={onViewProduct}
          onEdit={onEdit} 
          isAdmin={isAdmin}
        />
      ))}
    </div>
  );
};

export default ItemList;