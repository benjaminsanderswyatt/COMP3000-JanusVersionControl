import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { deleteUser } from '../api/fetch/fetchUsers';

import { useAuth } from '../contexts/AuthContext';

import ThemeToggle from '../components/ThemeToggle';

import { GenAccessToken } from '../api/fetch/fetchPAT';
import ProfilePictureCard from '../components/account/ProfileCard';

import Page from "../components/Page";

import styles from "../styles/pages/Account.module.css";

const Account = () => {
    const [tokenData , setTokenData] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const { logout, sessionExpired } = useAuth();


    const handleLogout = () => {
        // Remove token from localStorage
        logout();
        navigate("/login");
    };


    const handleDeleteAccount = async () => {
        if (window.confirm('Are you sure you want to delete your account?')) {
            const result = await deleteUser(sessionExpired);

            if (result.success) {
                handleLogout(); // Logout user after account is deleted
            } else {
                alert(result.message);
            }
        }
    };


    const handleGenAccessToken = async () => {
        setLoading(true);
        setError(null);
        setTokenData (null);
        try {
          
          const ExpirationInHours = 30;
    
          const response = await GenAccessToken(ExpirationInHours, sessionExpired);
          console.log('Finished response Try');
    
          if (!response.success) {
            console.log('Failed: ' + response.message + ' token: ' + response.token);
            throw new Error(response.message);
          }
    
          setTokenData(response);
        } catch (err) {
          console.log('Catch');
          setError(err.message);
        } finally {
          console.log('Final');
          setLoading(false);
        }
    }

    const headerSection = (pageStyles) => { return(
        <header className={pageStyles.header}>
            <ThemeToggle />
        </header>
    )};

    return (
        <Page header={headerSection}>

            <ProfilePictureCard/>


            <div className={styles.buttonHolder}>
                <button className={styles.logoutButton} onClick={handleLogout}>Logout</button>

                <button className={styles.deleteButton} onClick={handleDeleteAccount}>Delete Account</button>

            </div>
                

            <button onClick={handleGenAccessToken}>Generate PAT</button>
            {loading && <p>Loading...</p>}
            {error && <p style={{ color: 'red' }}>{error}</p>}
            {tokenData  && (
                <div className={styles.PATHolder}>
                <h2>Token Generated:</h2>
                <pre className={styles.GenPAT}>{JSON.stringify(tokenData, null, 2)}</pre>
                </div>
            )}


            
        </Page>
    );
};


export default Account;