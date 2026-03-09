import React from "react";
import styles from "../pages/BirthdaysPage.module.css";

export function Toolbar({
    onToday,
    onNextDay,
    onPrevDay,
    onRun,
    onCancel,
    isRunning,
    loading
}) {
    const isBusy = isRunning || loading;

    return (
        <div className={styles.toolbar}>
            <button
                className={styles.toolbarButton}
                onClick={onToday}
                disabled={isBusy}
            >
                Today
            </button>

            <div className={styles.toolbarGroup}>
                <button
                    className={styles.toolbarButton}
                    onClick={onNextDay}
                    disabled={isBusy}
                    title="Next Day"
                >
                    +
                </button>

                <button
                    className={styles.toolbarButton}
                    onClick={onPrevDay}
                    disabled={isBusy}
                    title="Previous Day"
                >
                    -
                </button>

                {isRunning ? (
                    <button
                        className={`${styles.toolbarButton} ${styles.cancelButton}`}
                        onClick={onCancel}
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