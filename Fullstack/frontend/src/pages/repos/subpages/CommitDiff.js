import React, { useState, useEffect } from 'react';
import { useParams, useNavigate } from 'react-router';

import { useAuth } from '../../../contexts/AuthContext';
import { fetchWithTokenRefresh, fetchFileWithTokenRefresh } from '../../../api/fetchWithTokenRefresh';

import Page from '../../../components/Page';
import Card from '../../../components/cards/Card';
import RepoPageHeader from '../../../components/repo/RepoPageHeader';
import LoadingSpinner from '../../../components/LoadingSpinner';

import styles from "../../../styles/pages/repos/subpages/CommitDiff/CommitDiff.module.css"

const CommitDiff = () => {
  const { owner, name, commitHash } = useParams();
  const [diffs, setDiffs] = useState([]);
  const [loading, setLoading] = useState(true);


  return (
    <Page>

    </Page>
  );
};


export default CommitDiff;