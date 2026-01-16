import React from 'react';

const AddItemButton = ({ onClick }) => {
  const styles = {
    container: {
      flexShrink: 0, 
    },
    button: {
      backgroundColor: '#3e342f', 
      color: '#ffffff',
      border: 'none',
      borderRadius: '50px',
      padding: '0 30px', 
      minWidth: 'auto', 
      height: '55px', 
      fontSize: '1rem',
      fontWeight: '600',
      cursor: 'pointer',
      whiteSpace: 'nowrap',
      boxShadow: '0 2px 4px rgba(0,0,0,0.1)',
      transition: 'background-color 0.2s ease',
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
    }
  };

  return (
    <div style={styles.container}>
      <button 
        style={styles.button} 
        onClick={onClick}
        onMouseOver={(e) => e.currentTarget.style.backgroundColor = '#2D1B18'}
        onMouseOut={(e) => e.currentTarget.style.backgroundColor = '#3e342f'}
      >
        Add Item
      </button>
    </div>
  );
};

export default AddItemButton;