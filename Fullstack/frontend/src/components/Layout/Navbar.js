import React, { useState, useEffect, useRef } from 'react';
import { useNavigate, useLocation } from 'react-router';
import ProfilePic from '../images/ProfilePic';
import { useAuth } from '../../contexts/AuthContext';


import styles from "../../styles/Components/Layout/Navbar.module.css";

const LoggedInHeader = ({ authUser }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const { authUserId } = useAuth();
  
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef(null);

  // Function to close menu when clicking outside
  useEffect(() => {
    function handleClickOutside(event) {
      if (menuRef.current && !menuRef.current.contains(event.target)) {
        setMenuOpen(false);
      }
    }

    if (menuOpen) {
      document.addEventListener("mousedown", handleClickOutside);
    }

    return () => {
      document.removeEventListener("mousedown", handleClickOutside);
    };
  }, [menuOpen]);


  // Function to navigate and close menu
  const handleNavigation = (path) => {
    navigate(path);
    setMenuOpen(false);
  };

  return (
    <div className={styles.container} ref={menuRef}>

      {/* Hamburger Menu Button (Visible on Small Screens) */}
      <button 
        className={`${styles.menuButton} ${menuOpen ? styles.selected : ""}`} 
        onClick={() => setMenuOpen(!menuOpen)}
      >
        <img 
          src="/Icons/burger.svg"
          alt={menuOpen ? "Close menu" : "Menu"} 
          className={styles.burger} 
        />
      </button>


      {/* Navbar section */}
      <nav className={`${styles.navbar} ${menuOpen ? styles.showMenu : styles.hideMenu}`}>
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/repositories") ? styles.selected : ""}`}
          onClick={() => handleNavigation("/repositories")}
        >
          My Repositories
        </button>
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/collaborating") ? styles.selected : ""}`}
          onClick={() => handleNavigation("/collaborating")}
        >
          Collaborating
        </button>
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/commandline") ? styles.selected : ""}`}
          onClick={() => handleNavigation("/commandline")}
        >
          Command Line
        </button>
      </nav>


      

      {/* Settings section */}
      <div className={styles.settings}>

        <ProfilePic
          userId={authUserId}
          innerClassName={styles.iconAccount}
          handleClick={() => handleNavigation("/account")}
        />

        {/* Username */}
        <button 
          className={`${styles.username} ${location.pathname.startsWith("/account") ? styles.selected : ""}`}
          onClick={() => handleNavigation("/account")}>{authUser}
        </button>

        

      </div>
      


      
      

    </div>
  );
};

export default LoggedInHeader;
