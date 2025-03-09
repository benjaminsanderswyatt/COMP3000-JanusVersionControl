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
  const [errorField, setErrorField] = useState('');

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

    setMessage("");
    setErrorField("");

    // Validate Username
    if (!formData.username.trim()) {
      setMessageType("error");
      setErrorField("username");
      setMessage("Username is required");
      return;
    }

    if (formData.username.length <= 10) {
      setMessageType("error");
      setErrorField("username");
      setMessage("Username must be longer than 10 characters");
      return;
    }

    const invalidChars = /[ ~^:?/\\*[\]\x7F]|(\.\.)/;
    if (invalidChars.test(formData.username)) {
      setMessageType("error");
      setErrorField("username");
      setMessage("Username contains invalid characters");
      return;
    }





    // Validate Email
    if (!formData.email.trim()) {
      setMessageType("error");
      setErrorField("email");
      setMessage("Email is required");
      return;
    }

    const emailRegex = /^[^\s@]+@[^\s@]+\.[^\s@]+$/;
    if (!emailRegex.test(formData.email)) {
      setMessageType("error");
      setErrorField("email");
      setMessage("Please enter a valid email address");
      return;
    }



    // Validate Password
    if (!formData.password) {
      setMessageType("error");
      setErrorField("password");
      setMessage("Password is required");
      return;
    }

    // Password security (min 8 length, uppercase, lowercase, number and special char)
    const passwordRegex = /^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&#])[A-Za-z\d@$!%*?&#]{8,}$/;
    if (!passwordRegex.test(formData.password)) {
      setMessageType("error");
      setErrorField("password");
      setMessage(
        "Password must be at least 8 characters and include uppercase, lowercase, a number, and a special character"
      );
      return;
    }







    // Validate Confirm Password
    if (!formData.confirmPassword) {
      setMessageType("error");
      setErrorField("confirmPassword");
      setMessage("Please confirm your password");
      return;
    }

    // Ensure password match
    if (formData.password !== formData.confirmPassword) {
      setMessageType('error');
      setErrorField("confirmPassword");
      setMessage("Passwords must match");
      return;
    }
    
    // Ensure terms agreement
    if (!agreedToTerms) {
      setMessageType('error');
      setMessage("You must agree to the terms of use to register");
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
          errorField={errorField}
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