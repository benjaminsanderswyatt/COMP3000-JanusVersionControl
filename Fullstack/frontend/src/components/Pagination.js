import React from 'react';
import styles from '../styles/components/Pagination.module.css';

const Pagination = ({ currentPage, totalPages, onPageChange }) => {
    return (
        <div className={styles.pagination}>
            <button 
                onClick={() => onPageChange(currentPage - 1)} 
                disabled={currentPage === 1} 
                className={styles.pageButton}
            >
                {"<"}
            </button>
            <span className={styles.pageNum}>
                {currentPage} / {totalPages}
            </span>
            <button 
                onClick={() => onPageChange(currentPage + 1)} 
                disabled={currentPage === totalPages}
                className={styles.pageButton}
            >
                {">"}
            </button>
        </div>
    );
};

export default Pagination;