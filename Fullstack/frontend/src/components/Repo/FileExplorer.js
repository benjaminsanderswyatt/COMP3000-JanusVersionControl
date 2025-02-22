import React from 'react';
import { formatOnlyDate } from "../../helpers/DateHelper";

import styles from "../../styles/Components/Repo/FileExplorer.module.css";

const FileExplorer = ({ files }) => {
  return (
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
        {files.map((file, index) => (
          <tr key={index} className={styles.tbodyRow}>
            <td>
              <img
                src={`/Icons/${file.type}.svg`}
                alt={file.type}
              />
            </td>

            <td className={`${styles.td}`}>{file.name}</td>

            <td className={styles.td}>{file.size}</td>
            <td className={styles.td}>{formatOnlyDate(file.date)}</td>
          </tr>
        ))}
      </tbody>

    </table>
  );
};


export default FileExplorer;