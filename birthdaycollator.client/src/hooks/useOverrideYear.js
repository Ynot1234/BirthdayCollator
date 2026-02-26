import { useState } from "react";

export function useOverrideYear() {
    const [overrideYear, setOverrideYear] = useState(null);
    const [overrideInput, setOverrideInput] = useState("");

    const base = import.meta.env.VITE_API_BASE_URL;

    async function loadOverride() {
        const res = await fetch(`${base}/api/birthdays/override`);
        const data = await res.json();
        setOverrideYear(data.overrideYear);
    }

    async function applyOverride() {
        if (!overrideInput) return;
        await fetch(`${base}/api/birthdays/override?value=${overrideInput}`, { method: "POST" });
        setOverrideInput("");
        await loadOverride();
    }

    async function clearOverride() {
        await fetch(`${base}/api/birthdays/override?value=`, { method: "POST" });
        await loadOverride();
    }

    return {
        overrideYear,
        overrideInput,
        setOverrideInput,
        loadOverride,
        applyOverride,
        clearOverride
    };
}
