import React from "react";
import { useAuth } from "../../contexts/AuthContext";

import styles from "../../styles/components/images/ProfilePic.module.css";


const ProfilePic = ({ userId, label, alt, innerClassName, handleClick }) => {
    const { profilePicRefresh } = useAuth();
  
    // Server (nginx) returns the default icons/account.svg if it doesnt exist
    const imageUrl = `/images/${userId}.png?time=${profilePicRefresh}`;

    return (
      <div
        title={label}
        className={`${styles.main} ${innerClassName}`}
        onClick={handleClick}
      >
        <img src={imageUrl} alt={alt} className={styles.image} />
      </div>
    );
};
  
export default ProfilePic;