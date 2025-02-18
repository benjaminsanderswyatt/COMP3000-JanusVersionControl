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
          noHover={location.pathname.startsWith("/repositories")}
          onClick={() => navigate("/repositories")}
        >
          My Repositories
        </button>
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/collaborating") ? styles.selected : ""}`}
          noHover={location.pathname.startsWith("/collaborating")}
          onClick={() => navigate("/collaborating")}
        >
          Collaborating
        </button>
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/commandline") ? styles.selected : ""}`}
          noHover={location.pathname.startsWith("/commandline")}
          onClick={() => navigate("/commandline")}
        >
          Command Line
        </button>
      </nav>


      {/* Username section */}
      <button 
        className={`${styles.username} ${location.pathname.startsWith("/account") ? styles.selected : ""}`}
        noHover={location.pathname.startsWith("/account")}
        onClick={() => navigate("/account")}>{authUser}
      </button>

      {/* Settings section */}
      <div className={styles.settings}>


        <ProfilePic
          userId={authUserId}
          innerClassName={styles.iconAccount}
          handleClick={() => navigate("/account")}
        />

        {/*
        <img src="/Icons/settings.svg"
          alt="Settings"
          style={styles.iconSettings}
          onClick={() => navigate("/settings")}
        />
        */}
      </div>
      


      
      

    </div>
  );
};

export default LoggedInHeader;
