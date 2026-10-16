document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");

    if (loginForm) {
        loginForm.addEventListener("submit", handleLogin);
    }

    if (registerForm) {
        registerForm.addEventListener("submit", handleRegister);
    }
});

async function handleLogin(event) {
    event.preventDefault();

    const email = document.getElementById("loginEmail").value.trim();
    const password = document.getElementById("loginPassword").value;
    const message = document.getElementById("loginMessage");

    message.textContent = "";

    try {
        const response = await fetch(
            "https://localhost:7063/api/auth/login",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    email,
                    password
                })
            });

        const data = await response.json();

        if (!response.ok) {
            message.textContent =
                data.message ?? "Login failed.";

            return;
        }

        localStorage.setItem(
            "expenseTrackerToken",
            data.token);

        localStorage.setItem(
            "expenseTrackerUser",
            JSON.stringify({
                userId: data.userId,
                email: data.email,
                firstName: data.firstName,
                lastName: data.lastName,
                expiresAt: data.expiresAt
            }));

        window.location.href = "/";
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}

async function handleRegister(event) {
    event.preventDefault();

    const firstName =
        document.getElementById("registerFirstName")
            .value
            .trim();

    const lastName =
        document.getElementById("registerLastName")
            .value
            .trim();

    const email =
        document.getElementById("registerEmail")
            .value
            .trim();

    const password =
        document.getElementById("registerPassword")
            .value;

    const confirmPassword =
        document.getElementById("registerConfirmPassword")
            .value;

    const message =
        document.getElementById("registerMessage");

    message.textContent = "";

    if (password !== confirmPassword) {
        message.textContent =
            "Passwords do not match.";

        return;
    }

    try {
        const response = await fetch(
            "https://localhost:7063/api/auth/register",
            {
                method: "POST",
                headers: {
                    "Content-Type": "application/json"
                },
                body: JSON.stringify({
                    firstName,
                    lastName,
                    email,
                    password,
                    confirmPassword
                })
            });

        const data = await response.json();

        if (!response.ok) {
            message.textContent =
                data.message ?? "Registration failed.";

            return;
        }

        localStorage.setItem(
            "expenseTrackerToken",
            data.token);

        localStorage.setItem(
            "expenseTrackerUser",
            JSON.stringify({
                userId: data.userId,
                email: data.email,
                firstName: data.firstName,
                lastName: data.lastName,
                expiresAt: data.expiresAt
            }));

        window.location.href = "/";
    }
    catch {
        message.textContent =
            "Could not connect to the server.";
    }
}
function logout() {
    localStorage.removeItem("expenseTrackerToken");
    localStorage.removeItem("expenseTrackerUser");

    window.location.href = "/Account/Login";
}

function isAuthenticated() {
    const token = localStorage.getItem("expenseTrackerToken");
    const userJson = localStorage.getItem("expenseTrackerUser");

    if (!token || !userJson) {
        return false;
    }

    try {
        const user = JSON.parse(userJson);

        if (!user.expiresAt) {
            return false;
        }

        const expiresAt = new Date(user.expiresAt);
        const now = new Date();

        if (expiresAt <= now) {
            localStorage.removeItem("expenseTrackerToken");
            localStorage.removeItem("expenseTrackerUser");

            return false;
        }

        return true;
    }
    catch {
        return false;
    }
}

function getCurrentUser() {
    const userJson = localStorage.getItem("expenseTrackerUser");

    if (!userJson) {
        return null;
    }

    try {
        return JSON.parse(userJson);
    }
    catch {
        return null;
    }
}