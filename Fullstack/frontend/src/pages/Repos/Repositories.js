import React, { useState, useEffect, useMemo } from 'react';
import { useNavigate, useSearchParams  } from 'react-router';

import Repository from '../../components/repo/Repository';
import Page from "../../components/Page";
import SearchBox from '../../components/search/SearchBox';
import { useDebounce } from '../../helpers/Debounce';

import { useAuth  } from '../../contexts/AuthContext';

import styles from "../../styles/pages/repos/Repositories.module.css";


// Example repo data
const repoData = [
  {
    id: 1,
    name: "Project_1",
    description: "Project description 1",
    visibility: false,
    lastUpdated: "2025-02-19T15:45:00Z",
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
    avatars: [
    ],
  }
];





const Repositories = () => {
  const { authUser } = useAuth();
  const navigate = useNavigate();
  
  
  const [searchParams, setSearchParams] = useSearchParams();
  const initialQuery = searchParams.get('search') || '';
  const [query, setQuery] = useState(initialQuery);
  const [debouncedQuery, { flush }] = useDebounce(query, 300);


  useEffect(() => {
    const params = new URLSearchParams(searchParams);
    if (debouncedQuery) {
      params.set('search', debouncedQuery);
    } else {
      params.delete('search');
    }
    setSearchParams(params);
  }, [debouncedQuery]);

  const handleSearch = (newQuery) => {
    setQuery(newQuery);
  };

  const handleSubmit = (e) => {
    e.preventDefault();
    flush(); // Immediately update the URL (skip debounce)
  };

  const filteredRepos = useMemo(() => {
    return repoData.filter(repo => {
      const searchTerm = query.toLowerCase();
      return (
        repo.name.toLowerCase().includes(searchTerm) ||
        repo.description.toLowerCase().includes(searchTerm)
      );
    });
  }, [query]);







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

        <SearchBox searchingWhat="repositories" value={query} onChange={handleSearch} onSearch={handleSubmit} />

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
  