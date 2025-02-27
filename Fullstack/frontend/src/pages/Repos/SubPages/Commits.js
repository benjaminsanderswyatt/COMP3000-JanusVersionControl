import React from 'react';
import { useParams, useNavigate, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/repo/RepoPageHeader';
import Page from '../../../components/Page';
import Card from "../../../components/cards/Card";
import LoadingSpinner from '../../../components/LoadingSpinner';
import Dropdown from "../../../components/Dropdown";
import CommitGrid from '../../../components/repo/CommitGrid';

import styles from "../../../styles/pages/repos/subpages/RepoPage.module.css";
import { DateType } from '../../../helpers/DateHelper';


// POST the earliest commit hash (or null) and get the previous few commits (for pagination)
const branchData = {
  commits: [
    {
      date: "18 May 2025",
      commits: [
        {
          userId: 1,
          userName: "User 1",
          message: "A much longer commit message",
          hash: "4a35387be739933f7c9e6486959ec1affb2c1648",
          date: "2025-05-19T11:46:00Z",
        },
      ],
    },
    {
      date: "13 Apr 2025",
      commits: [
        {
          userId: 2,
          userName: "User 2",
          message: "A much longer commit message",
          hash: "4a35387be739933f7c9e6486959ec1affb2c1648",
          date: "2025-04-19T10:45:00Z",
        },
        {
          userId: 1,
          userName: "User 3",
          message: "A much longer commit message",
          hash: "4a35387be739933f7c9e6486959ec1affb2c1648",
          date: "2025-04-19T09:45:00Z",
        },
      ],
    },
    {
      date: "10 Feb 2025",
      commits: [
        {
          userId: 3,
          userName: "User 3",
          message: "A much longer commit message",
          hash: "4a35387be739933f7c9e6486959ec1affb2c1648",
          date: "2025-02-19T08:45:00Z",
        },
        {
          userId: 1,
          userName: "User 1",
          message: "A much longer commit message",
          hash: "4a35387be739933f7c9e6486959ec1affb2c1648",
          date: "2025-02-19T07:45:00Z",
        },
        {
          userId: 1,
          userName: "User 1",
          message: "A much longer commit message",
          hash: "4a35387be739933f7c9e6486959ec1affb2c1648",
          date: "2025-02-19T06:00:00Z",
        },
      ],
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
        <div className={`${styles.header} ${styles.spaced}`}>
          <h1>{name}</h1>

          {/* Dropdown list for picking branch */}
          <Dropdown
            label="Branch"
            dataArray={repoData.branches}
            onSelect={handleBranchChange}
            selectedValue={branch}
          />
        </div>
      </Card>

      
      <CommitGrid groupedCommits={branchData.commits} dateType={DateType.TIME_ONLY} />
      

    </Page>
  );
};


export default Commits;
  