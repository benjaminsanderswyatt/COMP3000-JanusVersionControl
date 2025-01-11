import React, { createContext, useContext, useState, useEffect } from 'react';

// Create context for the theme
const ThemeContext = createContext();

export const useTheme = () => {
  return useContext(ThemeContext);
};


// Theme provider component
export const ThemeProvider = ({ children }) => {

  // Initialize the theme
  const getInitialTheme = () => {
    // Prioritise the chosen theme
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
      return savedTheme; // Use saved theme from localStorage
    }

    // Use the systems theme
    const prefersDarkMode = window.matchMedia('(prefers-color-scheme: dark)').matches;
    return prefersDarkMode ? 'dark' : 'light';
  };

  const [theme, setTheme] = useState(getInitialTheme());

  const toggleTheme = () => {
    setTheme((prevTheme) => (prevTheme === 'light' ? 'dark' : 'light'));
  };

  // Store the theme in localStorage (persistent theme)
  useEffect(() => {
    localStorage.setItem('theme', theme);
  }, [theme]);

  // Use the saved theme from localStorage if available
  useEffect(() => {
    const savedTheme = localStorage.getItem('theme');
    if (savedTheme) {
      setTheme(savedTheme);
    }
  }, []);


  return (
    <ThemeContext.Provider value={{ theme, toggleTheme }}>
      {children}
    </ThemeContext.Provider>
  );
};
