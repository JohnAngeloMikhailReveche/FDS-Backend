import React, { useEffect, useState } from 'react';
import CarouselSection from './CarouselSection'; 
import arrowLeftIcon from '../assets/arrow_left.svg'; 
import '../css/ProductModal.css';

const ProductModal = ({ product, onClose, relatedItems, onViewProduct }) => {
  const [isClosing, setIsClosing] = useState(false);

  const handleClose = () => {
    setIsClosing(true);
    setTimeout(() => {
      setIsClosing(false);
      onClose();
    }, 300); 
  };

  useEffect(() => {
    if (product) {
      const scrollbarWidth = window.innerWidth - document.documentElement.clientWidth;
      document.body.style.overflow = 'hidden';
      document.body.style.paddingRight = `${scrollbarWidth}px`;
      return () => {
        document.body.style.overflow = '';
        document.body.style.paddingRight = '';
      };
    }
  }, [product]);

  if (!product) return null;

  const filteredRelatedItems = relatedItems 
    ? relatedItems.filter(item => item.id !== product.id) 
    : [];

  const getDisplayPrice = () => {
      if (product.sizes && product.sizes.length > 0) {
          const prices = product.sizes.map(s => parseFloat(s.price)).filter(p => !isNaN(p));
          if (prices.length > 0) {
              return `Starts at ₱${Math.min(...prices).toFixed(2)}`;
          }
      }
      const priceStr = product.price ? product.price.toString() : '';
      if (!priceStr) return 'Price Unavailable';
      return priceStr.startsWith('₱') ? `Starts at ${priceStr}` : `Starts at ₱${priceStr}`;
  };

  return (
    <div 
      className={`modal-overlay ${isClosing ? 'closing' : ''}`} 
      onClick={handleClose}
    >
      <div className="modal-content" onClick={e => e.stopPropagation()}>
        
        <button className="modal-back-btn" onClick={handleClose}>
          <img src={arrowLeftIcon} alt="Back" />
        </button>

        {/* --- ANIMATION FIX: Add key={product.id} here --- */}
        {/* This forces the body to re-render (and animate) whenever the product changes */}
        <div className="modal-body" key={product.id}>
          
          <div className="product-split">
            <div className="modal-img-container">
              <img 
                src={product.img || 'https://via.placeholder.com/300'} 
                alt={product.name} 
                className="modal-img" 
              />
            </div>

            <div className="modal-info">
              <span className="modal-category-label">
                {product.category || 'Signature Series'}
              </span>

              <h1 className="modal-title">{product.name}</h1>
              <h2 className="modal-price">{getDisplayPrice()}</h2>
              
              <p className="modal-desc">
                {product.description || "Indulge in the perfect balance of flavors. This drink is crafted to perfection to make your day better."}
              </p>
              
              <button className="add-to-cart-btn">Add to Cart</button>
            </div>
          </div>

          {filteredRelatedItems.length > 0 && (
             <div className="modal-related-section">
                <CarouselSection 
                   title="Related Items"
                   subtitle="" 
                   items={filteredRelatedItems}
                   onViewProduct={onViewProduct}
                />
             </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default ProductModal;