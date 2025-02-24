import React, { useEffect, useState } from "react";
import { Outlet, useParams } from "react-router";


const mockRepoData = {
    id: 1,
    owner: { 
        id: 2, 
        userName: "User2" 
    },
    description: "Repository description",
    visibility: false,
    branches: [ "main", "first", "second"],
};


const RepoLayout = () => {
    const { owner, name } = useParams();
    const [repoData, setRepoData] = useState(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState(null);

    useEffect(() => {

        setTimeout(() => {
            
          setRepoData(mockRepoData);
          setLoading(false);
        }, 1000);

    }, [owner, name]);



    if (error) {
        return <div>Error: {error}</div>;
    }


    return <Outlet context={repoData} />;
}

export default RepoLayout;