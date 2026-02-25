import React, { useState } from "react";

export function YearOverridePanel({ overrideYear, setYearOverride }) {
  const [input, setInput] = useState("");

  const apply = () => {
    if (!input) return;
    setYearOverride(parseInt(input, 10));
    setInput("");
  };

  const clear = () => {
    setYearOverride(""); // server interprets empty as clear
  };

  return (
    <div style={{ marginBottom: "1rem" }}>
      <h3>Year Override</h3>

      <div>Current override: {overrideYear ?? "None"}</div>

      <input
        type="number"
        placeholder="Enter year"
        value={input}
        onChange={(e) => setInput(e.target.value)}
      />

      <button onClick={apply}>Apply</button>
      <button onClick={clear}>Clear</button>
    </div>
  );
}
