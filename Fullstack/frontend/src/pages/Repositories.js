import React, { useState } from 'react';
import Repository from '../components/Repo/Repository';
import { useNavigate, useLocation } from 'react-router';
import RepoBar from '../components/Repo/RepoBar';

import { GenAccessToken } from '../api/fetch/fetchPAT';

import SearchBox from '../components/SearchBox';

import { useAuth  } from '../contexts/AuthContext';


const Repositories = () => {
  const [tokenData , setTokenData] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);
  const navigate = useNavigate();


  const handleSearch = (query) => {
    console.log('Searching repositories for:', query);
    // Search
  };


  const { sessionExpired } = useAuth();

  const handleGenAccessToken = async () => {
    setLoading(true);
    setError(null);
    setTokenData (null);
    try {
      
      const ExpirationInHours = 30;

      const response = await GenAccessToken(ExpirationInHours, sessionExpired);
      console.log('Finished response Try');

      if (!response.success) {
        console.log('Failed: ' + response.message + ' token: ' + response.token);
        throw new Error(response.message);
      }

      setTokenData(response);
    } catch (err) {
      console.log('Catch');
      setError(err.message);
    } finally {
      console.log('Final');
      setLoading(false);
    }
  }



  const CreateNewRepo = () => {
    navigate("/repositories/create");
  };

  return (
    <div style={styles.container}>

      <header style={styles.header}>
        <button style={styles.button} onClick={() => CreateNewRepo()}>New Repository</button>

        <SearchBox searchingWhat="repositories" onSearch={() => handleSearch()} />

      </header>


      <div style={styles.repoHolder}>
        <Repository/>
        <Repository/>
        <Repository/>
      </div>


      <button onClick={handleGenAccessToken}>Generate PAT</button>
      {loading && <p>Loading...</p>}
      {error && <p style={{ color: 'red' }}>{error}</p>}
      {tokenData  && (
        <div style={styles.PATHolder}>
          <h2>Token Generated:</h2>
          <pre style={styles.GenPAT}>{JSON.stringify(tokenData, null, 2)}</pre>
        </div>
      )}
    </div>
  );
};

const styles = {
  header: {
    display: "flex",
    width: "100%",
    background: "var(--accent)",
    alignItems: "center",
    borderBottom: "var(--border) solid 1px",
    padding: "4px 10px",
    gap: "10px",
    justifyContent: "center",
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
  container: {
    width: "100%",
    justifyItems: "center",
  },
  repoHolder: {
    width: "90%",
    justifyItems: "center",
    display: "flex",
    flexDirection: "column",
    gap: "18px",
    alignItems: "center",
    marginTop: "20px",
  },
  PATHolder: {
    width: "100%",
  },
  GenPAT: {
    overflow: "auto",
  },
}


export default Repositories;
  