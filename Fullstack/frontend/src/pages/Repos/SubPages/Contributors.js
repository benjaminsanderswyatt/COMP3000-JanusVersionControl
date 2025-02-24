import React, { useState } from 'react';
import { useParams, useNavigate, useLocation, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/Repo/RepoPageHeader';
import Page from '../../../components/Page';
import Card from '../../../components/Cards/Card';
import LoadingSpinner from '../../../components/LoadingSpinner';
import ProfilePic from "../../../components/images/ProfilePic";

import styles from "../../../styles/Pages/Repos/SubPages/RepoPage.module.css";
import stylesContrib from "../../../styles/Pages/Repos/SubPages/Contributors.module.css";



const Contributors = () => {
  const { owner, name } = useParams(); // Get the name from the URL

  const repoData = useOutletContext();
  if (!repoData) {
    return <LoadingSpinner/>;
  }

  // repoData.owner.id
  

  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};
  
  return (
    <Page header={headerSection}>

      <Card>
        <div className={`${styles.header} ${styles.spaced}`}>
          <h1>{name}</h1>

          <div className={stylesContrib.ownerHolder}>
            <div>{repoData.owner.userName}</div>
            <ProfilePic
              userId={owner.id}
              label={owner.userName}
              alt={`Owner: ${owner.userName}}`}
              innerClassName={stylesContrib.owner}
            />
          </div>
        </div>
      </Card>

      <h1>Contributors</h1>
      <p>Users +-</p>
      <p>Access (read, write, admin)</p>

    </Page>
  );
};


export default Contributors;
  