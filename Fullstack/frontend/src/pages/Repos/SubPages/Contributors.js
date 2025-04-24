import React, { useState, useEffect, useCallback } from 'react';
import { useParams, useOutletContext, useNavigate } from 'react-router';

import RepoPageHeader from '../../../components/repo/RepoPageHeader';
import Page from '../../../components/Page';
import Card from '../../../components/cards/Card';
import LoadingSpinner from '../../../components/LoadingSpinner';
import ProfilePic from "../../../components/images/ProfilePic";

import { AccessLevel, accessLevelMapping } from '../../../helpers/AccessLevel';
import { useAuth } from '../../../contexts/AuthContext';
import { fetchWithTokenRefresh } from '../../../api/fetchWithTokenRefresh';

import styles from "../../../styles/pages/repos/subpages/RepoPage.module.css";
import stylesContrib from "../../../styles/pages/repos/subpages/Contributors.module.css";
import tableStyles from "../../../styles/components/Table.module.css";
import TextInput from '../../../components/inputs/TextInput';
import Dropdown from '../../../components/inputs/Dropdown';


const Contributors = () => {
  const navigate = useNavigate();
  const { authUser } = useAuth();
  const { owner, name } = useParams();
  const { sessionExpired } = useAuth()

  const [contributors, setContributors] = useState([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);


  
  const fetchContributors = useCallback(async () => {
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
  }, [sessionExpired, owner, name]);

  useEffect(() => {
    fetchContributors();
  }, [fetchContributors]);




  const [inviteUsername, setInviteUsername] = useState('');
  const [accessLevel, setAccessLevel] = useState(AccessLevel.READ);
  const [message, setMessage] = useState('');
  const [messageType, setMessageType] = useState('');


  // Determine permissions for current user
  const currentContributor = contributors.find(c => c.username === authUser);
  const isOwner = owner === authUser;
  const isAdmin = currentContributor?.accessLevel?.toUpperCase() === AccessLevel.ADMIN;
  const isCollaborator = !!currentContributor;
  const canInvite = isOwner || isAdmin;
  





  const handleSendInvite = async () => {
    try {
      const accessLevelKey = accessLevel.toUpperCase();
      const numericAccessLevel = accessLevelMapping[accessLevelKey];

      await fetchWithTokenRefresh(
        `https://localhost:82/api/web/contributors/${owner}/${name}/invite`,
        {
          method: 'POST',
          headers: { 'Content-Type': 'application/json' },
          body: JSON.stringify({
            inviteeUsername: inviteUsername,
            accessLevel: numericAccessLevel
          }),
        },
        sessionExpired
      );
      
      setInviteUsername('');
      setMessage('Invite sent');
      setMessageType('success');

      fetchContributors();

    } catch (err) {

      setMessage(err.message || 'Failed to send invite');
      setMessageType('error');
    }
  };



  const handleLeaveRepo = async () => {
    if (!window.confirm("Are you sure you want to leave this repository?")) return;

    try {
        await fetchWithTokenRefresh(
            `https://localhost:82/api/web/contributors/${owner}/${name}/leave`,
            {
                method: 'DELETE',
                headers: { 'Content-Type': 'application/json' },
            },
            sessionExpired
        );

        // Redirect after leaving
        navigate('/collaborating');
    } catch (err) {
        setMessage(err.message || 'Failed to leave repository');
        setMessageType('error');
    }
  };













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



  const groupContributors = (accessLevelParam) => {
    return contributors.filter(contributor =>
      contributor.accessLevel.toLowerCase() === accessLevelParam.toLowerCase()
    );
  };

  const groups = [
    { label: "Owner", emptyMessage: "Error: Failed to find owner" },
    { label: "Admin", emptyMessage: "No contributors with Admin permissions..." },
    { label: "Write", emptyMessage: "No contributors with Write permissions..." },
    { label: "Read", emptyMessage: "No contributors with Read permissions..." },
  ];

  const dropdownData = ["Read", "Write", "Admin"];

  
  
  return (
    <Page header={headerSection}>

      <Card>
        <div className={`${styles.header} ${styles.spaced}`}>
          <h1>{name}</h1>
        </div>
      </Card>

      {/* Invite button only for owner or admin */}
      {canInvite && (
        <Card>
          <h3 className={stylesContrib.header}>Invite Collaborator</h3>

          <Dropdown
            label="Access"
            dataArray={dropdownData} // Read, Write, Admin
            onSelect={(selectedValue) => setAccessLevel(selectedValue)}
            selectedValue={accessLevel}
          />

          <TextInput 
            name="username" 
            value={inviteUsername} 
            onChange={(e) => setInviteUsername(e.target.value)} 
            placeholder="Username..." 
            hasError={messageType === "error"}
          />

          <div className={stylesContrib.inviteHolder}>
            <button className="button" onClick={handleSendInvite}>
              Send Invite
            </button>

            {message && (
              <p className={messageType}>{message}</p>
            )}
          </div>
        </Card>
      )}



      

      
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

      {/* Show Leave button when authUser is a collaborator but not owner */}
      {isCollaborator && !isOwner && (
        <Card>
          <h3 className={stylesContrib.header}>Stop Colaborating</h3>
          <button 
            className="button"
            onClick={handleLeaveRepo}
          >
            Leave
          </button>
        </Card>
      )}



    </Page>
  );
};


export default Contributors;
