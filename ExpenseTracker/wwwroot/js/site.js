document.addEventListener("DOMContentLoaded", () => {
    const loginNavItem =
        document.getElementById("loginNavItem");

    const registerNavItem =
        document.getElementById("registerNavItem");

    const userNavItem =
        document.getElementById("userNavItem");

    const logoutNavItem =
        document.getElementById("logoutNavItem");

    const dashboardNavItem =
        document.getElementById("dashboardNavItem");

    const transactionsNavItem =
        document.getElementById("transactionsNavItem");

    const categoriesNavItem =
        document.getElementById("categoriesNavItem");

    const budgetsNavItem =
        document.getElementById("budgetsNavItem");

    const recurringNavItem =
        document.getElementById("recurringNavItem");

    const goalsNavItem =
        document.getElementById("goalsNavItem");

    const currentUserName =
        document.getElementById("currentUserName");

    const logoutButton =
        document.getElementById("logoutButton");

    const authenticated =
        isAuthenticated();

    if (authenticated) {
        const user =
            getCurrentUser();

        loginNavItem?.classList.add("d-none");
        registerNavItem?.classList.add("d-none");

        userNavItem?.classList.remove("d-none");
        logoutNavItem?.classList.remove("d-none");

        dashboardNavItem?.classList.remove("d-none");
        transactionsNavItem?.classList.remove("d-none");
        categoriesNavItem?.classList.remove("d-none");
        budgetsNavItem?.classList.remove("d-none");
        recurringNavItem?.classList.remove("d-none");
        goalsNavItem?.classList.remove("d-none");

        if (user && currentUserName) {
            currentUserName.textContent =
                `${user.firstName} ${user.lastName}`;
        }
    }
    else {
        loginNavItem?.classList.remove("d-none");
        registerNavItem?.classList.remove("d-none");

        userNavItem?.classList.add("d-none");
        logoutNavItem?.classList.add("d-none");

        dashboardNavItem?.classList.add("d-none");
        transactionsNavItem?.classList.add("d-none");
        categoriesNavItem?.classList.add("d-none");
        budgetsNavItem?.classList.add("d-none");
        recurringNavItem?.classList.add("d-none");
        goalsNavItem?.classList.add("d-none");

        if (currentUserName) {
            currentUserName.textContent = "";
        }
    }

    logoutButton?.addEventListener(
        "click",
        logout);
});