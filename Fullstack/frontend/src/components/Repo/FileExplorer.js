import React, { useState, useMemo, useCallback, memo } from 'react';
import { formatDate } from "../../helpers/DateHelper";

import styles from "../../styles/components/repo/FileExplorer.module.css";
import tableStyles from "../../styles/components/Table.module.css"

const FileExplorer = ({ root }) => {
  const [currentPath, setCurrentPath] = useState([]);

  const currentDir = useMemo(() => {
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
  }, [root, currentPath]);


  const sortedChildren = useMemo(() => {
    return [...currentDir.children].sort((a, b) => {
      const aIsFolder = a.hash === null;
      const bIsFolder = b.hash === null;
      
      if (aIsFolder && !bIsFolder) return -1;
      if (!aIsFolder && bIsFolder) return 1;
      return a.name.localeCompare(b.name);
    });
  }, [currentDir.children]);



  const handleFolderClick = useCallback((folderName) => {
    setCurrentPath(prev => [...prev, folderName]);
  }, []);

  const handleBreadcrumbClick = useCallback((index) => {
    setCurrentPath(prev => prev.slice(0, index + 1));
  }, []);

  const handleBack = useCallback(() => {
    setCurrentPath(prev => prev.slice(0, -1));
  }, []);

  const handleHome = useCallback(() => {
    setCurrentPath([]);
  }, []);


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
          <img src="/icons/back.svg" alt="Back" />
        </button>

        {/* Home button */}
        <button
          onClick={handleHome}
          className={styles.navButton}
          disabled={currentPath.length === 0}
          title="Go home"
        >
          <img src="/icons/home.svg" alt="Home" />
        </button>
        

        {/* Breadcrumbs */}
        <div className={styles.breadcrumb}>

          {currentPath.map((part, index) => (
            <React.Fragment key={index}>
              {index > 0 && <span className={styles.breadcrumbSeparator}>/</span>}
              
              <span
                onClick={() => handleBreadcrumbClick(index)}
                className={styles.breadcrumbPart}
                style={{ cursor: index < currentPath.length - 1 ? 'pointer' : 'default' }}
              >
                {part}
              </span>

            </React.Fragment>
          ))}
        </div>
      </div>

      <table className={tableStyles.table}>

        <thead>
          <tr className={tableStyles.theadRow}>
            <th className={tableStyles.th}></th>
            <th className={tableStyles.th}>Name</th>
            <th className={tableStyles.th}>Size</th>
            <th className={tableStyles.th}>Date Modified</th>
          </tr>
        </thead>

        <tbody>
          {sortedChildren.map((item, index) => {
            const isFolder = item.hash === null;
            const size = isFolder ? '-' : `${item.size} KB`;

            return (
              <tr
                key={index}
                className={tableStyles.tbodyRow}
                onClick={() => isFolder && handleFolderClick(item.name)}
                style={{ cursor: isFolder ? 'pointer' : 'default' }}
              >

                <td className={tableStyles.firstTd}>
                  <img
                    className={tableStyles.fileFolderImg}
                    src={`/icons/${isFolder ? 'folder' : 'file'}.svg`}
                    alt={isFolder ? 'folder' : 'file'}
                  />
                </td>
                
                <td className={tableStyles.td}>{item.name}</td>
                <td className={tableStyles.td}>{size}</td>
                <td className={tableStyles.td}>{formatDate(item.lastModified)}</td>
              </tr>
            );
          })}

        </tbody>

      </table>
    </>
  );
};

export default memo(FileExplorer);