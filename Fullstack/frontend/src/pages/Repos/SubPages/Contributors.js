import React, { useState } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router';

import RepoPageHeader from '../../../components/Repo/RepoPageHeader';
import Page from '../../../components/Page';

import styles from "../../../styles/Pages/Repos/SubPages/Contributors.module.css";


const Contributors = () => {
  const { name } = useParams(); // Get the name from the URL




  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};
  
  return (
    <Page header={headerSection}>
      <h1>Contributors</h1>
    </Page>
  );
};


export default Contributors;
  