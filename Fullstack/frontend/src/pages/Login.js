import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { useAuth } from '../contexts/AuthContext';

import LoginForm from '../components/Login/LoginForm';


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
      await login(formData.email, formData.password);

      navigate("/repositories");
    } catch (error) {
      setMessageType("error");
      setMessage(error.message || "Login failed");
    }
  };


  // Message colour, green for success, red for failure
  const messageStyle = messageType === 'success' ? { color: 'green' } : { color: 'red' };

  return (
    <div style={styles.container}>
      <h1 style={styles.heading}>Login</h1>

      <div style={styles.main}>
        
        <LoginForm
          formData={formData}
          onChange={handleChange}
          onSubmit={handleSubmit}
        />
        

        {message && <p style={{ ...styles.message, ...messageStyle }}>{message}</p>}

      </div>

      <button onClick={() => navigate("/register")} style={styles.toggleButton}>
        Don't have an account? Register here
      </button>
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
    marginBottom: '20px',
  }
}

export default Login;