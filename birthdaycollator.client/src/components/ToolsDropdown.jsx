import React from "react";
import styles from "./ToolsDropdown.module.css";

export default function ToolsDropdown({
    overrideInput,
    setOverrideInput,
    includeAll,
    setIncludeAll,
    applyOverride,
    clearOverride,
    years,
    isOpen,
    setIsOpen
}) {
    const safeYears = Array.isArray(years) ? years : [];

    return (
        <div className={styles.dropdown}>
            <button
                type="button"
                className={styles.toggle}
                onClick={() => setIsOpen(!isOpen)}
            >
                Tools {isOpen ? "▲" : "▼"}
            </button>

            {isOpen && (
                <div className={styles.panel}>
                    <div className={styles.row}>
                        <select
                            className={styles.input}
                            value={overrideInput}
                            onChange={(e) => setOverrideInput(e.target.value)}
                        >
                            <option value="">Year</option>
                            {safeYears.map((y) => (
                                <option key={y} value={y}>
                                    {y}
                                </option>
                            ))}
                        </select>

                        <label style={{ opacity: !overrideInput ? 0.5 : 1 }}>
                            <input
                                type="checkbox"
                                checked={includeAll}
                                disabled={!overrideInput}
                                onChange={(e) => setIncludeAll(e.target.checked)}/>
                            Show all
                        </label>
                    </div>

                    <div className={styles.buttonRow}>
                        <button
                            className={styles.smallButton}
                            onClick={() => {applyOverride({ year: overrideInput, includeAll });}} >
                            Apply
                        </button>

                        <button
                            className={styles.smallButton}
                            onClick={() => {clearOverride();}} >
                            Clear
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}