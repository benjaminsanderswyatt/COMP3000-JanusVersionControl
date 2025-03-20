import React, { useState, useEffect } from 'react';
import { useParams, useOutletContext } from 'react-router';

import RepoPageHeader from '../../../components/repo/RepoPageHeader';
import Page from '../../../components/Page';
import Card from '../../../components/cards/Card';
import LoadingSpinner from '../../../components/LoadingSpinner';
import ProfilePic from "../../../components/images/ProfilePic";

import { useAuth } from '../../../contexts/AuthContext';
import { fetchWithTokenRefresh } from '../../../api/fetchWithTokenRefresh';

import styles from "../../../styles/pages/repos/subpages/RepoPage.module.css";
import stylesContrib from "../../../styles/pages/repos/subpages/Contributors.module.css";
import tableStyles from "../../../styles/components/Table.module.css";


const Contributors = () => {
  const { owner, name } = useParams();
  const { sessionExpired } = useAuth()

  const [contributors, setContributors] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);



  useEffect(() => {
    const fetchContributors = async () => {
      setLoading(true);
      setError(null);
      try {
        const url = `https://localhost:82/api/web/contributors/${owner}/${name}/contributors`;
        const data = await fetchWithTokenRefresh(
          url,
          {
            method: 'GET',
            headers: { 'Content-Type': 'application/json' },
          },
          sessionExpired
        );

        setContributors(data.contributors);
      } catch (err) {
        setError(err.message);
      } finally {
        setLoading(false);
      }
    };

    fetchContributors();
  }, [sessionExpired, owner, name]);








  const headerSection = (pageStyles) => { return(
    <header className={pageStyles.header}>
        <RepoPageHeader/>
    </header>
  )};

  const { repoData } = useOutletContext();
  if (!repoData) {
    return (
      <Page header={headerSection}>
        <Card>
          <LoadingSpinner/>
        </Card>
      </Page>
    );
  }



  const groupContributors = (accessLevel) => {
    return contributors.filter(contributor =>
      contributor.accessLevel.toLowerCase() === accessLevel.toLowerCase()
    );
  };


  const groups = [
    { label: "Owner", emptyMessage: "Error: Failed to find owner" },
    { label: "Admin", emptyMessage: "No contributors with Admin permissions..." },
    { label: "Write", emptyMessage: "No contributors with Write permissions..." },
    { label: "Read", emptyMessage: "No contributors with Read permissions..." },
  ];

  
  
  return (
    <Page header={headerSection}>

      <Card>
        <div className={`${styles.header} ${styles.spaced}`}>
          <h1>{name}</h1>
        </div>
      </Card>


      <Card>
        <button className="button">New colaborator</button>
      </Card>

      
      {loading ? (
        <Card>
          <LoadingSpinner/>
        </Card>

      ) : error ? (
        <Card>
          <div>Error: {error}</div>
        </Card>

      ) : (
        groups.map((group) => (
          <Card key={group.label}>
            <h3 className={stylesContrib.groupTitle}>{group.label}</h3>

            {groupContributors(group.label).length ? (
              <table className={tableStyles.table}>
                
              <tbody>
                {groupContributors(group.label).map((contributor) => (
                  <tr key={contributor.userId} className={tableStyles.tbodyRow}>
                    <td className={stylesContrib.contributorItem} style={{ minWidth: "43px" }}>
                      <ProfilePic
                        userId={contributor.userId}
                        label={contributor.username}
                        alt={contributor.username}
                        innerClassName={stylesContrib.contributorPic}
                      />
                    </td>
                    <td className={tableStyles.td}>{contributor.username}</td>
                    <td className={tableStyles.td}>{contributor.email}</td>
                  </tr>
                ))}
              </tbody>
            </table>
              
            ) : (
              <p className={stylesContrib.empty}>{group.emptyMessage}</p>
            )}
          </Card>
        ))
      )}


    </Page>
  );
};


export default Contributors;
