import React from 'react';

import styles from "../../styles/components/login/TextInput.module.css";

const TextInput = ({ label, name, type = "text", value, onChange, required = false, placeholder = "" }) => (
  <div className={styles.container}>
    <label className={styles.label} htmlFor={name}>{label}</label>
    <input
      className={styles.input}
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