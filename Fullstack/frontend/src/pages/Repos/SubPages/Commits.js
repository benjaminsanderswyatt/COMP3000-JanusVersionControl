import React, { useState, useEffect } from 'react';
import { useParams, useNavigate, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/repo/RepoPageHeader';
import Page from '../../../components/Page';
import Card from "../../../components/cards/Card";
import LoadingSpinner from '../../../components/LoadingSpinner';
import Dropdown from "../../../components/Dropdown";
import CommitGrid from '../../../components/repo/CommitGrid';

import { useAuth } from '../../../contexts/AuthContext';
import { fetchWithTokenRefresh } from '../../../api/fetchWithTokenRefresh';

import styles from "../../../styles/pages/repos/subpages/RepoPage.module.css";
import commitStyles from "../../../styles/pages/repos/subpages/CommitsPage.module.css"
import { DateType } from '../../../helpers/DateHelper';


const Commits = () => {
  const { sessionExpired } = useAuth();
  const navigate = useNavigate();
  const { owner, name, branch } = useParams();


  



  const handleBranchChange = (newBranch) => {
    // Navigate to the new branch
    navigate(`/repository/${owner}/${name}/commits/${newBranch}`);
  };

  const [commits, setCommits] = useState([]);
  const [nextCursor, setNextCursor] = useState();
  const [loading, setLoading] = useState();
  const [error, setError] = useState();


  const fetchCommits = async (startHash = '') => {
    setLoading(true);
    setError(null);
    try {
      let url = `https://localhost:82/api/web/commit/${owner}/${name}/${branch}/commits`;
      if (startHash) {
        url += `?startHash=${startHash}`;
      }
      const data = await fetchWithTokenRefresh(
        url,
        {
          method: 'GET',
          headers: { 'Content-Type': 'application/json' },
        },
        sessionExpired
      );

      console.log("Data: ", data);

      // Append if paginating, otherwise set new commits
      if (startHash) {
        setCommits((prev) => [...prev, ...data.commits]);
      } else {
        setCommits(data.commits);
      }
      setNextCursor(data.nextCursor);
    } catch (err) {
      setError(err.message);
    } finally {
      setLoading(false);
    }
  };

  // On branch (or owner/name) change, reset commits and fetch fresh data.
  useEffect(() => {
    setCommits([]);
    setNextCursor(null);
    fetchCommits('');
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [owner, name, branch, sessionExpired]);


  

  // Group commits by formatted date (e.g. "18 May 2025")
  const groupCommitsByDate = (commits) => {
    const grouped = commits.reduce((acc, commit) => {
      // Format the commit date as "dd MMM yyyy"
      const formattedDate = new Date(commit.date).toLocaleDateString(undefined, {
        day: '2-digit',
        month: 'short',
        year: 'numeric',
      });
      if (!acc[formattedDate]) {
        acc[formattedDate] = [];
      }
      acc[formattedDate].push(commit);
      return acc;
    }, {});

    const groups = Object.keys(grouped).map((date) => ({
      date,
      commits: grouped[date],
    }));
    // Sort groups descending by date
    groups.sort((a, b) => new Date(b.date) - new Date(a.date));
    return groups;
  };

  const groupedCommits = groupCommitsByDate(commits);







  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};

  // Loading
  const { repoData } = useOutletContext();
  if (!repoData) {
    return (
      <Page header={headerSection}>
        <Card>
          <LoadingSpinner/>
        </Card>
      </Page>
    );
  }




  return (
    <Page header={headerSection}>


      <Card>
        <div className={`${styles.header} ${styles.spaced}`}>
          <h1>{name}</h1>
          <Dropdown
            label="Branch"
            dataArray={repoData.branches}
            onSelect={handleBranchChange}
            selectedValue={branch}
          />
        </div>
      </Card>

      <div className={commitStyles.commitPage}>
        {error && <div className={styles.error}>Error: {error}</div>}

        {loading && commits.length === 0 ? (
          <LoadingSpinner />
        ) : (
          <CommitGrid groupedCommits={groupedCommits} dateType={DateType.TIME_ONLY} />
        )}

        {nextCursor && !loading && (
          <button
            className={commitStyles.loadMoreButton}
            onClick={() => fetchCommits(nextCursor)}
          >
            Load More
          </button>
        )}


        {loading && commits.length > 0 && <LoadingSpinner />}
      </div>
    </Page>
  );

};


export default Commits;
