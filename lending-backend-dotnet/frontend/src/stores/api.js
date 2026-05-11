const configuredBase = import.meta.env.VITE_APP_API_URL || "";
const defaultBase = window.location.hostname === "localhost"
  ? "http://localhost:8000"
  : window.location.origin;

export const API_ROOT = (configuredBase || defaultBase).replace(/\/$/, "");
export const API_BASE_URL = `${API_ROOT}/api/v1`;

const TOKEN_KEY = "lending.accessToken";

export function getAccessToken() {
  return localStorage.getItem(TOKEN_KEY) || "";
}

export function saveAccessToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
}

export function clearAccessToken() {
  localStorage.removeItem(TOKEN_KEY);
}

function headers(extra = {}) {
  const token = getAccessToken();
  return {
    ...extra,
    ...(token ? { Authorization: `Bearer ${token}` } : {}),
  };
}

async function handleResponse(response) {
  if (response.status === 204) return null;

  const payload = await response.json().catch(() => null);
  if (!response.ok) {
    throw new Error(payload?.error || payload?.message || response.statusText || "伺服器錯誤");
  }

  return payload;
}

export function getFullImageUrl(path) {
  if (!path) return "";
  if (path.startsWith("http")) return path;
  return `${API_ROOT}${path}`;
}

export async function login(email, password) {
  const result = await fetch(`${API_BASE_URL}/auth/session`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ email, password }),
  }).then(handleResponse);

  saveAccessToken(result.access_token);
  return result;
}

export async function getAllItems() {
  return fetch(`${API_BASE_URL}/catalog/items`).then(handleResponse);
}

export async function getItem(itemId) {
  return fetch(`${API_BASE_URL}/catalog/items/${itemId}`).then(handleResponse);
}

export async function createItem(item) {
  return fetch(`${API_BASE_URL}/catalog/items`, {
    method: "POST",
    headers: headers({ "Content-Type": "application/json" }),
    body: JSON.stringify({
      object_name: item.objectName,
      description: item.description,
    }),
  }).then(handleResponse);
}

export async function updateItem(itemId, item) {
  return fetch(`${API_BASE_URL}/catalog/items/${itemId}`, {
    method: "PUT",
    headers: headers({ "Content-Type": "application/json" }),
    body: JSON.stringify({
      object_name: item.objectName,
      description: item.description,
      current_status: item.currentStatus,
      image_url: item.imageUrl,
    }),
  }).then(handleResponse);
}

export async function uploadItemImage(itemId, file) {
  const formData = new FormData();
  formData.append("file", file);

  return fetch(`${API_BASE_URL}/catalog/items/${itemId}/image`, {
    method: "POST",
    headers: headers(),
    body: formData,
  }).then(handleResponse);
}

export async function getItemMedia(itemId) {
  const response = await fetch(`${API_BASE_URL}/catalog/items/${itemId}/media`);
  if (response.status === 404) return [];
  return handleResponse(response);
}

export function uploadItemMedia(file, objectId, description, link = "", onProgress) {
  return new Promise((resolve, reject) => {
    const formData = new FormData();
    formData.append("file", file);
    formData.append("object_id", String(objectId));
    formData.append("description", description || "");
    formData.append("link", link || "");

    const xhr = new XMLHttpRequest();
    const token = getAccessToken();
    xhr.open("POST", `${API_BASE_URL}/catalog/items/media`);
    if (token) xhr.setRequestHeader("Authorization", `Bearer ${token}`);

    if (onProgress && xhr.upload) {
      xhr.upload.onprogress = (event) => {
        if (event.lengthComputable) {
          onProgress(Math.round((event.loaded / event.total) * 100));
        }
      };
    }

    xhr.onload = () => {
      if (xhr.status >= 200 && xhr.status < 300) {
        resolve(JSON.parse(xhr.responseText));
        return;
      }

      reject(new Error(xhr.responseText || "上傳失敗"));
    };
    xhr.onerror = () => reject(new Error("網路錯誤"));
    xhr.send(formData);
  });
}

export async function searchUserByName(name) {
  return fetch(`${API_BASE_URL}/users/search/${encodeURIComponent(name)}`).then(handleResponse);
}

export async function createUser(user) {
  return fetch(`${API_BASE_URL}/users`, {
    method: "POST",
    headers: headers({ "Content-Type": "application/json" }),
    body: JSON.stringify({
      name: user.name,
      email: user.email,
      password_hash: user.password,
    }),
  }).then(handleResponse);
}

export async function getActiveBorrowings(userId) {
  return fetch(`${API_BASE_URL}/users/${userId}/borrowings`).then(handleResponse);
}

export async function createBorrowing(userId, itemIds, durationHours) {
  return fetch(`${API_BASE_URL}/borrowings`, {
    method: "POST",
    headers: headers({ "Content-Type": "application/json" }),
    body: JSON.stringify({
      user_id: Number(userId),
      items_id: itemIds.map(Number),
      duration_hours: Number(durationHours),
    }),
  }).then(handleResponse);
}

export async function returnBorrowedItem(orderId, objectId) {
  return fetch(`${API_BASE_URL}/borrowings/${orderId}/items/${objectId}/return`, {
    method: "POST",
    headers: headers(),
  }).then(handleResponse);
}

export async function getBorrowingHistory(itemId) {
  const response = await fetch(`${API_BASE_URL}/catalog/items/${itemId}/borrowings/history`);
  if (response.status === 404) return [];
  return handleResponse(response);
}

export const GetItemByID = getItem;
export const GetLoanHistoryByItemID = getBorrowingHistory;
export const getActiveLoans = getActiveBorrowings;
