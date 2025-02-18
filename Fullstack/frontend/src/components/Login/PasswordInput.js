import React, { useState } from 'react';


const PasswordInput = ({ label, name, value, onChange, required = false, placeholder = "" }) => {
  const [showPassword, setShowPassword] = useState(false);

  const toggleVisibility = () => setShowPassword((prev) => !prev);

  return (
    <div style={styles.container}>
      <label style={styles.label} htmlFor={name}>{label}</label>
      <div style={styles.inputGroup}>
        <input
          style={styles.input}
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
          style={styles.toggleButton}
          aria-label={showPassword ? 'Hide password' : 'Show password'}
        >
          {showPassword ? 'Hide' : 'Show'}
        </button>
      </div>
    </div>
  );
};

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
  inputGroup: {
    display: 'flex',
    alignItems: 'center',
  },
  input: {
    flex: '1',
    padding: '10px',
    border: '1px solid #ccc',
    borderRadius: '8px',
    fontSize: '16px',
    boxSizing: "border-box",
    margin: '0px 4px 0px 0px',
  },
  toggleButton: {
    height: '40px',
    width: '50px',
    background: 'var(--border)',
    border: 'thin solid var(--border)',
    borderRadius: '8px',
    cursor: 'pointer',
    color: 'var(--text)',
  },
};


export default PasswordInput;
