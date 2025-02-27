import React from "react";

import styles from "../styles/components/Page.module.css";

const Page = ({ header, children }) => {

    return (
        <div className={styles.page}>
            {header && <div>{header(styles)}</div>}

            <div className={styles.content}>
                { children }
            </div>
        </div>
    );
}

export default Page;