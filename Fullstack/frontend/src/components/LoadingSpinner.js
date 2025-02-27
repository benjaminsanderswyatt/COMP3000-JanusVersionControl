import React from 'react';

import styles from '../styles/components/LoadingSpinner.module.css';

const LoadingSpinner = () => (
  <div className={styles.container}>
    <div className={styles.spinner}></div>
  </div>
);

export default LoadingSpinner;
