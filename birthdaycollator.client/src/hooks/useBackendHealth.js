import { useState, useEffect } from "react"; // <--- This was missing!

export function useBackendHealth() {
    const [status, setStatus] = useState("loading");

    useEffect(() => {
        let isMounted = true;
        const startTime = Date.now();

        async function check() {
            const base = import.meta.env.VITE_API_BASE_URL;
            try {
                const res = await fetch(`${base}/health`);
                if (isMounted) setStatus(res.ok ? "online" : "offline");
            } catch {
                // Wait at least 400ms total to prevent the "flicker"
                const elapsed = Date.now() - startTime;
                const remaining = Math.max(0, 400 - elapsed);

                setTimeout(() => {
                    if (isMounted) setStatus("offline");
                }, remaining);
            }
        }

        check();
        return () => { isMounted = false; };
    }, []);

    return status;
}