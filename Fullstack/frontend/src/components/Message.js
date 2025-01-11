import React from 'react';

const Message = ({ type, text }) => {
  if (!text) return null;

  const styles = {
    color: type === 'success' ? 'green' : 'red',
    fontWeight: 'bold',
    textAlign: 'center',
    marginTop: '15px',
  };

  return <p style={styles}>{text}</p>;
};

export default Message;
