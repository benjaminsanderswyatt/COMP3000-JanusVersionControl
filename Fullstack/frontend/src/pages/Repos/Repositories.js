import React from 'react';
import Repository from '../../components/Repo/Repository';
import { useNavigate } from 'react-router';
import RepoBar from '../../components/Repo/RepoBar';

import SearchBox from '../../components/SearchBox';

import { useAuth  } from '../../contexts/AuthContext';

import styles from "../../styles/Pages/Repos/Repositories.module.css";



// Example repo data
const repoData = [
  {
    id: 1,
    name: "Project 1",
    description: "Project description 1",
    visibility: false,
    lastUpdated: "2025-02-19T15:45:00Z",
    avatars: [
      { id: 1, userName: "User 1" },
      { id: 2, userName: "User 2" },
      { id: 3, userName: "User 3" },
      { id: 4, userName: "User 4" },
      { id: 5, userName: "User 5" },
      { id: 6, userName: "User 6" },
      { id: 7, userName: "User 7" },
      { id: 8, userName: "User 8" },
      { id: 9, userName: "User 9" },
      { id: 10, userName: "User 10" },
      { id: 11, userName: "User 11" },
    ],
  },
  {
    id: 2,
    name: "Project 2",
    description: "Project description 2",
    visibility: true,
    lastUpdated: "2024-02-18T09:30:00Z",
    avatars: [
      { id: 4, userName: "User 4" }
    ],
  },
  {
    id: 3,
    name: "Project 3",
    description: "Project description 3",
    visibility: false,
    lastUpdated: "2025-03-18T09:30:00Z",
    avatars: [
      { id: 5, userName: "User 5" },
      { id: 6, userName: "User 6" }
    ],
  },
  {
    id: 4,
    name: "Project 4",
    description: "Project description 4",
    visibility: true,
    lastUpdated: "2025-11-18T09:30:00Z",
    avatars: [
    ],
  }
];



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


      {/* Display repositories */}
      {repoData.length === 0 ? (
        <p className={styles.noRepositories}>No repositories...</p>
      ) : (
      
        repoData.map((repo) => (
          <Repository
            enterRepo={() => handleEnterRepo(repo.name)}
            repoName={repo.name}
            description={repo.description}
            visability={repo.visibility}
            lastUpdated={repo.lastUpdated}
            avatars={repo.avatars}
          />
      )))}



      
    </div>
  );
};


export default Repositories;
  