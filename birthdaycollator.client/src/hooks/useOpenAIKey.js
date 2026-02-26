import { useState, useEffect } from "react";

export function useOpenAIKey() {
    const [hasOpenAIKey, setHasOpenAIKey] = useState(false);

    async function checkOpenAIKey() {
        try {
            const res = await fetch("/api/ai/has-key");
            const data = await res.json();
            setHasOpenAIKey(data.hasKey);
        } catch {
            setHasOpenAIKey(false);
        }
    }

    useEffect(() => {
        checkOpenAIKey();
    }, []);

    return { hasOpenAIKey, checkOpenAIKey };
}
