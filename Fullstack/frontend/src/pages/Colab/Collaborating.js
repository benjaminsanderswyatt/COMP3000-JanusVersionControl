import React, { useState, useMemo, useEffect } from 'react';
import Repository from '../../components/repo/Repository';
import { useNavigate } from 'react-router';
import Page from "../../components/Page";
import SearchBox from '../../components/search/SearchBox';
import { useSearch } from '../../components/search/useSearch';
import Card from '../../components/cards/Card';
import LoadingSpinner from '../../components/LoadingSpinner';
import { fetchWithTokenRefresh } from '../../api/fetchWithTokenRefresh';
import { useAuth  } from '../../contexts/AuthContext';

import styles from "../../styles/pages/repos/Repositories.module.css";


const Colaborating = () => {
  const { authUser, sessionExpired } = useAuth();
  const navigate = useNavigate();


  const [repoData, setRepoData] = useState([]);
  const [repoError, setRepoError] = useState(null);
  const [loadingRepo, setLoadingRepo] = useState(true);

  useEffect(() => {
    const fetchColaboratingList = async () => {
          try {
            const data = await fetchWithTokenRefresh(
              `https://localhost:82/api/web/repo/colaborating-list`,
              {
                method: "GET",
                headers: { "Content-Type": "application/json" },
              },
              sessionExpired
            );
    
            console.log("Fetched colab repositories:", data);
            
            setRepoData(data);
          } catch (err) {
            setRepoError(err.message);
          } finally {
            setLoadingRepo(false);
          }
        };
    
        fetchColaboratingList();
        
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







  const handleEnterRepo = (ownerUsername, name) => {
    navigate(`/repository/${ownerUsername}/${name}/main`);
  }

  const handleEnterRepoContrib = (ownerUsername, name) => {
    navigate(`/repository/${ownerUsername}/${name}/contributors`);
  }




  const headerSection = (styling) => { return(
    <header className={styling.header}>
      
        <SearchBox searchingWhat="repositories" value={searchValue} onChange={setSearchValue} onSubmit={(e) => e.preventDefault()} />

    </header>
  )};

  return (
    <Page header={headerSection}>
      {loadingRepo ? (
        <Card>
          <LoadingSpinner />
        </Card>
      ) : repoError ? (
        <Card>
          <div>Error: {repoError}</div>
        </Card>
      ) : (
        <>
          {/* Display repositories */}
          {filteredRepos.length === 0 ? (
            <Card>
              <p className={styles.noRepositories}>No repositories...</p>
            </Card>
          ) : (
          
            filteredRepos.map((repo) => {
              // Get the owner from the collaborator with OWNER access level
              const ownerCollab = repo.collaborators.find(
                (collab) => collab.accessLevel === "OWNER"
              );
              
              if (!ownerCollab) {
                return (
                  <Card key={repo.id}>
                    <p>
                      Error: No owner found for repository "{repo.name}"
                    </p>
                  </Card>
                );
              }


              return (
                <Repository
                  key={repo.id}
                  enterRepo={() => handleEnterRepo(repo.name, ownerCollab.username)}
                  enterRepoContrib={() => handleEnterRepoContrib(repo.name, ownerCollab.username)}
                  owner={ownerCollab.username}
                  repoName={repo.name}
                  description={repo.description || ''}
                  visability={repo.isPrivate}
                  lastUpdated={repo.lastUpdated}
                  avatars={repo.collaborators}
                />
              );
            })


          )}
        </>
      )}


    </Page>
  );
};


export default Colaborating;
  