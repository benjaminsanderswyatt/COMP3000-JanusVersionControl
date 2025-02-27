import React, { useState } from 'react';
import { useNavigate } from "react-router";

import Page from "../components/Page";
import SearchBoxEnter from "../components/search/SearchBoxEnter";
import Repository from '../components/repo/Repository';

import styles from "../styles/pages/Discover.module.css";

const Discover = () => {
    const navigate = useNavigate();
    const [searchQuery, setSearchQuery] = useState(''); // Initialize search query
    const [searchResults, setSearchResults] = useState([]); // Store search results
    const [isLoading, setIsLoading] = useState(false); // Track loading state
    const [error, setError] = useState(null); // Track errors


    // Mock search function
    const search = async (query) => {
        // Simulate an API call
        await new Promise((resolve) => setTimeout(resolve, 1000));

        return [
        {
            id: 1,
            name: "Project_1",
            description: "Project description 1",
            lastUpdated: "2025-02-19T15:45:00Z",
            owner: { 
                id: 3, 
                userName: "User3" 
            },
            avatars: [
                { id: 1, userName: "User1" },
                { id: 2, userName: "User2" },
                { id: 3, userName: "User3" },
                { id: 4, userName: "User4" },
                { id: 5, userName: "User5" },
                { id: 6, userName: "User6" },
                { id: 7, userName: "User7" },
                { id: 8, userName: "User8" },
                { id: 9, userName: "User9" },
                { id: 10, userName: "User10" },
                { id: 11, userName: "User11" },
            ],
        },
        ];
    };



    // Handle search input change
    const handleSearch = async (query) => {
        if (!query) {
            setSearchResults([]);
            return;
        }

        setSearchQuery(query);
        setIsLoading(true);
        setError(null);

        try {
            const data = await search(query);
            setSearchResults(data);
        } catch (err) {
            setError(err.message);
        } finally {
            setIsLoading(false);
        }
    };




    const handleEnterRepo = (owner, name) => {
        navigate(`/repository/${owner}/${name}/main`);
    }

    const handleEnterRepoContrib = (owner, name) => {
        navigate(`/repository/${owner}/${name}/contributors`);
    }




    const headerSection = (pageStyles) => { return(
        <header className={pageStyles.header}>
            <SearchBoxEnter searchingWhat={"public repositories"} onSearch={handleSearch}/>
        </header>
    )};

    return (
        <Page header={headerSection}>
            <h1>Discover</h1>
            <p>Query: {searchQuery || "No query provided"}</p>
            
            {/* Display loading state */}
            {isLoading && <p>Loading...</p>}

            {/* Display error message */}
            {error && <p style={{ color: 'red' }}>Error: {error}</p>}


            {/* Display repositories */}
            {searchResults.length === 0 ? (
                <p className={styles.noRepositories}>No repositories...</p>
            ) : (

                searchResults.map((result) => (
                <Repository
                    key={result.id}
                    enterRepo={() => handleEnterRepo(result.owner, result.name)}
                    enterRepoContrib={() => handleEnterRepoContrib(result.owner, result.name)}
                    owner={result.owner}
                    repoName={result.name}
                    description={result.description}
                    visability={true}
                    lastUpdated={result.lastUpdated}
                    avatars={result.avatars}
                />
            )))}

        </Page>
    );
};


export default Discover;