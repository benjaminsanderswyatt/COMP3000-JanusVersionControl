import React from 'react';

import ProfilePic from "../images/ProfilePic";
import { formatRelativeTime } from '../../helpers/DateHelper';

import styles from "../../styles/Components/Repo/Commit.module.css";
import tableStyles from "../../styles/Components/Table.module.css";


const Commit = ({ commit, navToContrib }) => {

  if (!commit) return null;


  return (
    <div className={styles.container}>
      
      <table className={tableStyles.table}>

        <tbody>
          <tr>

            <td className={styles.avatar}>
              <div className={styles.avatar}>
                <ProfilePic
                  userId={commit.userId}
                  label={commit.userName}
                  alt={`${commit.userName}'s profile}`}
                  innerClassName={styles.profilePic}
                  handleClick={() => navToContrib}
                />
              </div>
            </td>
            
            <td className={tableStyles.td}>{commit.message}</td>


            <td className={tableStyles.td}>{commit.hash}</td>
            <td className={tableStyles.td}>{formatRelativeTime(commit.date)}</td>
          </tr>
            
        </tbody>

      </table>
    </div>
  );
};


export default Commit;