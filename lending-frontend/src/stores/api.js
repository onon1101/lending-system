// lending-frontend/src/stores/api.js
export const API_BASE_URL = 
import.meta.env.VITE_APP_API_URL + "/api" || 
"http://192.168.2.110:8000/api";

const MINIO_ENDPOINT = "https://lending-minio.onon1101.org"

async function handleResponse(response) {
    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: '伺服器錯誤' }));
        throw new Error(errorData.error || response.statusText);
    }
    return response.json();
}

/** 查詢所有物品 */
export async function getAllItems() {
    return fetch(`${API_BASE_URL}/items`).then(handleResponse);
}

/** 查詢特定物品詳細資訊 */
export async function GetItemByID(itemId) {
    return fetch(`${API_BASE_URL}/items/${itemId}`).then(handleResponse);
}

/** 查詢特定物品的借閱歷史紀錄 */
export async function GetLoanHistoryByItemID(itemId) {
    const res = await fetch(`${API_BASE_URL}/loans/items/history/${itemId}`);
    if (res.status === 404) return [];
    return handleResponse(res);
}

/** 創建使用者 */
export async function createUser(userData) {
    return fetch(`${API_BASE_URL}/users`, {
        method: 'POST',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(userData),
    }).then(handleResponse);
}

/** 查詢使用者目前借閱中的物品 */
export async function getActiveLoans(userId) {
    return fetch(`${API_BASE_URL}/users/${userId}/loans`).then(handleResponse);
}


export function getFullImageUrl(path) {
    console.log(path);
    if (!path) return "/default-placeholder.png";
    // 如果路徑已經是完整網址，直接回傳
    if (path.startsWith('http')) return path;
    // 否則拼接 MinIO Endpoint
    return `${MINIO_ENDPOINT}${path}`;
}

export async function getItemMedia(itemId) {
    const response = await fetch(`${API_BASE_URL}/items/media/${itemId}`);
    if (!response.ok) return [];
    if (response.json() === null) return [];
    return response.json();
}