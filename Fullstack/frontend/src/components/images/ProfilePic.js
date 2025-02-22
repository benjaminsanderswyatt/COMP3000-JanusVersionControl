import React, { useEffect, useState } from "react";
import { useAuth } from "../../contexts/AuthContext";

import styles from "../../styles/Components/images/ProfilePic.module.css";

const ProfilePic = ({ userId, label, alt, innerClassName, handleClick }) => {
    const { profilePicRefresh } = useAuth();
    const [imageSrc, setImageSrc] = useState(getImageUrl(userId, profilePicRefresh));
  
    // Helper function to build the URL with a cache-busting parameter
    function getImageUrl(userId, timestamp) {
        return `/images/${userId}.png?time=${timestamp}`;
    }

    // Update imageSrc whenever the refreshTimestamp or userId changes
    useEffect(() => {
        setImageSrc(getImageUrl(userId, profilePicRefresh));
    }, [userId, profilePicRefresh]);



    const handleError = () => {
        setImageSrc("/Icons/account.svg"); // Default image
      };

    return (
      <div title={label} className={`${styles.main} ${innerClassName}`} onClick={() => handleClick()}>
        <img
          src={imageSrc}
          alt={alt}
          onError={handleError} // Set to default if doesnt exist
          className={styles.image}
        />
      </div>
    );
};
  
export default ProfilePic;