import React, { useState, useMemo } from 'react';
import { formatOnlyDate } from "../../helpers/DateHelper";

import styles from "../../styles/Components/Repo/FileExplorer.module.css";

const FileExplorer = ({ root }) => {
  const [currentPath, setCurrentPath] = useState([]);

  const getCurrentDirectory = () => {
    let current = root;

    for (const folderName of currentPath) {
      const next = current.children.find(child => child.name === folderName);
      
      if (!next) {
        setCurrentPath([]);
        return root;
      }

      current = next;
    }

    return current;
  };

  const currentDir = getCurrentDirectory();
  const breadcrumbParts = [root.name, ...currentPath];


  // Sort children (folders first where hash=null, then files)
  const sortedChildren = [...currentDir.children].sort((a, b) => {
    const aIsFolder = a.hash === null;
    const bIsFolder = b.hash === null;
    
    if (aIsFolder && !bIsFolder) return -1;
    if (!aIsFolder && bIsFolder) return 1;
    return a.name.localeCompare(b.name);
  });


  const handleFolderClick = (folderName) => {
    setCurrentPath([...currentPath, folderName]);
  };

  const handleBreadcrumbClick = (index) => {
    setCurrentPath(currentPath.slice(0, index));
  };

  const handleBack = () => {
    setCurrentPath(currentPath.slice(0, -1));
  };

  const handleHome = () => {
    setCurrentPath([]);
  };

  return (
    <>
      <div className={styles.navigation}>

        {/* Back button */}
        <button
          onClick={handleBack}
          className={styles.navButton}
          disabled={currentPath.length === 0}
          title="Go back"
        >
          <img src="/Icons/back.svg" alt="Back" />
        </button>

        {/* Home button */}
        <button
          onClick={handleHome}
          className={styles.navButton}
          disabled={currentPath.length === 0}
          title="Go home"
        >
          <img src="/Icons/home.svg" alt="Home" />
        </button>
        
          
        {/* Breadcrumbs */}
        <div className={styles.breadcrumb}>
          
          {currentPath.map((part, index) => (
            <React.Fragment key={index}>
              <span className={styles.breadcrumbSeparator}>/</span>
              <span
                onClick={() => handleBreadcrumbClick(index + 1)}
                className={styles.breadcrumbPart}
              >
                {part}
              </span>
            </React.Fragment>
          ))}
        </div>


      </div>


      <table className={styles.table}>

        <thead>
          <tr className={styles.theadRow}>
            <th className={styles.th}></th>
            <th className={styles.th}>Name</th>
            <th className={styles.th}>Size</th>
            <th className={styles.th}>Date Modified</th>
          </tr>
        </thead>

        <tbody>

          {sortedChildren.map((item, index) => {

            const isFolder = item.hash === null;

            const size = isFolder ? '-' : `${item.size} KB`;

            return (
              <tr
                key={index}
                className={styles.tbodyRow}
                onClick={() => isFolder && handleFolderClick(item.name)}
                style={{ cursor: isFolder ? 'pointer' : 'default' }}
              >
                <td>
                  <img
                    src={`/Icons/${isFolder ? 'folder' : 'file'}.svg`}
                    alt={isFolder ? 'folder' : 'file'}
                  />
                </td>

                <td>{item.name}</td>
                <td>{size}</td>
                <td>{formatOnlyDate(item.lastModified)}</td>
              </tr>
            );
          })}

        </tbody>

      </table>
    </>
  );
};

export default FileExplorer;