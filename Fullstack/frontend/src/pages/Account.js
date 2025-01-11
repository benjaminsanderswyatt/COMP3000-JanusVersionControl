import React from 'react';
import { useNavigate } from 'react-router';
import { deleteUser } from '../api/fetchUsers';

import ThemeToggle from '../components/ThemeToggle';


const Account = () => {
    const navigate = useNavigate();

    const handleLogout = () => {
        // Remove token from localStorage
        localStorage.removeItem('token');
        navigate('/');
    };


    const handleDeleteAccount = async () => {
        if (window.confirm('Are you sure you want to delete your account?')) {
            const result = await deleteUser();

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