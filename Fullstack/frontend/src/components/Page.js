import React from "react";

import styles from "../styles/components/Page.module.css";
import headerStyles from "../styles/components/repo/RepoPageHeader.module.css";

const Page = ({ header, children }) => {

    return (
        <div className={styles.page}>
            {header && <div>{header(headerStyles)}</div>}

            <div className={styles.content}>
                { children }
            </div>
        </div>
    );
}

export default Page;