import React, { useState } from 'react';

import styles from "../../styles/components/search/SearchBox.module.css";


// Search on enter key
const SearchBoxEnter = ({ searchingWhat, onSearch }) => {
  const [query, setQuery] = useState('');

  
  
  const handleInputChange = (event) => {
    setQuery(event.target.value);
  };

  const handleSearch = (event) => {
    if (event){
      event.preventDefault();
    }

    if (query === "")
      return;

    // Search
    onSearch(query);
  };

  const handleKeyPress = (event) => {
    if (event.key === 'Enter') {
      handleSearch(event);
    }
  };

  
  
  return (
    <div className={styles.container}>
      <form onSubmit={handleSearch} className={styles.searchBox}>
        <input
          type="text"
          value={query}
          onChange={handleInputChange}
          onKeyDown={handleKeyPress}
          placeholder={searchingWhat ? `Search ${searchingWhat}...` : 'Search...'}
          className={styles.input}
        />
        <div className={styles.seperator}></div>
        <button className={styles.button} onClick={handleSearch}>Search</button>
      </form>
      
    </div>
  );
  
  
};


export default SearchBoxEnter;