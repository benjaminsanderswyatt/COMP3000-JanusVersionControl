import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from "react-router";


import { useAuth, AuthProvider } from './contexts/AuthContext';



import Layout from './pages/Layout';
import NoPage from './pages/NoPage';

import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';

import CommandLine from './pages/CommandLine';
import Discover from './pages/Discover';

import Repositories from './pages/Repos/Repositories';
import RepoCreate from './pages/Repos/Create';

import RepoPage from './pages/Repos/SubPages/RepoPage';
import Commits from './pages/Repos/SubPages/Commits';
import Contributers from './pages/Repos/SubPages/Contributors';
import Settings from './pages/Repos/SubPages/Settings';

import Collaborating from './pages/Colab/Collaborating';


import Account from './pages/Account';

import TermsOfUse from './pages/legal/TermsOfUse'
import PrivacyPolicy from './pages/legal/PrivacyPolicy';

import { ThemeProvider, useTheme } from './contexts/ThemeContext';
import './styles/App.css';
import Loader from './components/Loading';



// ProtectedRoute you can only access if you have valid Json Web Token
const ProtectedRoute = () => {
  const { isLoggedIn, loading } = useAuth();

  if (loading) { // Wait for the token to be validated
    return <Loader/>;
  }

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



              {/*Always available */}
              <Route path="discover">
                <Route index element={<Discover />}/>
              </Route>

              <Route path="commandline">
                <Route index element={<CommandLine />}/>
              </Route>



              {/*Protected Routes*/}

              <Route path="repositories/:owner" element={<ProtectedRoute />}>
                <Route index element={<Repositories />}/>
              </Route>

                <Route path="repository/create" element={<ProtectedRoute />}>
                  <Route index element={<RepoCreate />}/>
                </Route>

                <Route path="repository/:owner/:name/:branch" element={<ProtectedRoute />}>
                  <Route index element={<RepoPage />}/>
                </Route>

                <Route path="repository/:owner/:name/commits" element={<ProtectedRoute />}>
                  <Route index element={<Commits />}/>
                </Route>

                <Route path="repository/:owner/:name/contributors" element={<ProtectedRoute />}>
                  <Route index element={<Contributers />}/>
                </Route>

                <Route path="repository/:owner/:name/settings" element={<ProtectedRoute />}>
                  <Route index element={<Settings />}/>
                </Route>



              <Route path="collaborating/:owner" element={<ProtectedRoute />}>
                <Route index element={<Collaborating />}/>
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
