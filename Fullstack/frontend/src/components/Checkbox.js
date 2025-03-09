import React from "react";

const Checkbox = ({ id, checked, onChange, label, labelStyle }) => {
    return (
        <div style={styles.container}>
            <input 
                style={styles.checkbox}
                type="checkbox"
                id={id}
                checked={checked}
                onChange={onChange}
            />
            <label htmlFor={id} style={{ ...styles.label, ...labelStyle }}>
                {label}
            </label>

        </div>
    );
}

const styles = {
    container: {
      display: "flex",
      alignItems: "center",
      gap: "10px",
    },
    checkbox: {
      accentColor: "var(--secondary)",
      marginRight: "4px",
    },
    label: {
        fontSize: "0.95rem",
    },
};
  
export default Checkbox;