import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useAuth } from '../contexts/AuthContext';

import RegisterForm from '../components/Login/RegisterForm';

import Button from '../components/Button';

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


  // Message colour, green for success, red for failure
  const messageStyle = messageType === 'success' ? { color: 'green' } : { color: 'red' };

  return (
    <div style={styles.container}>
      <h1 style={styles.heading}>Register</h1>

      <div style={styles.main}>
        <RegisterForm
          formData={formData}
          onChange={handleChange}
          onSubmit={handleSubmit}
          agreedToTerms={agreedToTerms}
          setAgreedToTerms={setAgreedToTerms}
        />
        

        {message && <p style={{ ...styles.message, ...messageStyle }}>{message}</p>}

      </div>

      <Button onClick={() => navigate("/login")} style={styles.toggleButton}>
        Already have an account? Login here
      </Button>
    </div>
  );
};


const styles = {
  container: {
    display: "flex",
    justifyContent: "center",
    alignItems: "center",
    width: '100%',
    flexDirection: "column",
  },
  main: {
    backgroundColor: 'var(--card)',
    width: '100%',
    maxWidth: "400px",
    padding: "20px",
    border: 'var(--border) thin solid',
    borderRadius: "8px",
    boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
    margin: "20px 0px",
  },
  heading: {
    color: 'var(--text)',
    fontSize: "2.5rem",
    margin: "10px 0px",
    textShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
  },
  message: {
    fontWeight: 'bold',
  },
  toggleButton: {
    background: 'none',
    border: 'none',
    color: 'var(--primary)',
    textDecoration: 'underline',
    fontSize: '1rem',
    cursor: 'pointer',
  }
}

export default Register;