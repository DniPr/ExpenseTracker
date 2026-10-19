document.addEventListener("DOMContentLoaded", async () => {
    if (!isAuthenticated()) {
        window.location.href = "/Account/Login";
        return;
    }

    await loadFinancialGoals();

    const createForm =
        document.getElementById("createGoalForm");

    const editForm =
        document.getElementById("editGoalForm");

    createForm?.addEventListener(
        "submit",
        createFinancialGoal);

    editForm?.addEventListener(
        "submit",
        updateFinancialGoal);
});

async function loadFinancialGoals() {
    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl("/api/financial-goals"),
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
            showGoalsError(
                "Could not load financial goals.");

            return;
        }

        const goals =
            await response.json();

        renderFinancialGoals(
            goals);
    }
    catch {
        showGoalsError(
            "Could not connect to the server.");
    }
}

function renderFinancialGoals(goals) {
    const container =
        document.getElementById(
            "goalsContainer");

    if (!container) {
        return;
    }

    if (!goals ||
        goals.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No financial goals found.
            </p>`;

        return;
    }

    container.innerHTML =
        goals
            .map(goal => {

                const percentage =
                    calculateGoalPercentage(
                        goal.currentAmount,
                        goal.targetAmount);

                const statusInfo =
                    getStatusInfo(
                        goal.status);

                const remaining =
                    Math.max(
                        0,
                        goal.targetAmount -
                        goal.currentAmount);

                const targetDate =
                    goal.targetDate
                        ? formatDate(
                            goal.targetDate)
                        : "No target date";

                return `
                    <div class="border-bottom py-4">

                        <div class="d-flex
                                    justify-content-between
                                    align-items-start
                                    gap-3">

                            <div class="flex-grow-1">

                                <div class="d-flex
                                            align-items-center
                                            flex-wrap
                                            gap-2
                                            mb-2">

                                    <h5 class="mb-0">
                                        ${escapeHtml(
                    goal.name)}
                                    </h5>

                                    <span class="badge ${statusInfo.badgeClass}">
                                        ${statusInfo.text}
                                    </span>

                                </div>

                                ${goal.description
                        ? `
                                            <p class="text-muted mb-2">
                                                ${escapeHtml(
                            goal.description)}
                                            </p>
                                        `
                        : ""
                    }

                                <div class="row g-2 mb-3">

                                    <div class="col-md-4">
                                        <div class="small text-muted">
                                            Current
                                        </div>

                                        <strong>
                                            ${formatMoney(
                        goal.currentAmount)}
                                        </strong>
                                    </div>

                                    <div class="col-md-4">
                                        <div class="small text-muted">
                                            Target
                                        </div>

                                        <strong>
                                            ${formatMoney(
                            goal.targetAmount)}
                                        </strong>
                                    </div>

                                    <div class="col-md-4">
                                        <div class="small text-muted">
                                            Remaining
                                        </div>

                                        <strong>
                                            ${formatMoney(
                                remaining)}
                                        </strong>
                                    </div>

                                </div>

                                <div class="progress mb-2"
                                     style="height: 20px;">

                                    <div class="progress-bar"
                                         role="progressbar"
                                         style="width: ${percentage}%;"
                                         aria-valuenow="${percentage}"
                                         aria-valuemin="0"
                                         aria-valuemax="100">

                                        ${percentage.toFixed(0)}%

                                    </div>

                                </div>

                                <div class="small text-muted">
                                    Target date:
                                    ${targetDate}
                                </div>

                            </div>

                            <div class="d-flex gap-2">

                                <button type="button"
                                        class="btn
                                               btn-sm
                                               btn-outline-primary"
                                        onclick="openEditFinancialGoal(
                                            ${goal.id}
                                        )">
                                    Edit
                                </button>

                                <button type="button"
                                        class="btn
                                               btn-sm
                                               btn-outline-danger"
                                        onclick="deleteFinancialGoal(
                                            ${goal.id}
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

async function createFinancialGoal(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const name =
        document.getElementById(
            "goalName")
            .value
            .trim();

    const description =
        document.getElementById(
            "goalDescription")
            .value
            .trim();

    const targetAmount =
        Number(
            document.getElementById(
                "goalTargetAmount")
                .value);

    const currentAmount =
        Number(
            document.getElementById(
                "goalCurrentAmount")
                .value);

    const targetDateValue =
        document.getElementById(
            "goalTargetDate")
            .value;

    const message =
        document.getElementById(
            "createGoalMessage");

    message.textContent = "";

    if (!name ||
        !targetAmount ||
        currentAmount < 0) {

        message.textContent =
            "Please fill in all required fields correctly.";

        return;
    }

    if (targetDateValue &&
        isDateInPast(
            targetDateValue)) {

        message.textContent =
            "Target date cannot be in the past.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl(
                "/api/financial-goals"),
            {
                method: "POST",
                headers: {
                    "Content-Type":
                        "application/json",

                    "Authorization":
                        `Bearer ${token}`
                },
                body: JSON.stringify({
                    name,
                    description:
                        description || null,
                    targetAmount,
                    currentAmount,
                    targetDate:
                        targetDateValue || null
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Financial goal could not be created.";

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

        resetCreateGoalForm();

        const modalElement =
            document.getElementById(
                "createGoalModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadFinancialGoals();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function openEditFinancialGoal(id) {
    const token =
        getAuthToken();

    const message =
        document.getElementById(
            "editGoalMessage");

    if (message) {
        message.textContent = "";
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/financial-goals/${id}`),
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
            showGoalsError(
                "Could not load financial goal.");

            return;
        }

        const goal =
            await response.json();

        document.getElementById(
            "editGoalId").value =
            goal.id;

        document.getElementById(
            "editGoalName").value =
            goal.name;

        document.getElementById(
            "editGoalDescription").value =
            goal.description ?? "";

        document.getElementById(
            "editGoalTargetAmount").value =
            goal.targetAmount;

        document.getElementById(
            "editGoalCurrentAmount").value =
            goal.currentAmount;

        document.getElementById(
            "editGoalTargetDate").value =
            goal.targetDate
                ? toDateInputValue(
                    goal.targetDate)
                : "";

        document.getElementById(
            "editGoalStatus").value =
            goal.status;

        const modalElement =
            document.getElementById(
                "editGoalModal");

        const modal =
            new bootstrap.Modal(
                modalElement);

        modal.show();
    }
    catch {
        showGoalsError(
            "Could not connect to the server.");
    }
}

async function updateFinancialGoal(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const id =
        Number(
            document.getElementById(
                "editGoalId")
                .value);

    const name =
        document.getElementById(
            "editGoalName")
            .value
            .trim();

    const description =
        document.getElementById(
            "editGoalDescription")
            .value
            .trim();

    const targetAmount =
        Number(
            document.getElementById(
                "editGoalTargetAmount")
                .value);

    const currentAmount =
        Number(
            document.getElementById(
                "editGoalCurrentAmount")
                .value);

    const targetDateValue =
        document.getElementById(
            "editGoalTargetDate")
            .value;

    const status =
        Number(
            document.getElementById(
                "editGoalStatus")
                .value);

    const message =
        document.getElementById(
            "editGoalMessage");

    message.textContent = "";

    if (!id ||
        !name ||
        !targetAmount ||
        currentAmount < 0 ||
        !status) {

        message.textContent =
            "Please fill in all required fields correctly.";

        return;
    }

    if (targetDateValue &&
        isDateInPast(
            targetDateValue)) {

        message.textContent =
            "Target date cannot be in the past.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/financial-goals/${id}`),
            {
                method: "PUT",
                headers: {
                    "Content-Type":
                        "application/json",

                    "Authorization":
                        `Bearer ${token}`
                },
                body: JSON.stringify({
                    name,
                    description:
                        description || null,
                    targetAmount,
                    currentAmount,
                    targetDate:
                        targetDateValue || null,
                    status
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Financial goal could not be updated.";

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
                "editGoalModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadFinancialGoals();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function deleteFinancialGoal(id) {
    const confirmed =
        confirm(
            "Are you sure you want to delete this financial goal?");

    if (!confirmed) {
        return;
    }

    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl(
                `/api/financial-goals/${id}`),
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
            showGoalsError(
                "Financial goal could not be deleted.");

            return;
        }

        await loadFinancialGoals();
    }
    catch {
        showGoalsError(
            "Could not connect to the server.");
    }
}

function calculateGoalPercentage(
    currentAmount,
    targetAmount) {

    if (!targetAmount ||
        targetAmount <= 0) {

        return 0;
    }

    const percentage =
        (currentAmount /
            targetAmount) * 100;

    return Math.min(
        100,
        Math.max(
            0,
            percentage));
}

function getStatusInfo(status) {
    switch (status) {
        case 1:
            return {
                text: "Active",
                badgeClass: "bg-primary"
            };

        case 2:
            return {
                text: "Completed",
                badgeClass: "bg-success"
            };

        case 3:
            return {
                text: "Cancelled",
                badgeClass: "bg-secondary"
            };

        default:
            return {
                text: "Unknown",
                badgeClass: "bg-dark"
            };
    }
}

function resetCreateGoalForm() {
    const form =
        document.getElementById(
            "createGoalForm");

    form?.reset();

    const currentAmount =
        document.getElementById(
            "goalCurrentAmount");

    if (currentAmount) {
        currentAmount.value = "0";
    }

    const message =
        document.getElementById(
            "createGoalMessage");

    if (message) {
        message.textContent = "";
    }
}

function isDateInPast(value) {
    if (!value) {
        return false;
    }

    const selectedDate =
        new Date(
            `${value}T00:00:00`);

    const today =
        new Date();

    today.setHours(
        0,
        0,
        0,
        0);

    return selectedDate < today;
}

function showGoalsError(messageText) {
    const message =
        document.getElementById(
            "goalsMessage");

    if (!message) {
        return;
    }

    message.textContent =
        messageText;

    message.classList.remove(
        "d-none");
}