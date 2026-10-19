document.addEventListener("DOMContentLoaded", async () => {
    if (!isAuthenticated()) {
        window.location.href = "/Account/Login";
        return;
    }

    await loadDashboard();
});

let expensesByCategoryChart = null;
let monthlySummaryChart = null;

async function loadDashboard() {
    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl("/api/dashboard"),
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

        const dashboard =
            await response.json();

        renderSummaryCards(dashboard);

        renderExpensesByCategoryChart(
            dashboard.expensesByCategory);

        renderMonthlySummaryChart(
            dashboard.monthlySummary);

        renderBudgetProgress(
            dashboard.budgetProgress);

        renderRecentTransactions(
            dashboard.recentTransactions);

        renderActiveGoals(
            dashboard.activeGoals);

        renderUpcomingRecurringTransactions(
            dashboard.upcomingRecurringTransactions);
    }
    catch {
        showDashboardError(
            "Could not connect to the server.");
    }
}

function renderSummaryCards(dashboard) {
    setText(
        "currentBalance",
        formatMoney(
            dashboard.currentBalance));

    setText(
        "incomeThisMonth",
        formatMoney(
            dashboard.incomeThisMonth));

    setText(
        "expensesThisMonth",
        formatMoney(
            dashboard.expensesThisMonth));

    setText(
        "savingsThisMonth",
        formatMoney(
            dashboard.savingsThisMonth));

    const balanceElement =
        document.getElementById(
            "currentBalance");

    if (balanceElement) {
        balanceElement.classList.remove(
            "text-success",
            "text-danger");

        if (dashboard.currentBalance > 0) {
            balanceElement.classList.add(
                "text-success");
        }
        else if (dashboard.currentBalance < 0) {
            balanceElement.classList.add(
                "text-danger");
        }
    }

    const savingsElement =
        document.getElementById(
            "savingsThisMonth");

    if (savingsElement) {
        savingsElement.classList.remove(
            "text-success",
            "text-danger");

        if (dashboard.savingsThisMonth > 0) {
            savingsElement.classList.add(
                "text-success");
        }
        else if (dashboard.savingsThisMonth < 0) {
            savingsElement.classList.add(
                "text-danger");
        }
    }
}

