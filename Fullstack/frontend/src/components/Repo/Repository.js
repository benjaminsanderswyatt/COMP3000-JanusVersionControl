import React, { useState } from 'react';

const Repository = () => {



  return (
    <div style={styles.container}>
      <div style={styles.header}>
        <h2 style={styles.repoName}>Repository Name</h2>
        <div style={styles.visability}>Visibility</div>
        

        <div style={styles.avatars}>
          <span style={styles.avatar}></span>
          <span style={styles.avatar}></span>
          <span style={styles.avatar}></span>
        </div>
      </div>
      

      <hr style={styles.divider}/>

      <p style={styles.description}>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Leo magna metus
        sagittis accumsan quam ridiculus nisl sed. Egestas urna ornare primis
        venenatis; malesuada maecenas sed.
      </p>

      
      <span style={styles.lastUpdated}>Last Updated...</span>
      
    </div>
  );
};

const styles = {
  container: {
    display: "block",
    width: "90%",
    background: "var(--card)",
    border: 'var(--border) thin solid',
    borderRadius: "8px",
    boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
    padding: "20px",
    maxWidth: "1000px",
  },
  header: {
    display: "flex",
    color: "var(--text)",
    flexWrap: "wrap",
    gap: "10px",
    overflow: "hidden",
  },
  repoName: {
    fontSize: "1.2rem",
    fontWeight: "bold",
    color: "var(--text)",
  },
  visability: {
    background: "var(--secondary)",
    padding: "4px 8px",
    borderRadius: "4px",
    fontSize: "12px",
    color: "var(--text)",
  },
  avatars: {
    display: "flex",
    marginLeft: "auto",
  },
  avatar: {
    width: "24px",
    height: "24px",
    background: "var(--primary)",
    borderRadius: "50%",
    marginLeft: "4px",
  },
  divider: {
    border: "none",
    borderTop: "2px solid var(--primary)",
    margin: "10px 0px"
  },
  description: {
    fontSize: "0.9rem",
    color: "var(--text)",
  },
  lastUpdated: {
    fontStyle: "italic",
    fontSize: "0.8rem",
    color: "var(--primary)",
    float: "right",
  },
};


export default Repository;
