import React from 'react';
import Commit from './Commit';

const CommitGrid = ({ groupedCommits = [] }) => {
  return (
    <div style={styles.gridContainer}>
      {groupedCommits.map((group, index) => (
        <Commit key={index} group={group} />
      ))}
    </div>
  );
};

const styles = {
  gridContainer: {
    display: 'grid',
    gridTemplateColumns: '1fr', // Single column by default
    gap: '10px', // Space between rows
    maxWidth: '1000px',
    margin: '0 auto',
  },
};

export default CommitGrid;