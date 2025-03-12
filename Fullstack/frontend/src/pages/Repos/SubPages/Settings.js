import React from 'react';
import { useParams, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/repo/RepoPageHeader';
import Page from '../../../components/Page';
import LoadingSpinner from '../../../components/LoadingSpinner';
import Card from "../../../components/cards/Card";
import AreaInput from '../../../components/inputs/AreaInput';

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
        </div>
      </Card>

      <h1>Settings</h1>
      <p>Change description</p>
      <Card>
        <AreaInput className={styles.description}></AreaInput>

        <button>Edit Description</button>
      </Card>


      <p>Change visibility</p>
      <Card>
        <div className={styles.visibility}>{repoData.visibility ? "Private" : "Public"}</div>

        <button>{repoData.visibility ? "Change to Public" : "Change to Private"}</button>
      </Card>

      <p>Delete repo</p>
      <Card>
        <button>Delete Repository?</button>
      </Card>
    </Page>
  );
};


export default Settings;
  