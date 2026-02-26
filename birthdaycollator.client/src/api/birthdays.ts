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
            throw err; // Let the caller detect cancellation
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
