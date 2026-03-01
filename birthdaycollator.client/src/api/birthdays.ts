export async function fetchBirthdays(
    month: number | string,
    day: number | string,
    signal?: AbortSignal
) {
    const base = import.meta.env.VITE_API_BASE_URL;

    try {
        const response = await fetch(
            `${base}/api/birthdays?month=${month}&day=${day}`,
            { signal }
        );

        if (!response.ok) {
            throw new Error(`Server returned HTTP ${response.status}`);
        }

        try {
            return await response.json();
        } catch {
            throw new Error("Received invalid JSON from server.");
        }

    } catch (err) {
        // Clean cancellation handling
        if (err instanceof DOMException && err.name === "AbortError") {
            throw err; 
        }

        let message = "Unexpected error occurred.";

        if (err instanceof Error) {
            message = err.message;
        }

        if (
            message.includes("Failed to fetch") ||
            message.includes("NetworkError") ||
            message.includes("ERR_CONNECTION_REFUSED")
        ) {
            throw new Error("Backend is not running or unreachable.");
        }

        throw new Error(message);
    }
}

export async function clearBirthdayCache(
    month: number | string | { value: number },
    day: number | string | { value: number }
) {
    const base = import.meta.env.VITE_API_BASE_URL;

    // Normalize values coming from dropdowns, dates, or raw numbers
    const monthValue =
        typeof month === "object" && month !== null ? month.value : month;

    const dayValue =
        typeof day === "object" && day !== null ? day.value : day;

    const response = await fetch(
        `${base}/api/birthdays/${monthValue}/${dayValue}`,
        { method: "DELETE" }
    );


    if (!response.ok) {
        throw new Error(`Failed to clear cache: HTTP ${response.status}`);
    }
}


export async function fetchYears(signal?: AbortSignal) {
    const base = import.meta.env.VITE_API_BASE_URL;

    try {
        const response = await fetch(`${base}/api/birthdays/years`, { signal });


        if (!response.ok) {
            throw new Error(`Server returned HTTP ${response.status}`);
        }

        try {
            return await response.json(); 
        } catch {
            throw new Error("Received invalid JSON from server.");
        }

    } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") {
            throw err;
        }

        let message = "Unexpected error occurred.";

        if (err instanceof Error) {
            message = err.message;
        }

        if (
            message.includes("Failed to fetch") ||
            message.includes("NetworkError") ||
            message.includes("ERR_CONNECTION_REFUSED")
        ) {
            throw new Error("Backend is not running or unreachable.");
        }

        throw new Error(message);
    }
}