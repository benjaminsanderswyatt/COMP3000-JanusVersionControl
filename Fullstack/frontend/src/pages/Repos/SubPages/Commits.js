import React, { useState } from 'react';
import { useParams, useNavigate, useLocation } from 'react-router';

import RepoPageHeader from '../../../components/Repo/RepoPageHeader';
import Page from '../../../components/Page';

import styles from "../../../styles/Pages/Repos/SubPages/Commits.module.css";


const Commits = () => {
  const { name } = useParams(); // Get the name from the URL

  /*
  const commits = [
    { user: 'temp', message: 'Initial Commit', hash: '#4a35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-30T12:34:56Z' },
    { user: 'temp', message: 'Fix bug in login', hash: '#5b35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-30T14:20:10Z' },
    { user: 'temp', message: 'Add new feature', hash: '#6c35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-29T09:15:30Z' },
    { user: 'temp', message: 'Refactor code', hash: '#7d35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-29T18:45:22Z' },
    { user: 'temp', message: 'Update README', hash: '#8e35387be739933f7c9e6486959ec1affb2c1648', date: '2023-10-28T10:10:10Z' },
  ];
  */



  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};
  
  return (
    <Page header={headerSection}>
      <h1>Commits</h1>
    </Page>
  );
};


export default Commits;
  