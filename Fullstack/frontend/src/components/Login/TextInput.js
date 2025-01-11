import React from 'react';

const TextInput = ({ label, name, type = "text", value, onChange, required = false, placeholder = "" }) => (
  <div style={styles.container}>
    <label style={styles.label} htmlFor={name}>{label}</label>
    <input
      style={styles.input}
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

const styles = {
  container: {
    marginBottom: '20px',
  },
  label: {
    display: 'block',
    marginBottom: '5px',
    fontWeight: 'bold',
    color: 'var(--lighttext)',
  },
  input: {
    width: '100%',
    padding: '10px',
    border: '1px solid #ccc',
    borderRadius: '8px',
    fontSize: '16px',
    boxSizing: "border-box",
  },
};


export default TextInput;