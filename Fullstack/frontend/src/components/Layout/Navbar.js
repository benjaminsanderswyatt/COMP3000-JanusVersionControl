import React from 'react';
import { useNavigate } from 'react-router';


const LoggedInHeader = ({ authUser }) => {
  const navigate = useNavigate();
  
  return (
    <div style={styles.container}>




      {/* Navbar section */}
      <nav style={styles.navbar}>
        <ul style={styles.navbarLinks}>
          <button style={styles.navbarItem} onClick={() => navigate("/repositories")}>Repositories</button>
        </ul>
        <div style={styles.navbarSpacer}></div>
        <ul style={styles.navbarLinks}>
          <button style={styles.navbarItem} onClick={() => navigate("/account")}>Account</button>
        </ul>
      </nav>

      {/* Username section */}
      <button style={styles.username}>{authUser}</button>

      {/* Settings section */}
      <div style={styles.settings}>
        <button style={styles.settingsButton} onClick={() => navigate("/account")}>
          <svg viewBox="0 0 68 68" fill="none" xmlns="http://www.w3.org/2000/svg">
            <path fill-rule="evenodd" clip-rule="evenodd" d="M30 2a2 2 0 0 0-2 2v5.2c0 1.6-1.1 3-2.6 3.7-1.6.7-3.5.5-4.7-.7L17 8.5a2 2 0 0 0-2.8 0l-5.7 5.7a2 2 0 0 0 0 2.8l4.2 4.2c1.1 1.1 1.4 2.8.8 4.2A4 4 0 0 1 10 28H4a2 2 0 0 0-2 2v8c0 1.1.9 2 2 2h6.5c1.4 0 2.7 1 3.3 2.3.6 1.4.4 3-.7 4L8.5 51a2 2 0 0 0 0 2.8l5.7 5.7c.8.7 2 .7 2.8 0l4.6-4.6c1-1 2.7-1.3 4-.7 1.4.6 2.4 1.9 2.4 3.3V64c0 1.1.9 2 2 2h8a2 2 0 0 0 2-2v-6a4 4 0 0 1 2.6-3.5c1.4-.6 3-.3 4.2.8l4.2 4.2c.8.7 2 .7 2.8 0l5.7-5.7c.7-.8.7-2 0-2.8l-3.7-3.7a4.2 4.2 0 0 1-.7-4.7 4.2 4.2 0 0 1 3.7-2.6H64a2 2 0 0 0 2-2v-8a2 2 0 0 0-2-2h-4.6a4.5 4.5 0 0 1-4-3c-.7-1.6-.5-3.5.8-4.7l3.3-3.3c.7-.8.7-2 0-2.8l-5.7-5.7a2 2 0 0 0-2.8 0l-3.3 3.3a4.4 4.4 0 0 1-4.8.8c-1.6-.6-2.9-2.2-2.9-4V4a2 2 0 0 0-2-2h-8Zm4 44a12 12 0 1 0 0-24 12 12 0 0 0 0 24Z" 
            fill="var(--primary)" stroke="var(--lighttext)" stroke-width="3"/>
          </svg>
        </button>
      </div>

    </div>
  );
};

const styles = {
  settingsIcon: {
    width: 'auto',
    height: '40px',
    cursor: 'pointer',
  },
  container: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    width: '100%',
  },
  settings: {
    display: 'flex',
    alignItems: 'center',
  },
  settingsButton: {
    cursor: 'pointer',
    background: 'none',
    border: 'none',
    height: '40px',
    width: '40px',
    
  },
  username: {
    margin: '0px 10px',
    fontWeight: '600',
    fontSize: '1.1rem',
    color: 'var(--text)',
    background: 'none',
    border: 'none',
  },
  navbar: {
    display: "flex",
    padding: "0px 20px",
    height: "100%",
    width: '100%',
    margin: "0",
    gap: "2px",
    listStyle: "none",
    justifyContent: "left",
    alignItems: "center",
  },
  navbarLinks: {
    padding: "0px",
    margin: "0px",
  },
  navbarItem: {
    cursor: "pointer",
    border: "none",
    background: "none",
    color: "var(--text)",
  },
  navbarSpacer: {
    width: "1px",
    height: "24px",
    backgroundColor: "var(--lighttext)",
    margin: "0 0px",
  },
};



export default LoggedInHeader;
