document.addEventListener("DOMContentLoaded", async () => {
    if (!isAuthenticated()) {
        window.location.href = "/Account/Login";
        return;
    }

    await loadExpenseCategories();
    await loadBudgets();

    const createForm =
        document.getElementById("createBudgetForm");

    const editForm =
        document.getElementById("editBudgetForm");

    const budgetPeriod =
        document.getElementById("budgetPeriod");

    const editBudgetPeriod =
        document.getElementById("editBudgetPeriod");

    createForm?.addEventListener(
        "submit",
        createBudget);

    editForm?.addEventListener(
        "submit",
        updateBudget);

    budgetPeriod?.addEventListener(
        "change",
        updateCreateMonthVisibility);

    editBudgetPeriod?.addEventListener(
        "change",
        updateEditMonthVisibility);

    setDefaultBudgetValues();
    updateCreateMonthVisibility();
    updateEditMonthVisibility();
});

let expenseCategories = [];

async function loadExpenseCategories() {
    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl("/api/categories"),
            {
                method: "GET",
                headers: {
                    "Authorization":
                        `Bearer ${token}`
                }
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            showBudgetsError(
                "Could not load categories.");

            return;
        }

        const categories =
            await response.json();

        expenseCategories =
            categories.filter(
                category => category.type === 2);

        populateBudgetCategories();
    }
    catch {
        showBudgetsError(
            "Could not connect to the server.");
    }
}

function populateBudgetCategories() {
    const createCategorySelect =
        document.getElementById(
            "budgetCategory");

    const editCategorySelect =
        document.getElementById(
            "editBudgetCategory");

    if (createCategorySelect) {
        createCategorySelect.innerHTML =
            `<option value="">
                Overall Budget
            </option>`;

        expenseCategories.forEach(category => {
            const option =
                document.createElement("option");

            option.value =
                category.id;

            option.textContent =
                category.name;

            createCategorySelect.appendChild(
                option);
        });
    }

    if (editCategorySelect) {
        editCategorySelect.innerHTML =
            `<option value="">
                Overall Budget
            </option>`;

        expenseCategories.forEach(category => {
            const option =
                document.createElement("option");

            option.value =
                category.id;

            option.textContent =
                category.name;

            editCategorySelect.appendChild(
                option);
        });
    }
}

async function loadBudgets() {
    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl("/api/budgets"),
            {
                method: "GET",
                headers: {
                    "Authorization":
                        `Bearer ${token}`
                }
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            showBudgetsError(
                "Could not load budgets.");

            return;
        }

        const budgets =
            await response.json();

        renderBudgets(
            budgets);
    }
    catch {
        showBudgetsError(
            "Could not connect to the server.");
    }
}

