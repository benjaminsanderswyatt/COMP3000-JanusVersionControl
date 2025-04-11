import React, { useState, useEffect } from "react";
import { useParams, useNavigate, useOutletContext } from "react-router";

import { fetchWithTokenRefresh } from "../../../api/fetchWithTokenRefresh";
import { useAuth } from "../../../contexts/AuthContext";
import Page from "../../../components/Page";
import Card from "../../../components/cards/Card";
import Commit from "../../../components/repo/Commit"
import RepoPageHeader from "../../../components/repo/RepoPageHeader";
import FileExplorer from "../../../components/repo/FileExplorer";
import LoadingSpinner from "../../../components/LoadingSpinner";
import Dropdown from "../../../components/inputs/Dropdown";

import styles from "../../../styles/pages/repos/subpages/RepoPage.module.css";
import { DateType } from "../../../helpers/DateHelper";


import "../../../styles/Markdown.css";

import ReactMarkdown from 'react-markdown';


const RepoPage = () => {
  const { sessionExpired } = useAuth();
  const navigate = useNavigate();
  const { owner, name, branch } = useParams();


  const [branchData, setBranchData] = useState(null);
  const [branchError, setBranchError] = useState(null);
  const [loadingBranch, setLoadingBranch] = useState(true);
  
  useEffect(() => {
    const fetchBranchData = async () => {
      try {
        const data = await fetchWithTokenRefresh(
          `https://localhost:82/api/web/repo/${owner}/${name}/${branch}`,
          {
            method: "GET",
            headers: { "Content-Type": "application/json" },
          },
          sessionExpired
        );

        setBranchData(data);
      } catch (err) {
        setBranchError(err.message);
      } finally {
        setLoadingBranch(false);
      }
    };

    if (branch) {
      fetchBranchData();
    }
  }, [owner, name, branch, sessionExpired]);




  const handleBranchChange = (newBranch) => {
    // Navigate to the new branch
    navigate(`/repository/${owner}/${name}/${newBranch}`);
  };

  const handleCopyToClipboard = () => {
    const cloneUrl = `janus/${owner}/${name}`;

    navigator.clipboard
      .writeText(cloneUrl)
      .then(() => {
        
        alert("Clone copied to clipboard");
      })
      .catch((err) => {
        console.error("Failed to copy: ", err);

        alert("Failed to copy clone");
      });
  };





  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};

  // Loading repo data from RepoLayout
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
        <div className={styles.header}>
          <h1>{name}</h1>
          <div className={styles.visibility}>{repoData.isPrivate ? "Private": "Public" }</div>
        </div>

        <p>{repoData.description}</p>
      </Card>

      <Card>
        {/* Dropdown list for picking branch */}
        <div className={styles.repoDetails}>
          <Dropdown
            label="Branch"
            dataArray={repoData.branches}
            onSelect={handleBranchChange}
            selectedValue={branch}
          />

          <div className={styles.clone}>
            <div>janus/{owner}/{name}</div>
            
            {/* Copy to clipboard */}
            <button onClick={handleCopyToClipboard} className={styles.copyButton}>
              Copy
            </button>

          </div>
        </div>
      </Card>

      {/* Branch data loading */}
      {loadingBranch ? (
        <Card>
          <LoadingSpinner/>
        </Card>

      ) : branchError ? (
        <Card>
          <div>Error: {branchError}</div>
        </Card>

      ) : (
        <>
          <Card>
            <Commit commit={branchData.latestCommit} dateType={DateType.RELATIVE}/>
          </Card>

          <Card>
            <FileExplorer root={branchData.tree}></FileExplorer>
          </Card>

          {/* Only show readme when exists */}
          {branchData.readme && (
            <Card>
              <h2 className={styles.readme}>Read Me</h2>

              <div className={styles.markdownContent}>
                <div className="markdown">
                  <ReactMarkdown>{branchData.readme}</ReactMarkdown>
                </div>
              </div>

            </Card>
          )}
          
        </>
      )}


      
      
    </Page>
  );

  
};

export default RepoPage;
