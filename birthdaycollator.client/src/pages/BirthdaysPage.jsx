import { useState, useEffect } from "react";
import { fetchBirthdays, clearBirthdayCache } from "../api/birthdays";
import { useBackendHealth } from "../hooks/useBackendHealth";
import { useOverrideYear } from "../hooks/useOverrideYear";
import { useOpenAIKey } from "../hooks/useOpenAIKey";
import { useSummaries } from "../hooks/useSummaries";
import { daysInMonth, incrementDay, decrementDay } from "../utils/dateUtils";
import styles from "./BirthdaysPage.module.css";
import ToolsDropdown from "../components/ToolsDropdown";
import { fetchYears } from "../api/birthdays";





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

  const { hasOpenAIKey, checkOpenAIKey } = useOpenAIKey();

  const { summaries, setSummaries, summarizePerson } = useSummaries();

  const base = import.meta.env.VITE_API_BASE_URL;


  const backendOnline = useBackendHealth();

  const {
        overrideYear,
        overrideInput,
        setOverrideInput,
        loadOverride,
        applyOverride,
        clearOverride
    } = useOverrideYear();

  const monthNames = [
    "January","February","March","April","May","June",
    "July","August","September","October","November","December"
    ];

    const [selectedMonth, setSelectedMonth] = useState(null);
    const [selectedDay, setSelectedDay] = useState(null);


  const isStale =
    fetchedMonth !== null &&
    fetchedDay !== null &&
    (fetchedMonth !== month || fetchedDay !== day);

  const activeYear = overrideYear || new Date().getFullYear();


  const [years, setYears] = useState([]);

    useEffect(() => {
        const controller = new AbortController();

        fetchYears(controller.signal)
            .then(setYears)
            .catch(err =>
            {
                if (err.name !== "AbortError") console.error(err);
            });

        return () => controller.abort();
    }, []);


  // ---------------------------------------------------------
  // Auto-correct invalid days when month changes
  // ---------------------------------------------------------
  useEffect(() => {
    const max = daysInMonth(activeYear, month);
    if (day > max) {
      setDay(max);
    }
  }, [month, activeYear]);


 // ---------------------------------------------------------
  // Fetch birthdays
  // ---------------------------------------------------------
    async function run() {
        const controller = new AbortController();
        setRunController(controller);
        setIsRunning(true);
        setLoading(true);
        setError("");

        try {
            const data = await fetchBirthdays(month, day, controller.signal);
            setResults(data);
            setFetchedMonth(month);
            setFetchedDay(day);

          //  await checkExists();

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

  
  // ---------------------------------------------------------
  // Backend offline UI
  // ---------------------------------------------------------
  if (backendOnline === false) {
    return (
      <div className={styles.page}>
        <div className={styles.card}>
          <h1 className={styles.title}>Birthdays</h1>
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
        <div className={styles.card}>
          <h1 className={styles.title}>Birthdays</h1>
          <div className={styles.loadingText}>Checking backend status…</div>
        </div>
      </div>
    );
  }

    return (
        <div className={styles.page}>
            <div className={styles.card}>

                <ToolsDropdown
                    overrideYear={overrideYear}
                    overrideInput={overrideInput}
                    setOverrideInput={setOverrideInput}
                    applyOverride={applyOverride}
                    clearOverride={clearOverride}
                    years={years} />  

                <h1 className={styles.title}>Birthdays</h1>

                <div className={styles.toolbar}>
                    <button className={styles.toolbarButton} onClick={setToToday}>
                        Today
                    </button>

                    <div className={styles.toolbarGroup}>
                        <button
                            className={styles.toolbarButton}
                            onClick={() => {
                                const next = incrementDay(activeYear, month, day);
                                setMonth(next.month);
                                setDay(next.day);
                            }}
                        >
                            +
                        </button>

                        <button
                            className={styles.toolbarButton}
                            onClick={() => {
                                const next = decrementDay(activeYear, month, day);
                                setMonth(next.month);
                                setDay(next.day);
                            }}
                        >
                            -
                        </button>

                        {isRunning ? (
                            <button
                                className={`${styles.toolbarButton} ${styles.cancelButton}`}
                                onClick={() => runController?.abort()}
                                disabled={!runController}
                            >
                                Cancel
                            </button>
                        ) : (
                            <button
                                className={`${styles.toolbarButton} ${styles.runButton}`}
                                onClick={run}
                                disabled={loading}
                            >
                                {loading ? "Loading…" : "Run"}
                            </button>
                        )}

                        {/*{isCached && (*/}
                        {/*    <button*/}
                        {/*        className={styles.toolbarButton}*/}
                        {/*        onClick={() => clearBirthdayCache(month, day)}*/}
                        {/*        disabled={loading}*/}
                        {/*    >*/}
                        {/*        Clear Cache*/}
                        {/*    </button>*/}
                        {/*)}*/}

                    </div>
                </div>

                <div style={{ height: "6px", width: "100%" }} />

                <div className={styles.dateSelectors}>
                    <select value={month} onChange={e => setMonth(Number(e.target.value))}>
                        {monthNames.map((name, index) => (
                            <option key={index + 1} value={index + 1}>
                                {name}
                            </option>
                        ))}
                    </select>

                    <select value={day} onChange={e => setDay(Number(e.target.value))}>
                        {Array.from({ length: daysInMonth(activeYear, month) }, (_, i) => i + 1).map(d => (
                            <option key={d} value={d}>{d}</option>
                        ))}
                    </select>

                    {overrideYear && (
                        <span className={styles.overridePill}>
                            {overrideYear}
                        </span>
                    )}
                </div>

                {error && <div className={styles.error}>{error}</div>}

                {results.length > 0 && (
                    <div className={styles.results}>
                        <div className={styles.resultsHeader}>
                            {results.length} {results.length === 1 ? "result" : "results"}
                            {isStale && (
                                <span className={styles.staleNotice}> (stale — press Run)</span>
                            )}
                        </div>

                        <ul className={styles.list}>
                            {results.map((p, i) => (
                                <li
                                    key={i}
                                    className={`${styles.item} ${p.age === 90 || p.age === 100 ? styles.milestone : ""
                                        }`}
                                >
                                    <div className={styles.inlineRow}>
                                        <span className={styles.ageBadge}>{p.age}</span>

                                        <a
                                            href={p.url}
                                            target="_blank"
                                            rel="noopener noreferrer"
                                            className={styles.nameLink}
                                        >
                                            {p.name} — {p.description}
                                        </a>
                                    </div>

                                    <div className={styles.summaryRow}>
                                        <button
                                            className={styles.summaryChip}
                                            disabled={!hasOpenAIKey}
                                            title={
                                                !hasOpenAIKey
                                                    ? "Add an OpenAI API key to enable summaries"
                                                    : ""
                                            }
                                            onClick={() => {
                                                if (summaries[p.name]) {
                                                    const copy = { ...summaries };
                                                    delete copy[p.name];
                                                    setSummaries(copy);
                                                } else {
                                                    summarizePerson(p);
                                                }
                                            }}
                                        >
                                            {summaries[p.name] ? "Clear" : "Summarize"}
                                        </button>

                                        {summaries[p.name] && (
                                            <div className={styles.summaryBox}>
                                                {summaries[p.name]}
                                            </div>
                                        )}
                                    </div>
                                </li>
                            ))}
                        </ul>
                    </div>
                )}
            </div>
        </div>
    );


}
