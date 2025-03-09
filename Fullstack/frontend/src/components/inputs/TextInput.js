import React from 'react';

import styles from "../../styles/components/inputs/TextInput.module.css";

const TextInput = ({ 
  label,
  name,
  type = "text", 
  value, 
  onChange, 
  required = false, 
  placeholder = "", 
  hasError = false 
}) => (
  
  <div className={styles.container}>
    <label className={styles.label} htmlFor={name}>{label}</label>
    <input
      className={`${styles.input} ${hasError ? styles.errorInput : ""}`}
      type={type}
      id={name}
      name={name}
      value={value}
      onChange={onChange}
      placeholder={placeholder}
      required={required}
    />
  </div>
);


export default TextInput;