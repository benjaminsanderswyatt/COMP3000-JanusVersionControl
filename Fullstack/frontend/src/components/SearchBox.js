import React, { useState } from 'react';

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
    <div style={styles.container}>
      <form onSubmit={handleSearch} style={styles.searchBox}>
        <input
          type="text"
          value={query}
          onChange={handleInputChange}
          onKeyDown={handleKeyPress}
          placeholder={searchingWhat ? `Search ${searchingWhat}...` : 'Search...'}
          style={styles.input}
        />
      </form>
    </div>
  );
};

const styles = {
  container: {
    display: "flex",
    justifyContent: "center",
    width: "100%",
    maxWidth: "1000px",
  },
  input: {
    border: "none",
    background: "none",
    outline: "none",
    fontSize: "1rem",
    padding: "8px 0px",
    width: "100%",
  },
  searchBox: {
    display: "flex",
    alignItems: "center",
    backgroundColor: "var(--button)",
    borderRadius: "25px",
    padding: "0px 12px",
    boxShadow: "0 1px 0 0 rgba(0, 0, 0, 0.1)",
    width: "100%",
    minWidth: "180px",
    height: "min-content",
    border: "var(--primary) thin solid",
  },
  


};


export default SearchBox;