function renderExpensesByCategoryChart(
    expensesByCategory) {

    const canvas =
        document.getElementById(
            "expensesByCategoryChart");

    const emptyMessage =
        document.getElementById(
            "expensesByCategoryEmpty");

    if (!canvas) {
        return;
    }

    if (expensesByCategoryChart) {
        expensesByCategoryChart.destroy();
        expensesByCategoryChart = null;
    }

    const data =
        expensesByCategory ?? [];

    if (data.length === 0) {
        canvas.classList.add(
            "d-none");

        emptyMessage?.classList.remove(
            "d-none");

        return;
    }

    canvas.classList.remove(
        "d-none");

    emptyMessage?.classList.add(
        "d-none");

    const labels =
        data.map(item =>
            item.categoryName);

    const values =
        data.map(item =>
            item.amount);

    const backgroundColors =
        generateChartColors(
            values.length,
            0.72);

    const borderColors =
        generateChartColors(
            values.length,
            1);

    expensesByCategoryChart =
        new Chart(
            canvas,
            {
                type: "doughnut",
                data: {
                    labels,
                    datasets: [
                        {
                            data: values,
                            backgroundColor:
                                backgroundColors,
                            borderColor:
                                borderColors,
                            borderWidth: 1
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    cutout: "62%",
                    plugins: {
                        legend: {
                            position: "bottom"
                        },
                        tooltip: {
                            callbacks: {
                                label: context => {
                                    const label =
                                        context.label ?? "";

                                    const value =
                                        context.raw ?? 0;

                                    return `${label}: ${formatMoney(value)}`;
                                }
                            }
                        }
                    }
                }
            });
}

function renderMonthlySummaryChart(
    monthlySummary) {

    const canvas =
        document.getElementById(
            "monthlySummaryChart");

    if (!canvas) {
        return;
    }

    if (monthlySummaryChart) {
        monthlySummaryChart.destroy();
        monthlySummaryChart = null;
    }

    const data =
        monthlySummary ?? [];

    const labels =
        data.map(item =>
            formatMonthLabel(
                item.year,
                item.month));

    const incomeData =
        data.map(item =>
            item.income);

    const expensesData =
        data.map(item =>
            item.expenses);

    monthlySummaryChart =
        new Chart(
            canvas,
            {
                type: "bar",
                data: {
                    labels,
                    datasets: [
                        {
                            label: "Income",
                            data: incomeData,
                            backgroundColor:
                                "rgba(25, 135, 84, 0.7)",
                            borderColor:
                                "rgba(25, 135, 84, 1)",
                            borderWidth: 1
                        },
                        {
                            label: "Expenses",
                            data: expensesData,
                            backgroundColor:
                                "rgba(220, 53, 69, 0.7)",
                            borderColor:
                                "rgba(220, 53, 69, 1)",
                            borderWidth: 1
                        }
                    ]
                },
                options: {
                    responsive: true,
                    maintainAspectRatio: false,
                    scales: {
                        y: {
                            beginAtZero: true,
                            ticks: {
                                callback: value =>
                                    formatMoney(value)
                            }
                        }
                    },
                    plugins: {
                        legend: {
                            position: "bottom"
                        },
                        tooltip: {
                            callbacks: {
                                label: context => {
                                    const label =
                                        context.dataset.label;

                                    const value =
                                        context.raw ?? 0;

                                    return `${label}: ${formatMoney(value)}`;
                                }
                            }
                        }
                    }
                }
            });
}

function renderBudgetProgress(budgets) {
    const container =
        document.getElementById(
            "budgetProgressContainer");

    if (!container) {
        return;
    }

    if (!budgets ||
        budgets.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No active budgets for the current period.
            </p>`;

        return;
    }

    container.innerHTML =
        budgets
            .map(budget => {

                const categoryName =
                    budget.categoryName ??
                    "Overall Budget";

                const rawPercentage =
                    Number(
                        budget.percentageUsed ??
                        0);

                const progressWidth =
                    Math.min(
                        100,
                        Math.max(
                            0,
                            rawPercentage));

                const progressClass =
                    getBudgetProgressClass(
                        rawPercentage);

                const remainingClass =
                    budget.remainingAmount < 0
                        ? "text-danger"
                        : "text-muted";

                return `
                    <div class="mb-4">

                        <div class="d-flex
                                    justify-content-between
                                    align-items-center
                                    mb-1">

                            <strong>
                                ${escapeHtml(categoryName)}
                            </strong>

                            <span>
                                ${rawPercentage.toFixed(1)}%
                            </span>

                        </div>

                        <div class="progress mb-2"
                             style="height: 10px;">

                            <div class="progress-bar
                                        ${progressClass}"
                                 role="progressbar"
                                 style="width: ${progressWidth}%"
                                 aria-valuenow="${progressWidth}"
                                 aria-valuemin="0"
                                 aria-valuemax="100">
                            </div>

                        </div>

                        <div class="d-flex
                                    justify-content-between
                                    small">

                            <span class="text-muted">
                                Spent:
                                ${formatMoney(
                    budget.spentAmount)}
                            </span>

                            <span class="${remainingClass}">
                                Remaining:
                                ${formatMoney(
                        budget.remainingAmount)}
                            </span>

                        </div>

                        <div class="small text-muted mt-1">
                            Budget:
                            ${formatMoney(
                            budget.budgetAmount)}
                        </div>

                    </div>
                `;
            })
            .join("");
}

function renderRecentTransactions(
    transactions) {

    const container =
        document.getElementById(
            "recentTransactionsContainer");

    if (!container) {
        return;
    }

    if (!transactions ||
        transactions.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No recent transactions.
            </p>`;

        return;
    }

    container.innerHTML =
        transactions
            .map(transaction => {

                const isIncome =
                    transaction.type === 1;

                const amountClass =
                    isIncome
                        ? "text-success"
                        : "text-danger";

                const sign =
                    isIncome
                        ? "+"
                        : "-";

                return `
                    <div class="d-flex
                                justify-content-between
                                align-items-center
                                border-bottom
                                py-2">

                        <div>

                            <div class="fw-semibold">
                                ${escapeHtml(
                    transaction.description)}
                            </div>

                            <div class="small text-muted">
                                ${escapeHtml(
                        transaction.categoryName)}
                                ·
                                ${formatDate(
                            transaction.date)}
                            </div>

                        </div>

                        <div class="fw-bold ${amountClass}">
                            ${sign}
                            ${formatMoney(
                                transaction.amount)}
                        </div>

                    </div>
                `;
            })
            .join("");
}

function renderActiveGoals(goals) {
    const container =
        document.getElementById(
            "activeGoalsContainer");

    if (!container) {
        return;
    }

    if (!goals ||
        goals.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No active financial goals.
            </p>`;

        return;
    }

    container.innerHTML =
        goals
            .map(goal => {

                const percentage =
                    calculatePercentage(
                        goal.currentAmount,
                        goal.targetAmount);

                return `
                    <div class="mb-4">

                        <div class="d-flex
                                    justify-content-between
                                    mb-1">

                            <strong>
                                ${escapeHtml(
                    goal.name)}
                            </strong>

                            <span>
                                ${percentage.toFixed(0)}%
                            </span>

                        </div>

                        <div class="progress mb-2"
                             style="height: 10px;">

                            <div class="progress-bar"
                                 role="progressbar"
                                 style="width: ${percentage}%"
                                 aria-valuenow="${percentage}"
                                 aria-valuemin="0"
                                 aria-valuemax="100">
                            </div>

                        </div>

                        <div class="d-flex
                                    justify-content-between
                                    small
                                    text-muted">

                            <span>
                                ${formatMoney(
                        goal.currentAmount)}
                            </span>

                            <span>
                                ${formatMoney(
                            goal.targetAmount)}
                            </span>

                        </div>

                        ${goal.targetDate
                        ? `
                                    <div class="small
                                                text-muted
                                                mt-1">
                                        Target:
                                        ${formatDate(
                            goal.targetDate)}
                                    </div>
                                `
                        : ""
                    }

                    </div>
                `;
            })
            .join("");
}

function renderUpcomingRecurringTransactions(
    recurringTransactions) {

    const container =
        document.getElementById(
            "upcomingRecurringContainer");

    if (!container) {
        return;
    }

    if (!recurringTransactions ||
        recurringTransactions.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No upcoming recurring transactions.
            </p>`;

        return;
    }

    container.innerHTML =
        recurringTransactions
            .map(transaction => {

                const isIncome =
                    transaction.type === 1;

                const amountClass =
                    isIncome
                        ? "text-success"
                        : "text-danger";

                const sign =
                    isIncome
                        ? "+"
                        : "-";

                return `
                    <div class="d-flex
                                justify-content-between
                                align-items-center
                                border-bottom
                                py-2">

                        <div>

                            <div class="fw-semibold">
                                ${escapeHtml(
                    transaction.description)}
                            </div>

                            <div class="small text-muted">
                                ${escapeHtml(
                        transaction.categoryName)}
                                ·
                                ${getIntervalName(
                            transaction.interval)}
                            </div>

                            <div class="small text-muted">
                                Next:
                                ${formatDate(
                                transaction.nextExecutionDate)}
                            </div>

                        </div>

                        <div class="fw-bold ${amountClass}">
                            ${sign}
                            ${formatMoney(
                                    transaction.amount)}
                        </div>

                    </div>
                `;
            })
            .join("");
}

function getBudgetProgressClass(
    percentage) {

    if (percentage >= 100) {
        return "bg-danger";
    }

    if (percentage >= 80) {
        return "bg-warning";
    }

    return "bg-success";
}

function calculatePercentage(
    current,
    target) {

    if (!target ||
        target <= 0) {
        return 0;
    }

    const percentage =
        (current / target) * 100;

    return Math.min(
        100,
        Math.max(
            0,
            percentage));
}

function getIntervalName(interval) {
    switch (interval) {
        case 1:
            return "Weekly";

        case 2:
            return "Monthly";

        case 3:
            return "Yearly";

        default:
            return "Unknown";
    }
}

function formatMonthLabel(
    year,
    month) {

    const date =
        new Date(
            year,
            month - 1,
            1);

    return date.toLocaleString(
        "en-US",
        {
            month: "short",
            year: "2-digit"
        });
}

function generateChartColors(
    count,
    alpha) {

    const colors = [];

    for (let i = 0;
        i < count;
        i++) {

        const hue =
            Math.round(
                (360 / Math.max(
                    count,
                    1)) * i);

        colors.push(
            `hsla(${hue}, 65%, 55%, ${alpha})`);
    }

    return colors;
}

function setText(
    elementId,
    value) {

    const element =
        document.getElementById(
            elementId);

    if (element) {
        element.textContent =
            value;
    }
}

function showDashboardError(messageText) {
    const message =
        document.getElementById(
            "dashboardError");

    if (!message) {
        return;
    }

    message.textContent =
        messageText;

    message.classList.remove(
        "d-none");
}