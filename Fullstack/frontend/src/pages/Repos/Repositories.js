import React from 'react';
import Repository from '../../components/Repo/Repository';
import { useNavigate } from 'react-router';
import RepoBar from '../../components/Repo/RepoBar';

import SearchBox from '../../components/SearchBox';

import { useAuth  } from '../../contexts/AuthContext';

import styles from "../../styles/Pages/Repos/Repositories.module.css";


const Repositories = () => {
  const navigate = useNavigate();


  const handleSearch = (query) => {
    console.log('Searching repositories for:', query);
    // Search
  };


  const handleEnterRepo = (name) => {
    navigate(`/repository/${name}`);
  }


  const CreateNewRepo = () => {
    navigate("/repository/create");
  };

  return (
    <div className={styles.container}>

      <header className={styles.header}>
        <button className={styles.button} onClick={() => CreateNewRepo()}>New Repository</button>

        <SearchBox searchingWhat="repositories" onSearch={() => handleSearch()} />

      </header>


      {/* TODO For loop loads repos */}
      <Repository enterRepo={() => handleEnterRepo("RepoNameHere")}/>
      <Repository/>
      <Repository/>
      


      
    </div>
  );
};


export default Repositories;
  