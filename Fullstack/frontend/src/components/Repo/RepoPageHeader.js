import React from 'react';
import { useNavigate, useLocation, useParams } from 'react-router';

import styles from "../../styles/Components/Repo/RepoPageHeader.module.css";

const RepoPageHeader = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { name } = useParams();

    const GotoFiles = () => {
        navigate(`/repositories/${name}`);
    }

    const GotoCommits = () => {
        navigate(`/repositories/${name}/commits`);
    }

    const GotoContributors = () => {
        navigate(`/repositories/${name}/contributors`);
    }

    const GotoSettings = () => {
        navigate(`/repositories/${name}/settings`);
    }

    // Check which tab is active
    const isActive = (path) => location.pathname === path;

    
    return (
        <nav class={styles.navbar}>

            <button
                className={`${styles.button} ${isActive(`/repositories/${name}`) ? styles.selected: {}}`}
                onClick={() => GotoFiles()}>
                File
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repositories/${name}/commits`) ? styles.selected: {}}`}
                onClick={() => GotoCommits()}>
                Commits
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repositories/${name}/contributors`) ? styles.selected: {}}`}
                onClick={() => GotoContributors()}>
                Contributors
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repositories/${name}/settings`) ? styles.selected: {}}`}
                onClick={() => GotoSettings()}>
                Settings
            </button>
        </nav>
    );
};




export default RepoPageHeader;
