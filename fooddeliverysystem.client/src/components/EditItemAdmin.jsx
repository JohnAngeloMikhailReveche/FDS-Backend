import React, { useState, useEffect, useRef } from 'react';
import '../css/EditItemAdmin.css';
import arrowLeftIcon from '../assets/arrow_left.svg';
import deleteIcon from '../assets/delete.svg';
import { api } from '../services/api'; 

const FIXED_CATEGORIES = [
  'Classic Coffee Series', 
  'Frappe', 
  'Latte', 
  'Specialty Drinks', 
  'Cupcakes & Baked Goods', 
  'Snacks'
];

const EditItemAdmin = ({ product, onClose, onSave, onDelete }) => {
  const [formData, setFormData] = useState({ name: '', description: '', img: '' });
  const [imageFile, setImageFile] = useState(null);
  const [selectedCategory, setSelectedCategory] = useState('');  
  const [sizes, setSizes] = useState([]);
  const [isDirty, setIsDirty] = useState(false); 
  const fileInputRef = useRef(null);
  const isFoodCategory = (cat) => {
    const foodKeywords = ['Cupcakes & Baked Goods', 'Snacks', 'Pastries', 'Food', 'Dessert'];
    return foodKeywords.includes(cat);
  };

  // initialization
  useEffect(() => {
    if (product) {
      setFormData({
        name: product.name || '',
        description: product.description || '',
        img: product.img || '',
      });

      // category
      // if the product has a category that matches the list, select it.
      if (product.category && FIXED_CATEGORIES.includes(product.category)) {
        setSelectedCategory(product.category);
      } else if (product.category) {
        setSelectedCategory(''); 
      }

      // sizes
      if (product.sizes && product.sizes.length > 0) {
        const initialSizes = product.sizes.map((s, index) => ({ ...s, tempId: Date.now() + index }));
        setSizes(initialSizes);
      } else {
        // check "isFood" based on the product's existing category or default
        if (isFoodCategory(product.category)) {
             setSizes([{ tempId: Date.now(), label: '', price: '' }]);
        } else {
             setSizes([
                 { tempId: Date.now(), label: '16 oz', price: '' }, 
                 { tempId: Date.now() + 1, label: '22 oz', price: '' }
             ]);
        }
      }
    }
  }, [product]);

  const markDirty = () => setIsDirty(true);
  const handleInputChange = (e) => { setFormData(prev => ({ ...prev, [e.target.name]: e.target.value })); markDirty(); };

  const handleCategorySelect = (cat) => { 
    setSelectedCategory(cat); 
    markDirty();

    if (isFoodCategory(cat)) {
        setSizes(prevSizes => {
            const isDefaultDrinkSizes = prevSizes.some(s => s.label === '16 oz' || s.label === '22 oz');
            if (isDefaultDrinkSizes) {
                return [{ tempId: Date.now(), label: '', price: '' }];
            }
            return prevSizes;
        });
    }
  };

  const handleSizeChange = (tempId, field, value) => {
    setSizes(prevSizes => prevSizes.map(size => size.tempId === tempId ? { ...size, [field]: value } : size));
    markDirty();
  };
  const handleAddSize = () => { setSizes([...sizes, { tempId: Date.now(), label: '', price: '' }]); markDirty(); };
  const handleRemoveSize = (tempId) => { setSizes(sizes.filter(size => size.tempId !== tempId)); markDirty(); };
  const handleChangePhoto = () => { fileInputRef.current.click(); };
  const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file) {
      setImageFile(file);
      const reader = new FileReader();
      reader.onloadend = () => { setFormData(prev => ({ ...prev, img: reader.result })); markDirty(); };
      reader.readAsDataURL(file);
    }
  };

  const handleBack = () => {
    if (isDirty) {
        if (window.confirm("You have unsaved changes. Are you sure you want to discard them?")) onClose();
    } else { onClose(); }
  };

  const handleSave = async () => {
    if (!formData.name.trim()) return alert("Product Name is required.");
    if (!selectedCategory) return alert("Please select a category.");
    if (sizes.length === 0) return alert("Please add at least one size/variant.");
    
    // Just use selectedCategory directly
    const finalCategory = selectedCategory;

    const cleanedSizes = sizes.map(({ tempId, ...rest }) => rest);

    try {
        const updateData = {
            item_id: product.id,
            name: formData.name,
            description: formData.description,
            category: finalCategory,
            sizes: cleanedSizes
        };

        await api.updateItem(updateData);

        if (imageFile) {
            await api.uploadPhoto(product.id, imageFile);
        }

        alert('Item updated successfully!');
        
        if (onSave) onSave(); 

        setIsDirty(false); 
        onClose();

    } catch (error) {
        console.error('Error updating product:', error);
        alert(`Failed to update item: ${error.message}`);
    }
  };

  const handleDelete = async () => {
    if (window.confirm("Are you sure you want to delete this product?")) {
        try {
            await api.deleteItem(product.id);
            if (onSave) onSave(); 
            onClose();
        } catch (error) {
            console.error('Error deleting product:', error);
            alert("Failed to delete item.");
        }
    }
  };

  const isFood = isFoodCategory(selectedCategory);

  return (
    <div className="edit-modal-overlay">
      <div className="edit-modal-container">
        <div className="edit-modal-body">
          <div className="edit-image-section">
            <div className="image-wrapper">
              {formData.img ? <img src={formData.img} alt={formData.name} className="product-preview-img" /> : <div className="placeholder-img">No Image</div>}
            </div>
            <button className="btn btn-brown btn-full-width" onClick={handleChangePhoto}>Change Photo</button>
            <input type="file" ref={fileInputRef} onChange={handleFileChange} style={{ display: 'none' }} accept="image/*" />
          </div>

          <div className="edit-form-section">
            <div className="form-group">
              <label>Product Name</label>
              <input type="text" name="name" value={formData.name} onChange={handleInputChange} className="form-input" placeholder="Enter product name" />
            </div>
            <div className="form-group">
              <label>Description</label>
              <textarea name="description" value={formData.description} onChange={handleInputChange} className="form-input textarea-input" rows="3" placeholder="Enter description" />
            </div>
            <div className="form-group">
                <label>Category</label>
                <div className="category-scroll-container">
                    <div className="category-grid">
                        {FIXED_CATEGORIES.map((cat) => (
                            <label key={cat} className="radio-label">
                                <input type="radio" name="category" checked={selectedCategory === cat} onChange={() => handleCategorySelect(cat)} />
                                <span className="custom-radio"></span>{cat}
                            </label>
                        ))}
                    </div>
                </div>
            </div>
            
            <div className="sizes-section">
               <div className="sizes-header-row">
                   <span className="col-header-left">
                       {isFood ? 'Item Details / Variant' : 'Available Sizes'}
                   </span>
                   <span className="col-header-right">Price</span>
                   <span className="col-header-spacer"></span> 
               </div>
               <div className="sizes-list">
                   {sizes.map((size) => (
                       <div key={size.tempId} className="size-row">
                           <div className="size-col-left">
                                <div className="tick-circle">✓</div>
                                <input 
                                    type="text" 
                                    value={size.label} 
                                    onChange={(e) => handleSizeChange(size.tempId, 'label', e.target.value)} 
                                    placeholder={isFood ? "Add item details here" : "Example: 16 oz"} 
                                    className="form-input size-input" 
                                />
                           </div>
                           <div className="size-col-right">
                               <input type="number" value={size.price} onChange={(e) => handleSizeChange(size.tempId, 'price', e.target.value)} placeholder="0.00" className="form-input size-input" />
                           </div>
                           <button className="btn-icon-red" onClick={() => handleRemoveSize(size.tempId)}>
                               <img src={deleteIcon} alt="Delete" className="icon-img-delete" />
                           </button>
                       </div>
                   ))}
               </div>
               <button className="btn btn-brown btn-full-width add-size-btn-styled" onClick={handleAddSize}>
                   {isFood ? '+ Add Variant' : '+ Add Size'}
               </button>
            </div>
            
          </div>
        </div>
        <div className="edit-modal-footer">
            <button className="btn btn-dark-brown" onClick={handleBack}>Back</button>
            <button className="btn btn-red" onClick={handleDelete}>Delete</button>
            <button className="btn btn-dark-brown" onClick={handleSave}>Save</button>
        </div>
      </div>
    </div>
  );
};

export default EditItemAdmin;