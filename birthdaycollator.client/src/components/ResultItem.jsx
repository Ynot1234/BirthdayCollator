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

    const summaryKey = `${person.name}-${person.birthYear}`;
    const currentSummary = summaries[summaryKey];

    const toggleSummary = () => {
        if (currentSummary) {
            const { [summaryKey]: removed, ...rest } = summaries;
            setSummaries(rest);
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
                    title={!hasOpenAIKey ? "Add an OpenAI API key in Settings to enable summaries" : ""}
                    onClick={toggleSummary}
                >
                    {currentSummary ? "Clear Summary" : "AI Summarize"}
                </button>

                {currentSummary && (
                    <div className={styles.summaryBox}>
                        {currentSummary}
                    </div>
                )}
            </div>
        </li>
    );
}