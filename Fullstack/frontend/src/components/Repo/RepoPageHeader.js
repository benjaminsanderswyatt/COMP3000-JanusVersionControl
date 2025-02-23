import React from 'react';
import { useNavigate, useLocation, useParams } from 'react-router';

import { useAuth } from '../../contexts/AuthContext';

import styles from "../../styles/Components/Repo/RepoPageHeader.module.css";

const RepoPageHeader = () => {
    const { authUser } = useAuth();
    const navigate = useNavigate();
    const location = useLocation();
    const { owner, name, branch } = useParams();

    const GotoFiles = () => {
        navigate(`/repository/${owner}/${name}/${branch || 'main'}`);
    }

    const GotoCommits = () => {
        navigate(`/repository/${owner}/${name}/commits`);
    }

    const GotoContributors = () => {
        navigate(`/repository/${owner}/${name}/contributors`);
    }

    const GotoSettings = () => {
        navigate(`/repository/${owner}/${name}/settings`);
    }


    // Check which tab is active
    const isActive = (path) => {

        // Handle the Files tab (acounting for branch names)
        if (path === 'files') {
            return location.pathname.startsWith(`/repository/${owner}/${name}`) &&
                !location.pathname.includes('/commits') &&
                !location.pathname.includes('/contributors') &&
                !location.pathname.includes('/settings');
        }
        
        return location.pathname === path;
    };
    
    return (
        <nav class={styles.navbar}>

            <button
                className={`${styles.button} ${isActive(`files`) ? styles.selected: {}}`}
                onClick={() => GotoFiles()}>
                File
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repository/${owner}/${name}/commits`) ? styles.selected: {}}`}
                onClick={() => GotoCommits()}>
                Commits
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repository/${owner}/${name}/contributors`) ? styles.selected: {}}`}
                onClick={() => GotoContributors()}>
                Contributors
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repository/${owner}/${name}/settings`) ? styles.selected: {}}`}
                onClick={() => GotoSettings()}>
                Settings
            </button>
        </nav>
    );
};




export default RepoPageHeader;
