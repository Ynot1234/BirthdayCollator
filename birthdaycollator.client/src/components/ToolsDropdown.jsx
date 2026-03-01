import { useState } from "react";
import styles from "./ToolsDropdown.module.css";

export default function ToolsDropdown({
    overrideYear,
    overrideInput,
    setOverrideInput,
    applyOverride,
    clearOverride,
    years
}) {
    const [open, setOpen] = useState(false);

    const safeYears = Array.isArray(years) ? years : [];

    return (
        <div className={styles.dropdown}>
            <button
                className={styles.toggle}
                onClick={() => setOpen(o => !o)}
            >
                Tools {open ? "▲" : "▼"}
            </button>

            {open && (
                <div className={styles.panel}>
                    <div className={styles.row}>
                        <strong>Year:</strong> {overrideYear ?? ""}
                    </div>

                    <select
                        className={styles.input}
                        value={overrideInput}
                        onChange={e => setOverrideInput(e.target.value)}
                    >
                        <option value="">Entire Range</option>
                        {safeYears.map(y => (
                            <option key={y} value={y}>
                                {y}
                            </option>
                        ))}
                    </select>

                    <div className={styles.buttonRow}>
                        <button className={styles.smallButton} onClick={applyOverride}>
                            Apply
                        </button>
                        <button className={styles.smallButton} onClick={clearOverride}>
                            Clear
                        </button>
                    </div>
                </div>
            )}
        </div>
    );
}
