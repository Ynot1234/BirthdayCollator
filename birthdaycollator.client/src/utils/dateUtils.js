export function daysInMonth(year, month) {
    return new Date(year, month, 0).getDate();
}

export function incrementDay(year, m, d) {
    const max = daysInMonth(year, m);
    if (d < max) return { month: m, day: d + 1 };
    const nextMonth = m === 12 ? 1 : m + 1;
    return { month: nextMonth, day: 1 };
}

export function decrementDay(year, m, d) {
    if (d > 1) return { month: m, day: d - 1 };
    const prevMonth = m === 1 ? 12 : m - 1;
    const maxPrev = daysInMonth(year, prevMonth);
    return { month: prevMonth, day: maxPrev };
}
