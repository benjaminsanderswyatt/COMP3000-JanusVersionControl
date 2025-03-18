import React, { useState, useEffect } from 'react';
import { useParams, useOutletContext, useNavigate } from 'react-router';

import { fetchWithTokenRefresh } from '../../../api/fetchWithTokenRefresh';
import RepoPageHeader from '../../../components/repo/RepoPageHeader';
import Page from '../../../components/Page';
import LoadingSpinner from '../../../components/LoadingSpinner';
import Card from "../../../components/cards/Card";
import AreaInput from '../../../components/inputs/AreaInput';
import { useAuth } from '../../../contexts/AuthContext';

import headerStyles from "../../../styles/pages/repos/subpages/RepoPage.module.css";
import styles from "../../../styles/pages/repos/subpages/SettingsPage.module.css";


const Settings = () => {
  const { authUser, sessionExpired } = useAuth();
  const { name, owner } = useParams();
  const navigate = useNavigate();
  const { repoData, setRepoData } = useOutletContext();

  const [description, setDescription] = useState("");
  const [updating, setUpdating] = useState(false);
  const [visibilityUpdating, setVisibilityUpdating] = useState(false);
  
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState(''); // Stores if the error is 'success' or 'error'
  const [errorField, setErrorField] = useState('');

  
  // Only set data once repo has loaded
  useEffect(() => {
    if (repoData) {
      setDescription(repoData.description || "");
    }
  }, [repoData]);

  // Wait for repodata to load
  if (!repoData) {
    return <LoadingSpinner />;
  }


  





  // Update the description
  const handleDescriptionSave = async () => {
    setUpdating(true);

    setMessage('');
    setErrorField('');

    try {
      const response = await fetchWithTokenRefresh(
        `https://localhost:82/api/web/reposettings/${owner}/${name}/description`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ description })
        },
        sessionExpired
      );

      setRepoData({ ...repoData, description: response.description });
      
      setMessage("Description updated successfully");
      setMessageType("success");
      setErrorField("description");

    } catch (err) {
      setMessage(err.message || "Failed to update description");
      setMessageType("error");
      setErrorField("description");

    }

    setUpdating(false);
  };




  // Update the visibility
  const handleVisibilityToggle = async () => {
    setVisibilityUpdating(true);

    setMessage('');
    setErrorField('');

    try {
      const newVisibility = !repoData.isPrivate;

      const response = await fetchWithTokenRefresh(
        `https://localhost:82/api/web/reposettings/${owner}/${name}/visibility`,
        {
          method: "PUT",
          headers: { "Content-Type": "application/json" },
          body: JSON.stringify({ isPrivate: newVisibility })
        },
        sessionExpired
      );

      setRepoData({ ...repoData, isPrivate: response.isPrivate });

      setMessage(`Repository has been set to ${response.isPrivate ? "Private": "Public" }`);
      setMessageType("success");
      setErrorField("visibility");

    } catch (err) {
      setMessage(err.message || "Failed to change visibility");
      setMessageType("error");
      setErrorField("visibility");

    }

    setVisibilityUpdating(false);
  };




  // Delete the repository
  const handleDeleteRepo = async () => {
    if (!window.confirm("Are you sure you want to delete this repository? This action cannot be undone")) {
      return;
    }

    setMessage('');
    setErrorField('');

    try {
      await fetchWithTokenRefresh(
        `https://localhost:82/api/web/reposettings/${owner}/${name}`,
        {
          method: "DELETE",
          headers: { "Content-Type": "application/json" },
        },
        sessionExpired
      );

      navigate(`/repository/${authUser}`);

    } catch (err) {
      setMessage(err.message || "Failed to delete repository");
      setMessageType("error");
      setErrorField("delete");

    }
  };













  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};
  

  return (
    <Page header={headerSection}>

      <Card>
        <div className={headerStyles.header}>
          <h1>{name}</h1>
          <div className={headerStyles.visibility}>{repoData.isPrivate ? "Private": "Public" }</div>
        </div>

        <p>{repoData.description}</p>
      </Card>


      {/* Visibility */}
      <Card>
        <div>
          <h3 className={styles.headings}>Visibility</h3>
          <div className={`${headerStyles.visibility} ${styles.visibility}`}>{repoData.isPrivate ? "Private": "Public" }</div>
        </div>
        
        <button className="button" onClick={handleVisibilityToggle} disabled={visibilityUpdating}>
          {visibilityUpdating
            ? "Updating..."
            : (repoData.isPrivate ? "Change to Public" : "Change to Private")}
        </button>
        
        {errorField === "visibility" && message && (
          <div className={styles.message}>
            <p className={messageType}>{message}</p>
          </div>
        )}

      </Card>


      {/* Description */}
      <Card>
        <h3 className={styles.headings}>Description</h3>
        <AreaInput
          label=""
          name="description"
          value={description}
          onChange={(e) => setDescription(e.target.value)}
          placeholder="Enter new description..."
          rows={4}
        />

        <div className={styles.descriptionHolder}>
          <button 
            className="button"
           onClick={handleDescriptionSave}
            disabled={updating || repoData.description == description}
          >
            {updating ? "Saving..." : "Save"}
          </button>

          <button 
            className="button"
            onClick={() => setDescription(repoData.description)}
            disabled={updating || repoData.description == description}
          >
            Cancel
          </button>
        </div>

        {errorField === "description" && message && (
          <div className={styles.message}>
            <p className={messageType}>{message}</p>
          </div>
        )}
      </Card>


      {/* Delete */}
      <Card>
        <h2 className={styles.headings}>Delete Repository</h2>
        <button className={`button ${styles.deleteButton}`} onClick={handleDeleteRepo}>
          Delete Repository?
        </button>
        
        {errorField === "delete" && message && (
          <div className={styles.message}>
            <p className={messageType}>{message}</p>
          </div>
        )}

      </Card>
      

    </Page>
  );
};


export default Settings;
