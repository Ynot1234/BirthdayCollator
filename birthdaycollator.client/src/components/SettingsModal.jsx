import { useState, useEffect } from "react";
import { FiX, FiEye, FiEyeOff } from "react-icons/fi";
import styles from "./SettingsModal.module.css";

export default function SettingsModal({ isOpen, onClose }) {
    const [apiKey, setApiKey] = useState("");
    const [showKey, setShowKey] = useState(false);
    const [hasExistingKey, setHasExistingKey] = useState(false);

    useEffect(() => {
        if (isOpen) {
            const stored = localStorage.getItem("openai_api_key") ?? "";
            setApiKey(stored);
            setHasExistingKey(!!stored);
        }
    }, [isOpen]);

    function handleSave() {
        localStorage.setItem("openai_api_key", apiKey.trim());
        setHasExistingKey(true);
        onClose();
    }

    function handleClear() {
        localStorage.removeItem("openai_api_key");
        setApiKey("");
        setHasExistingKey(false);
        onClose();
    }

    if (!isOpen) return null;

    return (
        <div className={styles.backdrop} onClick={onClose}>
            <div className={styles.modal} onClick={(e) => e.stopPropagation()}>

                {/* Close button */}
                <button className={styles.closeButton} onClick={onClose}>
                    <FiX size={18} />
                </button>

                <div className={styles.badge}>SETTINGS</div>
                <h2 className={styles.title}>OpenAI API Key</h2>
                <p className={styles.subtitle}>
                    Your key is stored only in your browser and never sent to the server.
                </p>

                <label className={styles.label}>API Key</label>
                <div className={styles.inputRow}>
                    <input
                        className={styles.input}
                        type={showKey ? "text" : "password"}
                        value={apiKey}
                        onChange={(e) => setApiKey(e.target.value)}
                        placeholder="sk-..."
                    />
                    <button
                        className={styles.eyeButton}
                        onClick={() => setShowKey((v) => !v)}
                    >
                        {showKey ? <FiEyeOff size={18} /> : <FiEye size={18} />}
                    </button>
                </div>

                {/* Conditional button */}
                {!hasExistingKey ? (
                    <button className={styles.saveButton} onClick={handleSave}>
                        Save API Key
                    </button>
                ) : (
                    <button
                        className={styles.saveButton}
                        style={{ background: "#444" }}
                        onClick={handleClear}
                    >
                        Clear API Key
                    </button>
                )}
            </div>
        </div>
    );
}
