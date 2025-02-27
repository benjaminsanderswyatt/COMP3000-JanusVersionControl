import React from 'react';

import ProfilePic from "../images/ProfilePic";
import { DateType, formatRelativeTime, formatDate, formatOnlyDate, formatOnlyTime } from '../../helpers/DateHelper';

import styles from "../../styles/components/repo/Commit.module.css";
import tableStyles from "../../styles/components/Table.module.css";


const Commit = ({ commit, navToContrib, hasRows=false, dateType=DateType.RELATIVE }) => {

  if (!commit) return null;


  // How the date should be formatted
  const formatDateBasedOnType = (dateString) => {
    switch (dateType) {
      case DateType.DATE_ONLY:
        return formatOnlyDate(dateString);
      case DateType.TIME_ONLY:
        return formatOnlyTime(dateString);
      case DateType.FULL:
        return formatDate(dateString);
      case DateType.RELATIVE:
      default:
        return formatRelativeTime(dateString);
    }
  };


  return (
    <div className={styles.container}>
      
      <table className={tableStyles.table}>

        <tbody>
          <tr style={{minWidth: "43px"}} className={`${hasRows ? tableStyles.tbodyRow : ""}`}>

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
            <td className={tableStyles.td}>{formatDateBasedOnType(commit.date)}</td>
          </tr>
            
        </tbody>

      </table>
    </div>
  );
};


export default Commit;