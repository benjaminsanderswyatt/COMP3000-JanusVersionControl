import React, { createContext, useState, useContext, useEffect, useCallback } from 'react';
import { login as apiLogin, register as apiRegister, logout as apiLogout } from "../api/fetch/fetchUsers";
import { jwtDecode } from 'jwt-decode';


const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [authUser, setAuthUser] = useState(null); // Default needs to be set to Username claim inside token
  const [authUserId, setAuthUserId] = useState(null); // Default needs to be set to UserId claim inside token
  const [isLoggedIn, setIsLoggedIn] = useState(false); // Default needs to be set localstorage token if its valid
  const [loading, setLoading] = useState(true); // So that redirects wait for token to be validated
  const [profilePicRefresh, setProfilePicRefresh] = useState(Date.now());

  const updateProfilePicRefresh = () => {
    setProfilePicRefresh(Date.now());
  };




  const login = async (email, password) => {
    const response = await apiLogin(email, password);
  
    if (response.success) {
      const token = response.token;
  
      // Decode the token to extract user information
      const decodedToken = jwtDecode(token);
  
      setAuthUser(decodedToken.Username);
      setAuthUserId(decodedToken.UserId);
  
      setIsLoggedIn(true);
      localStorage.setItem('token', response.token);
  
      // Return the username for navigation (stops async race condition)
      return decodedToken.Username;

    } else {
      throw new Error(response.message);
    }
  }


  const register = async (username, email, password) => {
    const response = await apiRegister(username,email,password);

    if (response.success) {

    } else {
      throw new Error(response.message);
    }

  }


  const logout = useCallback(async () => {
    await apiLogout();
    handleLogout();
  }, []);

  // Session expired is like logout but keeping the refresh token
  const sessionExpired = () => {
    window.alert('Your session has expired. Please log in again.');
    handleLogout();
  }

  const handleLogout = () => {
    setAuthUser(null);
    setAuthUserId(null);

    setIsLoggedIn(false);
    localStorage.removeItem("token");
  }



  useEffect(() => {
    
    const token = localStorage.getItem('token');
    if (token) {
      try {
        const decodedToken = jwtDecode(token);

        // Check if the token is expired or not valid
        if (decodedToken.exp * 1000 > Date.now()) {
          setAuthUser(decodedToken.Username);
          setAuthUserId(decodedToken.UserId);

          setIsLoggedIn(true);
        } else {
          logout(); // Token is expired, log the user out
        }
      } catch (error) {
        logout(); // Token is invalid, log the user out
      }
    }

    setLoading(false); // Token check is done
  }, [logout]);








  const value = {
    authUser,
    authUserId,
    isLoggedIn,
    loading,
    profilePicRefresh,
    updateProfilePicRefresh,
    login,
    logout,
    register,
    sessionExpired,
  }

  return (
    <AuthContext.Provider value={value}>
      {children}
    </AuthContext.Provider>
  );
};

export const useAuth = () => useContext(AuthContext);
