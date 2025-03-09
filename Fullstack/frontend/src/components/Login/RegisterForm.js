import React from 'react';
import TextInput from '../inputs/TextInput';
import PasswordInput from '../inputs/PasswordInput';
import Checkbox from '../Checkbox';

import styles from "../../styles/components/login/UserForm.module.css";

const RegisterForm = ({ formData, onChange, onSubmit, agreedToTerms, setAgreedToTerms, errorField }) => (
  <form onSubmit={onSubmit}>
    <TextInput 
      label="Username" 
      name="username" 
      value={formData.username} 
      onChange={onChange} 
      placeholder="Username..."
      hasError={errorField === "username"}
    />

    <TextInput 
      label="Email" 
      name="email" 
      type="email" 
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
    
    <PasswordInput 
      label="Confirm Password" 
      name="confirmPassword" 
      value={formData.confirmPassword} 
      onChange={onChange} 
      placeholder="Confirm Password..."
      hasError={errorField === "confirmPassword"}
    />
    
    <div>
      <Checkbox
        id="terms"
        checked={agreedToTerms}
        onChange={(e) => setAgreedToTerms(e.target.checked)}
        label={
          <>
            I agree to the{" "}
            <a href="/legal/termsofuse" target="_blank" rel="noopener noreferrer">Terms of Use</a>{" "}
            and{" "}
            <a href="/legal/privacypolicy" target="_blank" rel="noopener noreferrer">Privacy Policy</a>.
          </>
        }
      />
    </div>

   
    <button type="submit" className={styles.button}>Register</button>
    
  </form>
);


export default RegisterForm;
