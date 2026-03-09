import { useState } from "react";

export function useOverrideYear() {
    const [overrideYear, setOverrideYear] = useState(null);
    const [overrideInput, setOverrideInput] = useState("");
    const [includeAll, setIncludeAll] = useState(false);
    const [isToolsOpen, setIsToolsOpen] = useState(false); // The state

    const base = import.meta.env.VITE_API_BASE_URL;

    async function loadOverride() {
        const res = await fetch(`${base}/api/birthdays/override`);
        const data = await res.json();
        setOverrideYear(data.overrideYear);
        setIncludeAll(data.includeAll ?? false);
    }

    async function applyOverride({ year, includeAll }) {
        await fetch(`${base}/api/birthdays/override`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ year, includeAll })
        });
        await loadOverride();
    }

    async function clearOverride() {
        await fetch(`${base}/api/birthdays/override`, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ year: "", includeAll: false })
        });
        await loadOverride();
        setOverrideInput("");
        setIncludeAll(false);
    }

    return {
        overrideYear,
        overrideInput,
        includeAll,
        isToolsOpen,      // <--- Ensure these are here
        setIsToolsOpen,   // <--- Ensure these are here
        setOverrideInput,
        setIncludeAll,
        loadOverride,
        applyOverride,
        clearOverride
    };
}