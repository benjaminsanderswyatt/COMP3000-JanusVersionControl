import React from 'react';
import TextInput from '../inputs/TextInput';
import PasswordInput from '../inputs/PasswordInput';

import styles from "../../styles/components/login/UserForm.module.css";

const LoginForm = ({ formData, onChange, onSubmit, errorField }) => (
  <form onSubmit={onSubmit}>

    <TextInput 
      label="Email" 
      name="email" 
      value={formData.email} 
      onChange={onChange} 
      placeholder="Email..." 
      hasError={errorField === "email"}
    />
    
    <PasswordInput 
      label="Password" 
      name="password" 
      value={formData.password} 
      onChange={onChange} 
      placeholder="Password..." 
      hasError={errorField === "password"}
    />
    
    <button type="submit" className={styles.button}>Login</button>

  </form>
);


export default LoginForm;