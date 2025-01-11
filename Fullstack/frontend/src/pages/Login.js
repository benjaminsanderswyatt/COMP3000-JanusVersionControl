import React, { useState, useEffect } from 'react';
import { useNavigate } from 'react-router';

import LoginForm from '../components/Login/LoginForm';
import RegisterForm from '../components/Login/RegisterForm';
import ToggleButton from '../components/Login/ToggleButton';
import { register, login } from '../api/fetchUsers';


const Login = () => {
  const navigate = useNavigate();
  const [isRegistering, setIsRegistering] = useState(false);
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState(''); // Stores if the error is 'success' or 'error'
  const [agreedToTerms, setAgreedToTerms] = useState(false); // Tracks agreement to terms and conditions/privacy

  const [formData, setFormData] = useState({
    email: "",
    username: "",
    password: "",
    confirmPassword: "",
  });

  // Redirect to repos if user has token (already logged in)
  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      navigate("/repositories", { replace: true }); // Redirect
    }
  }, [navigate]);
  

  const toggleForm = () => {
    setIsRegistering(!isRegistering);

    setAgreedToTerms(false);

    // Reset the form when switched
    setFormData({
      email: "",
      username: "",
      password: "",
      confirmPassword: "",
    });

    // Reset message type
    // The messageType is only = success when successfully registered (so it can be transfered to the login)
    if (messageType === 'error') { 
      setMessage("");
      setMessageType("");
    }
  };


  const handleChange = ({ target: { name, value } }) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
  };


  const handleSubmit = async (e) => {
    e.preventDefault();

    if (isRegistering) {
      // Ensure terms agreement
      if (!agreedToTerms) {
        setMessageType('error');
        setMessage("You must agree to the terms and conditions to register.");
        return;
      }

      // Register
      try {
        // Send request
        const response = await register(
          formData.username,
          formData.email,
          formData.password
        );

        if (response.success) {
          setMessageType('success');
          setMessage("Registration successful!");
          toggleForm(); // Go to the login page
        } else {
          setMessageType('error');
          setMessage(response.message || "Registration failed");
        }

      } catch (error) {
        console.error("Registration error:", error);
        setMessageType('error');
        setMessage("An error occurred during registration. Please try again.");
      }

    } else {
      // Login

      try {
        // Send request
        const response = await login(
          formData.email,
          formData.password
        );
        
        if (response.success){
          localStorage.setItem('token', response.token);
          navigate("/repositories"); // Navigate
        } else {
          setMessageType('error');
          setMessage("Incorrect email or password. Please try again.");
        }

      } catch (error) {
        console.error("Login error:", error);
        setMessageType('error');
        setMessage("An error occurred during login");
      }

    }
  };


  // Message colour, green for success, red for failure
  const messageStyle = messageType === 'success' ? { color: 'green' } : { color: 'red' };

  return (
    <div style={styles.container}>
      <h1 style={styles.heading}>{isRegistering ? "Register" : "Login"}</h1>

      <div style={styles.main}>
        {isRegistering ? (
          <RegisterForm
            formData={formData}
            onChange={handleChange}
            onSubmit={handleSubmit}
            agreedToTerms={agreedToTerms}
            setAgreedToTerms={setAgreedToTerms}
          />
        ) : (
          <LoginForm
            formData={formData}
            onChange={handleChange}
            onSubmit={handleSubmit}
          />
        )}

        {message && <p style={{ ...styles.message, ...messageStyle }}>{message}</p>}

      </div>

      <ToggleButton isRegistering={isRegistering} onClick={toggleForm} />

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
    backgroundColor: 'white',
    width: '100%',
    maxWidth: "400px",
    padding: "20px",
    border: '#d9d9d9 thin solid',
    borderRadius: "8px",
    boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
    margin: "20px 0px",
  },
  heading: {
    fontSize: "2.5rem",
    margin: "10px 0px",
    textShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
  },
  message: {
    marginTop: '15px',
    fontWeight: 'bold',
  },
}

export default Login;