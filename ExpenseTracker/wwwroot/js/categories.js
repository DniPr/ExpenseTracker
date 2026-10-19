document.addEventListener("DOMContentLoaded", async () => {
    if (!isAuthenticated()) {
        window.location.href = "/Account/Login";
        return;
    }

    await loadCategories();

    const createForm =
        document.getElementById("createCategoryForm");

    const editForm =
        document.getElementById("editCategoryForm");

    createForm?.addEventListener(
        "submit",
        createCategory);

    editForm?.addEventListener(
        "submit",
        updateCategory);
});

let categories = [];

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
            showCategoriesError(
                "Could not load categories.");

            return;
        }

        categories =
            await response.json();

        renderCategories();
    }
    catch {
        showCategoriesError(
            "Could not connect to the server.");
    }
}

function renderCategories() {
    const incomeContainer =
        document.getElementById(
            "incomeCategoriesContainer");

    const expenseContainer =
        document.getElementById(
            "expenseCategoriesContainer");

    const incomeCategories =
        categories.filter(
            category => category.type === 1);

    const expenseCategories =
        categories.filter(
            category => category.type === 2);

    renderCategoryList(
        incomeContainer,
        incomeCategories);

    renderCategoryList(
        expenseContainer,
        expenseCategories);
}

function renderCategoryList(
    container,
    categoryList) {

    if (!container) {
        return;
    }

    if (!categoryList ||
        categoryList.length === 0) {

        container.innerHTML =
            `<p class="text-muted mb-0">
                No categories found.
            </p>`;

        return;
    }

    container.innerHTML =
        categoryList
            .map(category => {

                const badge =
                    category.isDefault
                        ? `
                            <span class="badge bg-secondary">
                                Default
                            </span>
                        `
                        : `
                            <span class="badge bg-primary">
                                Personal
                            </span>
                        `;

                const actions =
                    category.isDefault
                        ? ""
                        : `
                            <div class="d-flex gap-2">

                                <button type="button"
                                        class="btn
                                               btn-sm
                                               btn-outline-primary"
                                        onclick="openEditCategory(
                                            ${category.id}
                                        )">
                                    Edit
                                </button>

                                <button type="button"
                                        class="btn
                                               btn-sm
                                               btn-outline-danger"
                                        onclick="deleteCategory(
                                            ${category.id}
                                        )">
                                    Delete
                                </button>

                            </div>
                        `;

                return `
                    <div class="border-bottom py-3">

                        <div class="d-flex
                                    justify-content-between
                                    align-items-start
                                    gap-3">

                            <div>

                                <div class="d-flex
                                            align-items-center
                                            gap-2
                                            mb-1">

                                    <h5 class="mb-0">
                                        ${escapeHtml(
                    category.name)}
                                    </h5>

                                    ${badge}

                                </div>

                                ${category.description
                        ? `
                                            <div class="text-muted small">
                                                ${escapeHtml(
                            category.description)}
                                            </div>
                                        `
                        : ""
                    }

                                ${category.icon
                        ? `
                                            <div class="small mt-1">
                                                Icon:
                                                <code>
                                                    ${escapeHtml(
                            category.icon)}
                                                </code>
                                            </div>
                                        `
                        : ""
                    }

                            </div>

                            ${actions}

                        </div>

                    </div>
                `;
            })
            .join("");
}

async function createCategory(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const name =
        document.getElementById(
            "categoryName")
            .value
            .trim();

    const description =
        document.getElementById(
            "categoryDescription")
            .value
            .trim();

    const icon =
        document.getElementById(
            "categoryIcon")
            .value
            .trim();

    const type =
        Number(
            document.getElementById(
                "categoryType")
                .value);

    const message =
        document.getElementById(
            "createCategoryMessage");

    message.textContent = "";

    if (!name || !type) {
        message.textContent =
            "Please fill in all required fields.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl("/api/categories"),
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
                    icon:
                        icon || null,
                    type
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Category could not be created.";

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

        resetCreateCategoryForm();

        const modalElement =
            document.getElementById(
                "createCategoryModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadCategories();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function openEditCategory(id) {
    const token =
        getAuthToken();

    const message =
        document.getElementById(
            "editCategoryMessage");

    if (message) {
        message.textContent = "";
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/categories/${id}`),
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
            showCategoriesError(
                "Could not load category.");

            return;
        }

        const category =
            await response.json();

        if (category.isDefault) {
            showCategoriesError(
                "Default categories cannot be edited.");

            return;
        }

        document.getElementById(
            "editCategoryId").value =
            category.id;

        document.getElementById(
            "editCategoryName").value =
            category.name;

        document.getElementById(
            "editCategoryDescription").value =
            category.description ?? "";

        document.getElementById(
            "editCategoryIcon").value =
            category.icon ?? "";

        document.getElementById(
            "editCategoryType").value =
            category.type;

        const modalElement =
            document.getElementById(
                "editCategoryModal");

        const modal =
            new bootstrap.Modal(
                modalElement);

        modal.show();
    }
    catch {
        showCategoriesError(
            "Could not connect to the server.");
    }
}

async function updateCategory(event) {
    event.preventDefault();

    const token =
        getAuthToken();

    const id =
        Number(
            document.getElementById(
                "editCategoryId")
                .value);

    const name =
        document.getElementById(
            "editCategoryName")
            .value
            .trim();

    const description =
        document.getElementById(
            "editCategoryDescription")
            .value
            .trim();

    const icon =
        document.getElementById(
            "editCategoryIcon")
            .value
            .trim();

    const type =
        Number(
            document.getElementById(
                "editCategoryType")
                .value);

    const message =
        document.getElementById(
            "editCategoryMessage");

    message.textContent = "";

    if (!id ||
        !name ||
        !type) {

        message.textContent =
            "Please fill in all required fields.";

        return;
    }

    try {
        const response = await fetch(
            getApiUrl(
                `/api/categories/${id}`),
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
                    icon:
                        icon || null,
                    type
                })
            });

        if (response.status === 401) {
            logout();
            return;
        }

        if (!response.ok) {
            let errorMessage =
                "Category could not be updated.";

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
                "editCategoryModal");

        const modal =
            bootstrap.Modal.getInstance(
                modalElement);

        modal?.hide();

        await loadCategories();
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function deleteCategory(id) {
    const confirmed =
        confirm(
            "Are you sure you want to delete this category?");

    if (!confirmed) {
        return;
    }

    const token =
        getAuthToken();

    try {
        const response = await fetch(
            getApiUrl(
                `/api/categories/${id}`),
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
            let errorMessage =
                "Category could not be deleted.";

            try {
                const data =
                    await response.json();

                errorMessage =
                    data.message ??
                    errorMessage;
            }
            catch {
            }

            showCategoriesError(
                errorMessage);

            return;
        }

        await loadCategories();
    }
    catch {
        showCategoriesError(
            "Could not connect to the server.");
    }
}

function resetCreateCategoryForm() {
    const form =
        document.getElementById(
            "createCategoryForm");

    form?.reset();

    const message =
        document.getElementById(
            "createCategoryMessage");

    if (message) {
        message.textContent = "";
    }
}

function showCategoriesError(messageText) {
    const message =
        document.getElementById(
            "categoriesMessage");

    if (!message) {
        return;
    }

    message.textContent =
        messageText;

    message.classList.remove(
        "d-none");
}