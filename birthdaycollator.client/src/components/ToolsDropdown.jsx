import { useState } from "react";
import styles from "./ToolsDropdown.module.css";

export default function ToolsDropdown({
  overrideYear,
  overrideInput,
  setOverrideInput,
  applyOverride,
  clearOverride
}) {
  const [open, setOpen] = useState(false);

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
            <strong>Year:</strong> {overrideYear ?? "Entire Range"}
          </div>

            <input
        type="text"
        className={styles.input}
        value={overrideInput}
        onChange={e => {
          const v = e.target.value;
          if (/^\d{0,4}$/.test(v)) setOverrideInput(v);
        }}
        onKeyDown={e => {
          if (e.key === "Enter") {
            applyOverride();
          }
        }}
      />


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
