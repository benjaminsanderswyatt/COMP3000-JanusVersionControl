import React, { useState } from 'react';
import { useParams, useNavigate, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/Repo/RepoPageHeader';
import Page from '../../../components/Page';
import Card from "../../../components/Cards/Card";
import LoadingSpinner from '../../../components/LoadingSpinner';
import Dropdown from "../../../components/Dropdown";

import styles from "../../../styles/Pages/Repos/SubPages/RepoPage.module.css";



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



  const handleBranchChange = (newBranch) => {
    // Navigate to the new branch
    navigate(`/repository/${owner}/${name}/${newBranch}/commits`);
  };


  
  return (
    <Page header={headerSection}>

      <Card>
        <div className={styles.header}>
          <h1>{name}</h1>
        </div>

        {/* Dropdown list for picking branch */}
        <Dropdown
          label="Branch"
          dataArray={repoData.branches}
          onSelect={handleBranchChange}
          selectedValue={branch}
        />
      
      </Card>

      <Card>
        
      </Card>
    </Page>
  );
};


export default Commits;
  