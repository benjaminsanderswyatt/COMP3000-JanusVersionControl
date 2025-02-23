import React, { useState, useMemo } from 'react';
import Repository from '../../components/Repo/Repository';
import { useNavigate } from 'react-router';
import Page from "../../components/Page";
import SearchBox from '../../components/search/SearchBox';
import { useDebounce } from '../../helpers/Debounce';

import { useAuth  } from '../../contexts/AuthContext';

import styles from "../../styles/Pages/Repos/Repositories.module.css";



// Example repo data
const repoData = [
  {
    id: 1,
    name: "Project_1",
    description: "Project description 1",
    visibility: false,
    lastUpdated: "2025-02-19T15:45:00Z",
    owner: { 
      id: 2, 
      userName: "User2" 
    },
    avatars: [
      { id: 1, userName: "User1" },
      { id: 2, userName: "User2" },
      { id: 3, userName: "User3" },
      { id: 4, userName: "User4" },
      { id: 5, userName: "User5" },
      { id: 6, userName: "User6" },
      { id: 7, userName: "User7" },
      { id: 8, userName: "User8" },
      { id: 9, userName: "User9" },
      { id: 10, userName: "User10" },
      { id: 11, userName: "User11" },
    ],
  },
  {
    id: 2,
    name: "Project_2",
    description: "Project description 2",
    visibility: true,
    lastUpdated: "2024-02-18T09:30:00Z",
    owner: { 
      id: 3, 
      userName: "User3" 
    },
    avatars: [
      { id: 4, userName: "User4" }
    ],
  },
  {
    id: 3,
    name: "Project_3",
    description: "Project description 3",
    visibility: false,
    lastUpdated: "2025-03-18T09:30:00Z",
    owner: { 
      id: 4, 
      userName: "User4" 
    },
    avatars: [
      { id: 5, userName: "User5" },
      { id: 6, userName: "User6" }
    ],
  },
  {
    id: 4,
    name: "Project_4",
    description: "Project description 4",
    visibility: true,
    lastUpdated: "2025-11-18T09:30:00Z",
    owner: { 
      id: 5, 
      userName: "User5" 
    },
    avatars: [
    ],
  }
];





const Repositories = () => {
  const { authUser } = useAuth();
  const navigate = useNavigate();
  const [searchQuery, setSearchQuery] = useState('');

  const debouncedSearchQuery = useDebounce(searchQuery, 300);

  const filteredRepos = useMemo(() => {
    
    return repoData.filter(repo => {
      const query = debouncedSearchQuery.toLowerCase();

      return (
        repo.name.toLowerCase().includes(query) ||
        repo.description.toLowerCase().includes(query)
      );
    });
  }, [debouncedSearchQuery]);


  const handleSearch = (query) => {
    // Search
    setSearchQuery(query);
  };


  const handleEnterRepo = (name) => {
    navigate(`/repository/${authUser}/${name}/main`);
  }

  const handleEnterRepoContrib = (name) => {
    navigate(`/repository/${authUser}/${name}/contributors`);
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
            owner={repo.owner}
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
  