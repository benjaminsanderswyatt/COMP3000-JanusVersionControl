import React from "react";
import { useParams, useNavigate, useLocation } from "react-router";

import Page from "../../components/Page";
import Card from "../../components/Card";
import Commit from "../../components/Repo/Commit"
import RepoPageHeader from "../../components/Repo/RepoPageHeader";
import FileExplorer from "../../components/Repo/FileExplorer";
import CommitHistory from "../../components/Repo/CommitHistory";

import styles from "../../styles/Pages/Repos/RepoPage.module.css";

const repoData = {
  id: 1,
  description: "Repository description",
  visibility: false,
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
};

// Test files for file explorer
const files = [
  { name: "Folder 1", type: "folder", size: "-", date: "2025-02-19T15:45:00Z" },
  { name: "Folder 2", type: "folder", size: "-", date: "2025-02-19T15:45:00Z" },
  { name: "File Name", type: "file", size: "1.3 kb", date: "2025-02-19T15:45:00Z" },
];




const RepoPage = () => {
  const navigate = useNavigate();
  const { name } = useParams(); // Get the name from the URL




  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};
  
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
        <Commit commit={repoData.latestCommit}></Commit>
      </Card>


      <Card>
        <FileExplorer root={repoData.tree}></FileExplorer>
      </Card>


      

      <Card>
        <h2 className={styles.readme}>Read Me</h2>
        <p>{repoData.readme.content}</p>
      </Card>
      
    </Page>
  );

  
};

export default RepoPage;
