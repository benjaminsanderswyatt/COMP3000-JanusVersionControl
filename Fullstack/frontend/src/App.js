import React from 'react';
import { BrowserRouter, Routes, Route, Navigate, Outlet } from "react-router";

import Layout from './pages/Layout';
import NoPage from './pages/NoPage';

import Login from './pages/Login';
import Repositories from './pages/Repositories';
import Account from './pages/Account';

import TermsOfUse from './pages/legal/TermsOfUse'
import PrivacyPolicy from './pages/legal/PrivacyPolicy';


import './styles/App.css';

// PrivateRoute you can only access if you have valid Json Web Token
const PrivateRoute = () => {
  const token = localStorage.getItem('token'); // Check for token in localStorage

  // If token exists, render the requested component
  return token ? <Outlet /> : <Navigate to="/" replace />;
};


const App = () => {

  return (
    <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>

          {/*Default route login page*/}
          <Route index element={<Login />} />



          {/*Protected Routes*/}
          <Route path="repositories" element={<PrivateRoute />}>
            <Route index element={<Repositories />}/>
          </Route>

          <Route path="account" element={<PrivateRoute />}>
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
  );
};

export default App;
