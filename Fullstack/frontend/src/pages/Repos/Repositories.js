import React, { useState, useEffect, useMemo } from 'react';
import Repository from '../../components/Repo/Repository';
import { useNavigate } from 'react-router';
import Page from "../../components/Page";
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

// Debouncing for search
function useDebounce(value, delay) {
  const [debouncedValue, setDebouncedValue] = useState(value);

  useEffect(() => {

    const handler = setTimeout(() => {
      setDebouncedValue(value);
    }, delay);
    return () => {
      clearTimeout(handler);
    };

  }, [value, delay]);

  return debouncedValue;
}



const Repositories = () => {
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');

  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  const filteredRepos = useMemo(() => {
    
    return repoData.filter(repo => {
      const query = debouncedSearchQuery.toLowerCase();

      return (
        repo.name.toLowerCase().includes(query) ||
        repo.description.toLowerCase().includes(query) ||
        repo.avatars.some(avatar => avatar.userName.toLowerCase().includes(query))
      );
    });
  }, [debouncedSearchQuery]);


  const handleSearch = (query) => {
    // Search
    setSearchQuery(query);
  };


  const handleEnterRepo = (name) => {
    navigate(`/repository/${name}/main`);
  }

  const handleEnterRepoContrib = (name) => {
    navigate(`/repository/${name}/contributors`);
  }


  const CreateNewRepo = () => {
    navigate("/repository/create");
  };


  const headerSection = (styling) => { return(
    <header className={styling.header}>
        <button className={styling.button} onClick={() => CreateNewRepo()}>New Repository</button>

        <SearchBox searchingWhat="repositories" onSearch={handleSearch} />

    </header>
  )};

  return (
    <Page header={headerSection}>

      {/* Display repositories */}
      {filteredRepos.length === 0 ? (
        <p className={styles.noRepositories}>No repositories...</p>
      ) : (
      
        filteredRepos.map((repo) => (
          <Repository
            key={repo.id}
            enterRepo={() => handleEnterRepo(repo.name)}
            enterRepoContrib={() => handleEnterRepoContrib(repo.name)}
            repoName={repo.name}
            description={repo.description}
            visability={repo.visibility}
            lastUpdated={repo.lastUpdated}
            avatars={repo.avatars}
          />
      )))}

    </Page>
  );
};


export default Repositories;
  