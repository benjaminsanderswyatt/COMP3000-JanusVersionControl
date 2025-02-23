import React from 'react';
import ProfilePic from '../images/ProfilePic';
import { formatDate } from "../../helpers/DateHelper";

import styles from "../../styles/Components/Repo/Repository.module.css";

import Card from "../Card";




const Repository = ({ enterRepo, enterRepoContrib, id, 
  owner,
  repoName="Repository Name",
  description="Repository description...",
  visibility=false,
  lastUpdated="1970-01-01T00:00:00Z",
  avatars=[]
  }) => {

  // Set a maximum number of avatars to display.
  const maxDisplayedAvatars = 3;
  const displayedAvatars = avatars.slice(0, maxDisplayedAvatars);
  const extraCount = avatars.length - maxDisplayedAvatars;

  const extraAvatars = avatars.slice(maxDisplayedAvatars);
  const extraUsernames = extraAvatars.map(avatar => avatar.userName).join(', ');


  return (
    <Card>
      <div className={styles.header}>
        <h2 className={styles.repoName} onClick={() => enterRepo()}>{repoName}</h2>
        
        <div className={styles.visibility}>{visibility ? "Public" : "Private"}</div>
        
        <div onClick={() => enterRepoContrib()} className={styles.colaborators}>
          
          <div className={styles.avatars}>
            
            {displayedAvatars.map((avatar) => (
              <ProfilePic
                key={avatar.id}
                userId={avatar.id}
                label={avatar.userName}
                innerClassName={styles.avatar}
                handleClick={() => {}}
              />
            ))}
          </div>

          {extraCount > 0 && (
            <div className={styles.avatarExtra} title={extraUsernames}>
              +{extraCount}
            </div>
          )}


          {owner && (
            <div 
              className={styles.ownerHolder} 
              title={`Owner: ${owner.userName}`}
              style={{ borderLeft: avatars.length > 0 ? "1px solid var(--primary)" : "none" }}
            >
              <ProfilePic
                key={owner.id}
                userId={owner.id}
                innerClassName={styles.avatar}
                handleClick={() => {}}
              />
            </div>
          )}


        </div>
      </div>
      

      <hr className={styles.divider}/>

      <p className={styles.description}>
        {description}
      </p>
      

      <div className={styles.lastUpdated}>
        <span>Last updated:</span>
        <span className={styles.lastUpdatedDate}>{formatDate(lastUpdated)}</span>
      </div>

    </Card>
  );
};

export default Repository;
