import React from 'react';
import { useNavigate, useLocation, useParams } from 'react-router';

import { useAuth } from '../../contexts/AuthContext';

import styles from "../../styles/Components/Repo/RepoPageHeader.module.css";

const RepoPageHeader = () => {
    const { authUser } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const { name } = useParams();

    const GotoFiles = () => {
        navigate(`/repository/${authUser}/${name}/main`);
    }

    const GotoCommits = () => {
        navigate(`/repository/${authUser}/${name}/commits`);
    }

    const GotoContributors = () => {
        navigate(`/repository/${authUser}/${name}/contributors`);
    }

    const GotoSettings = () => {
        navigate(`/repository/${authUser}/${name}/settings`);
    }

    // Check which tab is active
    const isActive = (path) => location.pathname === path;

    
    return (
        <nav class={styles.navbar}>

            <button
                className={`${styles.button} ${isActive(`/repository/${authUser}/${name}`) ? styles.selected: {}}`}
                onClick={() => GotoFiles()}>
                File
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repository/${authUser}/${name}/commits`) ? styles.selected: {}}`}
                onClick={() => GotoCommits()}>
                Commits
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repository/${authUser}/${name}/contributors`) ? styles.selected: {}}`}
                onClick={() => GotoContributors()}>
                Contributors
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repository/${authUser}/${name}/settings`) ? styles.selected: {}}`}
                onClick={() => GotoSettings()}>
                Settings
            </button>
        </nav>
    );
};




export default RepoPageHeader;
