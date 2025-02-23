import React, { useRef, useState } from 'react';
import ProfilePic from '../../components/images/ProfilePic';
import { uploadProfilePicture } from '../../api/fetch/fetchAccount';
import { useAuth } from '../../contexts/AuthContext';
import Card from "../Cards/Card";

import styles from "../../styles/Components/account/ProfileCard.module.css"


const ProfilePictureCard = () => {
  const { authUserId, sessionExpired, updateProfilePicRefresh } = useAuth();
  const [selectedFile, setSelectedFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(null);

  // Referance to the input to change profile pic
  const fileInputRef = useRef(null);
  const ActivateInput = () => {
    fileInputRef.current.click();
  };


  const handleFileChange = (event) => {
    const file = event.target.files[0];
    if (!file) return;
    event.target.value = ""; // Clear previous files

    // Ensure the file is a .png
    const fileExtension = file.name.split('.').pop().toLowerCase();
    if (fileExtension !== 'png') {
      alert('Please upload a .png file.');
      return;
    }

    setSelectedFile(file);

    // Generate preview using FileReader
    const reader = new FileReader();
    reader.onloadend = () => {
      setPreviewUrl(reader.result);
    };
    reader.readAsDataURL(file);
  };

  const handleSubmitUpload = async () => {
    const result = await uploadProfilePicture(selectedFile, sessionExpired);
    if (result.success) {
      console.log("Successfully changed profile pic");
      updateProfilePicRefresh(Date.now());

      // Clear the file and preview after upload
      setSelectedFile(null);
      setPreviewUrl(null);
    } else {
      alert("Upload failed: " + result.message);
    }
  };

  const handleCancelUpload = () => {
    // Clear the file and preview after cancelling
    setSelectedFile(null);
    setPreviewUrl(null);
  };

  return (
    <Card cardStyling={styles.profilePicCard}>
      <div className={styles.picContainer}>
        <h3>Profile Picture</h3>
        <ProfilePic handleClick={() => ActivateInput()} userId={authUserId} alt="Profile" innerClassName={styles.profileImage}/>
        <div className={styles.underPicContainer}>
          <input 
            ref={fileInputRef}
            type="file" 
            accept="image/png" 
            id="file-upload"
            className={styles.hiddenFileInput} 
            onChange={handleFileChange} 
          />
          <label htmlFor="file-upload" className={`${styles.baseButton} ${styles.fileInputButton}`} >
            Change image
          </label>
          <span className={styles.fileName}>
            {selectedFile ? selectedFile.name : ""}
          </span>
        </div>
      </div>

      {/* Display the preview if available */}
      {previewUrl && (
        <>
          <hr className={styles.verticalDivider} />
          <div className={styles.picContainer}>
            <h3>Preview</h3>
            <img src={previewUrl} alt="Preview" className={styles.previewImage} />
            <div className={styles.underPicContainer}>
              <button 
                className={`${styles.baseButton} ${styles.saveButton}`} 
                onClick={handleSubmitUpload} 
                disabled={!selectedFile}
              >
                Save
              </button>
              <button 
                className={`${styles.baseButton} ${styles.cancelButton}`} 
                onClick={handleCancelUpload} 
                disabled={!selectedFile}
              >
                Cancel
              </button>
            </div>
          </div>
        </>
      )}
    </Card>
  );
};


export default ProfilePictureCard;