function renderBudgets(budgets) {
    const container =
        document.getElementById(
            "budgetsContainer");

    if (!container) {
        return;
    }

    if (!budgets ||
        budgets.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No budgets found.
            </p>`;

        return;
    }

    container.innerHTML =
        budgets
            .map(budget => {

                const periodText =
                    budget.period === 1
                        ? "Monthly"
                        : "Yearly";

                const monthText =
                    budget.period === 1 &&
                        budget.month
                        ? getMonthName(
                            budget.month)
                        : "";

                const categoryText =
                    budget.categoryName ??
                    "Overall Budget";

                return `
                    <div class="border-bottom py-3">

                        <div class="d-flex
                                    justify-content-between
                                    align-items-center
                                    gap-3">

                            <div>

                                <div class="d-flex
                                            align-items-center
                                            gap-2
                                            mb-1">

                                    <h5 class="mb-0">
                                        ${escapeHtml(
                    categoryText)}
                                    </h5>

                                    <span class="badge bg-secondary">
                                        ${periodText}
                                    </span>

                                </div>

                                <div class="text-muted small">
                                    ${budget.period === 1
                        ? `${monthText} ${budget.year}`
                        : `${budget.year}`
                    }
                                </div>

                            </div>

                            <div class="text-end">

                                <div class="fw-bold
                                            fs-5
                                            mb-2">
                                    ${formatMoney(
                        budget.amount)}
                                </div>

                                <div class="d-flex
                                            gap-2
                                            justify-content-end">

                                    <button type="button"
                                            class="btn
                                                   btn-sm
                                                   btn-outline-primary"
                                            onclick="openEditBudget(
                                                ${budget.id}
                                            )">
                                        Edit
                                    </button>

                                    <button type="button"
                                            class="btn
                                                   btn-sm
                                                   btn-outline-danger"
                                            onclick="deleteBudget(
                                                ${budget.id}
                                            )">
                                        Delete
                                    </button>

                                </div>

                            </div>

                        </div>

                    </div>
                `;
            })
            .join("");
}

async function createBudget(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const amount =
        Number(
            document.getElementById(
                "budgetAmount")
                .value);

    const period =
        Number(
            document.getElementById(
                "budgetPeriod")
                .value);

    const monthValue =
        document.getElementById(
            "budgetMonth")
            .value;

    const year =
        Number(
            document.getElementById(
                "budgetYear")
                .value);

    const categoryValue =
        document.getElementById(
            "budgetCategory")
            .value;

    const message =
        document.getElementById(
            "createBudgetMessage");

    message.textContent = "";

    if (!amount ||
        !period ||
        !year) {

        message.textContent =
            "Please fill in all required fields.";

        return;
    }

    if (period === 1 &&
        !monthValue) {

        message.textContent =
            "Month is required for a monthly budget.";

        return;
    }

    const month =
        period === 1
            ? Number(monthValue)
            : null;

    const categoryId =
        categoryValue
            ? Number(categoryValue)
            : null;

    if (!isBudgetPeriodCurrentOrFuture(
        period,
        month,
        year)) {

        message.textContent =
            "Budget period cannot be in the past.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl("/api/budgets"),
            {
                method: "POST",
                headers: {
                    "Content-Type":
                        "application/json",

                    "Authorization":
                        `Bearer ${token}`
                },
                body: JSON.stringify({
                    amount,
                    period,
                    month,
                    year,
                    categoryId
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Budget could not be created.";

            try {
                const data =
                    await response.json();

                errorMessage =
                    data.message ??
                    errorMessage;
            }
            catch {
            }

            message.textContent =
                errorMessage;

            return;
        }

        resetCreateBudgetForm();

        const modalElement =
            document.getElementById(
                "createBudgetModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadBudgets();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function openEditBudget(id) {
    const token =
        getAuthToken();

    const message =
        document.getElementById(
            "editBudgetMessage");

    if (message) {
        message.textContent = "";
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/budgets/${id}`),
            {
                method: "GET",
                headers: {
                    "Authorization":
                        `Bearer ${token}`
                }
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            showBudgetsError(
                "Could not load budget.");

            return;
        }

        const budget =
            await response.json();

        document.getElementById(
            "editBudgetId").value =
            budget.id;

        document.getElementById(
            "editBudgetAmount").value =
            budget.amount;

        document.getElementById(
            "editBudgetPeriod").value =
            budget.period;

        document.getElementById(
            "editBudgetYear").value =
            budget.year;

        document.getElementById(
            "editBudgetCategory").value =
            budget.categoryId ?? "";

        if (budget.period === 1) {
            document.getElementById(
                "editBudgetMonth").value =
                budget.month ?? "";
        }
        else {
            document.getElementById(
                "editBudgetMonth").value = "";
        }

        updateEditMonthVisibility();

        const modalElement =
            document.getElementById(
                "editBudgetModal");

        const modal =
            new bootstrap.Modal(
                modalElement);

        modal.show();
    }
    catch {
        showBudgetsError(
            "Could not connect to the server.");
    }
}

