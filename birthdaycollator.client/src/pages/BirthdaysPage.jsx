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
import { StatusCard } from "../components/StatusCard";

// Assets
import { FiSettings } from "react-icons/fi";
import styles from "./BirthdaysPage.module.css";

export default function BirthdaysPage() {
    const [settingsOpen, setSettingsOpen] = useState(false);
    const scrollAnchorRef = useRef(null);

    // 1. Health & Configuration Hooks
    const backendOnline = useBackendHealth();
    const { hasOpenAIKey } = useOpenAIKey();
    const { summaries, setSummaries, summarizePerson } = useSummaries();

    // 2. Year Override Logic
    const override = useOverrideYear();
    const activeYear = override.overrideYear ?? new Date().getFullYear();

    // 3. Core Data Hook
    const bday = useBirthdays(activeYear);

    // Initial Load: Sync override settings from server/storage
    useEffect(() => {
        override.loadOverride();
    }, []);

    // UX: Smooth scroll to top of results on page change
    useEffect(() => {
        if (bday.page > 1) {
            scrollAnchorRef.current?.scrollIntoView({ behavior: "smooth" });
        }
    }, [bday.page]);

    // 4. Guard Clauses (Early Returns)
    if (backendOnline === null) {
        return <StatusCard message="Checking backend status..." />;
    }
    if (backendOnline === false) {
        return <StatusCard message="Backend offline — please start the server." />;
    }

    return (
        <div className={styles.page}>
            <div className={styles.card}>
                {/* Header Actions */}
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

                {/* Main Controls */}
                <Toolbar
                    onToday={bday.nav.today}
                    onNextDay={bday.nav.next}
                    onPrevDay={bday.nav.prev}
                    onRun={() => bday.run(override.includeAll)}
                    onCancel={() => bday.runController?.abort()}
                    isRunning={!!bday.runController}
                    loading={bday.loading}
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

                {/* Error Feedback */}
                {bday.error && <div className={styles.error}>{bday.error}</div>}

                {/* Results Anchor */}
                <div ref={scrollAnchorRef} style={{ scrollMarginTop: "20px" }} />

                {/* Results Display */}
                {bday.results.length > 0 && (
                    <ResultsList
                        results={bday.results}
                        currentPageItems={bday.currentItems}
                        isStale={bday.isStale}
                        includeAll={override.includeAll} // Pass through to show/hide dates
                        summaries={summaries}
                        setSummaries={setSummaries}
                        summarizePerson={summarizePerson}
                        hasOpenAIKey={hasOpenAIKey}
                    />
                )}

                {/* Navigation */}
                <Pagination
                    page={bday.page}
                    setPage={bday.setPage}
                    totalPages={bday.totalPages}
                />
            </div>

            {/* Modals */}
            <SettingsModal
                isOpen={settingsOpen}
                onClose={() => setSettingsOpen(false)}
            />
        </div>
    );
}