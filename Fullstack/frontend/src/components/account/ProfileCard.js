// ProfilePictureCard.jsx
import React, { useState } from 'react';
import ProfilePic from '../../components/images/ProfilePic';
import { uploadProfilePicture } from '../../api/fetch/fetchAccount';
import { useAuth } from '../../contexts/AuthContext';

const ProfilePictureCard = () => {
  const { authUserId, sessionExpired, updateProfilePicRefresh } = useAuth();
  const [selectedFile, setSelectedFile] = useState(null);
  const [previewUrl, setPreviewUrl] = useState(null);

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
    <div style={styles.profilePicCard}>
      <div style={styles.picContainer}>
        <h3>Profile Picture</h3>
        <ProfilePic userId={authUserId} style={styles.profileImage} />
        <div style={styles.underPicContainer}>
          <input 
            type="file" 
            accept="image/png" 
            id="file-upload"
            style={styles.hiddenFileInput} 
            onChange={handleFileChange} 
          />
          <label htmlFor="file-upload" style={styles.fileInputButton}>
            Change image
          </label>
          <span style={styles.fileName}>
            {selectedFile ? selectedFile.name : ""}
          </span>
        </div>
      </div>

      {/* Display the preview if available */}
      {previewUrl && (
        <>
          <hr style={styles.verticalDivider} />
          <div style={styles.picContainer}>
            <h3>Preview</h3>
            <img src={previewUrl} alt="Preview" style={styles.previewImage} />
            <div style={styles.underPicContainer}>
              <button 
                style={styles.saveButton} 
                onClick={handleSubmitUpload} 
                disabled={!selectedFile}
              >
                Save
              </button>
              <button 
                style={styles.cancelButton} 
                onClick={handleCancelUpload} 
                disabled={!selectedFile}
              >
                Cancel
              </button>
            </div>
          </div>
        </>
      )}
    </div>
  );
};

const styles = {
    profilePicCard: {
        display: "flex",
        width: "95%",
        background: "var(--tintcard)",
        border: 'var(--border) thin solid',
        borderRadius: "8px",
        boxShadow: "0 4px 6px rgba(0, 0, 0, 0.1)",
        padding: "20px",
        maxWidth: "1000px",
        justifyContent: "space-evenly",
        gap: "18px",
    },
    picContainer: {
        display: "flex",
        flexDirection: "column",
        gap: "18px",
        alignItems: "center",
    },
    profileImage: {
        width: "150px",
        height: "150px",
    },
    previewImage: {
        width: '150px',
        height: '150px',
        objectFit: 'cover',
        borderRadius: '50%',
        border: '2px solid var(--primary)',
    },


    underPicContainer: {
        display: "flex",
        flexDirection: "column",
        alignItems: "center",
        gap: "6px",
        padding: "4px",
        borderRadius: "6px",
        overflow: "hidden",
    },
    hiddenFileInput: {
        display: "none",
    },
    fileInputButton: {
        background: "var(--lightsecondary)",
        color: "var(--text)",
        fontSize: "1rem",
        borderRadius: "6px",
        padding: "4px 6px",
        cursor: "pointer",
        display: "inline-block",
        border: "1px solid var(--accent)",
        whiteSpace: "nowrap",
        width: "150px",
        textAlign: "center",
    },
    fileName: {
        color: "var(--text)",
        fontSize: "1rem",
    },


    saveButton: {
        background: "var(--lightgreenbutton)",
        color: "var(--text)",
        fontSize: "1rem",
        borderRadius: "6px",
        padding: "4px 6px",
        width: "150px",
        cursor: "pointer",
        display: "inline-block",
        border: "1px solid var(--accent)",
    },
    cancelButton: {
        background: "var(--lightredbutton)",
        color: "var(--text)",
        fontSize: "1rem",
        borderRadius: "6px",
        padding: "4px 6px",
        width: "150px",
        cursor: "pointer",
        display: "inline-block",
        border: "1px solid var(--accent)",
    },
    verticalDivider: {
        border: "none",
        borderLeft: "2px solid var(--primary)",
        margin: "0px"
    },
}

export default ProfilePictureCard;
