import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useAuth } from '../contexts/AuthContext';

import RegisterForm from '../components/login/RegisterForm';


import styles from "../styles/pages/Register.module.css";

const Register = () => {
  const { register } = useAuth();
  const navigate = useNavigate();
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState(''); // Stores if the error is 'success' or 'error'
  const [agreedToTerms, setAgreedToTerms] = useState(false); // Tracks agreement to terms and conditions/privacy

  const [formData, setFormData] = useState({
    email: "",
    username: "",
    password: "",
    confirmPassword: "",
  });

  const handleChange = ({ target: { name, value } }) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
  };


  const handleSubmit = async (e) => {
    e.preventDefault();

    // Ensure password match
    if (formData.password !== formData.confirmPassword) {
      setMessageType('error');
      setMessage("Passwords must match.");
      return;
    }
    
    // Ensure terms agreement
    if (!agreedToTerms) {
      setMessageType('error');
      setMessage("You must agree to the terms of use to register.");
      return;
    }


    // Register
    try {
      await register(formData.username, formData.email, formData.password);

      navigate("/login");
    } catch (error) {
      setMessageType("error");
      setMessage(error.message || "Login failed");
    }
  };


  return (
    <div className={styles.container}>
      <h1 className={styles.heading}>Register</h1>

      <div className={styles.main}>
        <RegisterForm
          formData={formData}
          onChange={handleChange}
          onSubmit={handleSubmit}
          agreedToTerms={agreedToTerms}
          setAgreedToTerms={setAgreedToTerms}
        />
        

        {message && <p className={`${styles.message} ${messageType}`}>{message}</p>}

      </div>

      <button onClick={() => navigate("/login")} className={styles.toggleButton}>
        Already have an account? Login here
      </button>
    </div>
  );
};



export default Register;