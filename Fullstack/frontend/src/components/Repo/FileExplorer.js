import React, { useEffect, useMemo, useCallback, memo } from 'react';
import { useNavigate, useParams, useSearchParams } from 'react-router';
import { formatDate } from "../../helpers/DateHelper";

import styles from "../../styles/components/repo/FileExplorer.module.css";
import tableStyles from "../../styles/components/Table.module.css"

// Helper to make size readable
const formatFileSize = (bytes) => {
  
  if (bytes === 0) 
    return '0 B';

  const k = 1024;
  const sizes = ['B', 'KB', 'MB', 'GB', 'TB'];
  const i = Math.floor(Math.log(bytes) / Math.log(k));

  return parseFloat((bytes / Math.pow(k, i)).toFixed(2)) + ' ' + sizes[i];
};



const FileExplorer = ({ root }) => {
  const navigate = useNavigate();
  const { owner, name } = useParams();

  const [searchParams, setSearchParams] = useSearchParams();

  const currentPath = useMemo(() => {
    const path = searchParams.get('path') || "";
    return path.split('/').filter(p => p);
  }, [searchParams]);


  

  const currentDir = useMemo(() => {
    let current = root || { children: [] };

    for (const folderName of currentPath) {
      const next = current.children.find(child => child.name === folderName);
      
      if (!next)
        return root;
      
      current = next;
    }

    return current;
  }, [root, currentPath]);


  useEffect(() => {
    let current = root || { children: [] };
    
    for (const folderName of currentPath) {
      const next = current.children.find(child => child.name === folderName);
      
      if (!next) {
        setSearchParams({});
        break;
      }

      current = next;
    }

  }, [root, currentPath, setSearchParams]);







  // Sort folders then files
  const sortedChildren = useMemo(() => {
    const children = currentDir?.children || [];

    return [...children].sort((a, b) => {
      const aIsFolder = a.hash === null;
      const bIsFolder = b.hash === null;

      if (aIsFolder && !bIsFolder) 
        return -1;

      if (!aIsFolder && bIsFolder) 
        return 1;

      return a.name.localeCompare(b.name);
    });
  }, [currentDir]);




  const updatePath = useCallback((newPathArray) => {
    const newPath = newPathArray.join('/');

    if (newPath) {
      setSearchParams({ path: newPath });
    } else {
      setSearchParams({});
    }
  }, [setSearchParams]);


  const handleFolderClick = useCallback((folderName) => {
    updatePath([...currentPath, folderName]);
  }, [currentPath, updatePath]);

  const handleFileClick = useCallback((fileName, mimeType, fileSize, fileHash) => {
    navigate(`/repository/${owner}/${name}/file/${fileHash}`, {
      state: { fileName, mimeType, fileSize }
    });
  }, [owner, name, navigate]);


  const handleBreadcrumbClick = useCallback((index) => {
    updatePath(currentPath.slice(0, index + 1));
  }, [currentPath, updatePath]);

  const handleBack = useCallback(() => {
    updatePath(currentPath.slice(0, -1));
  }, [currentPath, updatePath]);

  const handleHome = useCallback(() => {
    updatePath([]);
  }, [updatePath]);


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
          {sortedChildren.length === 0 ? (
            <tr className={tableStyles.tbodyRow}>
              <td className={tableStyles.td} colSpan={4} style={{ textAlign: 'center' }}>
                <div className={tableStyles.empty}>Empty...</div>
              </td>
            </tr>
          ) : (
            sortedChildren.map((item, index) => {
              const isFolder = item.hash === null;
              const size = formatFileSize(item.size);
              const mimeType = item.mimeType;

              return (
                <tr
                  key={index}
                  className={tableStyles.tbodyRow}
                  onClick={() =>
                    isFolder
                      ? handleFolderClick(item.name)
                      : handleFileClick(item.name, item.mimeType, item.size, item.hash)
                  }
                  style={{ cursor: isFolder ? 'pointer' : 'default' }}
                  data-mime-type={mimeType}
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
            })
          )}
        </tbody>

      </table>
    </>
  );
};

export default memo(FileExplorer);