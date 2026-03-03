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
    const [includeAll, setIncludeAll] = useState(false);

    const safeYears = Array.isArray(years) ? years : [];


    function applyWith(includeAllValue) {
        applyOverride({
            year: overrideInput,
            includeAll: includeAllValue
        });
    }


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
                    {/*<div className={styles.row}>*/}
                    {/*    <strong>Year</strong>*/}
                    {/*</div>*/}

                    <div className={styles.row}>
                        <select
                            className={styles.input}
                            value={overrideInput}
                            onChange={e => setOverrideInput(e.target.value)}>
                            <option value="">Year</option>
                            {safeYears.map(y => (
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
                                onChange={e => {
                                    const val = e.target.checked;
                                    setIncludeAll(val);
                                    applyOverride({ year: overrideInput, includeAll: val });
                                }}
                            />

                            Show all
                        </label>
                    </div>

                    <div className={styles.buttonRow}>
                        <button
                            className={styles.smallButton}
                            onClick={() =>
                                applyOverride({
                                    year: overrideInput,
                                    includeAll
                                })
                            }
                        >
                            Apply
                        </button>

                        <button
                            className={styles.smallButton}
                            onClick={clearOverride}
                        >
                            Clear
                        </button>
                    </div>
                </div>
            )}
        </div>
    );

}
