import React, { useEffect } from 'react';
import { useNavigate } from 'react-router';
import { deleteUser } from '../api/fetch/fetchUsers';

import { useAuth  } from '../contexts/AuthContext';

import ThemeToggle from '../components/ThemeToggle';
import { getProfilePicture, uploadProfilePicture } from '../api/fetch/fetchAccount';


const Account = () => {
    const navigate = useNavigate();
    const { logout, sessionExpired } = useAuth();
    const [ profilePicture, setProfilePicture ] = useState(null);
    const [ preview, setPreview ] = useState(null);

    useEffect(() => {
        loadProfilePicture();
    }, []);

    const loadProfilePicture = async () => {
        const imageUrl = await getProfilePicture(sessionExpired);
        
        if (imageUrl){
            setProfilePicture(imageUrl);
        } else {
            setProfilePicture(defaultProfilePic); // Default
        }
    }

    const handleUpload = async (event) => {
        const file = event.target.files[0];
        if (!file)
            return;

        const objectURL = URL.createObjectURL(file)
        setPreview(objectURL);

        const result = await uploadProfilePicture(file, sessionExpired);

        if (result.success){
            setProfilePicture(result.profilePictureUrl);
            setPreview(null);
        } else {
            alert("Upload failed: " + result.message);
            setPreview(null);
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


    return (
        <div style={styles.main}>
            <ThemeToggle />
            
            <h1>Account</h1>

            <div style={styles.holder}>
                
                <div style={styles.imageContainer}>
                    <img 
                        src={preview || profilePicture || defaultProfilePic} 
                        alt="Profile" 
                        style={styles.profileImage} 
                    />
                </div>

                <input type="file" accept="image/*" onChange={handleUpload} />





                <div style={styles.buttonHolder}>
                    <button style={styles.logoutButton} onClick={handleLogout}>Logout</button>

                    <button style={styles.deleteButton} onClick={handleDeleteAccount}>Delete Account</button>

                </div>
                
            </div>



            
        </div>
    );
};

const styles = {
    main: {
        padding: '20px',
        textAlign: 'left',
        width: '100%',
    },
    holder: {

        background: 'var(--background)',
        border: 'solid',
        borderColor: 'var(--border)',
        borderRadius: '8px',
        display: 'block',
        padding: '20px',
        width: 'auto',
    },
    imageContainer: {
        display: "flex",
        justifyContent: "center",
        marginBottom: "20px",
    },
    profileImage: {
        width: "150px",
        height: "150px",
        borderRadius: "50%",
        objectFit: "cover",
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
};


export default Account;