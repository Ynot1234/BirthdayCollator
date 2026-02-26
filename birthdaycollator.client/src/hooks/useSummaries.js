import { useState } from "react";

export function useSummaries() {
    const [summaries, setSummaries] = useState({});

    async function summarizePerson(p) {
        try {
            const res = await fetch("/api/ai/summarize", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({
                    name: p.name,
                    description: p.description
                })
            });

            const text = await res.text();

            setSummaries(prev => ({
                ...prev,
                [p.name]: text
            }));
        } catch (err) {
            console.error("Summarize failed:", err);
            alert("Failed to summarize this person.");
        }
    }

    return { summaries, setSummaries, summarizePerson };
}
