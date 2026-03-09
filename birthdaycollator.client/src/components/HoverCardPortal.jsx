import { useEffect, useState, useLayoutEffect } from "react";
import { createPortal } from "react-dom";

export default function HoverCardPortal({ anchorRef, children, visible }) {
    const [coords, setCoords] = useState({ top: 0, left: 0 });

    useLayoutEffect(() => {
        if (!visible || !anchorRef.current) return;

        const updatePosition = () => {
            const rect = anchorRef.current.getBoundingClientRect();

            setCoords({
                top: rect.bottom + 8, 
                left: rect.left
            });
        };

        updatePosition();
        window.addEventListener("resize", updatePosition);
        window.addEventListener("scroll", updatePosition);

        return () => {
            window.removeEventListener("resize", updatePosition);
            window.removeEventListener("scroll", updatePosition);
        };
    }, [visible, anchorRef]);

    if (!visible) return null;

    return createPortal(
        <div
            className={styles.hoverCard}
            style={{
                top: coords.top,
                left: coords.left,
                position: "fixed"
            }}
        >
            {children}
        </div>
,        document.body
    );
}