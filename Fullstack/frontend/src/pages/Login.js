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
  const [errorField, setErrorField] = useState('');

  


  const [formData, setFormData] = useState({
    email: "",
    password: "",
  });


  const handleChange = ({ target: { name, value } }) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
  };


  const handleSubmit = async (e) => {
    e.preventDefault();

    setMessage("");
    setErrorField('');

    // Check email is provided
    if (!formData.email.trim()) {
      setMessageType("error");
      setErrorField("email");
      setMessage("Email is required");
      return;
    }

    // Regex to validate email format
    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      setMessageType("error");
      setErrorField("email");
      setMessage("Please enter a valid email address");
      return;
    }

    // Check password is provided
    if (!formData.password) {
      setMessageType("error");
      setErrorField("password");
      setMessage("Password is required");
      return;
    }




    // Login
    try {
      // Get username from login
      const username = await login(formData.email, formData.password);

      navigate(`/repository/${username}`);
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
          errorField={errorField}
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