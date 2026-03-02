import React from "react";
import styles from "../pages/BirthdaysPage.module.css";

export function Pagination({ page, totalPages, setPage }) {
    if (totalPages <= 1) return null;

    return (
        <div className={styles.pagination}>
            <button disabled={page === 1} onClick={() => setPage(page - 1)}>
                Prev
            </button>

            <span>{page} / {totalPages}</span>

            <button disabled={page === totalPages} onClick={() => setPage(page + 1)}>
                Next
            </button>
        </div>
    );
}
