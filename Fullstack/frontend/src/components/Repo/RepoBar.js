import React from 'react';
import SearchBox from '../../components/SearchBox';

const RepoBar = () => {

  const handleSearch = (query) => {
    console.log('Searching repositories for:', query);
    // Search
  };

  return (
    <div style={styles.container}>
      <div style={styles.searchBox}>
        <SearchBox searchingWhat="repositories" onSearch={handleSearch} />
      </div>

      <div style={styles.buttonHolder}>
        <button style={styles.button}>New Repository</button>
        <button style={styles.button}>New Repository</button>
        <button style={styles.button}>New Repository</button>
        <button style={styles.button}>New Repository</button>
        <button style={styles.button}>New Repository</button>

      </div>


    </div>
  );
};

const styles = {
  container: {
    width: "100%",
    background: "var(--secondary)",
    border: 'var(--border) thin solid',
    borderRadius: "20px",
    boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
    padding: "20px",
    maxWidth: "1000px",
    marginBottom: "30px",
    padding: "10px 20px",
    alignItems: "center",
  },
  searchBox: {
    height: "100%",
    width: "100%",
    marginBottom: "10px",
  },

  buttonHolder: {
    display: "flex",
    flexWrap: "wrap",
    gap: "10px",
    justifyContent: "center",
  },
  button: {
    boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
    backgroundColor: "var(--button)",
    color: "var(--text)",
    border: "none",
    padding: "10px 15px",
    borderRadius: "8px",
    cursor: "pointer",
    fontSize: "16px",
    whiteSpace: "nowrap",
  },

};


export default RepoBar;
