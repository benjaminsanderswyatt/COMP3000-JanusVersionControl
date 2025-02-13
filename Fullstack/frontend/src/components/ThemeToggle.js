import React from 'react';
import { useTheme } from '../contexts/ThemeContext';

// Componant to toggle between light and dark theme
const ThemeToggle = ({ style }) => {
  const { theme, toggleTheme } = useTheme();

  
  return (
    <button 
    onClick={toggleTheme}
    style={{ ...style, ...styles.button }}
    >
      Toggle theme

      <div style={styles.imageHolder}>
        {theme === "light" ? 
          <img src="/Icons/moon.svg" alt="Moon icon" style={styles.image}></img>
        : 
          <img src="/Icons/sun.svg" alt="Sun icon" style={styles.image}></img>
        }
      </div>
      
      
    </button>
  );
};

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

export default ThemeToggle;
