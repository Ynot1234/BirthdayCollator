import { cleanText } from "../utils/cleanText";

export interface BirthdayPerson {
    name: string;
    description: string;
    age: number;
    month: number;
    day: number;
    birthYear: number;
    url: string;
}

export async function fetchBirthdays(
    month: number | string,
    day: number | string,
    includeAll: boolean,
    signal?: AbortSignal
): Promise<BirthdayPerson[]> {
    const base = import.meta.env.VITE_API_BASE_URL;

    try {
        const response = await fetch(
            `${base}/api/birthdays?month=${month}&day=${day}&includeAll=${includeAll}`,
            { signal }
        );

        if (!response.ok) {
            throw new Error(`Server returned HTTP ${response.status}`);
        }

        try {
            const data = (await response.json()) as BirthdayPerson[];

            const cleaned = data.map((p) => ({
                ...p,
                name: cleanText(p.name),
                description: cleanText(p.description)
            }));

            return cleaned;
        } catch {
            throw new Error("Received invalid JSON from server.");
        }
    } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") {
            throw err;
        }

        const message =
            err instanceof Error
                ? err.message
                : "Unexpected error occurred.";

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
): Promise<void> {
    const base = import.meta.env.VITE_API_BASE_URL;

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

export async function fetchYears(signal?: AbortSignal): Promise<number[]> {
    const base = import.meta.env.VITE_API_BASE_URL;

    try {
        const response = await fetch(`${base}/api/birthdays/years`, { signal });

        if (!response.ok) {
            throw new Error(`Server returned HTTP ${response.status}`);
        }

        try {
            return (await response.json()) as number[];
        } catch {
            throw new Error("Received invalid JSON from server.");
        }
    } catch (err) {
        if (err instanceof DOMException && err.name === "AbortError") {
            throw err;
        }

        const message =
            err instanceof Error
                ? err.message
                : "Unexpected error occurred.";

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
