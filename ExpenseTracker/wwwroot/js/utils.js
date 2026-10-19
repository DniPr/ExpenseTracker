function formatMoney(value) {
    return new Intl.NumberFormat(
        "bg-BG",
        {
            style: "currency",
            currency: "EUR"
        })
        .format(value ?? 0);
}

function formatDate(value) {
    if (!value) {
        return "-";
    }

    return new Date(value)
        .toLocaleDateString("bg-BG");
}

function escapeHtml(value) {
    const element =
        document.createElement("div");

    element.textContent =
        value ?? "";

    return element.innerHTML;
}

function toDateInputValue(value) {
    if (!value) {
        return "";
    }

    const date =
        new Date(value);

    const year =
        date.getFullYear();

    const month =
        String(date.getMonth() + 1)
            .padStart(2, "0");

    const day =
        String(date.getDate())
            .padStart(2, "0");

    return `${year}-${month}-${day}`;
}

function getTodayInputValue() {
    const today =
        new Date();

    const year =
        today.getFullYear();

    const month =
        String(today.getMonth() + 1)
            .padStart(2, "0");

    const day =
        String(today.getDate())
            .padStart(2, "0");

    return `${year}-${month}-${day}`;
}

function debounce(callback, delay) {
    let timeoutId;

    return (...args) => {
        clearTimeout(timeoutId);

        timeoutId =
            setTimeout(
                () => callback(...args),
                delay);
    };
}