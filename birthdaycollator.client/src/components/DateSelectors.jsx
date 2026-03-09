import React from "react";
import styles from "../pages/BirthdaysPage.module.css";
import { daysInMonth } from "../utils/dateUtils";

const MONTH_NAMES = [
    "January", "February", "March", "April", "May", "June",
    "July", "August", "September", "October", "November", "December"
];

export function DateSelectors({
    month,
    day,
    setMonth,
    setDay,
    activeYear,
    overrideYear
}) {
    const daysCount = daysInMonth(activeYear, month);

    return (
        <div className={styles.dateSelectors}>
            <select
                className={styles.toolbarButton}
                value={month}
                onChange={(e) => setMonth(Number(e.target.value))}
            /* includeAll check removed to keep it enabled */
            >
                {MONTH_NAMES.map((name, index) => (
                    <option key={index + 1} value={index + 1}>
                        {name}
                    </option>
                ))}
            </select>

            <select
                className={styles.toolbarButton}
                value={day}
                onChange={(e) => setDay(Number(e.target.value))}
            /* includeAll check removed to keep it enabled */
            >
                {Array.from({ length: daysCount }, (_, i) => i + 1).map((d) => (
                    <option key={d} value={d}>
                        {d}
                    </option>
                ))}
            </select>

            {overrideYear && (
                <span className={styles.overridePill}>
                    {overrideYear}
                </span>
            )}
        </div>
    );
}