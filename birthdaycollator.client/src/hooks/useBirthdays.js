import { useState, useEffect } from "react";
import { fetchBirthdays, fetchYears } from "../api/birthdays";
import { daysInMonth, incrementDay, decrementDay } from "../utils/dateUtils";

export function useBirthdays(activeYear) {
    const today = new Date();
    const [month, setMonth] = useState(today.getMonth() + 1);
    const [day, setDay] = useState(today.getDate());
    const [results, setResults] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");
    const [page, setPage] = useState(1);
    const [runController, setRunController] = useState(null);
    const [fetchedDate, setFetchedDate] = useState({ m: null, d: null });
    const [years, setYears] = useState([]);

    const page_size = 20;

    useEffect(() => setPage(1), [results]);

    useEffect(() => {
        const controller = new AbortController();
        fetchYears(controller.signal).then(setYears).catch(() => { });
        return () => controller.abort();
    }, []);

    useEffect(() => {
        const max = daysInMonth(activeYear, month);
        if (day > max) setDay(max);
    }, [month, activeYear]);

    const run = async (includeAll) => {
        const controller = new AbortController();
        setRunController(controller);
        setLoading(true);
        setError("");
        try {
            const data = await fetchBirthdays(month, day, includeAll, controller.signal);
            setResults(data);
            setFetchedDate({ m: month, d: day });
        } catch (err) {
            if (err.name !== "AbortError") setError(err.message);
        } finally {
            setLoading(false);
            setRunController(null);
        }
    };

    const nav = {
        next: () => {
            const n = incrementDay(activeYear, month, day);
            setMonth(n.month); setDay(n.day);
        },
        prev: () => {
            const p = decrementDay(activeYear, month, day);
            setMonth(p.month); setDay(p.day);
        },
        today: () => {
            setMonth(today.getMonth() + 1);
            setDay(today.getDate());
        }
    };

    const isStale = fetchedDate.m !== null && (fetchedDate.m !== month || fetchedDate.d !== day);

    const totalPages = Math.max(1, Math.ceil(results.length / page_size));

    const currentItems = results.slice(
        (page - 1) * page_size,
        page * page_size
    );
    // ------------------------------

    return {
        month, setMonth, day, setDay, results, loading, error,
        page, setPage, run, isStale, runController, nav, years,
        currentItems, totalPages 
    };
}