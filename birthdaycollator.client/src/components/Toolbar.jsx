import React from "react";
import styles from "../pages/BirthdaysPage.module.css";

export function Toolbar({
    onToday,
    onNextDay,
    onPrevDay,
    onRun,
    onCancel,
    isRunning,
    loading,
    hasRunController
}) {
    return (
        <div className={styles.toolbar}>
            <button className={styles.toolbarButton} onClick={onToday}>
                Today
            </button>

            <div className={styles.toolbarGroup}>
                <button
                    className={styles.toolbarButton}
                    onClick={onNextDay}
                >
                    +
                </button>

                <button
                    className={styles.toolbarButton}
                    onClick={onPrevDay}
                >
                    -
                </button>

                {isRunning ? (
                    <button
                        className={`${styles.toolbarButton} ${styles.cancelButton}`}
                        onClick={onCancel}
                        disabled={!hasRunController}
                    >
                        Cancel
                    </button>
                ) : (
                    <button
                        className={`${styles.toolbarButton} ${styles.runButton}`}
                        onClick={onRun}
                        disabled={loading}
                    >
                        {loading ? "Loading…" : "Run"}
                    </button>
                )}
            </div>
        </div>
    );
}
