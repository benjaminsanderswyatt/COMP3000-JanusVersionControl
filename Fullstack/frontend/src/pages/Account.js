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
import Dropdown from '../components/inputs/Dropdown';

import styles from "../styles/pages/Account.module.css";

const Account = () => {
    const [tokenData, setTokenData] = useState(null);
    const [revokeToken, setRevokeToken] = useState("");
    const [loadingGen, setLoadingGen] = useState(false);
    const [loadingRevoke, setLoadingRevoke] = useState(false);
    const navigate = useNavigate();
    const { logout, sessionExpired } = useAuth();

    const [message, setMessage] = useState('');
    const [messageType, setMessageType] = useState('');
    const [errorField, setErrorField] = useState('');



    
    const expirationLabels = [
        '12 Hours',
        '1 Day', 
        '7 Days',
        '30 Days',
        '90 Days',
        '6 Months',
        '1 Year'
    ];
    
    const labelToHoursMap = {
        '12 Hours': 12,
        '1 Day': 24,
        '7 Days': 168,
        '30 Days': 720,
        '90 Days': 2160,
        '6 Months': 4320,
        '1 Year': 8760
    };

    const [selectedExpirationLabel, setSelectedExpirationLabel] = useState('30 Days');

    const handleGenAccessToken = async () => {
        setLoadingGen(true);
        setMessage('');
        setMessageType('');
        setErrorField('');

        try {
            const hours = labelToHoursMap[selectedExpirationLabel];

            console.log("hours", hours);

            const response = await GenAccessToken(hours, sessionExpired);
    
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
            setLoadingGen(false);
        }
    }

    const handleCopyToClipboard = () => {
    
        navigator.clipboard
          .writeText(tokenData)
          .then(() => {
            
            alert("PAT copied to clipboard");
          })
          .catch((err) => {
            console.error("Failed to copy: ", err);
    
            alert("Failed to copy PAT");
          });
      };



    const handleRevokeAccessToken = async () => {
        if (!revokeToken) {
            setMessage("Please enter a PAT to revoke");
            setMessageType('error');
            setErrorField('revokePAT');
            return;
        }

        setLoadingRevoke(true);
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
            setLoadingRevoke(false);
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
                <h3 className={styles.headings}>Generate Personal Access Token</h3>

                <Dropdown
                    label="Expiration Time"
                    dataArray={expirationLabels}
                    onSelect={setSelectedExpirationLabel}
                    selectedValue={selectedExpirationLabel}
                />

                <div className={styles.GenPATHolder}>
                    <button className="button" style={{width: "fit-content"}} onClick={handleGenAccessToken}>
                        {loadingGen ? "Generating..." : "Generate PAT"}
                    </button>

                    {tokenData && (
                        <>
                            <p className={styles.GenPAT}>{tokenData}</p>
                            <button onClick={handleCopyToClipboard} className={styles.copyButton}>
                                Copy
                            </button>
                        </>
                    )}

                    {errorField === 'generatePAT' && message && (
                        <p className={messageType}>{message}</p>
                    )}
                </div>
            </Card>

            {/* Revoke PAT */}
            <Card>
                <h3 className={styles.headings}>Revoke Personal Access Token</h3>

                <TextInput
                    name="revokeToken"
                    value={revokeToken}
                    onChange={(e) => setRevokeToken(e.target.value)}
                    placeholder="Enter PAT to revoke"
                    hasError={errorField === "revokePAT" && messageType === "error"}
                />
                
                <button className="button" onClick={handleRevokeAccessToken}>
                    {loadingRevoke ? "Revoking..." : "Revoke PAT"}
                </button>
                
                {errorField === 'revokePAT' && message && (
                    <div className={styles.revokeMessage}>
                        <p className={messageType}>{message}</p>
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
                    <p className={messageType}>{message}</p>
                )}
            </Card>
        </Page>
    );
};


export default Account;