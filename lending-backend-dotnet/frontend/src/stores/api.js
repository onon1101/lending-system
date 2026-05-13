const configuredBase = import.meta.env.VITE_APP_API_URL || "";
const defaultBase = window.location.hostname === "localhost"
  ? "http://localhost:8000"
  : window.location.origin;

export const API_ROOT = (configuredBase || defaultBase).replace(/\/$/, "");
export const API_BASE_URL = `${API_ROOT}/api/v1`;
export const MEDIA_ROOT = (import.meta.env.VITE_APP_MEDIA_URL || "https://lending-minio.onon1101.org").replace(/\/$/, "");

const TOKEN_KEY = "lending.accessToken";
const hasOwn = (value, key) => Object.prototype.hasOwnProperty.call(value, key);

export function getAccessToken() {
  return localStorage.getItem(TOKEN_KEY) || "";
}

export function saveAccessToken(token) {
  if (token) localStorage.setItem(TOKEN_KEY, token);
}

export function clearAccessToken() {
  localStorage.removeItem(TOKEN_KEY);
}

function decodeJwtPayload(token) {
  if (!token) return null;

  const payload = token.split(".")[1];
  if (!payload) return null;

  try {
    const base64 = payload.replace(/-/g, "+").replace(/_/g, "/");
    const padded = base64.padEnd(Math.ceil(base64.length / 4) * 4, "=");
    const bytes = Uint8Array.from(window.atob(padded), (char) => char.charCodeAt(0));
    return JSON.parse(new TextDecoder().decode(bytes));
  } catch {
    return null;
  }
}

export function getCurrentUserFromToken(token = getAccessToken()) {
  const payload = decodeJwtPayload(token);
  if (!payload) return null;

  const roleClaim = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";
  const emailClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress";
  const idClaim = "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
  const userId = payload.id ?? payload.user_id ?? payload.sub ?? payload[idClaim];

  return {
    user_id: userId ? Number(userId) : null,
    email: payload.email ?? payload[emailClaim] ?? "",
    role: payload.role ?? payload[roleClaim] ?? "",
  };
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
    throw new Error(
      payload?.errorMessage ||
        payload?.message ||
        payload?.error ||
        response.statusText ||
        "伺服器錯誤",
    );
  }

  return normalizeApiPayload(unwrapApiResponse(payload));
}

function unwrapApiResponse(payload) {
  if (!payload || typeof payload !== "object") return payload;

  const hasEnvelope =
    hasOwn(payload, "Data") ||
    hasOwn(payload, "data") ||
    hasOwn(payload, "Issuccess") ||
    hasOwn(payload, "isSuccess");

  if (!hasEnvelope) return payload;

  const isSuccess = payload.Issuccess ?? payload.isSuccess ?? true;
  if (!isSuccess) {
    throw new Error(payload.errorMessage || payload.message || payload.error || "伺服器錯誤");
  }

  return payload.Data ?? payload.data ?? null;
}

function normalizeApiPayload(value) {
  if (Array.isArray(value)) return value.map(normalizeApiPayload);
  if (!value || typeof value !== "object") return value;

  const normalized = Object.fromEntries(
    Object.entries(value).map(([key, entry]) => [key, normalizeApiPayload(entry)]),
  );

  if (hasOwn(normalized, "item_id") && !hasOwn(normalized, "object_id")) {
    normalized.object_id = normalized.item_id;
  }

  if (hasOwn(normalized, "object_id") && !hasOwn(normalized, "item_id")) {
    normalized.item_id = normalized.object_id;
  }

  return normalized;
}

export function getFullImageUrl(path) {
  if (!path) return "";
  if (path.startsWith("http")) return path;
  if (path.startsWith("/api/")) return `${API_ROOT}${path}`;

  const normalizedPath = path.replace(/^\/+/, "");
  if (normalizedPath.startsWith("lending-images-production/")) {
    return `${MEDIA_ROOT}/${normalizedPath}`;
  }

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
  return fetch(`${API_BASE_URL}/catalog/items`)
    .then(handleResponse)
    .then((items) => (Array.isArray(items) ? items : []));
}

