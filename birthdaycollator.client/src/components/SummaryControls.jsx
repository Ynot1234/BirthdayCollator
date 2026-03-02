export default function SummaryControls({
    person,
    summaries,
    setSummaries,
    summarizePerson,
    hasOpenAIKey,
    styles
}) {
    const hasSummary = Boolean(summaries[person.name]);

    const toggle = () => {
        if (hasSummary) {
            const copy = { ...summaries };
            delete copy[person.name];
            setSummaries(copy);
        } else {
            summarizePerson(person);
        }
    };

    return (
        <div className={styles.summaryRow}>
            <button
                className={styles.summaryChip}
                disabled={!hasOpenAIKey}
                title={!hasOpenAIKey ? "Add an OpenAI API key to enable summaries" : ""}
                onClick={toggle}
            >
                {hasSummary ? "Clear" : "Summarize"}
            </button>

            {hasSummary && (
                <div className={styles.summaryBox}>
                    {summaries[person.name]}
                </div>
            )}
        </div>
    );
}
