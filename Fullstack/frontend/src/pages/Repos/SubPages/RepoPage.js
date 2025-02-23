import React from "react";
import { useParams, useNavigate, useLocation, useOutletContext } from "react-router";

import { useAuth } from "../../../contexts/AuthContext";
import Page from "../../../components/Page";
import Card from "../../../components/Cards/Card";
import Commit from "../../../components/Repo/Commit"
import RepoPageHeader from "../../../components/Repo/RepoPageHeader";
import FileExplorer from "../../../components/Repo/FileExplorer";
import LoadingSpinner from "../../../components/LoadingSpinner";


import styles from "../../../styles/Pages/Repos/SubPages/RepoPage.module.css";



const branchData = {
  latestCommit: { 
    userId: 1, 
    userName: "User 1",
    message: "A much longer commit message",
    hash: "4a35387be739933f7c9e6486959ec1affb2c1648",
    date: "2025-02-19T15:45:00Z",
  },
  readme: {
    content: "Readme content"
  },
  tree: {
    name: "root",
    hash: null,
    size: null,
    lastModified: "2025-02-19T15:45:00Z",
    children: [
      {
        name: "file1.txt",
        hash: "60b27f004e454aca81b0480209cce5081ec52390",
        size: 1.5,
        lastModified: "2025-02-19T15:45:00Z",
        children: []
      },
      {
        name: "file2.txt",
        hash: "cb99b709a1978bd205ab9dfd4c5aaa1fc91c7523",
        size: 1.1,
        lastModified: "2025-02-19T15:45:00Z",
        children: []
      },
      {
        name: "folder2",
        hash: null,
        lastModified: "2025-02-19T15:45:00Z",
        children: [
          {
            name: "subfile2.txt",
            hash: "10857312f8e7b367c7205972009d243501562a40",
            size: 2.0,
            lastModified: "2025-02-19T15:45:00Z",
            children: []
          }
        ]
      },
      {
        name: "New folder",
        hash: null,
        size: null,
        lastModified: "2025-02-19T15:45:00Z",
        children: [
          {
            name: "PBig.pptx",
            hash: "dcf15b8669eab90d495d7c469a79050dc4b684ed",
            size: 5.2,
            lastModified: "2025-02-19T15:45:00Z",
            children: []
          },
          {
            name: "TxtSmall.txt",
            hash: "e7fb9f85c9f7ba9ba239751333cbbeb53da7926c",
            size: 99,
            lastModified: "2025-02-19T15:45:00Z",
            children: []
          },
          {
            name: "WordBig.docx",
            hash: "a8e361783ffc2f72ca7d142cf9a44347423dc525",
            size: 0.1,
            lastModified: "2025-02-19T15:45:00Z",
            children: []
          },
          {
            name: "Sub folder",
            hash: null,
            size: null,
            lastModified: "2025-02-19T15:45:00Z",
            children: [
              {
                name: "SubFile.png",
                hash: "5pe361783ffc2f72ca7d142c7i844347423dc525",
                size: 0.2,
                lastModified: "2025-02-19T15:45:00Z",
                children: []
              }
            ]
          }
        ]
      }
    ]
  }
}





const RepoPage = () => {
  const { authUser } = useAuth();
  const navigate = useNavigate();
  const { owner, name, branch } = useParams();

  
  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};

  // Loading
  const repoData = useOutletContext();
  if (!repoData) {
    return (
      <Page header={headerSection}>
        <Card>
          <LoadingSpinner/>
        </Card>
      </Page>
    );
  }



  const handleBranchChange = (e) => {
    // Navigate to the new branch
    navigate(`/repository/${owner}/${name}/${e.target.value}`);
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




  
  
  return (
    <Page header={headerSection}>

      <Card>
        <div className={styles.header}>
          <h1>{name}</h1>
          <div className={styles.visibility}>{repoData.visibility ? "Public" : "Private"}</div>
        </div>

        <p>{repoData.description}</p>
      </Card>

      <Card>
        {/* Dropdown list for picking branch */}
        <div className={styles.repoDetails}>
          <div className={styles.branchHolder}>
            <label htmlFor="branch-select">Branch:</label>
            <select
              id="branch-select"
              value={branch}
              onChange={handleBranchChange}
              className={styles.branchSelect}
            >
              {repoData.branches.map((branchOption) => (
                <option key={branchOption} value={branchOption}>
                  {branchOption}
                </option>
              ))}
            </select>
          </div>

          <div className={styles.clone}>
            <div>janus/{owner}/{name}</div>
            {/* Copy to clipboard */}
            <button onClick={handleCopyToClipboard} className={styles.copyButton}>
              Copy
            </button>

          </div>
        </div>
      </Card>

      <Card>
        <Commit commit={branchData.latestCommit}></Commit>
      </Card>


      <Card>
        <FileExplorer root={branchData.tree}></FileExplorer>
      </Card>


      

      <Card>
        <h2 className={styles.readme}>Read Me</h2>
        <p>{branchData.readme.content}</p>
      </Card>
      
    </Page>
  );

  
};

export default RepoPage;
