import React from 'react';
import TextInput from './TextInput';
import PasswordInput from './PasswordInput';

import Button from '../Button';

const LoginForm = ({ formData, onChange, onSubmit }) => (
  <form onSubmit={onSubmit}>

    <TextInput 
      label="Email" 
      name="email" 
      type="email" 
      value={formData.email} 
      onChange={onChange} 
      placeholder="Email..." 
      required 
    />
    
    <PasswordInput 
      label="Password" 
      name="password" 
      value={formData.password} 
      onChange={onChange} 
      placeholder="Password..." 
      required 
    />
    
    <Button type="submit" style={styles.button}>Login</Button>

  </form>
);

const styles = {
  button: {
    width: '100%',
    padding: '10px',
    backgroundColor: 'var(--primary)',
    color: 'white',
    border: 'none',
    borderRadius: '8px',
    fontWeight: 'bold',
    cursor: 'pointer',
    margin: '20px 0px',
    fontSize: '1rem',
  },
};

export default LoginForm;