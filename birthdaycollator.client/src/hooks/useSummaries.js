import { useState } from "react";

export function useSummaries() {
    const [summaries, setSummaries] = useState({});
    const base = import.meta.env.VITE_API_BASE_URL;

    async function summarizePerson(person) {
        const summaryKey = `${person.name}-${person.birthYear}`;

        try {
            const res = await fetch(`${base}/api/ai/summarize`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    name: person.name,
                    description: person.description
                })
            });

            if (!res.ok) throw new Error("Summarization failed");

            // CHANGE HERE: Get the response as text, not JSON
            const textData = await res.text();

            setSummaries(prev => ({
                ...prev,
                [summaryKey]: textData // Use the raw text directly
            }));
        } catch (err) {
            console.error("AI Summary Error:", err);
            alert("Failed to generate summary. Check backend logs.");
        }
    }

    return { summaries, setSummaries, summarizePerson };
}