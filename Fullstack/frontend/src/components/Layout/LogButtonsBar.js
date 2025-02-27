import React from 'react';
import { useNavigate } from 'react-router';

import styles from "../../styles/components/layout/LogButtonBar.module.css";

const LoggedOutHeader = () => {
  const navigate = useNavigate();
  

  return (
    <div className={styles.holder}>
      <button
        className={styles.buttonLogin}
        onClick={() => navigate("/login")}
      >
        Login
      </button>
      <button
        className={styles.buttonRegister}
        onClick={() => navigate('/register')}
      >
        Register
      </button>
    </div>
  );

};



export default LoggedOutHeader;
