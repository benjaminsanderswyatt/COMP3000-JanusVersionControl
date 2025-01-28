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
        <img src="/Icons/account.svg"
        alt="Settings"
        style={styles.iconAccount}
        onClick={() => navigate("/settings")}
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
