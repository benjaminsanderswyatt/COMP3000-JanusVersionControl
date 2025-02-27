import React from 'react';
import { useTheme } from '../contexts/ThemeContext';

import styles from "../styles/components/ThemeToggle.module.css";

// Componant to toggle between light and dark theme
const ThemeToggle = () => {
  const { theme, toggleTheme } = useTheme();

  
  return (
    <button 
    onClick={toggleTheme}
    className={styles.button}
    >
      Toggle theme

      <div className={styles.imageHolder}>
        {theme === "light" ? 
          <img src="/icons/moon.svg" alt="Moon icon" className={styles.image}></img>
        : 
          <img src="/icons/sun.svg" alt="Sun icon" className={styles.image}></img>
        }
      </div>
      
      
    </button>
  );
};

export default ThemeToggle;
