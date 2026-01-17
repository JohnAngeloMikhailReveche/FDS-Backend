import React from 'react';
import '../css/ProductCard.css'; 

const ProductCard = ({ item, onView, onEdit, isAdmin = false }) => {
  const getDisplayPrice = () => {
    if (item.sizes && Array.isArray(item.sizes) && item.sizes.length > 0) {
      const prices = item.sizes
        .map(size => parseFloat(size.price))
        .filter(p => !isNaN(p)); 

      if (prices.length > 0) {
        const minPrice = Math.min(...prices);
        return `Starts at ₱${minPrice.toFixed(2)}`;
      }
    }

    if (item.price) {
      const priceStr = item.price.toString();
      return priceStr.startsWith('₱') 
        ? `Starts at ${priceStr}` 
        : `Starts at ₱${priceStr}`;
    }

    return 'Price Unavailable';
  };

  return (
    <div className="product-card">
      <div className="img-container">
        <img 
          src={item.img || 'https://via.placeholder.com/150'} 
          alt={item.name} 
          className="product-img" 
        />
      </div>
      <div className="card-details">
        <h3 className="product-name">{item.name}</h3>
        <p className="product-price">{getDisplayPrice()}</p>
        
        <div className="card-buttons">
          {isAdmin ? (
            // admin: edit Button
            <button className="view-btn" onClick={() => onEdit(item)}>
              Edit
            </button>
          ) : (
            // user: view Button
            <button className="view-btn" onClick={() => onView(item)}>
              View
            </button>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProductCard;