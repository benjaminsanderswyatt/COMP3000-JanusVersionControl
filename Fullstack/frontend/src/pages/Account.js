import React, { useState } from 'react';
import { useNavigate } from 'react-router';
import { deleteUser } from '../api/fetch/fetchUsers';

import { useAuth } from '../contexts/AuthContext';

import ThemeToggle from '../components/ThemeToggle';

import { GenAccessToken, RevokePAT } from '../api/fetch/fetchPAT';
import ProfilePictureCard from '../components/account/ProfileCard';

import Page from "../components/Page";
import Card from '../components/cards/Card';
import TextInput from '../components/inputs/TextInput';

import styles from "../styles/pages/Account.module.css";

const Account = () => {
    const [tokenData, setTokenData] = useState(null);
    const [revokeToken, setRevokeToken] = useState("");
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const { logout, sessionExpired } = useAuth();

    const [message, setMessage] = useState('');
    const [messageType, setMessageType] = useState('');
    const [errorField, setErrorField] = useState('');




    const handleGenAccessToken = async () => {
        setLoading(true);
        setMessage('');
        setMessageType('');
        setErrorField('');

        try {
          
            const ExpirationInHours = 30;
    
            const response = await GenAccessToken(ExpirationInHours, sessionExpired);
    
            if (!response.success)
                throw new Error(response.message);
          

            setTokenData(response.token);
            setMessage('Token generated successfully');
            setMessageType('success');
            setErrorField('generatePAT');

        } catch (err) {
            setMessage(err.message);
            setMessageType('error');
            setErrorField('generatePAT');
        } finally {
            setLoading(false);
        }
    }


    const handleRevokeAccessToken = async () => {
        if (!revokeToken) {
            setMessage("Please enter a token to revoke");
            setMessageType('error');
            setErrorField('revokePAT');
            return;
        }

        setLoading(true);
        setMessage('');
        setMessageType('');
        setErrorField('');

        try {
            const response = await RevokePAT(revokeToken, sessionExpired);
            if (!response.success) 
                throw new Error(response.message);

            
            setMessage(response.message);
            setMessageType('success');
            setErrorField('revokePAT');
            setRevokeToken("");
            
        } catch (err) {
            setMessage(err.message);
            setMessageType('error');
            setErrorField('revokePAT');
        } finally {
            setLoading(false);
        }
    }







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
                setMessage(result.message || "Failed to delete account");
                setMessageType('error');
                setErrorField('deleteAccount');
            }
        }
    };





    const headerSection = (pageStyles) => { return(
        <header className={pageStyles.header}>
            <ThemeToggle />
        </header>
    )};

    return (
        <Page header={headerSection}>

            <ProfilePictureCard/>

            

            {/* Generate PAT */}
            <Card>
                <button onClick={handleGenAccessToken}>
                    {loading ? "Generating..." : "Generate PAT"}
                </button>
                
                {errorField === 'generatePAT' && message && (
                    <div className={styles.message}>
                        <p className={styles[messageType]}>{message}</p>
                    </div>
                )}

                {tokenData && (
                    <div className={styles.PATHolder}>
                        <h2>Token Generated:</h2>
                        <pre className={styles.GenPAT}>{tokenData}</pre>
                    </div>
                )}
            </Card>

            {/* Revoke PAT */}
            <Card>
                <TextInput
                    label="PAT to Revoke"
                    name="revokeToken"
                    value={revokeToken}
                    onChange={(e) => setRevokeToken(e.target.value)}
                    placeholder="Enter PAT to revoke"
                />
                
                <button onClick={handleRevokeAccessToken}>
                    {loading ? "Revoking..." : "Revoke PAT"}
                </button>
                
                {errorField === 'revokePAT' && message && (
                    <div className={styles.message}>
                        <p className={styles[messageType]}>{message}</p>
                    </div>
                )}
            </Card>



            {/* Account Actions */}
            <Card>
                <button className="button" style={{ width: "100%" }} onClick={handleLogout}>
                    Logout
                </button>
                
                <button className={styles.deleteButton} onClick={handleDeleteAccount}>
                    Delete Account
                </button>

                {errorField === 'deleteAccount' && message && (
                    <div className={styles.message}>
                        <p className={styles[messageType]}>{message}</p>
                    </div>
                )}
            </Card>
        </Page>
    );
};


export default Account;