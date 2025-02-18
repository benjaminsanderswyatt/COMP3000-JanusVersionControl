import React from 'react';
import TextInput from './TextInput';
import PasswordInput from './PasswordInput';
import Checkbox from '../Checkbox';

import Button from '../Button';

const RegisterForm = ({ formData, onChange, onSubmit, agreedToTerms, setAgreedToTerms }) => (
  <form onSubmit={onSubmit}>
    <TextInput 
      label="Username" 
      name="username" 
      value={formData.username} 
      onChange={onChange} 
      placeholder="Username..." 
      required 
    />

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
    
    <PasswordInput 
      label="Confirm Password" 
      name="confirmPassword" 
      value={formData.confirmPassword} 
      onChange={onChange} 
      placeholder="Confirm Password..." 
      required 
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

   
    <Button type="submit" style={styles.button} disabled={!agreedToTerms}>Register</Button>
    
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

export default RegisterForm;
