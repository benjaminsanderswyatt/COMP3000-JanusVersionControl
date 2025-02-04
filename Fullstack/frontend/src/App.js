import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from "react-router";


import { useAuth, AuthProvider } from './contexts/AuthContext';



import Layout from './pages/Layout';
import NoPage from './pages/NoPage';

import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';

import Repositories from './pages/Repos/Repositories';
import RepoCreate from './pages/Repos/Create';
import RepoPage from './pages/Repos/RepoPage';

import Account from './pages/Account';

import TermsOfUse from './pages/legal/TermsOfUse'
import PrivacyPolicy from './pages/legal/PrivacyPolicy';

import { ThemeProvider, useTheme } from './contexts/ThemeContext';
import './styles/App.css';



// ProtectedRoute you can only access if you have valid Json Web Token
const ProtectedRoute = () => {
  const { isLoggedIn } = useAuth();

  // If token exists, render the requested component
  return isLoggedIn ? <Outlet /> : <Navigate to="/login" replace />;
};

const App = () => {
  const { theme } = useTheme();

  // Set the theme from ThemeContext useTheme
  useEffect(() => {
    document.body.setAttribute('data-theme', theme);
  }, [theme]);


  return (
      <AuthProvider>
        <BrowserRouter>
          <Routes>
            <Route path="/" element={<Layout />}>

              {/*Default route*/}
              <Route index element={<Home />} />


              {/*User Pages*/}
              <Route path="login">
                <Route index element={<Login />}/>
              </Route>

              <Route path="register">
                <Route index element={<Register />}/>
              </Route>



              {/*Protected Routes*/}
              <Route path="repositories" element={<ProtectedRoute />}>
                <Route index element={<Repositories />}/>
              </Route>

                <Route path="repositories/create" element={<ProtectedRoute />}>
                  <Route index element={<RepoCreate />}/>
                </Route>

                <Route path="repositories/:name" element={<ProtectedRoute />}>
                  <Route index element={<RepoPage />}/>
                </Route>


              <Route path="account" element={<ProtectedRoute />}>
                <Route index element={<Account />}/>
              </Route>





              {/* Legal Pages */}
              <Route path="legal/termsofuse" element={<TermsOfUse />} />
              <Route path="legal/privacypolicy" element={<PrivacyPolicy />} />

              {/*Catch all invalid routes (404)*/}
              <Route path="*" element={<NoPage />} />
            </Route>
          </Routes>
        </BrowserRouter>
    </AuthProvider>
  );
};

// Wrap app in the theme and export that as the new app
const AppWithTheme = () => (
  <ThemeProvider>
    <App />
  </ThemeProvider>
);

export default AppWithTheme;
