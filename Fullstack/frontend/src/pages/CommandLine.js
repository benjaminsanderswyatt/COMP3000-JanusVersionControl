import React, { useState, useEffect, useMemo } from 'react';
import { useSearchParams } from 'react-router';
import ReactMarkdown from "react-markdown";
import rehypeSlug from 'rehype-slug';

import Page from "../components/Page";

import "../styles/Markdown.css";
import styles from "../styles/components/repo/RepoPageHeader.module.css";


const CommandLine = () => {
    const [searchParams, setSearchParams] = useSearchParams();
    const doc = searchParams.get("doc") || "cli";

    const [markdown, setMarkdown] = useState("");

    const filePath = useMemo(() => {
        return doc === "cli"
          ? "/doc/CLI_DOCUMENTATION.md"
          : "/doc/PLUGIN_DEVELOPMENT_GUIDE.md";
    }, [doc]);
    

    useEffect(() => {
        const controller = new AbortController();
        const { signal } = controller;
    
        const loadMarkdown = async () => {
          try {
            const res = await fetch(filePath, { signal }); // Fetch markdown file

            if (!res.ok) 
                throw new Error(`HTTP error! Status: ${res.status}`);

            const text = await res.text();

            setMarkdown(text);

          } catch (err) {

            if (err.name === 'AbortError') {
              console.log('Fetch aborted');

            } else {

              console.error("Error loading Markdown:", err);
              setMarkdown(`# Error\nCould not load the ${doc} documentation.`);
            }

          }
        };
    
        loadMarkdown();
    
        // Abort fetch if component unmounts
        return () => controller.abort();
    }, [filePath, doc]);


    const updateDocType = (type) => {
        setSearchParams({ doc: type });
    };

    

    const headerSection = (pageStyles) => (
        <header className={pageStyles.header}>
            
            <button
                className={`${styles.button} ${doc === "cli" ? styles.selected : ""}`}
                onClick={() => updateDocType("cli")}
            >
                CLI
            </button>
            <button
                className={`${styles.button} ${doc === "plugin" ? styles.selected : ""}`}
                onClick={() => updateDocType("plugin")}
            >
                Plugins
            </button>

        </header>
    );

    return (
        <Page header={headerSection}>
            <div className="markdown">

                <ReactMarkdown rehypePlugins={[rehypeSlug]}>
                    {markdown}
                </ReactMarkdown>
                
            </div>
        </Page>
    );
};


export default CommandLine;