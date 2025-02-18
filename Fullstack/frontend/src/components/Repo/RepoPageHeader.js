import React from 'react';
import { useNavigate, useLocation, useParams } from 'react-router';

import Button from '../Button';

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
        <nav style={styles.navbar}>

            <Button
                style={{ ...styles.button, ...(isActive(`/repositories/${name}`) ? styles.selected : {}) }}
                onClick={() => GotoFiles()}>
                File
            </Button>

            <Button 
                style={{ ...styles.button, ...(isActive(`/repositories/${name}/commits`) ? styles.selected : {}) }}
                onClick={() => GotoCommits()}>
                Commits
            </Button>

            <Button 
                style={{ ...styles.button, ...(isActive(`/repositories/${name}/contributors`) ? styles.selected : {}) }}
                onClick={() => GotoContributors()}>
                Contributors
            </Button>

            <Button 
                style={{ ...styles.button, ...(isActive(`/repositories/${name}/settings`) ? styles.selected : {}) }}
                onClick={() => GotoSettings()}>
                Settings
            </Button>
        </nav>
    );
};

const styles = {
    navbar: {
        display: "flex",
        justifyContent: "center",
        flex: 1,
        alignItems: "center",
        gap: "8px",
        flexWrap: "wrap"
    },
    button: {
        boxShadow: "0 1px 0 0 rgba(0, 0, 0, 0.1)",
        backgroundColor: "var(--button)",
        color: "var(--lighttext)",
        fontSize: "1rem",
        border: "var(--primary) thin solid",
        height: "100%",
        padding: "6px 12px",
        borderRadius: "8px",
        cursor: "pointer",
        whiteSpace: "nowrap",
    },
    selected: {
        background: "var(--border)",
    },
    
};



export default RepoPageHeader;
