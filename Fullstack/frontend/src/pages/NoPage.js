import React from 'react';
import { useNavigate } from 'react-router';

import styles from "../styles/pages/NoPage.module.css";

import Page from "../components/Page";



const NoPage = () => {
  const navigate = useNavigate();

  return (
    <Page>
      <div className={styles.insideCard}>
        <h1>Error: 404</h1>
        <p>Could not find page</p>

        <button 
          className="button"
          onClick={() => navigate("/")}
        >
          Go Home
        </button>
      </div>
    </Page>
  );
};


export default NoPage;
