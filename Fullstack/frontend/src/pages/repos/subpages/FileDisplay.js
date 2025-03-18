import React, { useState, useEffect } from "react";
import { useLocation, useParams } from 'react-router';
import Editor from '@monaco-editor/react';
import styled from "styled-components";
import { useTheme } from "../../../contexts/ThemeContext";

import { useAuth } from "../../../contexts/AuthContext";
import Page from "../../../components/Page";
import Card from "../../../components/cards/Card";
import RepoPageHeader from "../../../components/repo/RepoPageHeader";

import { fetchFileWithTokenRefresh } from "../../../api/fetchWithTokenRefresh";

import styles from "../../../styles/pages/repos/subpages/FileDisplay.module.css";


const EditorContainer = styled.div`
  display: block;
  width: 100%;
  height: 65vh;
  background: var(--tintcard);
  border: var(--border) thin solid;
  border-radius: 8px;
  box-shadow: 0 4px 6px rgba(0, 0, 0, 0.1);
  padding: 12px 4px 16px 0px;
`;

const handleEditorWillMount = (monaco) => {
  monaco.editor.defineTheme("CustomLight", {
    base: "vs",
    inherit: true,
    rules: [],
    colors: {
      "editor.background": "#FCFCFD",
    },
  });

  monaco.editor.defineTheme("CustomDark", {
    base: "vs-dark",
    inherit: true,
    rules: [],
    colors: {
      "editor.background": "#252525",
    },
  });
};







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
    glyphMargin: false,
    lineNumbersMinChars: 3,
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

      <EditorContainer>
        <Editor
          height="100%"
          theme={theme === "dark" ? "CustomDark" : "CustomLight"}
          value={content}
          onChange={(value) => setContent(value)}
          language={getEditorLanguage({ fileName, mimeType })}
          options={editorOptions}
          beforeMount={handleEditorWillMount}
        />
      </EditorContainer>


      {/*
      <Card>
        <button onClick={handleSave} disabled={isSaving}>
          Save Changes
        </button>

        {saveSuccess && <p>Changes saved successfully!</p>}
        {saveError && <p style={{ color: "red" }}>{saveError}</p>}
        
      </Card>
      */}

    </Page>
  );

  
};

export default FileDisplay;
