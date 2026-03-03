import React from "react";
import styles from "../pages/BirthdaysPage.module.css";

export function ResultItem({
    person,
    includeAll,
    summaries,
    setSummaries,
    summarizePerson,
    hasOpenAIKey
}) {
    const isMilestone = person.age === 90 || person.age === 100;
    const hasSummary = summaries[person.name];

    const toggleSummary = () => {
        if (hasSummary) {
            const copy = { ...summaries };
            delete copy[person.name];
            setSummaries(copy);
        } else {
            summarizePerson(person);
        }
    };

    return (
        <li className={`${styles.item} ${isMilestone ? styles.milestone : ""}`}>
            <div className={styles.inlineRow}>
                <span className={styles.ageBadge}>{person.age}</span>

                <a
                    href={person.url}
                    target="_blank"
                    rel="noopener noreferrer"
                    className={styles.nameLink}
                >
                    {person.name} — {person.description} 
                </a>

                {includeAll && (
                    <span className={styles.dateBadge}>
                       {person.month}/{person.day}
                    </span>
                )}
            </div>

            <div className={styles.summaryRow}>
                <button
                    className={styles.summaryChip}
                    disabled={!hasOpenAIKey}
                    title={!hasOpenAIKey ? "Add an OpenAI API key to enable summaries" : ""}
                    onClick={toggleSummary}
                >
                    {hasSummary ? "Clear" : "Summarize"}
                </button>

                {hasSummary && (
                    <div className={styles.summaryBox}>
                        {summaries[person.name]}
                    </div>
                )}
            </div>
        </li>
    );
}