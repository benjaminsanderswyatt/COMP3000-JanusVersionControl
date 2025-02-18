import React from 'react';
import { useNavigate } from 'react-router';

import Button from '../Button';

const LoggedOutHeader = () => {
  const navigate = useNavigate();
  

  return (
    <div style={styles.holder}>
      <Button
        style={styles.buttonLogin}
        onClick={() => navigate("/login")}
      >
        Login
      </Button>
      <Button
        style={styles.buttonRegister}
        onClick={() => navigate('/register')}
      >
        Register
      </Button>
    </div>
  );

};

const styles = {
    holder: {
        display: 'flex',
        marginLeft: 'auto',
        gap: '12px',
        height: '32px',
    },
        buttonLogin: {
        backgroundColor: 'var(--secondary)',
        borderRadius: '8px',
        border: 'none',
        width: '83px',
        color: 'black',
        fontSize: '0.9rem',
        cursor: 'pointer',
    },
        buttonRegister: {
        backgroundColor: 'var(--primary)',
        color: 'white',
        borderRadius: '8px',
        border: 'none',
        width: '83px',
        fontSize: '0.9rem',
        cursor: 'pointer',
    },
};

export default LoggedOutHeader;
