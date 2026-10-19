document.addEventListener("DOMContentLoaded", async () => {
    if (!isAuthenticated()) {
        window.location.href = "/Account/Login";
        return;
    }

    await loadCategories();
    await loadRecurringTransactions();

    const createForm =
        document.getElementById("createRecurringForm");

    const editForm =
        document.getElementById("editRecurringForm");

    const createType =
        document.getElementById("recurringType");

    const editType =
        document.getElementById("editRecurringType");

    createForm?.addEventListener(
        "submit",
        createRecurringTransaction);

    editForm?.addEventListener(
        "submit",
        updateRecurringTransaction);

    createType?.addEventListener(
        "change",
        filterCreateCategories);

    editType?.addEventListener(
        "change",
        () => filterEditCategories());

    setDefaultStartDate();
});

let allCategories = [];

async function loadCategories() {
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
            showRecurringError(
                "Could not load categories.");

            return;
        }

        allCategories =
            await response.json();

        filterCreateCategories();
    }
    catch {
        showRecurringError(
            "Could not connect to the server.");
    }
}

function filterCreateCategories() {
    const typeSelect =
        document.getElementById(
            "recurringType");

    const categorySelect =
        document.getElementById(
            "recurringCategory");

    if (!typeSelect ||
        !categorySelect) {

        return;
    }

    const selectedType =
        Number(typeSelect.value);

    categorySelect.innerHTML =
        `<option value="">
            Select category
        </option>`;

    if (!selectedType) {
        return;
    }

    allCategories
        .filter(category =>
            category.type === selectedType)
        .forEach(category => {
            const option =
                document.createElement("option");

            option.value =
                category.id;

            option.textContent =
                category.name;

            categorySelect.appendChild(option);
        });
}

function filterEditCategories(
    selectedCategoryId = null) {

    const typeSelect =
        document.getElementById(
            "editRecurringType");

    const categorySelect =
        document.getElementById(
            "editRecurringCategory");

    if (!typeSelect ||
        !categorySelect) {

        return;
    }

    const selectedType =
        Number(typeSelect.value);

    categorySelect.innerHTML = "";

    allCategories
        .filter(category =>
            category.type === selectedType)
        .forEach(category => {
            const option =
                document.createElement("option");

            option.value =
                category.id;

            option.textContent =
                category.name;

            if (selectedCategoryId &&
                category.id ===
                Number(selectedCategoryId)) {

                option.selected = true;
            }

            categorySelect.appendChild(option);
        });
}

async function loadRecurringTransactions() {
    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl(
                "/api/recurring-transactions"),
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
            showRecurringError(
                "Could not load recurring transactions.");

            return;
        }

        const recurringTransactions =
            await response.json();

        renderRecurringTransactions(
            recurringTransactions);
    }
    catch {
        showRecurringError(
            "Could not connect to the server.");
    }
}

function renderRecurringTransactions(
    recurringTransactions) {

    const container =
        document.getElementById(
            "recurringContainer");

    if (!container) {
        return;
    }

    if (!recurringTransactions ||
        recurringTransactions.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No recurring transactions found.
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

                const typeText =
                    isIncome
                        ? "Income"
                        : "Expense";

                const typeBadge =
                    isIncome
                        ? "bg-success"
                        : "bg-danger";

                const intervalText =
                    getIntervalName(
                        transaction.interval);

                const activeBadge =
                    transaction.isActive
                        ? `
                            <span class="badge bg-success">
                                Active
                            </span>
                        `
                        : `
                            <span class="badge bg-secondary">
                                Inactive
                            </span>
                        `;

                const endDateText =
                    transaction.endDate
                        ? formatDate(
                            transaction.endDate)
                        : "No end date";

                return `
                    <div class="border-bottom py-3">

                        <div class="d-flex
                                    justify-content-between
                                    align-items-start
                                    gap-3">

                            <div>

                                <div class="d-flex
                                            align-items-center
                                            flex-wrap
                                            gap-2
                                            mb-2">

                                    <h5 class="mb-0">
                                        ${escapeHtml(
                    transaction.description)}
                                    </h5>

                                    <span class="badge ${typeBadge}">
                                        ${typeText}
                                    </span>

                                    <span class="badge bg-info text-dark">
                                        ${intervalText}
                                    </span>

                                    ${activeBadge}

                                </div>

                                <div class="text-muted small mb-1">
                                    Category:
                                    ${escapeHtml(
                        transaction.categoryName)}
                                </div>

                                <div class="text-muted small mb-1">
                                    Starts:
                                    ${formatDate(
                            transaction.startDate)}
                                </div>

                                <div class="text-muted small mb-1">
                                    Ends:
                                    ${endDateText}
                                </div>

                                <div class="text-muted small">
                                    Next execution:
                                    <strong>
                                        ${formatDate(
                                transaction.nextExecutionDate)}
                                    </strong>
                                </div>

                            </div>

                            <div class="text-end">

                                <div class="fw-bold
                                            fs-5
                                            ${amountClass}
                                            mb-2">
                                    ${isIncome ? "+" : "-"}
                                    ${formatMoney(
                                    transaction.amount)}
                                </div>

                                <div class="d-flex
                                            gap-2
                                            justify-content-end">

                                    <button type="button"
                                            class="btn
                                                   btn-sm
                                                   btn-outline-primary"
                                            onclick="openEditRecurringTransaction(
                                                ${transaction.id}
                                            )">
                                        Edit
                                    </button>

                                    <button type="button"
                                            class="btn
                                                   btn-sm
                                                   btn-outline-danger"
                                            onclick="deleteRecurringTransaction(
                                                ${transaction.id}
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

async function createRecurringTransaction(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const description =
        document.getElementById(
            "recurringDescription")
            .value
            .trim();

    const amount =
        Number(
            document.getElementById(
                "recurringAmount")
                .value);

    const type =
        Number(
            document.getElementById(
                "recurringType")
                .value);

    const categoryId =
        Number(
            document.getElementById(
                "recurringCategory")
                .value);

    const startDate =
        document.getElementById(
            "recurringStartDate")
            .value;

    const endDateValue =
        document.getElementById(
            "recurringEndDate")
            .value;

    const interval =
        Number(
            document.getElementById(
                "recurringInterval")
                .value);

    const message =
        document.getElementById(
            "createRecurringMessage");

    message.textContent = "";

    if (!description ||
        !amount ||
        !type ||
        !categoryId ||
        !startDate ||
        !interval) {

        message.textContent =
            "Please fill in all required fields.";

        return;
    }

    if (endDateValue &&
        endDateValue < startDate) {

        message.textContent =
            "End date cannot be before start date.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl(
                "/api/recurring-transactions"),
            {
                method: "POST",
                headers: {
                    "Content-Type":
                        "application/json",

                    "Authorization":
                        `Bearer ${token}`
                },
                body: JSON.stringify({
                    description,
                    amount,
                    type,
                    startDate,
                    endDate:
                        endDateValue || null,
                    interval,
                    categoryId
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Recurring transaction could not be created.";

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

        resetCreateRecurringForm();

        const modalElement =
            document.getElementById(
                "createRecurringModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadRecurringTransactions();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function openEditRecurringTransaction(id) {
    const token =
        getAuthToken();

    const message =
        document.getElementById(
            "editRecurringMessage");

    if (message) {
        message.textContent = "";
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/recurring-transactions/${id}`),
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
            showRecurringError(
                "Could not load recurring transaction.");

            return;
        }

        const transaction =
            await response.json();

        document.getElementById(
            "editRecurringId").value =
            transaction.id;

        document.getElementById(
            "editRecurringDescription").value =
            transaction.description;

        document.getElementById(
            "editRecurringAmount").value =
            transaction.amount;

        document.getElementById(
            "editRecurringType").value =
            transaction.type;

        document.getElementById(
            "editRecurringStartDate").value =
            toDateInputValue(
                transaction.startDate);

        document.getElementById(
            "editRecurringEndDate").value =
            transaction.endDate
                ? toDateInputValue(
                    transaction.endDate)
                : "";

        document.getElementById(
            "editRecurringInterval").value =
            transaction.interval;

        document.getElementById(
            "editRecurringIsActive").checked =
            transaction.isActive;

        filterEditCategories(
            transaction.categoryId);

        const modalElement =
            document.getElementById(
                "editRecurringModal");

        const modal =
            new bootstrap.Modal(
                modalElement);

        modal.show();
    }
    catch {
        showRecurringError(
            "Could not connect to the server.");
    }
}