export async function getItemsByUserId(userId) {
  return fetch(`${API_BASE_URL}/catalog/items/user/${userId}`, {
    headers: headers(),
  })
    .then(handleResponse)
    .then((items) => (Array.isArray(items) ? items : []));
}

export async function getItemsByUserName(username) {
  return fetch(`${API_BASE_URL}/catalog/items/user/${encodeURIComponent(username)}`)
    .then(handleResponse)
    .then((items) => (Array.isArray(items) ? items : []));
}

export async function getItem(itemId) {
  return fetch(`${API_BASE_URL}/catalog/items/${itemId}`).then(handleResponse);
}

export async function createItem(item) {
  if (item.cover) {
    const formData = new FormData();
    formData.append("object_name", item.objectName);
    formData.append("maker", item.maker || "");
    formData.append("material", item.material || "");
    formData.append("description", item.description || "");
    formData.append("image", item.cover);

    return fetch(`${API_BASE_URL}/catalog/items/form`, {
      method: "POST",
      headers: headers(),
      body: formData,
    }).then(handleResponse);
  }

  return fetch(`${API_BASE_URL}/catalog/items`, {
    method: "POST",
    headers: headers({ "Content-Type": "application/json" }),
    body: JSON.stringify({
      object_name: item.objectName,
      maker: item.maker || "",
      material: item.material || "",
      description: item.description || "",
    }),
  }).then(handleResponse);
}

export async function updateItem(itemId, item) {
  return fetch(`${API_BASE_URL}/catalog/items/${itemId}`, {
    method: "PUT",
    headers: headers({ "Content-Type": "application/json" }),
    body: JSON.stringify({
      object_name: item.objectName,
      maker: item.maker,
      material: item.material,
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
  const media = await handleResponse(response);
  return Array.isArray(media) ? media : [];
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
        resolve(unwrapApiResponse(parseJson(xhr.responseText)));
        return;
      }

      const payload = parseJson(xhr.responseText);
      reject(new Error(payload?.errorMessage || payload?.message || payload?.error || "上傳失敗"));
    };
    xhr.onerror = () => reject(new Error("網路錯誤"));
    xhr.send(formData);
  });
}

function parseJson(text) {
  try {
    return JSON.parse(text || "null");
  } catch {
    return null;
  }
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
  return fetch(`${API_BASE_URL}/users/${userId}/borrowings`)
    .then(handleResponse)
    .then((borrowings) => (Array.isArray(borrowings) ? borrowings : []));
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
  const history = await handleResponse(response);
  return Array.isArray(history) ? history : [];
}

export async function createBorrowingRecord(record) {
  return fetch(`${API_BASE_URL}/management/borrowings`, {
    method: "POST",
    headers: headers({ "Content-Type": "application/json" }),
    body: JSON.stringify({
      user_id: Number(record.userId),
      borrower_name: record.borrowerName,
      item_id: Number(record.itemId),
      start_time: record.startTime,
      end_time: record.endTime,
    }),
  }).then(handleResponse);
}

export async function updateBorrowingRecordTime(orderId, record) {
  return fetch(`${API_BASE_URL}/management/borrowings/${orderId}/time`, {
    method: "PATCH",
    headers: headers({ "Content-Type": "application/json" }),
    body: JSON.stringify({
      user_id: Number(record.userId),
      start_time: record.startTime,
      end_time: record.endTime,
    }),
  }).then(handleResponse);
}

export async function deleteBorrowingRecord(orderId, userId) {
  return fetch(`${API_BASE_URL}/management/borrowings/${orderId}?user_id=${encodeURIComponent(userId)}`, {
    method: "DELETE",
    headers: headers(),
  }).then(handleResponse);
}

export const GetItemByID = getItem;
export const GetLoanHistoryByItemID = getBorrowingHistory;
export const getActiveLoans = getActiveBorrowings;
