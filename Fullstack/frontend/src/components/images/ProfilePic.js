import React, { useEffect, useState } from "react";
import { useAuth } from "../../contexts/AuthContext";

const ProfilePic = ({ userId, style, handleClick }) => {
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
      <div style={{ ...styles.main, ...style }} onClick={() => handleClick()}>
        <img
          src={imageSrc}
          alt="Profile picture"
          onError={handleError} // Set to default if doesnt exist
          style={styles.image}
        />
      </div>
    );
};

const styles = {
    main: {
        borderRadius: "50%",
        overflow: "hidden",
        display: "flex",
        justifyContent: "center",
        alignItems: "center",
        border: "2px solid var(--primary)",
    },
    image: {
        width: "100%",
        height: "100%",
        objectFit: "cover",
    },
};

  
export default ProfilePic;