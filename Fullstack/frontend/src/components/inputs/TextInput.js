import React from 'react';

import styles from "../../styles/components/inputs/TextInput.module.css";

const TextInput = ({ 
  label,
  name,
  value, 
  onChange,
  placeholder = "", 
  hasError = false 
}) => (
  
  <div className={styles.container}>
    <label className={styles.label} htmlFor={name}>{label}</label>
    <input
      className={`${styles.input} ${hasError ? styles.errorInput : ""}`}
      type="text"
      id={name}
      name={name}
      value={value}
      onChange={onChange}
      placeholder={placeholder}
    />
  </div>
);


export default TextInput;