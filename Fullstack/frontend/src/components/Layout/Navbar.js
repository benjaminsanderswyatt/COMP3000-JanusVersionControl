import React from 'react';
import { useNavigate, useLocation } from 'react-router';
import ProfilePic from '../images/ProfilePic';
import { useAuth } from '../../contexts/AuthContext';

import styles from "../../styles/Components/Layout/Navbar.module.css";

const LoggedInHeader = ({ authUser }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { authUserId } = useAuth();
  
  return (
    <div className={styles.container}>




      {/* Navbar section */}
      <nav className={styles.navbar}>
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/repositories") ? styles.selected : ""}`}
          onClick={() => navigate("/repositories")}
        >
          My Repositories
        </button>
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/collaborating") ? styles.selected : ""}`}
          onClick={() => navigate("/collaborating")}
        >
          Collaborating
        </button>
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/commandline") ? styles.selected : ""}`}
          onClick={() => navigate("/commandline")}
        >
          Command Line
        </button>
      </nav>


      

      {/* Settings section */}
      <div className={styles.settings}>

        <ProfilePic
          userId={authUserId}
          innerClassName={styles.iconAccount}
          handleClick={() => navigate("/account")}
        />

        {/* Username */}
        <button 
          className={`${styles.username} ${location.pathname.startsWith("/account") ? styles.selected : ""}`}
          onClick={() => navigate("/account")}>{authUser}
        </button>

        

      </div>
      


      
      

    </div>
  );
};

export default LoggedInHeader;
