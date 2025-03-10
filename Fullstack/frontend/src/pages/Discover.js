import React, { useState, useEffect } from 'react';
import { useNavigate, useSearchParams } from "react-router";

import Page from "../components/Page";
import Card from "../components/cards/Card";
import LoadingSpinner from '../components/LoadingSpinner';
//import SearchBoxEnter from "../components/search/SearchBoxEnter";
import Repository from '../components/repo/Repository';

import styles from "../styles/pages/Discover.module.css";

const Discover = () => {
    const navigate = useNavigate();
    const [searchParams, setSearchParams] = useSearchParams();
    const initialPage = parseInt(searchParams.get('page')) || 1;

    const [repositories, setRepositories] = useState([]);
    const [page, setPage] = useState(initialPage);
    const [totalPages, setTotalPages] = useState(1);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState(null);


    
    const fetchRepos = async (pageNum) => {
        setLoading(true);
        setError(null);

        try {
            const response = await fetch(
                `https://localhost:82/api/web/discover/repositories?page=${pageNum}`,
                {
                    method: "GET",
                }
            );

            if (!response.ok){
                throw new Error('Error getting repositories');
            }
            
            const data = await response.json();

            setRepositories(data.repositories);
            setPage(data.page);
            setTotalPages(data.totalPages);

        }
         catch (err) {
            setError(err.message);
        } finally {
            setLoading(false);
            window.scrollTo(0, 0); // Scroll to top of page when new page
        }

    };

    useEffect(() => {
        const urlPage = parseInt(searchParams.get('page')) || 1;
        setPage(urlPage);
    }, [searchParams]);

    useEffect(() => {
        fetchRepos(page);
    }, [page]);



    // Handler for pagination buttons
    const handlePageChange = (newPage) => {
        if (newPage >= 1 && newPage <= totalPages) {
            setSearchParams({ page: newPage });
        }
    };




    const handleEnterRepo = (owner, name) => {
        navigate(`/repository/${owner}/${name}/main`);
    }

    const handleEnterRepoContrib = (owner, name) => {
        navigate(`/repository/${owner}/${name}/contributors`);
    }




    const headerSection = (pageStyles) => { return(
        <header className={pageStyles.header}></header>
    )};

    return (
        <Page header={headerSection}>
            

            {loading ? (
                <Card>
                    <LoadingSpinner/>
                </Card>

            ) : error ? (
                <Card>
                    <div>Error: {error}</div>
                </Card>

            ) : (

                <>
                    {/* Display repositories */}
                    {repositories.length === 0 ? (
                        <Card>
                            <p className={styles.noRepositories}>No repositories...</p>
                        </Card>
                    ) : (
                    
                        repositories.map((repo) => (
                            <Repository
                                key={repo.id}
                                enterRepo={() => handleEnterRepo(repo.owner.username, repo.name)}
                                enterRepoContrib={() => handleEnterRepoContrib(repo.owner.username, repo.name)}
                                repoName={repo.name}
                                description={repo.description || ''}
                                visability={repo.isPrivate}
                                lastUpdated={repo.lastUpdated}
                                owner={repo.owner}
                            />
                        ))
            
                    )}
                </>
            )}

            <div className={styles.pagination}>
                <button 
                    onClick={() => handlePageChange(page - 1)} 
                    disabled={page === 1} 
                    className={styles.pageButton}
                >
                    {"<"}
                </button>

                <span className={styles.pageNum}>
                    {page} / {totalPages}
                </span>

                <button 
                    onClick={() => handlePageChange(page + 1)} 
                    disabled={page === totalPages}
                    className={styles.pageButton}
                >
                    {">"}
                </button>
            </div>

        </Page>
    );
};


export default Discover;