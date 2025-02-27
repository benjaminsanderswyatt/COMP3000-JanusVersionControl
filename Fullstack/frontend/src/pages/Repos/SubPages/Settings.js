import React from 'react';
import { useParams, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/repo/RepoPageHeader';
import Page from '../../../components/Page';
import LoadingSpinner from '../../../components/LoadingSpinner';
import Card from "../../../components/cards/Card";

import styles from "../../../styles/pages/repos/subpages/RepoPage.module.css";



const Settings = () => {
  const { name } = useParams(); // Get the name from the URL

  const repoData = useOutletContext();
  if (!repoData) {
    return <LoadingSpinner/>;
  }



  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};
  
  return (
    <Page header={headerSection}>

      <Card>
        <div className={styles.header}>
          <h1>{name}</h1>
          <div className={styles.visibility}>{repoData.visibility ? "Public" : "Private"}</div>
        </div>
      </Card>

      <h1>Settings</h1>
      <p>Change description</p>
      <p>Change visibility</p>
      <p>Delete repo</p>
    </Page>
  );
};


export default Settings;
  