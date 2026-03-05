import { useState, useEffect, useRef } from "react";

import { fetchBirthdays, clearBirthdayCache, fetchYears } from "../api/birthdays";

import { useBackendHealth } from "../hooks/useBackendHealth";
import { useOverrideYear } from "../hooks/useOverrideYear";
import { useOpenAIKey } from "../hooks/useOpenAIKey";
import { useSummaries } from "../hooks/useSummaries";

import { Toolbar } from "../components/Toolbar";
import { DateSelectors } from "../components/DateSelectors";
import { ResultsList } from "../components/ResultsList";
import { Pagination } from "../components/Pagination";
import ToolsDropdown from "../components/ToolsDropdown";
import { FiSettings } from "react-icons/fi";
import SettingsModal from "../components/SettingsModal";

import { daysInMonth, incrementDay, decrementDay } from "../utils/dateUtils";

import styles from "./BirthdaysPage.module.css";

export default function BirthdaysPage() {
    const today = new Date();
    const todayMonth = today.getMonth() + 1;
    const todayDay = today.getDate();

    const [month, setMonth] = useState(todayMonth);
    const [day, setDay] = useState(todayDay);

    const [results, setResults] = useState([]);
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState("");

    const [fetchedMonth, setFetchedMonth] = useState(null);
    const [fetchedDay, setFetchedDay] = useState(null);

    const [runController, setRunController] = useState(null);
    const [isRunning, setIsRunning] = useState(false);

    const { hasOpenAIKey } = useOpenAIKey();
    const { summaries, setSummaries, summarizePerson } = useSummaries();

    const [page, setPage] = useState(1);
    const pageSize = 20;

    const [settingsOpen, setSettingsOpen] = useState(false);

    const totalPages = Math.ceil(results.length / pageSize);

    const currentPageItems = results.slice(
        (page - 1) * pageSize,
        page * pageSize
    );

    useEffect(() => {
        setPage(1);
    }, [results]);

    const isStale =
        fetchedMonth !== null &&
        fetchedDay !== null &&
        (fetchedMonth !== month || fetchedDay !== day);

    const topRef = useRef(null);

    useEffect(() => {
        topRef.current?.scrollIntoView({ behavior: "smooth" });
    }, [page]);

    const base = import.meta.env.VITE_API_BASE_URL;
    const backendOnline = useBackendHealth();

    const {
        overrideYear,
        overrideInput,
        setOverrideInput,
        includeAll,
        setIncludeAll,
        loadOverride,
        applyOverride,
        clearOverride
    } = useOverrideYear();

    const monthNames = [
        "January", "February", "March", "April", "May", "June",
        "July", "August", "September", "October", "November", "December"
    ];

    const activeYear = overrideYear || new Date().getFullYear();

    const [years, setYears] = useState([]);

    useEffect(() => {
        const controller = new AbortController();

        fetchYears(controller.signal)
            .then(setYears)
            .catch(err => {
                if (err.name !== "AbortError") console.error(err);
            });

        return () => controller.abort();
    }, []);

    useEffect(() => {
        const max = daysInMonth(activeYear, month);
        if (day > max) {
            setDay(max);
        }
    }, [month, activeYear]);

    async function run() {
        const controller = new AbortController();
        setRunController(controller);
        setIsRunning(true);
        setLoading(true);
        setError("");

        try {
            const data = await fetchBirthdays(month, day, includeAll, controller.signal);
            setResults(data);
            setFetchedMonth(month);
            setFetchedDay(day);
        } catch (err) {
            if (err.name === "AbortError") {
                console.log("Run cancelled");
            } else {
                setError(err.message);
            }
        } finally {
            setLoading(false);
            setIsRunning(false);
            setRunController(null);
        }
    }

    const [isCached, setIsCached] = useState(false);

    useEffect(() => {
        async function check() {
            const res = await fetch(`${base}/api/birthdays/exists/${month}/${day}`);
            const exists = await res.json();
            setIsCached(exists);
        }
        check();
    }, [month, day]);

    function setToToday() {
        setMonth(todayMonth);
        setDay(todayDay);
    }

    if (backendOnline === false) {
        return (
            <div className={styles.page}>
                <h1 className={styles.pageTitle}>Birthdays</h1>
                <div className={styles.card}>
                    <h1 className={styles.headerTitle}>Birthdays</h1>
                    <div className={styles.offlineBox}>
                        Backend offline — start the server to enable features.
                    </div>
                </div>
            </div>
        );
    }

    if (backendOnline === null) {
        return (
            <div className={styles.page}>
                <h1 className={styles.pageTitle}>Birthdays</h1>
                <div className={styles.card}>
                    <h1 className={styles.headerTitle}>Birthdays</h1>
                    <div className={styles.loadingText}>Checking backend status…</div>
                </div>
            </div>
        );
    }

    return (
        <div className={styles.page}>
            <div className={styles.card}>

                {/* Tools + Gear at the very top */}
                <div className={styles.topRow}>
                    <ToolsDropdown
                        overrideYear={overrideYear}
                        overrideInput={overrideInput}
                        setOverrideInput={setOverrideInput}
                        applyOverride={applyOverride}
                        clearOverride={clearOverride}
                        years={years}
                    />

                    <button
                        className={styles.settingsButton}
                        onClick={() => setSettingsOpen(true)}
                        aria-label="Settings"
                    >
                        <FiSettings size={20} />
                    </button>
                </div>

                {/* Optional: move the title inside the card */}
                <h1 className={styles.pageTitle}>Birthdays</h1>

                <div className={styles.toolbar}>
                    <Toolbar
                        onToday={setToToday}
                        onNextDay={() => {
                            const next = incrementDay(activeYear, month, day);
                            setMonth(next.month);
                            setDay(next.day);
                        }}
                        onPrevDay={() => {
                            const next = decrementDay(activeYear, month, day);
                            setMonth(next.month);
                            setDay(next.day);
                        }}
                        onRun={run}
                        onCancel={() => runController?.abort()}
                        isRunning={isRunning}
                        loading={loading}
                        hasRunController={!!runController}
                    />
                </div>

                <DateSelectors
                    month={month}
                    day={day}
                    setMonth={setMonth}
                    setDay={setDay}
                    monthNames={monthNames}
                    activeYear={activeYear}
                    overrideYear={overrideYear}
                    daysInMonth={daysInMonth}
                    includeAll={includeAll}
                />

                {error && <div className={styles.error}>{error}</div>}

                <div ref={topRef}></div>

                {results.length > 0 && (
                    <div className={styles.results}>
                        <ResultsList
                            results={results}
                            currentPageItems={currentPageItems}
                            isStale={isStale}
                            includeAll={includeAll}
                            summaries={summaries}
                            setSummaries={setSummaries}
                            summarizePerson={summarizePerson}
                            hasOpenAIKey={hasOpenAIKey}
                        />
                    </div>
                )}

                <Pagination
                    page={page}
                    totalPages={totalPages}
                    setPage={setPage}
                />
            </div>

            <SettingsModal
                isOpen={settingsOpen}
                onClose={() => setSettingsOpen(false)}
            />
        </div>
    );


}
