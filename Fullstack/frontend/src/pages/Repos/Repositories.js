import React, { useMemo, useState, useEffect } from 'react';
import { useNavigate  } from 'react-router';

import Repository from '../../components/repo/Repository';
import Page from "../../components/Page";
import SearchBox from '../../components/search/SearchBox';
import { useSearch } from '../../components/search/useSearch';
import Card from '../../components/cards/Card';
import LoadingSpinner from '../../components/LoadingSpinner';

import { fetchWithTokenRefresh } from '../../api/fetchWithTokenRefresh';
import { useAuth } from '../../contexts/AuthContext';

import styles from "../../styles/pages/repos/Repositories.module.css";

/*
// Example repo data
const repoData = [
  {
    id: 1,
    name: "Project_1",
    description: "Project description 1",
    visibility: false,
    lastUpdated: "2025-02-19T15:45:00Z",
    colaborators: [
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
    colaborators: [
      { id: 4, userName: "User4" }
    ],
  },
  {
    id: 3,
    name: "Project_3",
    description: "Project description 3",
    visibility: false,
    lastUpdated: "2025-03-18T09:30:00Z",
    colaborators: [
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
    colaborators: [
    ],
  }
];
*/




const Repositories = () => {
  const { authUser, sessionExpired } = useAuth();
  const navigate = useNavigate();
  



  const [repoData, setRepoData] = useState([]);
  const [repoError, setRepoError] = useState(null);
  const [loadingRepo, setLoadingRepo] = useState(true);
  
  useEffect(() => {
    const fetchRepositoryList = async () => {
      try {
        const data = await fetchWithTokenRefresh(
          `https://localhost:82/api/web/repo/repository-list`,
          {
            method: "GET",
            headers: { "Content-Type": "application/json" },
          },
          sessionExpired
        );

        console.log("Fetched repositories:", data);
        
        setRepoData(data);
      } catch (err) {
        setRepoError(err.message);
      } finally {
        setLoadingRepo(false);
      }
    };

    fetchRepositoryList();
    
  }, [sessionExpired]);









  // Searching hook handles urls and debounce
  const [searchValue, setSearchValue, debouncedSearchValue] = useSearch(500);

  const filteredRepos = useMemo(() => {
    const searchTerm = debouncedSearchValue.toLowerCase();
    return repoData.filter(repo =>
      repo.name.toLowerCase().includes(searchTerm) ||
      repo.description.toLowerCase().includes(searchTerm)
    );
  }, [debouncedSearchValue, repoData]);








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

        <SearchBox searchingWhat="repositories" value={searchValue} onChange={setSearchValue} onSubmit={(e) => e.preventDefault()} />

    </header>
  )};

  return (
    <Page header={headerSection}>

      {/* Repo data loading */}
      {loadingRepo ? (
        <Card>
          <LoadingSpinner/>
        </Card>

      ) : repoError ? (
        <Card>
          <div>Error: {repoError}</div>
        </Card>

      ) : (
        <>
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
                visability={repo.isPrivate}
                lastUpdated={repo.lastUpdated}
                avatars={repo.colaborators}
              />
          )))}
        </>
      )}



      

    </Page>
  );
};


export default Repositories;