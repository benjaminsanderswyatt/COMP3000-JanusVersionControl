import React, { useRef, useState, useEffect } from 'react';
import { useNavigate, useLocation } from 'react-router';
import ProfilePic from '../images/ProfilePic';

import styles from "../../styles/components/layout/Navbar.module.css";
import stylesLog from "../../styles/components/layout/LogButtonBar.module.css";

const Navbar = ({ authUser, authUserId, isLoggedIn }) => {
  const navigate = useNavigate();
  const location = useLocation();
  const menuRef = useRef(null);
  const [menuOpen, setMenuOpen] = useState(false);

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
          src="/icons/burger.svg"
          alt={menuOpen ? "Close menu" : "Menu"} 
          className={styles.burger} 
        />
      </button>


      {/* Navbar section */}
      <nav className={`${styles.navbar} ${menuOpen ? styles.showMenu : styles.hideMenu}`}>
        
        {/* Display when logged in */}
        {isLoggedIn ? (
          <>
            <button
              className={`${styles.navbarItem} ${location.pathname.startsWith("/repository") ? styles.selected : ""}`}
              onClick={() => handleNavigation(`/repository/${authUser}`)}
            >
              My Repositories
            </button>

            <button
              className={`${styles.navbarItem} ${location.pathname.startsWith("/collaborating") ? styles.selected : ""}`}
              onClick={() => handleNavigation(`/collaborating/${authUser}`)}
            >
              Collaborating
            </button>

            
            <button
              className={`${styles.navbarItem} ${location.pathname.startsWith("/discover") ? styles.selected : ""}`}
              onClick={() => handleNavigation("/discover")}
            >
              Discover
            </button>

          </>
        ) : null}

        {/* Always display */}
        <button
          className={`${styles.navbarItem} ${location.pathname.startsWith("/commandline") ? styles.selected : ""}`}
          onClick={() => handleNavigation("/commandline")}
        >
          Command Line
        </button>

      </nav>


      {/* Settings section */}
      {isLoggedIn ? (
        <div className={styles.settings}>

          <ProfilePic
            userId={authUserId}
            alt="Profile"
            innerClassName={styles.iconAccount}
            handleClick={() => handleNavigation("/account")}
          />

          <button 
            className={`${styles.username} ${location.pathname.startsWith("/account") ? styles.selected : ""}`}
            onClick={() => handleNavigation("/account")}
          >
            {authUser}
          </button>

        </div>

      ) : (

        <div className={stylesLog.holder}>
          <button
            className={stylesLog.buttonLogin}
            onClick={() => navigate("/login")}
          >
            Login
          </button>

          <button
            className={stylesLog.buttonRegister}
            onClick={() => navigate('/register')}
          >
            Register
          </button>
        </div>

      )}

    </div>
  );
};

export default Navbar;