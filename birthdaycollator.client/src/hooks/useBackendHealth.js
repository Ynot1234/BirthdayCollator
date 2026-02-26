import { useState, useEffect } from "react";

export function useBackendHealth() {
    const [backendOnline, setBackendOnline] = useState(null);

    async function checkBackend() {
        const base = import.meta.env.VITE_API_BASE_URL;
        try {
            const res = await fetch(`${base}/health`);
            return res.ok;
        } catch {
            return false;
        }
    }

    useEffect(() => {
        async function init() {
            const online = await checkBackend();
            setBackendOnline(online);
        }
        init();
    }, []);

    return backendOnline;
}
