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


const Repositories = () => {
  const { authUser, sessionExpired } = useAuth();
  const navigate = useNavigate();
  



  const [repoData, setRepoData] = useState([]);
  const [repoError, setRepoError] = useState(null);
  const [loadingRepo, setLoadingRepo] = useState(true);
  
  const fetchRepositoryList = async () => {
    setLoadingRepo(true);
    setRepoError(null);

    try {
      const data = await fetchWithTokenRefresh(
        `https://localhost:82/api/web/repo/repository-list`,
        {
          method: "GET",
          headers: { "Content-Type": "application/json" },
        },
        sessionExpired
      );
      
      setRepoData(data);
    } catch (err) {
      setRepoError(err.message);
    } finally {
      setLoadingRepo(false);
    }
    
  };

  useEffect(() => {
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
          <p className='errorMessage'>Error: {repoError}</p>
          <button onClick={fetchRepositoryList} className="button">Try Again</button>
        </Card>
      ) : (
        <>
          {/* Display repositories */}
          {filteredRepos.length === 0 ? (
            <Card>
              <p className={styles.noRepositories}>No repositories...</p>
              <button>Create Me</button>
            </Card>
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