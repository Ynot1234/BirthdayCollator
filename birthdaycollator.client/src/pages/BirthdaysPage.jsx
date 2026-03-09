import React, { useState, useRef, useEffect } from "react";

// Hooks
import { useBackendHealth } from "../hooks/useBackendHealth";
import { useOverrideYear } from "../hooks/useOverrideYear";
import { useOpenAIKey } from "../hooks/useOpenAIKey";
import { useSummaries } from "../hooks/useSummaries";
import { useBirthdays } from "../hooks/useBirthdays";

// Components
import { Toolbar } from "../components/Toolbar";
import { DateSelectors } from "../components/DateSelectors";
import { ResultsList } from "../components/ResultsList";
import { Pagination } from "../components/Pagination";
import ToolsDropdown from "../components/ToolsDropdown";
import SettingsModal from "../components/SettingsModal";

// Assets
import { FiSettings, FiAlertCircle } from "react-icons/fi";
import styles from "./BirthdaysPage.module.css";

export default function BirthdaysPage() {
    const [settingsOpen, setSettingsOpen] = useState(false);
    const scrollAnchorRef = useRef(null);

    const healthStatus = useBackendHealth();
    const isOffline = healthStatus === "offline";
    const isChecking = healthStatus === "loading";

    const { hasOpenAIKey } = useOpenAIKey();
    const { summaries, setSummaries, summarizePerson } = useSummaries();

    const override = useOverrideYear();
    const activeYear = override.overrideYear ?? new Date().getFullYear();
    const bday = useBirthdays(activeYear);

    useEffect(() => {
        override.loadOverride();
    }, []);

    useEffect(() => {
        if (bday.page > 1) {
            scrollAnchorRef.current?.scrollIntoView({ behavior: "smooth" });
        }
    }, [bday.page]);

    return (
        <div className={styles.page}>
            <div className={styles.card}>

                {isOffline && (
                    <div className={styles.offlineBanner}>
                        <FiAlertCircle size={18} />
                        <span>Backend is offline. Please start the server to fetch data.</span>
                    </div>
                )}

                <div className={styles.topRow}>
                    <ToolsDropdown
                        overrideInput={override.overrideInput}
                        setOverrideInput={override.setOverrideInput}
                        includeAll={override.includeAll}
                        setIncludeAll={override.setIncludeAll}
                        applyOverride={override.applyOverride}
                        clearOverride={override.clearOverride}
                        years={bday.years}
                        isOpen={override.isToolsOpen}
                        setIsOpen={override.setIsToolsOpen}
                    />

                    <button
                        className={styles.settingsButton}
                        onClick={() => setSettingsOpen(true)}
                        title="API Settings"
                    >
                        <FiSettings size={20} />
                    </button>
                </div>

                <h1 className={styles.pageTitle}>Birthdays</h1>

                {/* 4. Pass disabled state to Toolbar */}
                <Toolbar
                    onToday={bday.nav.today}
                    onNextDay={bday.nav.next}
                    onPrevDay={bday.nav.prev}
                    onRun={() => bday.run(override.includeAll)}
                    onCancel={() => bday.runController?.abort()}
                    isRunning={!!bday.runController}
                    loading={bday.loading || isChecking}
                    disabled={isOffline || isChecking}
                />

                <DateSelectors
                    month={bday.month}
                    day={bday.day}
                    setMonth={bday.setMonth}
                    setDay={bday.setDay}
                    activeYear={activeYear}
                    overrideYear={override.overrideYear}
                    includeAll={override.includeAll}
                />

                {bday.error && <div className={styles.error}>{bday.error}</div>}

                <div ref={scrollAnchorRef} style={{ scrollMarginTop: "20px" }} />

                {bday.results.length > 0 && (
                    <ResultsList
                        results={bday.results}
                        currentPageItems={bday.currentItems}
                        isStale={bday.isStale}
                        includeAll={override.includeAll}
                        summaries={summaries}
                        setSummaries={setSummaries}
                        summarizePerson={summarizePerson}
                        hasOpenAIKey={hasOpenAIKey}
                    />
                )}

                <Pagination
                    page={bday.page}
                    setPage={bday.setPage}
                    totalPages={bday.totalPages}
                />
            </div>

            <SettingsModal
                isOpen={settingsOpen}
                onClose={() => setSettingsOpen(false)}
            />
        </div>
    );
}