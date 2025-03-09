import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { InitRepo } from '../../api/fetch/fetchCreateRepo';
import Page from "../../components/Page";

import TextInput from '../../components/inputs/TextInput';
import AreaInput from '../../components/inputs/AreaInput';
import Checkbox from '../../components/Checkbox';

import { useAuth  } from '../../contexts/AuthContext';

import styles from "../../styles/pages/repos/Create.module.css";

const Create = () => {
  const navigate = useNavigate();
  const [message, setError] = useState('');
  const [loading , setLoading] = useState(false);
  const [errorField, setErrorField] = useState('');
  const { sessionExpired, authUser } = useAuth();

  const [formData, setFormData] = useState({
    name: "",
    description: "",
    isPrivate: false,
  });

  

  const onChange = ({ target: { name, value, type, checked } }) => {
    setFormData((prev) => ({
       ...prev, 
       [name]: type === "checkbox" ? checked : value, 
    }));
  };

  const onSubmit = async (event) => {
    event.preventDefault();

    setError("");
    setErrorField('');
    setLoading(true);

    // Check repo name is longer than 10 chars
    if (formData.name.length <= 10) {
      setError("Repository name must be longer than 10 characters");
      setErrorField("name");
      setLoading(false);
      return;
    }

    // Check repo name doesnt contain spaces
    if (/\s/.test(formData.name)) {
      setError("Repository name cannot contain spaces");
      setErrorField("name");
      setLoading(false);
      return;
    }

    // Check for invalid chars space ~, ^, :, ?, /, \, *, [, ]
    const invalidChars = /[ ~^:?/\\*[\]\x7F]|(\.\.)/;
    if (invalidChars.test(formData.name)) {
      setError("Repository name cannot contain invalid characters: ~ ^ : ? . / \\ * []");
      setErrorField("name");
      setLoading(false);
      return;
    }

    try {
      
      const response = await InitRepo(formData.name, formData.description, formData.isPrivate, sessionExpired);

      if (response.success) {
        // Goto the new repo page
        navigate(`/repository/${authUser}/${formData.name}`);
      } else {
        setError(response.message);
      }

    } catch (error) {
      setError("Failed to create repository. Try again later");
    }

    setLoading(false);
  };


  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
    </header>
  )};
  
  return (
    <Page header={headerSection}>


      <form onSubmit={onSubmit} className="form">

        <div>
          <TextInput 
            label="Repository name" 
            name="name" 
            type="text" 
            value={formData.name} 
            onChange={onChange} 
            placeholder="Name..." 
            hasError={errorField === "name"}
          />

          <AreaInput 
            label="Repository description" 
            name="description" 
            type="text" 
            value={formData.description} 
            onChange={onChange} 
            placeholder="Description..."
          />

          <Checkbox
            id="privateRepo"
            checked={formData.isPrivate}
            onChange={(e) => setFormData((prev) => ({ ...prev, isPrivate: e.target.checked }))}
            label="Private Repository?"
            labelStyle={{ fontWeight: "bold", color: "var(--lighttext)" }}
          />
        </div>
        
        <button type="submit" className={styles.button} disabled={loading}>
          {loading ? "Creating..." : "Create Repository"}
        </button>

        {message && <p className={styles.error}>{message}</p>}

      </form>
      

    </Page>
  );
};


export default Create;