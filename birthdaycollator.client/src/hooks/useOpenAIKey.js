import { useState, useEffect } from "react";

export function useOpenAIKey() {
    const [hasOpenAIKey, setHasOpenAIKey] = useState(false);
    const base = import.meta.env.VITE_API_BASE_URL;

    async function checkOpenAIKey() {
        try {
            const res = await fetch(`${base}/api/ai/has-key`);
            if (!res.ok) throw new Error("Status check failed");
            const data = await res.json();
            setHasOpenAIKey(data.hasKey);
        } catch (err) {
            console.error("OpenAI status check error:", err);
            setHasOpenAIKey(false);
        }
    }

    async function saveKeyToServer(apiKey) {
        try {
            const res = await fetch(`${base}/api/ai/set-key`, {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify({ apiKey: apiKey.trim() })
            });
            if (!res.ok) throw new Error("Failed to save key");
            await checkOpenAIKey();
        } catch (err) {
            console.error("Save key error:", err);
            throw err;
        }
    }

    async function clearKeyFromServer() {
        try {
            const res = await fetch(`${base}/api/ai/clear-key`, {
                method: "POST"
            });
            if (!res.ok) throw new Error("Failed to clear key");
            await checkOpenAIKey();
        } catch (err) {
            console.error("Clear key error:", err);
            throw err;
        }
    }

    useEffect(() => {
        checkOpenAIKey();
    }, []);

    return {
        hasOpenAIKey,
        checkOpenAIKey,
        saveKeyToServer,
        clearKeyFromServer
    };
}