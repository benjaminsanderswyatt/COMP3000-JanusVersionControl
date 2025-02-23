import React, { useState } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router';

import styles from "../../styles/Pages/Repos/RepoCommits.module.css";

const RepoCommits = () => {
  const { name } = useParams(); // Get the name from the URL

  const commits = [
    { user: 'temp', message: 'Initial Commit', hash: '#4a35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-30T12:34:56Z' },
    { user: 'temp', message: 'Fix bug in login', hash: '#5b35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-30T14:20:10Z' },
    { user: 'temp', message: 'Add new feature', hash: '#6c35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-29T09:15:30Z' },
    { user: 'temp', message: 'Refactor code', hash: '#7d35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-29T18:45:22Z' },
    { user: 'temp', message: 'Update README', hash: '#8e35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-28T10:10:10Z' },
  ];




  return (
    <div style={styles.container}>
      <header style={styles.header}>
      </header>

      <div style={styles.createHolder}>
        
      </div>

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


export default RepoCommits;
  