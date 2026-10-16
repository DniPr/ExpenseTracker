document.addEventListener("DOMContentLoaded", async () => {
    if (!isAuthenticated()) {
        window.location.href = "/Account/Login";
        return;
    }

    await loadDashboard();
});

async function loadDashboard() {
    const token =
        localStorage.getItem("expenseTrackerToken");

    const message =
        document.getElementById("dashboardMessage");

    try {
        const response = await fetch(
            "https://localhost:7063/api/dashboard",
            {
                method: "GET",
                headers: {
                    "Authorization": `Bearer ${token}`
                }
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            showDashboardError(
                "Could not load dashboard data.");

            return;
        }

        const data = await response.json();

        renderSummary(data);
        renderExpensesByCategory(data.expensesByCategory);
        renderMonthlySummary(data.monthlySummary);
        renderBudgetProgress(data.budgetProgress);
        renderRecentTransactions(data.recentTransactions);
        renderFinancialGoals(data.activeGoals);
        renderRecurringTransactions(
            data.upcomingRecurringTransactions);
    }
    catch {
        showDashboardError(
            "Could not connect to the server.");
    }
}

function renderSummary(data) {
    document.getElementById("currentBalance").textContent =
        formatMoney(data.currentBalance);

    document.getElementById("incomeThisMonth").textContent =
        formatMoney(data.incomeThisMonth);

    document.getElementById("expensesThisMonth").textContent =
        formatMoney(data.expensesThisMonth);

    document.getElementById("savingsThisMonth").textContent =
        formatMoney(data.savingsThisMonth);
}

function renderExpensesByCategory(items) {
    const container =
        document.getElementById(
            "expensesByCategoryContainer");

    if (!items || items.length === 0) {
        container.innerHTML =
            `<p class="text-muted">
                No expense data available.
            </p>`;

        return;
    }

    container.innerHTML = items
        .map(item => `
            <div class="d-flex
                        justify-content-between
                        border-bottom
                        py-2">

                <span>
                    ${escapeHtml(item.categoryName)}
                </span>

                <strong>
                    ${formatMoney(item.amount)}
                </strong>

            </div>
        `)
        .join("");
}

function renderMonthlySummary(items) {
    const container =
        document.getElementById(
            "monthlySummaryContainer");

    if (!items || items.length === 0) {
        container.innerHTML =
            `<p class="text-muted">
                No monthly data available.
            </p>`;

        return;
    }

    container.innerHTML = items
        .map(item => `
            <div class="border-bottom py-2">

                <div class="fw-semibold mb-1">
                    ${getMonthName(item.month)}
                    ${item.year}
                </div>

                <div class="d-flex
                            justify-content-between">

                    <span>
                        Income:
                        ${formatMoney(item.income)}
                    </span>

                    <span>
                        Expenses:
                        ${formatMoney(item.expenses)}
                    </span>

                </div>

            </div>
        `)
        .join("");
}

function renderBudgetProgress(items) {
    const container =
        document.getElementById(
            "budgetProgressContainer");

    if (!items || items.length === 0) {
        container.innerHTML =
            `<p class="text-muted">
                No active budgets.
            </p>`;

        return;
    }

    container.innerHTML = items
        .map(item => {
            const percentage =
                Math.min(
                    Math.max(item.percentageUsed, 0),
                    100);

            const title =
                item.categoryName ??
                "Overall Budget";

            return `
                <div class="mb-4">

                    <div class="d-flex
                                justify-content-between
                                mb-1">

                        <strong>
                            ${escapeHtml(title)}
                        </strong>

                        <span>
                            ${formatMoney(item.spentAmount)}
                            /
                            ${formatMoney(item.budgetAmount)}
                        </span>

                    </div>

                    <div class="progress"
                         style="height: 20px;">

                        <div class="progress-bar"
                             role="progressbar"
                             style="width: ${percentage}%">

                            ${item.percentageUsed.toFixed(1)}%

                        </div>

                    </div>

                    <small class="text-muted">
                        Remaining:
                        ${formatMoney(item.remainingAmount)}
                    </small>

                </div>
            `;
        })
        .join("");
}

function renderRecentTransactions(items) {
    const container =
        document.getElementById(
            "recentTransactionsContainer");

    if (!items || items.length === 0) {
        container.innerHTML =
            `<p class="text-muted">
                No transactions yet.
            </p>`;

        return;
    }

    container.innerHTML = items
        .map(item => `
            <div class="d-flex
                        justify-content-between
                        align-items-center
                        border-bottom
                        py-2">

                <div>
                    <div class="fw-semibold">
                        ${escapeHtml(item.description)}
                    </div>

                    <small class="text-muted">
                        ${escapeHtml(item.categoryName)}
                        ·
                        ${formatDate(item.date)}
                    </small>
                </div>

                <strong>
                    ${item.type === 1 ? "+" : "-"}
                    ${formatMoney(item.amount)}
                </strong>

            </div>
        `)
        .join("");
}

function renderFinancialGoals(items) {
    const container =
        document.getElementById(
            "financialGoalsContainer");

    if (!items || items.length === 0) {
        container.innerHTML =
            `<p class="text-muted">
                No active goals.
            </p>`;

        return;
    }

    container.innerHTML = items
        .map(item => {
            const percentage =
                item.targetAmount > 0
                    ? Math.min(
                        (item.currentAmount /
                            item.targetAmount) * 100,
                        100)
                    : 0;

            return `
                <div class="mb-3">

                    <div class="d-flex
                                justify-content-between">

                        <strong>
                            ${escapeHtml(item.name)}
                        </strong>

                        <span>
                            ${percentage.toFixed(1)}%
                        </span>

                    </div>

                    <div class="progress my-2"
                         style="height: 18px;">

                        <div class="progress-bar"
                             style="width: ${percentage}%">
                        </div>

                    </div>

                    <small class="text-muted">
                        ${formatMoney(item.currentAmount)}
                        /
                        ${formatMoney(item.targetAmount)}
                    </small>

                </div>
            `;
        })
        .join("");
}

function renderRecurringTransactions(items) {
    const container =
        document.getElementById(
            "recurringTransactionsContainer");

    if (!items || items.length === 0) {
        container.innerHTML =
            `<p class="text-muted">
                No upcoming recurring transactions.
            </p>`;

        return;
    }

    container.innerHTML = items
        .map(item => `
            <div class="d-flex
                        justify-content-between
                        border-bottom
                        py-2">

                <div>
                    <div class="fw-semibold">
                        ${escapeHtml(item.description)}
                    </div>

                    <small class="text-muted">
                        Next:
                        ${formatDate(item.nextExecutionDate)}
                    </small>
                </div>

                <strong>
                    ${formatMoney(item.amount)}
                </strong>

            </div>
        `)
        .join("");
}

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
    return new Date(value)
        .toLocaleDateString("bg-BG");
}

function getMonthName(month) {
    const date =
        new Date(2026, month - 1, 1);

    return date.toLocaleString(
        "en-US",
        {
            month: "short"
        });
}

function showDashboardError(messageText) {
    const message =
        document.getElementById(
            "dashboardMessage");

    if (!message) {
        return;
    }

    message.textContent = messageText;
    message.classList.remove("d-none");
}

function escapeHtml(value) {
    const element =
        document.createElement("div");

    element.textContent = value ?? "";

    return element.innerHTML;
}