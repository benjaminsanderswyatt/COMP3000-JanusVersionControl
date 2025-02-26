import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { InitRepo } from '../../api/fetch/fetchCreateRepo';

import TextInput from '../../components/Login/TextInput';
import Checkbox from '../../components/Checkbox';

import { useAuth  } from '../../contexts/AuthContext';



const Create = () => {
  const navigate = useNavigate();
  const [message, setError] = useState('');
  const [loading , setLoading] = useState(false);
  const { sessionExpired, authUser } = useAuth();

  const [formData, setFormData] = useState({
      name: "",
      description: "",
      isPrivate: false,
    });

  const isValidRepoName = (name) => {
    const regex = /^[a-zA-Z0-9_-]+$/; // Only letters, numbers, dash, underscore (chars that arnt encoded in url)
    return regex.test(name);
  };

  const onChange = ({ target: { name, value, type, checked } }) => {
    setFormData((prev) => ({
       ...prev, 
       [name]: type === "checkbox" ? checked : value, 
      }));
  };

  const onSubmit = async (event) => {
    event.preventDefault();

    setError("");
    setLoading(true);

    // Validate repo name
    if (!isValidRepoName(formData.name)) {
      setError("Repository name can only contain letters, numbers, dashes, and underscores.");
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
      setError("Failed to create repository");
    }

    setLoading(false);
  };


  return (
    <div style={styles.container}>
      <header style={styles.header}>
      </header>

      {message && <p style={styles.error}>{message}</p>}

      <form onSubmit={onSubmit}>

        <TextInput 
          label="Repository name" 
          name="name" 
          type="text" 
          value={formData.name} 
          onChange={onChange} 
          placeholder="Repository name..." 
          required 
        />

        <TextInput 
          label="Repository description" 
          name="description" 
          type="text" 
          value={formData.description} 
          onChange={onChange} 
          placeholder="Repository description..." 
          required 
        />

        <Checkbox
          id="privateRepo"
          checked={formData.isPrivate}
          onChange={(e) => setFormData((prev) => ({ ...prev, isPrivate: e.target.checked }))}
          label="Private Repository"
        />

        <button type="submit" style={styles.button} disabled={loading}>
          {loading ? "Creating..." : "Create Repository"}
        </button>

      </form>
      

    </div>
  );
};

const styles = {
  container: {
    background: "var(--card)",
    width: "90%",
    display: "flex",
    flexDirection: "column",
    gap: "18px",
    alignItems: "center",
    borderRadius: "8px",
    marginTop: "20px",
    justifyItems: "center",
    height: "fit-content",
  },
  header: {
    display: "flex",
    width: "100%",
    background: "var(--accent)",
    alignItems: "center",
    borderBottom: "var(--border) solid 1px",
    padding: "4px 10px",
    gap: "10px",
    justifyContent: "center",
    minHeight: "46px",
    borderRadius: "8px 8px 0px 0px",
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

  checkboxLabel: {
    display: "flex",
    alignItems: "center",
    gap: "10px",
    fontSize: "16px",
    cursor: "pointer",
  },
  error: {
    color: "red",
    fontSize: "14px",
  },
  button: {
    padding: "10px 15px",
    backgroundColor: "var(--primary)",
    color: "#fff",
    border: "none",
    borderRadius: "5px",
    cursor: "pointer",
  },
}


export default Create;
  