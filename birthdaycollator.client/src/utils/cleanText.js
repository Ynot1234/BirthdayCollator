export function cleanText(text) {
    if (!text) return "";

    return text
        .normalize("NFKC")      // normalize Unicode
        .replace(/\uFFFD/g, "") // remove replacement glyph
        .replace(/\s+/g, " ")   // collapse weird spacing
        .trim();
}
