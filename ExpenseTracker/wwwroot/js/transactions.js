document.addEventListener("DOMContentLoaded", async () => {
    if (!isAuthenticated()) {
        window.location.href = "/Account/Login";
        return;
    }

    await loadCategories();
    await loadTransactions();

    const typeFilter =
        document.getElementById("transactionTypeFilter");

    const categoryFilter =
        document.getElementById("transactionCategoryFilter");

    const searchInput =
        document.getElementById("transactionSearch");

    const clearFiltersButton =
        document.getElementById("clearTransactionFilters");

    const createForm =
        document.getElementById("createTransactionForm");

    const transactionType =
        document.getElementById("transactionType");

    const editForm =
        document.getElementById("editTransactionForm");

    const editTransactionType =
        document.getElementById("editTransactionType");

    typeFilter?.addEventListener(
        "change",
        loadTransactions);

    categoryFilter?.addEventListener(
        "change",
        loadTransactions);

    searchInput?.addEventListener(
        "input",
        debounce(loadTransactions, 300));

    clearFiltersButton?.addEventListener(
        "click",
        async () => {
            typeFilter.value = "";
            categoryFilter.value = "";
            searchInput.value = "";

            await loadTransactions();
        });

    transactionType?.addEventListener(
        "change",
        filterCreateCategories);

    editTransactionType?.addEventListener(
        "change",
        () => filterEditCategories());

    createForm?.addEventListener(
        "submit",
        createTransaction);

    editForm?.addEventListener(
        "submit",
        updateTransaction);

    setDefaultTransactionDate();
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
                    "Authorization": `Bearer ${token}`
                }
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            showTransactionsError(
                "Could not load categories.");

            return;
        }

        allCategories =
            await response.json();

        populateCategoryFilter();
        filterCreateCategories();
    }
    catch {
        showTransactionsError(
            "Could not connect to the server.");
    }
}

function populateCategoryFilter() {
    const categoryFilter =
        document.getElementById(
            "transactionCategoryFilter");

    if (!categoryFilter) {
        return;
    }

    categoryFilter.innerHTML =
        `<option value="">
            All Categories
        </option>`;

    allCategories.forEach(category => {
        const option =
            document.createElement("option");

        option.value =
            category.id;

        option.textContent =
            category.name;

        categoryFilter.appendChild(option);
    });
}

function filterCreateCategories() {
    const typeSelect =
        document.getElementById(
            "transactionType");

    const categorySelect =
        document.getElementById(
            "transactionCategory");

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
            "editTransactionType");

    const categorySelect =
        document.getElementById(
            "editTransactionCategory");

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

async function loadTransactions() {
    const token =
        getAuthToken();

    const type =
        document.getElementById(
            "transactionTypeFilter")
            ?.value;

    const categoryId =
        document.getElementById(
            "transactionCategoryFilter")
            ?.value;

    const searchTerm =
        document.getElementById(
            "transactionSearch")
            ?.value
            .trim();

    const queryParams =
        new URLSearchParams();

    if (type) {
        queryParams.append(
            "type",
            type);
    }

    if (categoryId) {
        queryParams.append(
            "categoryId",
            categoryId);
    }

    if (searchTerm) {
        queryParams.append(
            "searchTerm",
            searchTerm);
    }

    const queryString =
        queryParams.toString();

    const path =
        queryString
            ? `/api/transactions?${queryString}`
            : "/api/transactions";

    try {
        const response = await fetch(
            getApiUrl(path),
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
            showTransactionsError(
                "Could not load transactions.");

            return;
        }

        const transactions =
            await response.json();

        renderTransactions(
            transactions);
    }
    catch {
        showTransactionsError(
            "Could not connect to the server.");
    }
}

