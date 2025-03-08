import React, { useState, useEffect } from "react";
import { useLocation, useParams } from 'react-router';
import Editor from '@monaco-editor/react';
import { useTheme } from "../../../contexts/ThemeContext";

import { useAuth } from "../../../contexts/AuthContext";
import Page from "../../../components/Page";
import Card from "../../../components/cards/Card";
import RepoPageHeader from "../../../components/repo/RepoPageHeader";

import { fetchFileWithTokenRefresh } from "../../../api/fetchWithTokenRefresh";


//const owner = "user1";
//const repoName = "repo1";
//const fileHash = "10857312f8e7b367c7205972009d243501562a40";
//const fileName = "testFile.txt";
//const mimeType = "text/plain";

const getEditorLanguage = ({ fileName, mimeType }) => {
  const extension = fileName.split('.').pop().toLowerCase();
  if (mimeType === 'text/markdown' || extension === 'md') return 'markdown';
  if (mimeType === 'application/javascript' || extension === 'js') return 'javascript';
  if (mimeType === 'application/json' || extension === 'json') return 'json';
  if (mimeType === 'text/html' || extension === 'html') return 'html';
  if (mimeType === 'text/css' || extension === 'css') return 'css';
  // Add more mappings as needed
  return 'plaintext';
};




const FileDisplay = () => {
  const { theme } = useTheme();
  const { sessionExpired } = useAuth();
  const { owner, name, fileHash } = useParams();
  const location = useLocation();
  const { fileName = 'default.txt', mimeType = 'text/plain' } = location.state || {};

  const [content, setContent] = useState('');
  const [loading, setLoading] = useState(true);
  const [isSaving, setIsSaving] = useState(false);
  const [saveError, setSaveError] = useState(null);
  const [saveSuccess, setSaveSuccess] = useState(false);


  

  const editorOptions = {
    lineNumbers: 'on',
    scrollBeyondLastLine: true,
    automaticLayout: true,
    quickSuggestions: {
      other: true,
      comments: false,
      strings: true,
    },
    wordWrap: 'on',
    matchBrackets: 'always',
    folding: true,

    contextmenu: false,
    minimap: { enabled: false },
    parameterHints: { enabled: false },
    suggestOnTriggerCharacters: false,
  };

  useEffect(() => {
    const loadFile = async () => {
      try {
        const response = await fetchFileWithTokenRefresh(
          `https://localhost:82/api/web/repo/file/${owner}/${name}/${fileHash}`,
          {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
          },
          sessionExpired
        );

        if (!response.ok) 
          throw new Error('Failed to load file');
        
        
        
        const fileText = await response.text();
        setContent(fileText);
        
      } catch (err) {
        console.error('Error fetching file:', err);
      } finally {
        setLoading(false);
      }

    };

    loadFile();
  }, [owner, name, fileHash, sessionExpired]);


  // temp save handler
  const handleSave = async () => {
    setIsSaving(true);
    try {
      // Save
      setSaveSuccess(true);
    } catch (err) {
      setSaveError("Failed to save changes.");
    } finally {
      setIsSaving(false);
    }
  };




  if (loading) return <div>Loading...</div>;

  
  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};


  return (
    <Page header={headerSection}>
      <Card>
          <Editor
            height="70vh"
            theme={theme === 'dark' ? 'vs-dark' : 'vs-light'}
            value={content}
            onChange={(value) => setContent(value)}
            language={getEditorLanguage({ fileName, mimeType })}
            options={editorOptions}
          />

          <button onClick={handleSave} disabled={isSaving}>
            Save Changes
          </button>

          {saveSuccess && <p>Changes saved successfully!</p>}
          {saveError && <p style={{ color: "red" }}>{saveError}</p>}
        
      </Card>
    </Page>
  );

  
};

export default FileDisplay;
