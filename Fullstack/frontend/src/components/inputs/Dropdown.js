import React from 'react';

import styles from "../../styles/components/inputs/Dropdown.module.css"


const Dropdown = ({ label ,dataArray , onSelect, selectedValue }) => {
  const handleChange = (e) => {
    // Call the onSelect
    onSelect(e.target.value);
  };


  return (
    <div className={styles.holder}>
      <label htmlFor="dropdown-select">{label}:</label>
      <select
        id="dropdown-select"
        value={selectedValue}
        onChange={handleChange}
        className={styles.select}
      >
        {dataArray.map((item) => (
          <option className={styles.option} key={item} value={item}>
            {item}
          </option>
        ))}
      </select>
    </div>
  );
};


export default Dropdown;