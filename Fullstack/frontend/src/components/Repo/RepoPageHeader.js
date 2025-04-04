import React from 'react';
import { useNavigate, useLocation, useParams } from 'react-router';

import { useAuth } from '../../contexts/AuthContext';
import styles from "../../styles/components/repo/RepoPageHeader.module.css";

const RepoPageHeader = () => {
    const navigate = useNavigate();
    const location = useLocation();
    const { owner, name, branch } = useParams();
    const { authUser } = useAuth();

    const GotoFiles = () => {
        navigate(`/repository/${owner}/${name}/${branch || 'main'}`);
    }

    const GotoCommits = () => {
        navigate(`/repository/${owner}/${name}/commits/${branch || 'main'}`);
    }

    const GotoContributors = () => {
        navigate(`/repository/${owner}/${name}/contributors`);
    }

    const GotoSettings = () => {
        navigate(`/repository/${owner}/${name}/settings`);
    }


    // Check which tab is active
    const isActive = (path) => {

        return location.pathname === path;
    };
    
    return (
        <nav class={styles.navbar}>

            <button
                className={`${styles.button} ${isActive(`/repository/${owner}/${name}/${branch}`) ? styles.selected: {}}`}
                onClick={() => GotoFiles()}>
                File
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repository/${owner}/${name}/commits/${branch}`) ? styles.selected: {}}`}
                onClick={() => GotoCommits()}>
                Commits
            </button>

            <button 
                className={`${styles.button} ${isActive(`/repository/${owner}/${name}/contributors`) ? styles.selected: {}}`}
                onClick={() => GotoContributors()}>
                Contributors
            </button>

            {/* Only owner sees setting page */}
            {authUser === owner && (
                <button 
                    className={`${styles.button} ${isActive(`/repository/${owner}/${name}/settings`) ? styles.selected: {}}`}
                    onClick={() => GotoSettings()}>
                    Settings
                </button>
            )}
        </nav>
    );
};




export default RepoPageHeader;
