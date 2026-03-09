export default function SummaryControls({
    person,
    summaries,
    setSummaries,
    summarizePerson,
    hasOpenAIKey,
    styles
}) {
    // Unique key: name + birthYear prevents collisions
    const summaryKey = `${person.name}-${person.birthYear}`;
    const summaryText = summaries[summaryKey];
    const hasSummary = Boolean(summaryText);

    const toggle = () => {
        if (hasSummary) {
            // Atomic removal: Create a new object without this key
            const { [summaryKey]: _, ...remaining } = summaries;
            setSummaries(remaining);
        } else {
            summarizePerson(person);
        }
    };

    return (
        <div className={styles.summaryRow}>
            <button
                className={styles.summaryChip}
                disabled={!hasOpenAIKey}
                title={!hasOpenAIKey ? "Add an OpenAI API key in Settings to enable summaries" : ""}
                onClick={toggle}
            >
                {hasSummary ? "Clear" : "Summarize"}
            </button>

            {hasSummary && (
                <div className={styles.summaryBox}>
                    {summaryText}
                </div>
            )}
        </div>
    );
}