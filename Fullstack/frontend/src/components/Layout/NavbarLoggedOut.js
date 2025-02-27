import React, { useState, useEffect, useRef, useMemo } from 'react';
import { useNavigate, useLocation } from 'react-router';
import SearchBox from '../search/SearchBoxEnter';
import { useDebounce } from '../../helpers/Debounce';

import styles from "../../styles/components/layout/Navbar.module.css";
import stylesLog from "../../styles/components/layout/LogButtonBar.module.css";


const NavbarLoggedOut = () => {
  const navigate = useNavigate();
  const location = useLocation();
  
  const [menuOpen, setMenuOpen] = useState(false);
  const menuRef = useRef(null);

  const [searchQuery, setSearchQuery] = useState('');

  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  const returnedSearch = useMemo(() => {
    // Search backend all public repos / access
  }, [debouncedSearchQuery]);


  const handleSearch = (query) => {
    // Search
    setSearchQuery(query);
  };




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
          className={`${styles.navbarItem} ${location.pathname.startsWith("/commandline") ? styles.selected : ""}`}
          onClick={() => handleNavigation("/commandline")}
        >
          Command Line
        </button>
      </nav>

      <SearchBox searchingWhat="pubic repositories" onSearch={handleSearch} />

      
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

      
      

    </div>
  );
};

export default NavbarLoggedOut;
