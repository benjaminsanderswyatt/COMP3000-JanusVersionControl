import React from 'react';

import styles from "../../styles/Components/Repo/Repository.module.css";

const Repository = ({ enterRepo }) => {



  return (
    <div onClick={() => enterRepo()} className={styles.container}>
      <div className={styles.header}>
        <h2 className={styles.repoName}>Repository Name</h2>
        <div className={styles.visability}>Visibility</div>
        

        <div className={styles.avatars}>
          <span className={styles.avatar}></span>
          <span className={styles.avatar}></span>
          <span className={styles.avatar}></span>
        </div>
      </div>
      

      <hr className={styles.divider}/>

      <p className={styles.description}>
        Lorem ipsum dolor sit amet, consectetur adipiscing elit. Leo magna metus
        sagittis accumsan quam ridiculus nisl sed. Egestas urna ornare primis
        venenatis; malesuada maecenas sed.
      </p>

      
      <span className={styles.lastUpdated}>Last Updated...</span>
      
    </div>
  );
};

export default Repository;
