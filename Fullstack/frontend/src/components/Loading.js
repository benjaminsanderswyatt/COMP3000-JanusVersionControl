import React from 'react';

const Loader = () => (
  <div style={styles.loaderContainer}>
    <div style={styles.spinner}></div>
  </div>
);

const styles = {
  loaderContainer: {
    display: 'flex',
    justifyContent: 'center',
    alignItems: 'center',
    height: '100px',
  },
  spinner: {
    width: '30px',
    height: '30px',
    border: '4px solid #ccc',
    borderTop: '4px solid #3F7FAA',
    borderRadius: '50%',
    animation: 'spin 1s linear infinite',
  },
};

export default Loader;
