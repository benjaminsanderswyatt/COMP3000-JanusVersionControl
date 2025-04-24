import React, { useState, useMemo, useEffect, useCallback } from 'react';
import Repository from '../../components/repo/Repository';
import { useNavigate } from 'react-router';
import Page from "../../components/Page";
import SearchBox from '../../components/search/SearchBox';
import { useSearch } from '../../components/search/useSearch';
import Card from '../../components/cards/Card';
import LoadingSpinner from '../../components/LoadingSpinner';
import { fetchWithTokenRefresh } from '../../api/fetchWithTokenRefresh';
import { useAuth  } from '../../contexts/AuthContext';
import { AccessLevel, accessLevelMapping, displayAccessLevel } from '../../helpers/AccessLevel';

import tableStyles from "../../styles/components/Table.module.css";
import styles from "../../styles/pages/repos/Repositories.module.css";


const Colaborating = () => {
  const { sessionExpired } = useAuth();
  const navigate = useNavigate();


  const [repoData, setRepoData] = useState([]);
  const [repoError, setRepoError] = useState(null);
  const [loadingRepo, setLoadingRepo] = useState(true);

  const fetchColaboratingList = useCallback(async () => {
    setLoadingRepo(true);
    setRepoError(null);

    try {
      const data = await fetchWithTokenRefresh(
        `https://localhost:82/api/web/repo/colaborating-list`,
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
    
  }, [sessionExpired]);


  useEffect(() => {
    fetchColaboratingList();
  }, [fetchColaboratingList]);












  const [invites, setInvites] = useState([]);

  useEffect(() => {
    const fetchInvites = async () => {
      try {
        const data = await fetchWithTokenRefresh(
          `https://localhost:82/api/web/repoinvites`,
          { method: 'GET' },
          sessionExpired
        );

        setInvites(data);

      } catch (err) {
        console.error('Failed to fetch invites:', err);
      }
    };

    fetchInvites();
  }, [sessionExpired]);


  const handleAcceptInvite = async (inviteId) => {
    try {
      await fetchWithTokenRefresh(
        `https://localhost:82/api/web/repoinvites/${inviteId}/accept`,
        { method: 'POST' },
        sessionExpired
      );

      setInvites(invites.filter(inv => inv.inviteId !== inviteId));

      fetchColaboratingList(); // Refresh colaborating repos

    } catch (err) {
      console.error('Accept failed:', err);
    }
  };


  const handleDeclineInvite = async (inviteId) => {
    try {
      await fetchWithTokenRefresh(
        `https://localhost:82/api/web/repoinvites/${inviteId}/decline`,
        { method: 'POST' },
        sessionExpired
      );

      setInvites(invites.filter(inv => inv.inviteId !== inviteId));

    } catch (err) {
      console.error('Decline failed:', err);
    }
  };






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

      <Card>
        <h2 className={styles.header}>Invites</h2>
        {invites.length > 0 ? (
          <table className={tableStyles.table}>
            <tbody>
              {invites.map(invite => (
                <tr key={invite.inviteId} className={tableStyles.tbodyRow}>
                  <td>{invite.repoName}</td>
                  <td>{invite.inviterUsername}</td>
                  <td style={{textAlign: "center"}}>{displayAccessLevel(invite.accessLevel)}</td>
                  
                  <td className={styles.inviteActions}>
                    <button className={styles.inviteButton}>
                      <img src="/icons/tick.svg"
                        alt="Accept"
                        className={styles.inviteImg} 
                        onClick={() => handleAcceptInvite(invite.inviteId)}
                      />
                    </button>
                    <button className={styles.inviteButton}>
                      <img src="/icons/x.svg"
                        alt="Decline"
                        className={styles.inviteImg} 
                        onClick={() => handleDeclineInvite(invite.inviteId)}
                      />
                    </button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        ) : (
          <p>No pending invites...</p>
        )}
      </Card>






      {loadingRepo ? (
        <Card>
          <LoadingSpinner />
        </Card>
      ) : repoError ? (
        <Card>
          <p className='errorMessage'>Error: {repoError}</p>
          <button onClick={fetchColaboratingList} className="button">Try Again</button>
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
                collab => collab.accessLevel.toUpperCase() === "OWNER"
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
                  enterRepo={() => handleEnterRepo(ownerCollab.username, repo.name)}
                  enterRepoContrib={() => handleEnterRepoContrib(ownerCollab.username, repo.name)}
                  owner={ownerCollab}
                  repoName={repo.name}
                  description={repo.description || ''}
                  visibility={repo.isPrivate}
                  lastUpdated={repo.lastUpdated}
                  avatars={repo.collaborators.filter(c => c.id !== ownerCollab.id)}
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
