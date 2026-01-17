import React, { useState, useRef, useEffect } from 'react';
import { ArrowLeft, ArrowRight } from 'lucide-react';
import ProductCard from './ProductCard';
import '../css/CarouselSection.css'; 

const CarouselSection = ({ title, subtitle, items, onViewProduct }) => {
  const scrollRef = useRef(null);
  const [showLeftBtn, setShowLeftBtn] = useState(false);
  const [showRightBtn, setShowRightBtn] = useState(true);

  const checkScroll = () => {
    if (scrollRef.current) {
      const { scrollLeft, scrollWidth, clientWidth } = scrollRef.current;
      setShowLeftBtn(scrollLeft > 5);
      setShowRightBtn(scrollLeft < scrollWidth - clientWidth - 5);
    }
  };

  const scroll = (direction) => {
    if (scrollRef.current) {
      const containerWidth = scrollRef.current.clientWidth;
      
      const scrollAmount = containerWidth / 4; 
      
      scrollRef.current.scrollBy({
        left: direction === 'left' ? -scrollAmount : scrollAmount,
        behavior: 'smooth'
      });
    }
  };

  useEffect(() => {
    const currentRef = scrollRef.current;
    if (currentRef) {
      checkScroll();
      currentRef.addEventListener('scroll', checkScroll);
      window.addEventListener('resize', checkScroll);
    }
    return () => {
      if (currentRef) {
        currentRef.removeEventListener('scroll', checkScroll);
        window.removeEventListener('resize', checkScroll);
      }
    };
  }, [items]);

  return (
    <section className="section">
      <div className="section-header">
        <h2 className="section-title">{title}</h2>
        <p className="section-subtitle">{subtitle}</p>
      </div>
      <div className="carousel-container">
        <button className={`nav-arrow left ${!showLeftBtn ? 'hidden' : ''}`} onClick={() => scroll('left')}>
          <ArrowLeft size={24} />
        </button>
        <div className="products-scroll" ref={scrollRef}>
          {items.map(item => (
            <div className="carousel-item" key={item.id}>
               <ProductCard item={item} onView={onViewProduct} />
            </div>
          ))}
        </div>
        <button className={`nav-arrow right ${!showRightBtn ? 'hidden' : ''}`} onClick={() => scroll('right')}>
          <ArrowRight size={24} />
        </button>
      </div>
    </section>
  );
};

export default CarouselSection;