const API_BASE_URL = "https://localhost:7063";

function getApiUrl(path) {
    if (!path) {
        return API_BASE_URL;
    }

    if (!path.startsWith("/")) {
        path = `/${path}`;
    }

    return `${API_BASE_URL}${path}`;
}