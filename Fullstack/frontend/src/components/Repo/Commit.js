import React from 'react';


const Commit = () => {



  return (
    <div style={styles.container}>

      <div style={styles.left}>
        <span style={styles.avatar}></span>

        <h3 style={styles.text}>Initial Commit</h3>
      </div>

      

      <div style={styles.right}>
        <h3 style={styles.hash}>#4a35387be739933f7c9e6486959ec1affb2c1648</h3>

        <h3 style={styles.date}>4 days ago</h3>
      </div>
      
        
    </div>
  );
};

const styles = {
  container: {
    display: "flex",
    alignItems: "center",
    justifyContent: "space-between",
    width: "100%",
    background: "var(--tintcard)",
    border: "var(--border) thin solid",
    borderRadius: "8px",
    boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
    padding: "10px 20px",
    maxWidth: "1000px",
    overflow: "hidden",
    gap: "10px",
  },
  left: {
    display: "flex",
    alignItems: "center",
    gap: "10px",
    flexShrink: 0,
    minWidth: 0,
  },
  avatar: {
    width: "24px",
    height: "24px",
    background: "var(--primary)",
    borderRadius: "50%",
    flexShrink: 0,
  },
  text: {
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
    maxWidth: "150px",
    minWidth: 0,
  },
  right: {
    display: "flex",
    gap: "20px",
    flex: "1",
    justifyContent: "flex-end",
    minWidth: 0,
  },
  hash: {
    overflow: "hidden",
    textOverflow: "ellipsis",
    whiteSpace: "nowrap",
    minWidth: 0,
  },
  date: {
    whiteSpace: "nowrap",
    flexShrink: 0,
  },
};


export default Commit;
