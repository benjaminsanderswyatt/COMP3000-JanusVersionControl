import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router';

const Commits = () => {
  const navigate = useNavigate();

  

  return (
    <div style={styles.container}>
      <header style={styles.header}>
      </header>

      <Commits></Commits>

    </div>
  );
};

const styles = {
  container: {
    width: "100%",
    justifyItems: "center",
  },
  header: {
    display: "flex",
    width: "90%",
    background: "var(--accent)",
    alignItems: "center",
    borderBottom: "var(--border) solid 1px",
    padding: "4px 10px",
    gap: "10px",
    justifyContent: "center",
    marginTop: "20px",
    borderRadius: "8px 8px 0px 0px",
    minHeight: "46px",
    flexWrap: "wrap",
  },
  createHolder: {
    background: "var(--card)",
    width: "90%",
    padding: "18px",
    justifyItems: "center",
    display: "flex",
    flexDirection: "column",
    gap: "18px",
    alignItems: "center",
    borderRadius: "0px 0px 8px 8px",
  },
}


export default Create;
  