import React from "react";
import styles from "../pages/BirthdaysPage.module.css";
import SummaryControls from "./SummaryControls";

export function ResultsList({
    results,
    currentPageItems,
    isStale,
    includeAll,
    summaries,
    setSummaries,
    summarizePerson,
    hasOpenAIKey
}) {
    if (results.length === 0) return null;

    return (
        <div className={styles.results}>
            <div className={styles.resultsHeader}>
                {results.length} {results.length === 1 ? "result" : "results"}
                {isStale && (
                    <span className={styles.staleNotice}> (stale — press Run)</span>
                )}
            </div>

            <ul className={styles.list}>
                {currentPageItems.map((p, i) => (
                    <li
                        key={i}
                        className={`${styles.item} ${p.age === 90 || p.age === 100 ? styles.milestone : ""
                            }`}
                    >
                        <div className={styles.inlineRow}>
                            <span className={styles.ageBadge}>{p.age}</span>

                            <a
                                href={p.url}
                                target="_blank"
                                rel="noopener noreferrer"
                                className={styles.nameLink}
                            >
                                {p.name} - {p.description}
                            </a>

                            {includeAll && (
                                <span className={styles.dateBadge}>
                                    {p.month}/{p.day}/{p.birthYear}
                                </span>
                            )}
                        </div>

                        <SummaryControls
                            person={p}
                            summaries={summaries}
                            setSummaries={setSummaries}
                            summarizePerson={summarizePerson}
                            hasOpenAIKey={hasOpenAIKey}
                            styles={styles}
                        />

                    </li>
                ))}
            </ul>
        </div>
    );
}
