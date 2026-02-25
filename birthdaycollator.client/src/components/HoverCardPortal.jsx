import { useEffect, useState } from "react";
import { createPortal } from "react-dom";

export default function HoverCardPortal({ anchorRef, children, visible }) {
  const [coords, setCoords] = useState({ top: 0, left: 0 });

useEffect(() => {
  if (!visible || !anchorRef.current) return;

  const rect = anchorRef.current.getBoundingClientRect();

  setCoords({
    top: rect.top + window.scrollY + rect.height + 8,
    left: rect.left + window.scrollX
  });
}, [visible, anchorRef]);

  if (!visible) return null;

return createPortal(
  <div
    style={{
      position: "fixed",
      top: 200,
      left: 200,
      background: "yellow",
      padding: 40,
      fontSize: 30,
      zIndex: 999999999
    }}
  >
    TEST POPUP
  </div>,
  document.body
);


  return createPortal(
    <div
      style={{
        position: "fixed",
        top: coords.top,
        left: coords.left,
        background: "white",
        padding: "10px 14px",
        borderRadius: "8px",
        boxShadow: "0 4px 12px rgba(0,0,0,0.15)",
        width: "260px",
        zIndex: 999999,
        fontSize: "14px",
        lineHeight: 1.35
      }}
    >
      {children}
    </div>,
    document.body
  );
}
