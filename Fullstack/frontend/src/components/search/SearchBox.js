import React, { useState } from 'react';

import styles from "../../styles/components/search/SearchBox.module.css";


const SearchBox = ({ searchingWhat, onSearch }) => {
  const [query, setQuery] = useState('');


  const handleInputChange = (event) => {
    const newQuery = event.target.value;
    setQuery(newQuery);
    // Search on every change
    onSearch(newQuery);
  };




  return (
    <div className={styles.container}>
      <form className={styles.searchBox}>
        <input
          type="text"
          value={query}
          onChange={handleInputChange}
          placeholder={searchingWhat ? `Search ${searchingWhat}...` : 'Search...'}
          className={styles.input}
        />
      </form>
    </div>
  );

};


export default SearchBox;