async function updateBudget(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const id =
        Number(
            document.getElementById(
                "editBudgetId")
                .value);

    const amount =
        Number(
            document.getElementById(
                "editBudgetAmount")
                .value);

    const period =
        Number(
            document.getElementById(
                "editBudgetPeriod")
                .value);

    const monthValue =
        document.getElementById(
            "editBudgetMonth")
            .value;

    const year =
        Number(
            document.getElementById(
                "editBudgetYear")
                .value);

    const categoryValue =
        document.getElementById(
            "editBudgetCategory")
            .value;

    const message =
        document.getElementById(
            "editBudgetMessage");

    message.textContent = "";

    if (!id ||
        !amount ||
        !period ||
        !year) {

        message.textContent =
            "Please fill in all required fields.";

        return;
    }

    if (period === 1 &&
        !monthValue) {

        message.textContent =
            "Month is required for a monthly budget.";

        return;
    }

    const month =
        period === 1
            ? Number(monthValue)
            : null;

    const categoryId =
        categoryValue
            ? Number(categoryValue)
            : null;

    if (!isBudgetPeriodCurrentOrFuture(
        period,
        month,
        year)) {

        message.textContent =
            "Budget period cannot be in the past.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/budgets/${id}`),
            {
                method: "PUT",
                headers: {
                    "Content-Type":
                        "application/json",

                    "Authorization":
                        `Bearer ${token}`
                },
                body: JSON.stringify({
                    amount,
                    period,
                    month,
                    year,
                    categoryId
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Budget could not be updated.";

            try {
                const data =
                    await response.json();

                errorMessage =
                    data.message ??
                    errorMessage;
            }
            catch {
            }

            message.textContent =
                errorMessage;

            return;
        }

        const modalElement =
            document.getElementById(
                "editBudgetModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadBudgets();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function deleteBudget(id) {
    const confirmed =
        confirm(
            "Are you sure you want to delete this budget?");

    if (!confirmed) {
        return;
    }

    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl(
                `/api/budgets/${id}`),
            {
                method: "DELETE",
                headers: {
                    "Authorization":
                        `Bearer ${token}`
                }
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            showBudgetsError(
                "Budget could not be deleted.");

            return;
        }

        await loadBudgets();
    }
    catch {
        showBudgetsError(
            "Could not connect to the server.");
    }
}

function updateCreateMonthVisibility() {
    const period =
        Number(
            document.getElementById(
                "budgetPeriod")
                ?.value);

    const monthGroup =
        document.getElementById(
            "budgetMonthGroup");

    const monthSelect =
        document.getElementById(
            "budgetMonth");

    if (!monthGroup ||
        !monthSelect) {

        return;
    }

    if (period === 2) {
        monthGroup.classList.add(
            "d-none");

        monthSelect.value = "";
    }
    else {
        monthGroup.classList.remove(
            "d-none");
    }
}

function updateEditMonthVisibility() {
    const period =
        Number(
            document.getElementById(
                "editBudgetPeriod")
                ?.value);

    const monthGroup =
        document.getElementById(
            "editBudgetMonthGroup");

    const monthSelect =
        document.getElementById(
            "editBudgetMonth");

    if (!monthGroup ||
        !monthSelect) {

        return;
    }

    if (period === 2) {
        monthGroup.classList.add(
            "d-none");

        monthSelect.value = "";
    }
    else {
        monthGroup.classList.remove(
            "d-none");
    }
}

function setDefaultBudgetValues() {
    const today =
        new Date();

    const year =
        today.getFullYear();

    const month =
        today.getMonth() + 1;

    const createYear =
        document.getElementById(
            "budgetYear");

    const createMonth =
        document.getElementById(
            "budgetMonth");

    if (createYear) {
        createYear.value =
            year;
    }

    if (createMonth) {
        createMonth.value =
            month;
    }
}

function resetCreateBudgetForm() {
    const form =
        document.getElementById(
            "createBudgetForm");

    form?.reset();

    const message =
        document.getElementById(
            "createBudgetMessage");

    if (message) {
        message.textContent = "";
    }

    setDefaultBudgetValues();
    updateCreateMonthVisibility();
}

function isBudgetPeriodCurrentOrFuture(
    period,
    month,
    year) {

    const now =
        new Date();

    const currentYear =
        now.getFullYear();

    const currentMonth =
        now.getMonth() + 1;

    if (period === 1) {
        if (!month) {
            return false;
        }

        if (year < currentYear) {
            return false;
        }

        if (year === currentYear &&
            month < currentMonth) {

            return false;
        }

        return true;
    }

    if (period === 2) {
        return year >= currentYear;
    }

    return false;
}

function getMonthName(month) {
    const date =
        new Date(
            2026,
            month - 1,
            1);

    return date.toLocaleString(
        "en-US",
        {
            month: "long"
        });
}

function showBudgetsError(messageText) {
    const message =
        document.getElementById(
            "budgetsMessage");

    if (!message) {
        return;
    }

    message.textContent =
        messageText;

    message.classList.remove(
        "d-none");
}