import React from 'react';
import { BrowserRouter, Routes, Route, Navigate } from "react-router-dom";

import Layout from "./pages/Layout";
import Login from "./pages/Login";
import Repos from "./pages/Repos";
import NoPage from "./pages/NoPage";

import './styles/App.css';

// PrivateRoute you can only access if you have valid Json Web Token
const PrivateRoute = ({ element: Component, ...rest }) => {
  const token = localStorage.getItem('token'); // Check for token in localStorage

  if (!token) {
    // If no token, redirect to login
    return <Navigate to="/" replace />;
  }

  // If token exists, render the requested component
  return <Component {...rest} />;
};


const App = () => {

  return (
      <BrowserRouter>
      <Routes>
        <Route path="/" element={<Layout />}>

          {/*Default route login page*/}
          <Route index element={<Login />} />

          {/*Protected Routes*/}
          <Route path="repos" element={<PrivateRoute element={Repos} />} />


          {/*Catch all invalid routes (404)*/}
          <Route path="*" element={<NoPage />} />
        </Route>
      </Routes>
      </BrowserRouter>
    );
  };

export default App;
