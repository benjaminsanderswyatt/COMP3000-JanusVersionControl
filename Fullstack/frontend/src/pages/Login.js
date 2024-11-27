import React, { useState } from "react";
import { useNavigate } from "react-router-dom";

import LoginForm from "../components/LoginForm";
import RegisterForm from "../components/RegisterForm";
import ToggleButton from "../components/ToggleButton";
import { register, login } from "../api/fetchUsers";

import styles from "../styles/pages/Login.module.css";

const Login = () => {
  const [isRegistering, setIsRegistering] = useState(false);

  const [formData, setFormData] = useState({
    email: "",
    password: "",
    username: "",
  });

  const navigate = useNavigate();

  const toggleForm = () => {
    setIsRegistering(!isRegistering);

    // Reset the form when switched
    setFormData({
      email: "",
      password: "",
      username: "",
    });
  };

  const handleChange = (e) => {
    const { name, value } = e.target;

    // Set the value when user types it in
    setFormData({
      ...formData,
      [name]: value,
    });
  };

  const handleSubmit = async (e) => {
    e.preventDefault();

    if (isRegistering) {
      // Register

      try {
        // Send request
        const response = await register(
          formData.username,
          formData.email,
          formData.password
        );

        if (response.success) {
          alert("Registration successful!");
          toggleForm(); // Go to the login page
        } else {
          alert(response.message || "Registration failed");
        }

      } catch (error) {
        console.error("Registration error:", error);
        alert("An error occurred during registration");
      }

    } else {
      // Login

      try {
        // Send request
        const token = await login(
          formData.email,
          formData.password
        );

        if (token) {
          alert("Login successful!");
          navigate("/home"); // Navigate to the home page
        } else {
          alert("Login failed");
        }

      } catch (error) {
        console.error("Login error:", error);
        alert("An error occurred during login");
      }

    }
  };

  return (
    <div className={styles.login}>
      <h1>{isRegistering ? "Register" : "Login"}</h1>
      {isRegistering ? (
        <RegisterForm
          formData={formData}
          onChange={handleChange}
          onSubmit={handleSubmit}
        />
      ) : (
        <LoginForm
          formData={formData}
          onChange={handleChange}
          onSubmit={handleSubmit}
        />
      )}
      <ToggleButton isRegistering={isRegistering} onClick={toggleForm} />
    </div>
  );
};

export default Login;