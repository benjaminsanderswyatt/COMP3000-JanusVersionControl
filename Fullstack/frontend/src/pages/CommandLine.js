import React from 'react';

import Page from "../components/Page";

import styles from "../styles/Pages/CommandLine.module.css";

const CommandLine = () => {


    const headerSection = (pageStyles) => { return(
        <header className={pageStyles.header}>
            
        </header>
    )};

    return (
        <Page header={headerSection}>
            <h1>Command line</h1>
            <p>Documentation</p>
        </Page>
    );
};


export default CommandLine;