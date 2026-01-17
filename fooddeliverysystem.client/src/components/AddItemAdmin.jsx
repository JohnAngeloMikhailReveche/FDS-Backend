import React, { useState, useRef } from 'react';
import '../css/AddItemAdmin.css';
import deleteIcon from '../assets/delete.svg'; 
import { api } from '../services/api'; 

// id generator
const generateId = () => Date.now().toString(36) + Math.random().toString(36).substr(2);

const FIXED_CATEGORIES = [
  'Classic Coffee Series', 
  'Frappe', 
  'Latte', 
  'Specialty Drinks', 
  'Cupcakes & Baked Goods', 
  'Snacks'
];

const AddItemAdmin = ({ show, handleClose, onSave }) => { 
  const [productName, setProductName] = useState('');
  const [description, setDescription] = useState('');
  const [imageFile, setImageFile] = useState(null); 
  const [category, setCategory] = useState('');
  const fileInputRef = useRef(null); 
  
  // default size logic
  const [sizes, setSizes] = useState([
      { id: generateId(), label: '', price: '' },
  ]);

  if (!show) return null;

  const isFood = ['Cupcakes & Baked Goods', 'Snacks', 'Pastries', 'Food', 'Dessert'].includes(category);

  const resetForm = () => {
    setProductName('');
    setDescription('');
    setCategory('');
    setImageFile(null);
    setSizes([{ id: generateId(), label: '', price: '' }]);
  };

  const handleCloseAndReset = () => {
    resetForm();
    handleClose();
  }

  const handleImageUploadClick = () => {
    fileInputRef.current.click();
  };

  const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (file) {
      setImageFile(file);
    }
  };

  const handleSizeChange = (id, field, value) => {
    setSizes(prevSizes => prevSizes.map(size => 
        size.id === id ? { ...size, [field]: value } : size
    ));
  };

  const handleAddSize = () => {
    setSizes(prevSizes => [...prevSizes, { id: generateId(), label: '', price: '' }]);
  };

  const handleDeleteSize = (id) => {
    setSizes(prevSizes => {
        if (prevSizes.length > 1) { 
            return prevSizes.filter(size => size.id !== id);
        }
        return prevSizes;
    });
  };
  
  const handleSave = async () => {
    if (!productName.trim() || !description.trim() || !category || sizes.length === 0) {
      alert("Please fill out the Product Name, Description, Category, and at least one item detail/price.");
      return;
    }
    
    try {
        const newItemData = {
            productName,
            description,
            category, 
            sizes: sizes.map(size => ({
                label: size.label,
                price: parseFloat(size.price).toFixed(2), 
            }))
        };

        const createResult = await api.createItem(newItemData);
        const newItemId = createResult.item_id; 

        if (imageFile) {
            await api.uploadPhoto(newItemId, imageFile);
        }

        alert('Item added successfully!');
        
        if (onSave) onSave();
        handleCloseAndReset(); 

    } catch (error) {
        console.error("Error creating item:", error);
        alert(`Failed to add item: ${error.message}`);
    }
  };

  return (
    <div className="edit-modal-overlay">
      <div className="edit-modal-container">
        
        <div className="edit-modal-body">
          {/* image upload */}
          <div className="edit-image-section">
            <div className="image-wrapper">
              {imageFile ? (
                <img src={URL.createObjectURL(imageFile)} alt="Product Preview" className="product-preview-img" />
              ) : (
                <div className="placeholder-img">No Image Selected</div>
              )}
            </div>
            <button className="btn btn-brown btn-full-width" onClick={handleImageUploadClick}>
                {imageFile ? "Change Photo" : "Add Photo"}
            </button>
            <input type="file" ref={fileInputRef} onChange={handleFileChange} accept="image/*" style={{ display: 'none' }} />
          </div>

          {/* form */}
          <div className="edit-form-section">
            <div className="form-group">
              <label>Product Name</label>
              <input 
                type="text" 
                className="form-input" 
                value={productName} 
                onChange={(e) => setProductName(e.target.value)} 
                placeholder="Enter Product Name" 
              />
            </div>
            
            <div className="form-group">
              <label>Description</label>
              <textarea 
                className="form-input textarea-input" 
                rows="3" 
                value={description} 
                onChange={(e) => setDescription(e.target.value)} 
                placeholder="Enter description" 
              />
            </div>

            <div className="form-group">
                <label>Category</label>
                <div className="category-scroll-container">
                    <div className="category-grid">
                        {FIXED_CATEGORIES.map((cat) => (
                            <label key={cat} className="radio-label">
                                <input 
                                    type="radio" 
                                    name="category" 
                                    checked={category === cat} 
                                    onChange={() => setCategory(cat)} 
                                />
                                <span className="custom-radio"></span>{cat}
                            </label>
                        ))}
                    </div>
                </div>
            </div>
            
            <div className="sizes-section">
               <div className="sizes-header-row">
                   <span className="col-header-left">
                       {isFood ? 'Item Details' : 'Available Sizes'}
                   </span>
                   <span className="col-header-right">Price</span>
                   <span className="col-header-spacer"></span> 
               </div>
               <div className="sizes-list">
                   {sizes.map((size) => (
                       <div key={size.id} className="size-row">
                           <div className="size-col-left">
                                <div className="tick-circle">✓</div>
                                <input 
                                    type="text" 
                                    value={size.label} 
                                    onChange={(e) => handleSizeChange(size.id, 'label', e.target.value)} 
                                    placeholder={isFood ? "Details" : "Size (e.g. 16oz)"} 
                                    className="form-input size-input" 
                                />
                           </div>
                           <div className="size-col-right">
                               <input 
                                    type="number" 
                                    value={size.price} 
                                    onChange={(e) => handleSizeChange(size.id, 'price', e.target.value)} 
                                    placeholder="0.00" 
                                    className="form-input size-input" 
                                />
                           </div>
                           <button className="btn-icon-red" onClick={() => handleDeleteSize(size.id)}>
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

        {/* footer */}
        <div className="edit-modal-footer">
            <button className="btn btn-dark-brown" onClick={handleCloseAndReset}>Back</button>
            <button className="btn btn-dark-brown" onClick={handleSave}>Save</button>
        </div>

      </div>
    </div>
  );
};

export default AddItemAdmin;