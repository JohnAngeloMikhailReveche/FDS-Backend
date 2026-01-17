import React from 'react';
import '../css/SearchBar.css'; 
import { Search } from 'lucide-react';

const SearchBar = ({ placeholder = "Search" }) => {
  return (
    <div className="search-bar-container">
      <input 
        type="text" 
        className="search-input" 
        placeholder={placeholder} 
      />
      <Search size={20} className="search-icon"/>
    </div>
  );
};

export default SearchBar;