import React from 'react';
import { useNavigate, useLocation } from 'react-router';
import SearchBox from '../../components/SearchBox';

import Button from '../Button';

const RepoBar = () => {
  const navigate = useNavigate();


  const handleSearch = (query) => {
    console.log('Searching repositories for:', query);
    // Search
  };


  const CreateNewRepo = () => {
    navigate("/createrepository");
  };

  return (
    <>
    {/* Header */}
    <header style={styles.header}>

    </header>

    <div style={styles.container}>
      <div style={styles.searchBox}>
        <SearchBox searchingWhat="repositories" onSearch={handleSearch} />
      </div>

      <div style={styles.buttonHolder}>
        <Button style={styles.button} onClick={() => CreateNewRepo()}>New Repository</Button>

        

      </div>


    </div>
    </>
  );
};

const styles = {
  header: {
    height: "100px",
    width: "100%",
    background: "var(--accent)",
  },
  container: {
    width: "100%",
    background: "var(--card)",
    border: 'var(--primary) thin solid',
    borderRadius: "8px",
    padding: "20px 10px",
    maxWidth: "1100px",
    margin: "10px 0px 20px 0px",
    padding: "8px 5px",
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
    background: "var(--darkcard)",
    padding: "5px 10px",
    borderRadius: "8px",
    border: "var(--primary) thin solid",
  },
  button: {
    boxShadow: "0 1px 0 0 rgba(0, 0, 0, 0.1)",
    backgroundColor: "var(--accent)",
    background: "var(--card)",
    color: "var(--text)",
    border: "none",
    border: "var(--secondary) thin solid",
    padding: "5px 15px",
    borderRadius: "8px",
    cursor: "pointer",
    fontSize: "16px",
    whiteSpace: "nowrap",
  },

};


export default RepoBar;
