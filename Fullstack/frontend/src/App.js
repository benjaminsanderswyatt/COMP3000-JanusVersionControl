import React, { useEffect } from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from "react-router";


import { useAuth, AuthProvider } from './contexts/AuthContext';


import FileDisplay from './pages/repos/subpages/FileDisplay';




import Layout from './pages/Layout';
import NoPage from './pages/NoPage';

import Home from './pages/Home';
import Login from './pages/Login';
import Register from './pages/Register';

import CommandLine from './pages/CommandLine';
import Discover from './pages/Discover';

import Repositories from './pages/repos/Repositories';
import RepoCreate from './pages/repos/Create';


import RepoLayout from './pages/repos/subpages/RepoLayout';
import RepoPage from './pages/repos/subpages/RepoPage';

import Commits from './pages/repos/subpages/Commits';
import CommitDiff from './pages/repos/subpages/CommitDiff';

import Contributors from './pages/repos/subpages/Contributors';
import Settings from './pages/repos/subpages/Settings';

import Collaborating from './pages/colab/Collaborating';


import Account from './pages/Account';

import TermsOfUse from './pages/legal/TermsOfUse'
import PrivacyPolicy from './pages/legal/PrivacyPolicy';

import { ThemeProvider, useTheme } from './contexts/ThemeContext';
import './styles/App.css';
import LoadingSpinner from './components/LoadingSpinner';



// ProtectedRoute you can only access if you have valid Json Web Token
const ProtectedRoute = () => {
  const { isLoggedIn, loading } = useAuth();

  if (loading) { // Wait for the token to be validated
    return <div><LoadingSpinner/></div>;
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
              <Route path="login" element={<Login />}/>

              <Route path="register"element={<Register />}/>



              {/* Public Routes */}
              <Route path="commandline" element={<CommandLine />}/>



              {/*Protected Routes*/}
              <Route element={<ProtectedRoute />}>

                <Route path="discover" element={<Discover />}/>


                <Route path="repository/:owner" element={<Repositories />} />
                <Route path="repository/create" element={<RepoCreate />} />

                {/* Subpages wrapped in RepoLayout */}
                <Route path="repository/:owner/:name" element={<RepoLayout />}>
                  <Route index element={<Navigate to="main" replace />}/> {/* Redirect to main branch */}
                  
                  <Route path="file/:fileHash" element={<FileDisplay />}/>
                  
                  <Route path="commits/:branch" element={<Commits />} />
                  <Route path="commit/:commitHash/diff" element={<CommitDiff />}/>
                  
                  <Route path="contributors" element={<Contributors />} />
                  <Route path="settings" element={<Settings />} />

                  <Route path=":branch" element={<RepoPage />} />

                </Route>



                <Route path="collaborating/:owner" element={<Collaborating />} />
                <Route path="account" element={<Account />} />

              </Route>


              {/* Legal Pages */}
              <Route path="legal/termsofuse" element={<TermsOfUse />} />
              <Route path="legal/privacypolicy" element={<PrivacyPolicy />} />

              {/* Catch all invalid routes (404) */}
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
