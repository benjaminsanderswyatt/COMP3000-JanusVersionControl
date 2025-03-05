import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useAuth } from '../contexts/AuthContext';

import LoginForm from '../components/login/LoginForm';

import styles from "../styles/pages/Login.module.css";


const Login = () => {
  const { login } = useAuth();
  const navigate = useNavigate();
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState(''); // Stores if the error is 'success' or 'error'

  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });


  const handleChange = ({ target: { name, value } }) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
  };


  const handleSubmit = async (e) => {
    e.preventDefault();

    // Login
    try {
      // Get username from login
      const username = await login(formData.email, formData.password);

      navigate(`/repositories/${username}`);
    } catch (error) {
      setMessageType("error");
      setMessage(error.message || "Login failed");
    }
  };



  return (
    <div className={styles.container}>

      <h1 className={styles.heading}>Login</h1>

      <div className={styles.main}>
        
        <LoginForm
          formData={formData}
          onChange={handleChange}
          onSubmit={handleSubmit}
        />
        

        {message && <p className={`${styles.message} ${messageType}`}>{message}</p>}

      </div>

      <button onClick={() => navigate("/register")} className={styles.toggleButton}>
        Don't have an account? Register here
      </button>
    </div>
  );
};


export default Login;