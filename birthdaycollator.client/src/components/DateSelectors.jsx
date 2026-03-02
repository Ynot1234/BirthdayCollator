import React from "react";
import styles from "../pages/BirthdaysPage.module.css";

export function DateSelectors({
    month,
    day,
    setMonth,
    setDay,
    monthNames,
    activeYear,
    overrideYear,
    daysInMonth
}) {
    return (
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
    );
}
