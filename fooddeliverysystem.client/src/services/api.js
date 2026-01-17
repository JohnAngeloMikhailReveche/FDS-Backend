// Change this to your actual backend URL when deployed
const API_BASE_URL = 'http://localhost:3000';

const groupByCategory = (items) => {
  const grouped = {
    classic: [],
    frappe: [],
    latte: [],
    specialty: [],
    baked: [],
    snacks: [], 
    bestsellers: []
  };

  if (!Array.isArray(items)) return grouped;

  items.forEach(item => {
    let key = '';
    const catLower = item.category?.toLowerCase() || '';

    if (catLower.includes('classic')) key = 'classic';
    else if (catLower.includes('frappe')) key = 'frappe';
    else if (catLower.includes('latte')) key = 'latte';
    else if (catLower.includes('specialty')) key = 'specialty';
    else if (catLower.includes('baked') || catLower.includes('cupcake')) key = 'baked';
    else if (catLower.includes('snack')) key = 'snacks';
    
    if (key && grouped[key]) {
      grouped[key].push(item);
    }
  });

  return grouped;
};

// --- API METHODS ---
export const api = {
  // GET ALL PRODUCTS
  fetchProducts: async () => {
    try {
      const response = await fetch(`${API_BASE_URL}/items`);
      if (!response.ok) throw new Error('Failed to fetch products');
      const data = await response.json();
      return groupByCategory(data); 
    } catch (error) {
      console.error("API Error:", error);
      return null; 
    }
  },

  // CREATE ITEM
  createItem: async (productData) => {
    const response = await fetch(`${API_BASE_URL}/create_item`, {
      method: 'POST',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(productData),
    });
    if (!response.ok) throw new Error(`Create failed: ${response.status}`);
    return await response.json();
  },

  // UPDATE ITEM
  updateItem: async (productData) => {
    const response = await fetch(`${API_BASE_URL}/update_item`, {
      method: 'PUT',
      headers: { 'Content-Type': 'application/json' },
      body: JSON.stringify(productData),
    });
    if (!response.ok) throw new Error(`Update failed: ${response.status}`);
    return await response.json();
  },

  // DELETE ITEM
  deleteItem: async (itemId) => {
    const response = await fetch(`${API_BASE_URL}/delete_item/${itemId}`, {
      method: 'DELETE',
    });
    if (!response.ok) throw new Error(`Delete failed: ${response.status}`);
    return true;
  },

  // UPLOAD PHOTO
  uploadPhoto: async (itemId, file) => {
    const formData = new FormData();
    formData.append('item_id', itemId);
    formData.append('productImage', file);

    const response = await fetch(`${API_BASE_URL}/upload_photo`, {
      method: 'POST',
      body: formData,
    });
    if (!response.ok) throw new Error(`Upload failed: ${response.status}`);
    return await response.json();
  }
};