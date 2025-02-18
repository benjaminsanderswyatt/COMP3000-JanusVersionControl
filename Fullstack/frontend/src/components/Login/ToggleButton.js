import React from 'react';


const ToggleButton = ({ isRegistering, onClick }) => (
  <button style={styles.toggleButton} onClick={onClick}>
    {isRegistering
      ? "Already have an account? Login"
      : "Don't have an account? Register"}
  </button>
);

const styles = {
  toggleButton: {
    background: 'none',
    border: 'none',
    color: 'var(--primary)',
    textDecoration: 'underline',
    fontSize: '1rem',
    cursor: 'pointer',
  }
}

export default ToggleButton;