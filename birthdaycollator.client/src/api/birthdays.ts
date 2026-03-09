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

const BASE_URL = import.meta.env.VITE_API_BASE_URL;

function handleApiError(err: unknown): never {
    if (err instanceof DOMException && err.name === "AbortError") {
        throw err;
    }

    const message = err instanceof Error ? err.message : "Unexpected error occurred.";

    const connectionIssues = ["Failed to fetch", "NetworkError", "ERR_CONNECTION_REFUSED"];
    if (connectionIssues.some(issue => message.includes(issue))) {
        throw new Error("Backend is not running or unreachable.");
    }

    throw new Error(message);
}

async function apiFetch<T>(url: string, options?: RequestInit): Promise<T> {
    try {
        const response = await fetch(`${BASE_URL}${url}`, options);

        if (!response.ok) {
            throw new Error(`Server returned HTTP ${response.status}`);
        }

        try {
            return await response.json() as T;
        } catch {
            throw new Error("Received invalid JSON from server.");
        }
    } catch (err) {
        return handleApiError(err);
    }
}

export async function fetchBirthdays(
    month: number | string,
    day: number | string,
    includeAll: boolean,
    signal?: AbortSignal
): Promise<BirthdayPerson[]> {
    const data = await apiFetch<BirthdayPerson[]>(
        `/api/birthdays?month=${month}&day=${day}&includeAll=${includeAll}`,
        { signal }
    );

    return data.map((p) => ({
        ...p,
        name: cleanText(p.name),
        description: cleanText(p.description)
    }));
}

export async function fetchYears(signal?: AbortSignal): Promise<number[]> {
    return apiFetch<number[]>("/api/birthdays/years", { signal });
}

export async function clearBirthdayCache(
    month: number | string | { value: number },
    day: number | string | { value: number }
): Promise<void> {
    const monthValue = typeof month === "object" && month !== null ? month.value : month;
    const dayValue = typeof day === "object" && day !== null ? day.value : day;

    const response = await fetch(`${BASE_URL}/api/birthdays/${monthValue}/${dayValue}`, {
        method: "DELETE"
    });

    if (!response.ok) {
        throw new Error(`Failed to clear cache: HTTP ${response.status}`);
    }
}