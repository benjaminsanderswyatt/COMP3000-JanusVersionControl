import React from 'react';

import Card from "../../components/Card";
import Commit from "../../components/Repo/Commit";
import { formatOnlyDate } from "../../helpers/DateHelper";

import styles from "../../styles/Components/Repo/Commit.module.css";


const CommitHistory = ({ commits }) => {
  if (!commits?.length) return <div>No commits found</div>;

  // Group commits by day
  const groupedCommits = commits
    .sort((a, b) => new Date(b.date) - new Date(a.date)) // Sort newest first
    .reduce((groups, commit) => {
      const dateKey = new Date(commit.date).toISOString().split('T')[0];
      if (!groups[dateKey]) {
        groups[dateKey] = [];
      }
      groups[dateKey].push(commit);
      return groups;
    }, {});

  return (
    <div className={styles.commitHistory}>
      {Object.entries(groupedCommits).map(([date, dailyCommits]) => (
        <Card key={date}>
          <div className={styles.commitDayGroup}>
            <h3 className={styles.commitDayHeader}>
              {formatOnlyDate(date)}
            </h3>
            {dailyCommits.map((commit) => (
              <Commit 
                key={commit.commitHash} 
                commit={commit} 
                className={styles.commitItem}
              />
            ))}
          </div>
        </Card>
      ))}
    </div>
  );
};


export default CommitHistory;