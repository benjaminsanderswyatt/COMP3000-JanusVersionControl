import React, { useEffect, useState } from "react";
import { Outlet, useNavigate, useParams } from "react-router";

import { useAuth } from "../../../contexts/AuthContext";
import { fetchWithTokenRefresh } from "../../../api/fetchWithTokenRefresh";
import Page from "../../../components/Page";
import Card from "../../../components/cards/Card";


const RepoLayout = () => {
    const navigate = useNavigate();
    const { sessionExpired, authUser } = useAuth();
    const { owner, name } = useParams();
    const [repoData, setRepoData] = useState(null);
    const [error, setError] = useState(null);

    useEffect(() => {

        const fetchRepoData = async () => {
            try {
              const data = await fetchWithTokenRefresh(
                `https://localhost:82/api/web/repo/${owner}/${name}`,
                {
                  method: "GET",
                  headers: { "Content-Type": "application/json" },
                },
                sessionExpired
              );

              console.log(data);

              setRepoData(data);
            } catch (err) {
              setError(err.message);
            }
          };
      
          fetchRepoData();

    }, [owner, name, sessionExpired]);


    


    if (error) {
      const headerSection = (pageStyles) => { return(
        <header className={pageStyles.header}>
        </header>
      )};


      return (
          <Page header={headerSection}>
              <Card>
                <p style={{paddingBottom: "20px"}}>Error: {error}</p>
                <button 
                  className="button"
                  onClick={() => navigate(`/repository/${authUser}`)}
                  >
                    Go Back
                </button>
              </Card>
          </Page>
      )
        
    }


    return <Outlet context={repoData} />;
}

export default RepoLayout;