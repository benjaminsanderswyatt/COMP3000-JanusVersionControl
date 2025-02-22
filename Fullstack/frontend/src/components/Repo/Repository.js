import React from 'react';
import ProfilePic from '../images/ProfilePic';
import { formatDate } from "../../helpers/DateHelper";

import styles from "../../styles/Components/Repo/Repository.module.css";




const Repository = ({ enterRepo, id, 
  repoName="Repository Name",
  description="Repository description...",
  visability=false,
  lastUpdated="1970-01-01T00:00:00Z",
  avatars=[]
  }) => {

  // Set a maximum number of avatars to display.
  const maxDisplayedAvatars = 3;
  const displayedAvatars = avatars.slice(0, maxDisplayedAvatars);
  const extraCount = avatars.length - maxDisplayedAvatars;

  const extraAvatars = avatars.slice(maxDisplayedAvatars);
  const extraUsernames = extraAvatars.map(avatar => avatar.userName).join(', ');

  const handleNavigation = async () => {

  }

  return (
    <div onClick={() => enterRepo()} className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.repoName}>{repoName}</h2>
        
        <div className={styles.visability}>{visability ? "Public" : "Private"}</div>
        

        <div className={styles.avatars}>
          
          {displayedAvatars.map((avatar) => (
            <ProfilePic
              userId={avatar.id}
              label={avatar.userName}
              innerClassName={styles.avatar}
              handleClick={() => handleNavigation(avatar.id)}
            />
          ))}
        </div>

        {extraCount > 0 && (
          <div className={styles.avatarExtra} title={extraUsernames}>
            +{extraCount}
          </div>
        )}

      </div>
      

      <hr className={styles.divider}/>

      <p className={styles.description}>
        {description}
      </p>
      

      <div className={styles.lastUpdated}>
        <span>Last updated:</span>
        <span className={styles.lastUpdatedDate}>{formatDate(lastUpdated)}</span>
      </div>

    </div>
  );
};

export default Repository;
