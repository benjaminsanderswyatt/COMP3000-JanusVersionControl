import React from 'react';
import { useNavigate, useLocation } from 'react-router';


const LoggedInHeader = ({ authUser }) => {
  const navigate = useNavigate();
  const location = useLocation();
  
  return (
    <div style={styles.container}>




      {/* Navbar section */}
      <nav style={styles.navbar}>
        <button
          style={{ ...styles.navbarItem, ...(location.pathname.startsWith("/repositories") ? styles.selected : {}) }}
          onClick={() => navigate("/repositories")}
        >
          My Repositories
        </button>
        <button
          style={{ ...styles.navbarItem, ...(location.pathname.startsWith("/collaborating") ? styles.selected : {}) }}
          onClick={() => navigate("/collaborating")}
        >
          Collaborating
        </button>
        <button
          style={{ ...styles.navbarItem, ...(location.pathname.startsWith("/commandline") ? styles.selected : {}) }}
          onClick={() => navigate("/commandline")}
        >
          Command Line
        </button>
      </nav>


      {/* Username section */}
      <button style={{ ...styles.username, ...(location.pathname.startsWith("/account") ? styles.selected : {}) }}
       onClick={() => navigate("/account")}>{authUser}</button>

      {/* Settings section */}
      <div style={styles.settings}>
        <img src="/Icons/account.svg"
        alt="Account"
        style={styles.iconAccount}
        onClick={() => navigate("/account")}
        />

        <img src="/Icons/settings.svg"
        alt="Settings"
        style={styles.iconSettings}
        onClick={() => navigate("/settings")}
        />
      </div>
      


      
      

    </div>
  );
};

const styles = {
  settings: {
    display: "flex",
    alignItems: "center",
    gap: "12px",
  },
  iconAccount: {
    width: '40px',
    height: '40px',
    cursor: 'pointer',
  },
  iconSettings: {
    width: '45px',
    height: '45px',
    cursor: 'pointer',
  },
  container: {
    display: 'flex',
    justifyContent: 'space-between',
    alignItems: 'center',
    width: '100%',
  },
  username: {
    margin: '0px 10px',
    fontSize: '1.1rem',
    color: 'var(--text)',
    background: 'none',
    border: 'none',
    cursor: "pointer",
    padding: "8px 12px",
    borderRadius: "8px",
  },
  navbar: {
    display: "flex",
    padding: "0px 20px",
    height: "100%",
    width: '100%',
    margin: "0",
    gap: "10px",
    listStyle: "none",
    justifyContent: "left",
    alignItems: "center",
  },
  navbarItem: {
    cursor: "pointer",
    border: "none",
    background: "none",
    color: "var(--text)",
    padding: "8px 12px",
    borderRadius: "8px",
    fontSize: "0.9rem",
  },
  selected: {
    background: "var(--border)",
  },
};



export default LoggedInHeader;
