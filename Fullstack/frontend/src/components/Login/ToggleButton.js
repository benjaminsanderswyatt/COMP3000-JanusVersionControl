import React from 'react';

import Button from '../Button';

const ToggleButton = ({ isRegistering, onClick }) => (
  <Button style={styles.toggleButton} onClick={onClick}>
    {isRegistering
      ? "Already have an account? Login"
      : "Don't have an account? Register"}
  </Button>
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