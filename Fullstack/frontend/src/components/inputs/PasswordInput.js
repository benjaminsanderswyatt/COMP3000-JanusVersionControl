import React, { useState } from 'react';

import styles from "../../styles/components/inputs/TextInput.module.css";

const PasswordInput = ({ 
  label, 
  name, 
  value, 
  onChange, 
  required = false, 
  placeholder = "",
  hasError = false
}) => {

  const [showPassword, setShowPassword] = useState(false);

  const toggleVisibility = () => setShowPassword((prev) => !prev);

  return (
    <div className={styles.container}>
      <label className={styles.label} htmlFor={name}>{label}</label>
      <div className={styles.inputGroup}>
        <input
          className={styles.inputPassword}
          type={showPassword ? 'text' : 'password'}
          id={name}
          name={name}
          value={value}
          onChange={onChange}
          placeholder={placeholder}
          required={required}
        />
        <button
          type="button"
          onClick={toggleVisibility}
          className={styles.showButton}
          aria-label={showPassword ? 'Hide password' : 'Show password'}
        >
          {showPassword ? 'Hide' : 'Show'}
        </button>
      </div>
    </div>
  );
};


export default PasswordInput;
