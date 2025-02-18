import React, { useState } from 'react';
import Repository from '../../components/Repo/Repository';
import { useNavigate, useLocation } from 'react-router';
import RepoBar from '../../components/Repo/RepoBar';

import SearchBox from '../../components/SearchBox';

import { useAuth  } from '../../contexts/AuthContext';


const Repositories = () => {
  const navigate = useNavigate();


  const handleSearch = (query) => {
    console.log('Searching repositories for:', query);
    // Search
  };


  const handleEnterRepo = (name) => {
    navigate(`/repository/${name}`);
  }


  const CreateNewRepo = () => {
    navigate("/repository/create");
  };

  return (
    <div style={styles.container}>

      <header style={styles.header}>
        <button style={styles.button} onClick={() => CreateNewRepo()}>New Repository</button>

        <SearchBox searchingWhat="repositories" onSearch={() => handleSearch()} />

      </header>


      {/* TODO For loop loads repos */}
      <Repository enterRepo={() => handleEnterRepo("RepoNameHere")}/>
      <Repository/>
      <Repository/>
      


      
    </div>
  );
};

const styles = {
  container: {
    background: "var(--card)",
    width: "90%",
    justifyItems: "center",
    display: "flex",
    flexDirection: "column",
    gap: "18px",
    alignItems: "center",
    borderRadius: "8px",
    marginTop: "20px",
    justifyItems: "center",
    paddingBottom: "18px",
    height: "fit-content",
  },
  header: {
    display: "flex",
    width: "100%",
    background: "var(--accent)",
    alignItems: "center",
    borderBottom: "var(--border) solid 1px",
    padding: "4px 10px",
    gap: "10px",
    justifyContent: "center",
    minHeight: "46px",
    borderRadius: "8px 8px 0px 0px",
  },
  button: {
    boxShadow: "0 1px 0 0 rgba(0, 0, 0, 0.1)",
    backgroundColor: "var(--button)",
    color: "var(--lighttext)",
    fontSize: "1rem",
    border: "var(--primary) thin solid",
    height: "100%",
    padding: "6px 12px",
    borderRadius: "8px",
    cursor: "pointer",
    whiteSpace: "nowrap",
  },
  
}


export default Repositories;
  