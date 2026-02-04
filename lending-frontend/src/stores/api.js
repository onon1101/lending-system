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
    if (!path) return "/default-placeholder.png";
    // 如果路徑已經是完整網址，直接回傳
    if (path.startsWith('http')) return path;
    // 否則拼接 MinIO Endpoint
    return `${MINIO_ENDPOINT}${path}`;
}

export async function getItemMedia(itemId) {
    try {
        const response = await fetch(`${API_BASE_URL}/items/media/${itemId}`);
        if (!response.ok) return [];
        return response.json(); 
    }
    catch(e)
    {
        console.error(e);
    }
}

// lending-frontend/src/stores/api.js

// ... 保留原本的 API_BASE_URL 和其他函數

/**
 * 上傳物品媒體檔案 (支援進度追蹤)
 * @param {File} file 檔案物件
 * @param {string|number} objectId 物品 ID
 * @param {string} description 描述
 * @param {function} onProgress 進度回呼函數 (percent) => {}
 * @returns {Promise}
 */
export function uploadItemMedia(file, objectId, description, onProgress) {
    return new Promise((resolve, reject) => {
        const formData = new FormData();
        formData.append("file", file);
        formData.append("object_id", String(objectId));
        formData.append("description", description);

        const xhr = new XMLHttpRequest();

        // 監聽進度
        if (onProgress && xhr.upload) {
            xhr.upload.onprogress = (event) => {
                if (event.lengthComputable) {
                    const percent = Math.round((event.loaded / event.total) * 100);
                    onProgress(percent);
                }
            };
        }

        xhr.onload = () => {
            if (xhr.status >= 200 && xhr.status < 300) {
                try {
                    const response = JSON.parse(xhr.responseText);
                    resolve(response);
                } catch (e) {
                    resolve(xhr.responseText);
                }
            } else {
                reject(new Error(xhr.responseText || "上傳失敗"));
            }
        };

        xhr.onerror = () => reject(new Error("網路錯誤"));
        
        // 使用定義好的 API_BASE_URL
        xhr.open("POST", `${API_BASE_URL}/items/media`);
        xhr.send(formData);
    });
}