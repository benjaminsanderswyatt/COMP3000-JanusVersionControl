import React, { useState } from 'react';
import { useParams, useNavigate, useLocation, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/Repo/RepoPageHeader';
import Page from '../../../components/Page';

import styles from "../../../styles/Pages/Repos/SubPages/Contributors.module.css";
import LoadingSpinner from '../../../components/LoadingSpinner';


const Contributors = () => {
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
      <h1>Contributors</h1>
      <p>Users +-</p>
      <p>Access (read, write, admin)</p>

    </Page>
  );
};


export default Contributors;
  