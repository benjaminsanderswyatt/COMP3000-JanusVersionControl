import React from 'react';
import { useTheme } from '../contexts/ThemeContext';

import styles from "../styles/Components/ThemeToggle.module.css";

// Componant to toggle between light and dark theme
const ThemeToggle = ({ innerClassName }) => {
  const { theme, toggleTheme } = useTheme();

  
  return (
    <button 
    onClick={toggleTheme}
    className={styles.button}
    >
      Toggle theme

      <div className={styles.imageHolder}>
        {theme === "light" ? 
          <img src="/Icons/moon.svg" alt="Moon icon" className={styles.image}></img>
        : 
          <img src="/Icons/sun.svg" alt="Sun icon" className={styles.image}></img>
        }
      </div>
      
      
    </button>
  );
};

/*
const styles = {
  button: {
    display: "flex",
    alignItems: "center",
    paddingRight: "30px",
    gap: "5px",
    position: "relative",
  },
  imageHolder: {
    position: "absolute",
    right: "5px",
    width: "20px",
    display: "flex",
    alignItems: "center",
  },
  image: {
    width: "100%",
    height: "100%",
  }
}
*/

export default ThemeToggle;
