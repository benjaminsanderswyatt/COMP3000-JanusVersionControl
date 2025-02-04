import React, { useState } from 'react';
import { useNavigate, useLocation } from 'react-router';


import TextInput from '../../components/Login/TextInput';

import { useAuth  } from '../../contexts/AuthContext';


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
      <header style={styles.header}>
      </header>


      <div style={styles.createHolder}>

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
      

    </div>
  );
};

const styles = {
  container: {
    width: "100%",
    justifyItems: "center",
  },
  header: {
    display: "flex",
    width: "90%",
    background: "var(--accent)",
    alignItems: "center",
    borderBottom: "var(--border) solid 1px",
    padding: "4px 10px",
    gap: "10px",
    justifyContent: "center",
    marginTop: "20px",
    borderRadius: "8px 8px 0px 0px",
    minHeight: "46px",
    flexWrap: "wrap",
  },
  createHolder: {
    background: "var(--card)",
    width: "90%",
    padding: "18px",
    justifyItems: "center",
    display: "flex",
    flexDirection: "column",
    gap: "18px",
    alignItems: "center",
    borderRadius: "0px 0px 8px 8px",
  },
}


export default Create;
  