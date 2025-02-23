import React, { useState } from 'react';
import { useParams, useNavigate, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/Repo/RepoPageHeader';
import Page from '../../../components/Page';
import Card from "../../../components/Cards/Card";

import styles from "../../../styles/Pages/Repos/SubPages/Commits.module.css";
import LoadingSpinner from '../../../components/LoadingSpinner';


const branchData = {
  commits: [
    {
      userId: 1, 
      userName: "User 1",
      message: "A much longer commit message",
      hash: "4a35387be739933f7c9e6486959ec1affb2c1648",
      date: "2025-02-19T15:45:00Z",
    },
  ]
}


const Commits = () => {
  const navigate = useNavigate();
  const { owner, name, branch } = useParams();

  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};

  // Loading
  const repoData = useOutletContext();
  if (!repoData) {
    return (
      <Page header={headerSection}>
        <Card>
          <LoadingSpinner/>
        </Card>
      </Page>
    );
  }



  const handleBranchChange = (e) => {
    // Navigate to the new branch
    navigate(`/repository/${owner}/${name}/${e.target.value}/commits`);
  };


  
  return (
    <Page header={headerSection}>
      <h1>Commits</h1>
      <p>branch</p>
      <p>all commits</p>

      <Card>
        <div className={styles.header}>
          <h1>{name}</h1>
          <div className={styles.visibility}>{repoData.visibility ? "Public" : "Private"}</div>
        </div>

        <p>{repoData.description}</p>
      </Card>

      <Card>
        {/* Dropdown list for picking branch */}
        <div className={styles.repoDetails}>
          <div className={styles.branchHolder}>
            <label htmlFor="branch-select">Branch:</label>
            <select
              id="branch-select"
              value={branch}
              onChange={handleBranchChange}
              className={styles.branchSelect}
            >
              {repoData.branches.map((branchOption) => (
                <option key={branchOption} value={branchOption}>
                  {branchOption}
                </option>
              ))}
            </select>
          </div>
        </div>
      </Card>
    </Page>
  );
};


export default Commits;
  