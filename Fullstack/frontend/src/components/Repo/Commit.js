import React from 'react';

const Commit = ({ commit }) => {
  return (
    <div style={styles.container}>
      <div style={styles.left}>
        <span style={styles.avatar}></span>
        <p style={styles.text}>{commit?.message || 'No commit message'}</p>
      </div>
      <div style={styles.right}>
        <p style={styles.hash}>{commit?.hash || ''}</p>
        <p style={styles.date}>{commit?.date || ''}</p>
      </div>
    </div>
  );
};

const styles = {
  container: {
    display: 'flex',
    alignItems: 'center',
    justifyContent: 'space-between',
    width: '100%',
    padding: '8px 0',
    borderBottom: '1px solid var(--border)',
  },
  left: {
    display: 'flex',
    alignItems: 'center',
    gap: '10px',
    flexShrink: 0,
    minWidth: 0,
  },
  avatar: {
    width: '24px',
    height: '24px',
    background: 'var(--primary)',
    borderRadius: '50%',
    flexShrink: 0,
  },
  text: {
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
    maxWidth: '150px',
    minWidth: 0,
  },
  right: {
    display: 'flex',
    gap: '20px',
    flex: '1',
    justifyContent: 'flex-end',
    minWidth: 0,
  },
  hash: {
    overflow: 'hidden',
    textOverflow: 'ellipsis',
    whiteSpace: 'nowrap',
    minWidth: 0,
  },
  date: {
    whiteSpace: 'nowrap',
    flexShrink: 0,
  },
};

export default Commit;