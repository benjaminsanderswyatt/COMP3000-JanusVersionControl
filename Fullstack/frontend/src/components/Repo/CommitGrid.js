import React from 'react';
import Commit from './Commit';
import Card from "../Cards/Card";

import styles from "../../styles/Components/Repo/CommitGrid.module.css";

const CommitGrid = ({ groupedCommits = [] , dateType}) => {
  return (
    <div className={styles.gridContainer}>
      {groupedCommits.map((group, index) => (

        <Card key={index}>
          <h3 className={styles.date}>{group.date}</h3>

          {group.commits.map((commit, idx) => (

            <Commit key={idx} commit={commit} dateType={dateType} hasRows={true}/>
          ))}
          
        </Card>

      ))}
    </div>
  );
};


export default CommitGrid;