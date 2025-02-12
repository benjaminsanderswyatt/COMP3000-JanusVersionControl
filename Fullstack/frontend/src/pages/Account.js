import React, { useEffect, useState } from 'react';
import { useNavigate } from 'react-router';
import { deleteUser } from '../api/fetch/fetchUsers';

import { useAuth  } from '../contexts/AuthContext';

import ThemeToggle from '../components/ThemeToggle';
import { uploadProfilePicture } from '../api/fetch/fetchAccount';
import ProfilePic from '../components/images/ProfilePic';

import { GenAccessToken } from '../api/fetch/fetchPAT';


const Account = () => {
    const [tokenData , setTokenData] = useState(null);
    const [error, setError] = useState(null);
    const [loading, setLoading] = useState(false);
    const navigate = useNavigate();
    const { logout, sessionExpired, authUserId, updateProfilePicRefresh } = useAuth();



    const handleUpload = async (event) => {
        const file = event.target.files[0];
        if (!file)
            return;

        // Check if the file is a .png
        const fileExtension = file.name.split('.').pop().toLowerCase();
        if (fileExtension !== 'png') {
            alert('Please upload a .png file.');
            return;
        }

        const result = await uploadProfilePicture(file, sessionExpired);

        if (result.success){
            console.log("Successfully changed profile pic");
            updateProfilePicRefresh(Date.now());
        } else {
            alert("Upload failed: " + result.message);
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

    const CreateNewRepo = async () => {

    }

    return (
        <div style={styles.container}>

            <header style={styles.header}>
                
                <button style={styles.button} onClick={() => CreateNewRepo()}>New Repository</button>

                <ThemeToggle style={styles.button} />
            </header>

            <ThemeToggle />
            

                
            <ProfilePic
                userId={authUserId}
                style={styles.profileImage}
            />

            <input type="file" accept="image/png" onChange={handleUpload} />





            <div style={styles.buttonHolder}>
                <button style={styles.logoutButton} onClick={handleLogout}>Logout</button>

                <button style={styles.deleteButton} onClick={handleDeleteAccount}>Delete Account</button>

            </div>
                

            <button onClick={handleGenAccessToken}>Generate PAT</button>
            {loading && <p>Loading...</p>}
            {error && <p style={{ color: 'red' }}>{error}</p>}
            {tokenData  && (
                <div style={styles.PATHolder}>
                <h2>Token Generated:</h2>
                <pre style={styles.GenPAT}>{JSON.stringify(tokenData, null, 2)}</pre>
                </div>
            )}


            
        </div>
    );
};

const styles = {
    container: {
        background: "var(--card)",
        width: "90%",
        justifyItems: "center",
        display: "flex",
        flexDirection: "column",
        gap: "18px",
        alignItems: "center",
        borderRadius: "8px",
        marginTop: "20px",
        justifyItems: "center",
        paddingBottom: "18px",
        height: "fit-content",
      },
      header: {
        display: "flex",
        width: "100%",
        background: "var(--accent)",
        alignItems: "center",
        borderBottom: "var(--border) solid 1px",
        padding: "4px 10px",
        gap: "10px",
        justifyContent: "center",
        minHeight: "46px",
        borderRadius: "8px 8px 0px 0px",
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

    profileImage: {
        width: "150px",
        height: "150px",
    },
    buttonHolder: {
        display: 'flex',
        gap: '20px',
        overflow: 'auto',
    },
    logoutButton: {
        background: '#FF4747',
        marginTop: '50px',
        width: '70%',
        minWidth: '100px',
        border: 'none',
        borderRadius: '10px',
        color: 'var(--inverttext)',
        fontSize: '1rem',
        cursor: 'pointer',
        padding: '5px',
    },
    deleteButton: {
        background: '#BF3535',
        marginTop: '20px',
        width: '30%',
        minWidth: '100px',
        border: 'none',
        borderRadius: '10px',
        color: 'var(--inverttext)',
        fontSize: '1rem',
        cursor: 'pointer',
        padding: '5px',
    },
    PATHolder: {
        width: "100%",
    },
    GenPAT: {
        overflow: "auto",
    },
};


export default Account;