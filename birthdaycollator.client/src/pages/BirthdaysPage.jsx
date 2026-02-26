import { useState, useEffect } from "react";
import styles from "./BirthdaysPage.module.css";
import { fetchBirthdays } from "../api/birthdays";
import ToolsDropdown from "../components/ToolsDropdown";

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

  const [overrideYear, setOverrideYear] = useState(null);
  const [overrideInput, setOverrideInput] = useState("");

  const [summaries, setSummaries] = useState({});
  const [hasOpenAIKey, setHasOpenAIKey] = useState(false);

  const [backendOnline, setBackendOnline] = useState(null);

  const [runController, setRunController] = useState(null);
  const [isRunning, setIsRunning] = useState(false);


  const monthNames = [
    "January","February","March","April","May","June",
    "July","August","September","October","November","December"
  ];

  const isStale =
    fetchedMonth !== null &&
    fetchedDay !== null &&
    (fetchedMonth !== month || fetchedDay !== day);

  // ---------------------------------------------------------
  // Determine active year (override or real year)
  // ---------------------------------------------------------
  const activeYear = overrideYear || new Date().getFullYear();

  // ---------------------------------------------------------
  // Correct days-in-month using active year
  // ---------------------------------------------------------
  function daysInMonth(year, month) {
    return new Date(year, month, 0).getDate();
  }

  // ---------------------------------------------------------
  // Rolling increment logic
  // ---------------------------------------------------------
  function incrementDay(m, d) {
    const max = daysInMonth(activeYear, m);
    if (d < max) return { month: m, day: d + 1 };

    const nextMonth = m === 12 ? 1 : m + 1;
    return { month: nextMonth, day: 1 };
  }

  // ---------------------------------------------------------
  // Rolling decrement logic
  // ---------------------------------------------------------
  function decrementDay(m, d) {
    if (d > 1) return { month: m, day: d - 1 };

    const prevMonth = m === 1 ? 12 : m - 1;
    const maxPrev = daysInMonth(activeYear, prevMonth);
    return { month: prevMonth, day: maxPrev };
  }

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
  // Backend health check
  // ---------------------------------------------------------
    async function checkBackend() {
        const base = import.meta.env.VITE_API_BASE_URL;

        try {
            const res = await fetch(`${base}/health`);
            return res.ok;
        } catch {
            return false;   
        }
    }


    // ---------------------------------------------------------
    // Override year
    // ---------------------------------------------------------
    async function loadOverride() {
        try {
            const base = import.meta.env.VITE_API_BASE_URL;
            const res = await fetch(`${base}/api/birthdays/override`);
            const data = await res.json();
            setOverrideYear(data.overrideYear);
        } catch (err) {
            console.error("Failed to load override", err);
        }
    }

    async function applyOverride() {
        if (!overrideInput) return;

        const base = import.meta.env.VITE_API_BASE_URL;

        await fetch(`${base}/api/birthdays/override?value=${overrideInput}`, {
            method: "POST"
        });

        setOverrideInput("");
        await loadOverride();
    }

    async function clearOverride() {
        const base = import.meta.env.VITE_API_BASE_URL;

        await fetch(`${base}/api/birthdays/override?value=`, {
            method: "POST"
        });

        await loadOverride();
    }

  // ---------------------------------------------------------
  // OpenAI key check
  // ---------------------------------------------------------
  async function checkOpenAIKey() {
    try {
      const res = await fetch("/api/ai/has-key");
      const data = await res.json();
      setHasOpenAIKey(data.hasKey);
    } catch {
      setHasOpenAIKey(false);
    }
  }

  // ---------------------------------------------------------
  // Initialize backend + dependent data
  // ---------------------------------------------------------
  useEffect(() => {
    async function init() {
      const online = await checkBackend();
      setBackendOnline(online);

      if (online) {
        loadOverride();
        checkOpenAIKey();
      }
    }

    init();
  }, []);

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


  function setToToday() {
    setMonth(todayMonth);
    setDay(todayDay);
  }

  // ---------------------------------------------------------
  // Summaries
  // ---------------------------------------------------------
  async function summarizePerson(p) {
    try {
      const res = await fetch("/api/ai/summarize", {
        method: "POST",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({
          name: p.name,
          description: p.description
        })
      });

      const text = await res.text();

      setSummaries(prev => ({
        ...prev,
        [p.name]: text
      }));
    } catch (err) {
      console.error("Summarize failed:", err);
      alert("Failed to summarize this person.");
    }
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

  // ---------------------------------------------------------
  // MAIN UI
  // ---------------------------------------------------------
    return (
        <div className={styles.page}>
            <div className={styles.card}>

                {/* TOOLS DROPDOWN — moved inside the card */}
                <ToolsDropdown
                    overrideYear={overrideYear}
                    overrideInput={overrideInput}
                    setOverrideInput={setOverrideInput}
                    applyOverride={applyOverride}
                    clearOverride={clearOverride}
                />

                {/* TITLE */}
                <h1 className={styles.title}>Birthdays</h1>

                {/* TOOLBAR */}
                <div className={styles.toolbar}>
                    <button className={styles.toolbarButton} onClick={setToToday}>
                        Today
                    </button>

                    <div className={styles.toolbarGroup}>
                       <button
                            className={styles.toolbarButton}
                            onClick={() => {
                                const next = incrementDay(month, day);
                                setMonth(next.month);
                                setDay(next.day);
                            }}
                        > +
                        </button>
                        <button
                            className={styles.toolbarButton}
                            onClick={() => {
                                const next = decrementDay(month, day);
                                setMonth(next.month);
                                setDay(next.day);
                            }}
                        > -
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
                                                    const copy = { ...summaries }
                                                    delete copy[p.name]
                                                    setSummaries(copy)
                                                } else {
                                                    summarizePerson(p)
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
