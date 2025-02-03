import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router';


import TextInput from '../components/Login/TextInput';

import { useAuth  } from '../contexts/AuthContext';


const Create = () => {
  const navigate = useNavigate();

  const [formData, setFormData] = useState({
      name: "",
    });

  

  const onChange = ({ target: { name, value } }) => {
    setFormData((prev) => ({ ...prev, [name]: value }));
  };

  const onSubmit = (event) => {
    event.preventDefault();

    console.log("create repo: " + formData.name);
  };


  return (
    <div style={styles.container}>
      <form onSubmit={onSubmit}>

        <TextInput 
          label="Name" 
          name="name" 
          type="text" 
          value={formData.name} 
          onChange={onChange} 
          placeholder="Name..." 
          required 
        />

        <button type="submit" style={styles.button}>Create repository</button>

      </form>

    </div>
  );
};

const styles = {
  container: {
    width: "100%",
    justifyItems: "center",
  },
}


export default Create;
  