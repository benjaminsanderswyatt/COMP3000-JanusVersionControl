import React, { useState } from 'react';
import { useParams, useNavigate, useLocation, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/Repo/RepoPageHeader';
import Page from '../../../components/Page';

import styles from "../../../styles/Pages/Repos/SubPages/Settings.module.css";
import LoadingSpinner from '../../../components/LoadingSpinner';


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
      <h1>Settings</h1>
      <p>Change description</p>
      <p>Change visibility</p>
      <p>Delete repo</p>
    </Page>
  );
};


export default Settings;
  