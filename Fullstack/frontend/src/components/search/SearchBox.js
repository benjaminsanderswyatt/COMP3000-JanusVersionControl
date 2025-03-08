
import React from 'react';

import styles from "../../styles/components/search/SearchBox.module.css";


const SearchBox = ({ searchingWhat, value, onChange, onSubmit }) => {
  const handleSubmitForm = (e) => {
    e.preventDefault();
    if (onSubmit) { // Ensure its set
      onSubmit(e);
    }
  };

  return (
    <div className={styles.container}>
      <form className={styles.searchBox} onSubmit={handleSubmitForm}>
        <input
          type="text"
          value={value}
          onChange={(e) => onChange(e.target.value)}
          placeholder={searchingWhat ? `Search ${searchingWhat}...` : 'Search...'}
          className={styles.input}
        />
      </form>
    </div>
  );
};

export default SearchBox;