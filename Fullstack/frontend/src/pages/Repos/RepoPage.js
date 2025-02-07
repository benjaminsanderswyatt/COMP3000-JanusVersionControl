import React from "react";
import { useParams, useNavigate, useLocation } from "react-router";

import Commit from "../../components/Repo/Commit"
import RepoPageHeader from "../../components/Repo/RepoPageHeader";

const RepoPage = () => {
  const navigate = useNavigate();
  const { name } = useParams(); // Get the name from the URL

  
  return (
    <div style={styles.container}>
      <header style={styles.header}>
        <div style={styles.leftHeader}>
          <h2>{name}</h2>
          <div style={styles.visability}>Visibility</div>
        </div>
        <RepoPageHeader/>
        
      </header>

      <div style={styles.repoHolder}>
        
        <Commit/>




      </div>
      
    </div>
  );

  
};

const styles = {
  header: {
    display: "flex",
    width: "90%",
    background: "var(--accent)",
    alignItems: "center",
    borderBottom: "var(--border) solid 1px",
    padding: "4px 10px",
    gap: "10px",
    marginTop: "20px",
    borderRadius: "8px 8px 0px 0px",
    minHeight: "46px",
    flexWrap: "wrap",
  },
  leftHeader: {
    display: "flex",
    justifyContent: "left",
    gap: "8px",
    alignItems: "center",
  },
  visability: {
    background: "var(--lightsecondary)",
    padding: "4px 8px",
    borderRadius: "4px",
    fontSize: "12px",
    color: "var(--text)",
    height: "fit-content",
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
  PATHolder: {
    width: "100%",
  },
  GenPAT: {
    overflow: "auto",
  },
}

export default RepoPage;
