import React, { useState } from 'react';

import styles from "../styles/Components/SearchBox.module.css";

const SearchBox = ({ searchingWhat, onSearch }) => {
  const [query, setQuery] = useState('');


  const handleInputChange = (event) => {
    setQuery(event.target.value);
  };

  const handleSearch = (event) => {
    if (event){
      event.preventDefault();
    }

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
      </form>
    </div>
  );
};


export default SearchBox;