function renderTransactions(
    transactions) {

    const container =
        document.getElementById(
            "transactionsContainer");

    if (!container) {
        return;
    }

    if (!transactions ||
        transactions.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No transactions found.
            </p>`;

        return;
    }

    container.innerHTML =
        transactions
            .map(transaction => {

                const isIncome =
                    transaction.type === 1;

                const sign =
                    isIncome
                        ? "+"
                        : "-";

                const amountClass =
                    isIncome
                        ? "text-success"
                        : "text-danger";

                const typeText =
                    isIncome
                        ? "Income"
                        : "Expense";

                return `
                    <div class="d-flex
                                justify-content-between
                                align-items-center
                                border-bottom
                                py-3">

                        <div>

                            <div class="d-flex
                                        align-items-center
                                        gap-2">

                                <h5 class="mb-0">
                                    ${escapeHtml(
                    transaction.description)}
                                </h5>

                                <span class="badge
                                    ${isIncome
                        ? "bg-success"
                        : "bg-danger"}">
                                    ${typeText}
                                </span>

                            </div>

                            <div class="text-muted small mt-1">
                                ${escapeHtml(
                            transaction.categoryName)}
                                ·
                                ${formatDate(
                                transaction.date)}
                            </div>

                            ${transaction.notes
                        ? `
                                        <div class="small mt-1">
                                            ${escapeHtml(
                            transaction.notes)}
                                        </div>
                                    `
                        : ""
                    }

                        </div>

                        <div class="text-end">

                            <div class="fw-bold
                                        fs-5
                                        ${amountClass}">
                                ${sign}
                                ${formatMoney(
                        transaction.amount)}
                            </div>

                            <div class="mt-2
                                        d-flex
                                        gap-2
                                        justify-content-end">

                                <button type="button"
                                        class="btn
                                               btn-sm
                                               btn-outline-primary"
                                        onclick="openEditTransaction(
                                            ${transaction.id}
                                        )">
                                    Edit
                                </button>

                                <button type="button"
                                        class="btn
                                               btn-sm
                                               btn-outline-danger"
                                        onclick="deleteTransaction(
                                            ${transaction.id}
                                        )">
                                    Delete
                                </button>

                            </div>

                        </div>

                    </div>
                `;
            })
            .join("");
}

async function openEditTransaction(id) {
    const token =
        getAuthToken();

    const message =
        document.getElementById(
            "editTransactionMessage");

    if (message) {
        message.textContent = "";
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/transactions/${id}`),
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
            showTransactionsError(
                "Could not load transaction.");

            return;
        }

        const transaction =
            await response.json();

        document.getElementById(
            "editTransactionId").value =
            transaction.id;

        document.getElementById(
            "editTransactionDescription").value =
            transaction.description;

        document.getElementById(
            "editTransactionAmount").value =
            transaction.amount;

        document.getElementById(
            "editTransactionDate").value =
            toDateInputValue(
                transaction.date);

        document.getElementById(
            "editTransactionType").value =
            transaction.type;

        document.getElementById(
            "editTransactionNotes").value =
            transaction.notes ?? "";

        filterEditCategories(
            transaction.categoryId);

        const modalElement =
            document.getElementById(
                "editTransactionModal");

        const modal =
            new bootstrap.Modal(
                modalElement);

        modal.show();
    }
    catch {
        showTransactionsError(
            "Could not connect to the server.");
    }
}

async function updateTransaction(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const id =
        Number(
            document.getElementById(
                "editTransactionId")
                .value);

    const description =
        document.getElementById(
            "editTransactionDescription")
            .value
            .trim();

    const amount =
        Number(
            document.getElementById(
                "editTransactionAmount")
                .value);

    const date =
        document.getElementById(
            "editTransactionDate")
            .value;

    const type =
        Number(
            document.getElementById(
                "editTransactionType")
                .value);

    const categoryId =
        Number(
            document.getElementById(
                "editTransactionCategory")
                .value);

    const notes =
        document.getElementById(
            "editTransactionNotes")
            .value
            .trim();

    const message =
        document.getElementById(
            "editTransactionMessage");

    message.textContent = "";

    if (!id ||
        !description ||
        !amount ||
        !date ||
        !type ||
        !categoryId) {

        message.textContent =
            "Please fill in all required fields.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/transactions/${id}`),
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
                    date,
                    type,
                    notes:
                        notes || null,
                    categoryId
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Transaction could not be updated.";

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
                "editTransactionModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadTransactions();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function createTransaction(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const description =
        document.getElementById(
            "transactionDescription")
            .value
            .trim();

    const amount =
        Number(
            document.getElementById(
                "transactionAmount")
                .value);

    const date =
        document.getElementById(
            "transactionDate")
            .value;

    const type =
        Number(
            document.getElementById(
                "transactionType")
                .value);

    const categoryId =
        Number(
            document.getElementById(
                "transactionCategory")
                .value);

    const notes =
        document.getElementById(
            "transactionNotes")
            .value
            .trim();

    const message =
        document.getElementById(
            "createTransactionMessage");

    message.textContent = "";

    if (!description ||
        !amount ||
        !date ||
        !type ||
        !categoryId) {

        message.textContent =
            "Please fill in all required fields.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl(
                "/api/transactions"),
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
                    date,
                    type,
                    notes:
                        notes || null,
                    categoryId
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Transaction could not be created.";

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

        resetCreateTransactionForm();

        const modalElement =
            document.getElementById(
                "createTransactionModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadTransactions();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function deleteTransaction(id) {
    const confirmed =
        confirm(
            "Are you sure you want to delete this transaction?");

    if (!confirmed) {
        return;
    }

    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl(
                `/api/transactions/${id}`),
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
            showTransactionsError(
                "Transaction could not be deleted.");

            return;
        }

        await loadTransactions();
    }
    catch {
        showTransactionsError(
            "Could not connect to the server.");
    }
}

function resetCreateTransactionForm() {
    const form =
        document.getElementById(
            "createTransactionForm");

    form?.reset();

    const message =
        document.getElementById(
            "createTransactionMessage");

    if (message) {
        message.textContent = "";
    }

    filterCreateCategories();
    setDefaultTransactionDate();
}

function setDefaultTransactionDate() {
    const dateInput =
        document.getElementById(
            "transactionDate");

    if (!dateInput) {
        return;
    }

    dateInput.value =
        getTodayInputValue();
}

function showTransactionsError(messageText) {
    const message =
        document.getElementById(
            "transactionsMessage");

    if (!message) {
        return;
    }

    message.textContent =
        messageText;

    message.classList.remove(
        "d-none");
}