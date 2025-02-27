import React from "react";

import styles from '../../styles/components/cards/Card.module.css';

const Card = ({ children, cardStyling }) => {

    return (
        <div className={`${cardStyling} ${styles.card}`}>
            { children }
        </div>
    );
}

export default Card;