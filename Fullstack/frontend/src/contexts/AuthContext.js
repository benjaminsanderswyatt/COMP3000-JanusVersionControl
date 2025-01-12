import React, { createContext, useState, useContext } from 'react';
import { login as apiLogin, register as apiRegister } from "../api/fetch/fetchUsers";
import { jwtDecode } from 'jwt-decode';


const AuthContext = createContext();

export const AuthProvider = ({ children }) => {
  const [authUser, setAuthUser] = useState(null);
  const [isLoggedIn, setIsLoggedIn] = useState(false);


  const login = async (email, password) => {
    const response = await apiLogin(email, password);

    if (response.success) {
      const token = response.token;

      // Decode the token to extract user information
      const decodedToken = jwtDecode(token);

      setAuthUser(decodedToken.Username);
      setIsLoggedIn(true);
      localStorage.setItem('token', response.token);

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

  const sessionExpired = () => {
    alert('Your session has expired. Please log in again.');
    logout();
  }



  const logout = () => {
    setAuthUser(null);
    setIsLoggedIn(false);
    localStorage.removeItem("token");
    document.cookie = "refreshToken=; expires=Thu, 01 Jan 1970 00:00:00 UTC; path=/";
  };
  

  const value = {
    authUser,
    isLoggedIn,
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
