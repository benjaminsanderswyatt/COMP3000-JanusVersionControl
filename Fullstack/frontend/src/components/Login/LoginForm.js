import React from 'react';
import TextInput from './TextInput';
import PasswordInput from './PasswordInput';

import styles from "../../styles/Components/Login/UserForm.module.css";

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
    
    <button type="submit" className={styles.button}>Login</button>

  </form>
);


export default LoginForm;