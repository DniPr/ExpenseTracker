document.addEventListener("DOMContentLoaded", () => {
    const loginForm = document.getElementById("loginForm");
    const registerForm = document.getElementById("registerForm");

    loginForm?.addEventListener("submit", handleLogin);
    registerForm?.addEventListener("submit", handleRegister);
});

async function handleLogin(event) {
    event.preventDefault();

    const email = document.getElementById("loginEmail").value.trim();
    const password = document.getElementById("loginPassword").value;
    const message = document.getElementById("loginMessage");

    message.textContent = "";

    if (!email || !password) {
        message.textContent = "Please enter email and password.";
        return;
    }

    try {
        const response = await fetch(
            getApiUrl("/api/auth/login"),
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

        if (!response.ok) {
            let errorMessage = "Invalid email or password.";

            try {
                const data = await response.json();
                errorMessage = data.message ?? errorMessage;
            }
            catch {
            }

            message.textContent = errorMessage;
            return;
        }

        const data = await response.json();

        saveAuthenticationData(data);

        window.location.href = "/";
    }
    catch {
        message.textContent = "Could not connect to the server.";
    }
}

async function handleRegister(event) {
    event.preventDefault();

    const firstName = document.getElementById("registerFirstName").value.trim();
    const lastName = document.getElementById("registerLastName").value.trim();
    const email = document.getElementById("registerEmail").value.trim();
    const password = document.getElementById("registerPassword").value;
    const confirmPassword = document.getElementById("registerConfirmPassword").value;
    const message = document.getElementById("registerMessage");

    message.textContent = "";

    if (!firstName || !lastName || !email || !password || !confirmPassword) {
        message.textContent = "Please fill in all required fields.";
        return;
    }

    if (password !== confirmPassword) {
        message.textContent = "Passwords do not match.";
        return;
    }

    try {
        const response = await fetch(
            getApiUrl("/api/auth/register"),
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

        if (!response.ok) {
            let errorMessage = "Registration failed.";

            try {
                const data = await response.json();
                errorMessage = data.message ?? errorMessage;
            }
            catch {
            }

            message.textContent = errorMessage;
            return;
        }

        const data = await response.json();

        saveAuthenticationData(data);

        window.location.href = "/";
    }
    catch {
        message.textContent = "Could not connect to the server.";
    }
}

function saveAuthenticationData(data) {
    localStorage.setItem("expenseTrackerToken", data.token);

    localStorage.setItem(
        "expenseTrackerUser",
        JSON.stringify({
            userId: data.userId,
            email: data.email,
            firstName: data.firstName,
            lastName: data.lastName,
            expiresAt: data.expiresAt
        }));
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
            clearAuthenticationData();
            return false;
        }

        const expiresAt = new Date(user.expiresAt);

        if (Number.isNaN(expiresAt.getTime())) {
            clearAuthenticationData();
            return false;
        }

        if (expiresAt <= new Date()) {
            clearAuthenticationData();
            return false;
        }

        return true;
    }
    catch {
        clearAuthenticationData();
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

function getAuthToken() {
    return localStorage.getItem("expenseTrackerToken");
}

function clearAuthenticationData() {
    localStorage.removeItem("expenseTrackerToken");
    localStorage.removeItem("expenseTrackerUser");
}