async function updateRecurringTransaction(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const id =
        Number(
            document.getElementById(
                "editRecurringId")
                .value);

    const description =
        document.getElementById(
            "editRecurringDescription")
            .value
            .trim();

    const amount =
        Number(
            document.getElementById(
                "editRecurringAmount")
                .value);

    const type =
        Number(
            document.getElementById(
                "editRecurringType")
                .value);

    const categoryId =
        Number(
            document.getElementById(
                "editRecurringCategory")
                .value);

    const startDate =
        document.getElementById(
            "editRecurringStartDate")
            .value;

    const endDateValue =
        document.getElementById(
            "editRecurringEndDate")
            .value;

    const interval =
        Number(
            document.getElementById(
                "editRecurringInterval")
                .value);

    const isActive =
        document.getElementById(
            "editRecurringIsActive")
            .checked;

    const message =
        document.getElementById(
            "editRecurringMessage");

    message.textContent = "";

    if (!id ||
        !description ||
        !amount ||
        !type ||
        !categoryId ||
        !startDate ||
        !interval) {

        message.textContent =
            "Please fill in all required fields.";

        return;
    }

    if (endDateValue &&
        endDateValue < startDate) {

        message.textContent =
            "End date cannot be before start date.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/recurring-transactions/${id}`),
            {
                method: "PUT",
                headers: {
                    "Content-Type":
                        "application/json",

                    "Authorization":
                        `Bearer ${token}`
                },
                body: JSON.stringify({
                    description,
                    amount,
                    type,
                    startDate,
                    endDate:
                        endDateValue || null,
                    interval,
                    isActive,
                    categoryId
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Recurring transaction could not be updated.";

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
                "editRecurringModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadRecurringTransactions();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function deleteRecurringTransaction(id) {
    const confirmed =
        confirm(
            "Are you sure you want to delete this recurring transaction?");

    if (!confirmed) {
        return;
    }

    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl(
                `/api/recurring-transactions/${id}`),
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
            showRecurringError(
                "Recurring transaction could not be deleted.");

            return;
        }

        await loadRecurringTransactions();
    }
    catch {
        showRecurringError(
            "Could not connect to the server.");
    }
}

function resetCreateRecurringForm() {
    const form =
        document.getElementById(
            "createRecurringForm");

    form?.reset();

    const message =
        document.getElementById(
            "createRecurringMessage");

    if (message) {
        message.textContent = "";
    }

    filterCreateCategories();
    setDefaultStartDate();
}

function setDefaultStartDate() {
    const startDateInput =
        document.getElementById(
            "recurringStartDate");

    if (!startDateInput) {
        return;
    }

    startDateInput.value =
        getTodayInputValue();
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

function showRecurringError(messageText) {
    const message =
        document.getElementById(
            "recurringMessage");

    if (!message) {
        return;
    }

    message.textContent =
        messageText;

    message.classList.remove(
        "d-none");
}