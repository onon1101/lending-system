export const API_BASE_URL = import.meta.env.VITE_APP_API_URL + "/api" || "http://192.168.2.110:8000/api";

export async function getActiveLoans(userId) {
    if (!userId) {
        throw new Error('User ID cannot be empty')
    }

    const response = await fetch (`${API_BASE_URL}/users/${userId}/loans`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        }
    })

    if (response.status === 404)
    {
        return [];
    }

    if (!response.ok)
    {
        const errorData = await response.json().catch(() => ({message: 'Server error'}));
        throw new Error(errorData.error || response.statusText);
    }

    return response.json();
}

/**
 * 查詢所有物品列表
 * @returns {Promise<Array<object>>} - 回傳所有物品列表
 */
export async function getAllItems() {
    const response = await fetch(`${API_BASE_URL}/items`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    });

    console.log(API_BASE_URL)

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Server error' }));
        throw new Error(errorData.error || response.statusText);
    }

    return response.json();
}

export async function createUser(userData) {
    const response = await fetch(`${API_BASE_URL}/users`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(userData),
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Server error' }));
        throw new Error(errorData.error || response.statusText);
    }

    return response.json();
}

export async function UploadItemImage(itemId, file) {
    const formData = new FormData();
    formData.append('image', file); 
    
    // 假設後端有一個專門的路由來處理圖片上傳
    const response = await fetch(`${API_BASE_URL}/items/${itemId}/image`, {
        method: 'POST',
        // IMPORTANT: 讓瀏覽器自動設定 'multipart/form-data'
        body: formData, 
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Upload failed' }));
        throw new Error(errorData.error || response.statusText);
    }

    return response.json(); // 預期返回 { message: "...", path: "/uploads/..." }
}

// 這是更新物品的函式 (已經在 Item CRUD 中定義過)
export async function UpdateItem(itemId, itemData) {
    // 假設後端 API 路由是 /api/items/{object_id} PUT
    const response = await fetch(`${API_BASE_URL}/items/${itemId}`, { 
        method: 'PUT',
        headers: { 'Content-Type': 'application/json' },
        body: JSON.stringify(itemData),
    });
    // ... (錯誤處理邏輯)
    if (!response.ok) {
         const errorData = await response.json().catch(() => ({ message: 'Server error' }));
        throw new Error(errorData.error || response.statusText);
    }
    return response.json();
}

/**
 * 創建新的物品實體 (POST /api/items)
 * @param {object} itemData - 包含 object_name 和 description 的物品數據
 * @returns {Promise<object>} - 回傳新建立的物品物件
 */
export async function CreateItem(itemData) {
    const response = await fetch(`${API_BASE_URL}/items`, {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
        },
        body: JSON.stringify(itemData),
    });

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Server error' }));
        throw new Error(errorData.error || response.statusText);
    }

    return response.json();
}

// src/stores/api.js (新增)

/**
 * 查詢特定物品 (GET /api/items/{id})
 * @param {number} itemId - 物品 ID
 * @returns {Promise<object>} - 回傳物品物件
 */
export async function GetItemByID(itemId) {
    if (!itemId) {
        throw new Error('Item ID cannot be empty');
    }
    
    const response = await fetch(`${API_BASE_URL}/items/${itemId}`, {
        method: 'GET',
        headers: {
            'Content-Type': 'application/json',
        },
    });
    
    if (response.status === 404) {
        throw new Error(`Item ID ${itemId} not found.`);
    }

    if (!response.ok) {
        const errorData = await response.json().catch(() => ({ message: 'Server error' }));
        throw new Error(errorData.error || response.statusText);
    }

    return response.json();
}