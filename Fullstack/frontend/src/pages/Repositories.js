import React, { useState } from 'react';
import Repository from '../components/Repo/Repository';
import RepoBar from '../components/Repo/RepoBar';

import { GenAccessToken } from '../api/fetch/fetchPAT';



import { useAuth  } from '../contexts/AuthContext';


const Repositories = () => {
  const [tokenData , setTokenData] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);




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
};



return (
  <div style={styles.container}>
    <h1>Repos</h1>

    <RepoBar/>


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
  container: {
    width: "90%",
    justifyItems: "center",
  },
  repoHolder: {
    width: "100%",
    justifyItems: "center",
    display: "flex",
    flexDirection: "column",
    gap: "18px",
    alignItems: "center",
  },
  PATHolder: {
    width: "100%",
  },
  GenPAT: {
    overflow: "auto",
  },
}


export default Repositories;
  