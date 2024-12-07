import React, { useState } from "react";
import { GenAccessToken } from "../api/fetchPAT";

const Repos = () => {
  const [tokenData , setTokenData] = useState(null);
  const [error, setError] = useState(null);
  const [loading, setLoading] = useState(false);

  const handleGenAccessToken = async () => {
    setLoading(true);
    setError(null);
    setTokenData (null);
    try {
      
      const response = await GenAccessToken();
      console.log("Finished response Try");

      if (!response.success) {
        console.log("Failed: " + response.message + " token: " + response.token);
        throw new Error(response.message);
      }

      setTokenData(response);
    } catch (err) {
      console.log("Catch");
      setError(err.message);
    } finally {
      console.log("Final");
      setLoading(false);
    }
  };



  return (
    <div>
      <h1>Repos</h1>
      <button onClick={handleGenAccessToken}>Generate PAT</button>
      {loading && <p>Loading...</p>}
      {error && <p style={{ color: "red" }}>{error}</p>}
      {tokenData  && (
        <div>
          <h2>Token Generated:</h2>
          <pre>{JSON.stringify(tokenData, null, 2)}</pre>
        </div>
      )}
    </div>
    );
  };
  
  export default Repos;
  