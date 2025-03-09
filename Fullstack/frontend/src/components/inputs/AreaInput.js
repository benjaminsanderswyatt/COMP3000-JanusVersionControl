import React, { useState, useEffect } from 'react';

import styles from "../../styles/components/inputs/TextInput.module.css"

const AreaInput = ({ 
  label, 
  name, 
  value, 
  onChange, 
  required = false, 
  placeholder = "", 
  maxLength = 256,
  rows = 4,
}) => {
  const [remainingChars, setRemainingChars] = useState(maxLength);

  useEffect(() => {
    setRemainingChars(maxLength - value.length);
  }, [value, maxLength]);

  const handleChange = (e) => {
    if (e.target.value.length <= maxLength) {
      onChange(e);
    }
  };

  return (
    <div className={styles.container}>
      <label className={styles.label} htmlFor={name}>
        {label}
      </label>
      <div className="textAreaContainer">
        <textarea
            className={`${styles.input} ${styles.descriptionInput}`}
            id={name}
            name={name}
            value={value}
            onChange={handleChange}
            placeholder={placeholder}
            required={required}
            rows={rows}
            maxLength={maxLength}
        />

        {value.length > 0 && (
            <span className={styles.charCount}>
                {remainingChars} characters remaining
            </span>
        )}
        
      </div>
      
    </div>
  );
};

export default AreaInput;