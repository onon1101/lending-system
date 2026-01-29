// lending-frontend/src/stores/api.js
export const API_BASE_URL = "http://localhost:8000/api";

async function handleResponse(response) {
    console.log("hello");
    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: '伺服器錯誤' }));
        throw new Error(errorData.error || response.statusText);
    }
    return response.json();
}

/** 查詢所有物品 */
export async function getAllItems() {
    console.log(`${API_BASE_URL}/items`);
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