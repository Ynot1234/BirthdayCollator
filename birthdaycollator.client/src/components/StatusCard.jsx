import styles from "../pages/BirthdaysPage.module.css";

export function StatusCard({ message, title = "Birthdays" }) {
    return (
        <div className={styles.page}>
            <h1 className={styles.pageTitle}>{title}</h1>
            <div className={styles.card}>
                <h1 className={styles.headerTitle}>{title}</h1>
                <div className={styles.offlineBox}>
                    {message}
                </div>
            </div>
        </div>
    );
}