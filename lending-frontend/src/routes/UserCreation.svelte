<script>
    // 確保路徑指向 src/stores/api.js
    import { createUser } from '../stores/api'; 

    // 表單數據
    let name = '';
    let email = '';
    let passwordHash = ''; // 應對應後端 model.CreateUserRequest 中的 password_hash
    let loading = false;
    let successMessage = '';
    let errorMessage = '';

    async function handleSubmit() {
        loading = true;
        successMessage = '';
        errorMessage = '';

        // 檢查基本輸入
        if (!name || !email || !passwordHash) {
            errorMessage = '所有欄位都必須填寫。';
            loading = false;
            return;
        }

        try {
            // 構建傳送給 Go 後端 API 的數據
            const userData = {
                name: name,
                email: email,
                password_hash: passwordHash, // 確保鍵名與後端 Go 結構體標籤一致
            };
            
            const newUser = await createUser(userData);
            
            successMessage = `使用者 ${newUser.name} (ID: ${newUser.user_id}) 創建成功! (狀態: ${newUser.status})`;
            
            // 清空表單
            name = '';
            email = '';
            passwordHash = '';

        } catch (error) {
            console.error('創建失敗:', error);
            // 嘗試解析錯誤訊息，如果後端傳回 JSON 錯誤
            errorMessage = `創建使用者失敗: ${error.message}`;
        } finally {
            loading = false;
        }
    }
</script>

<div class="user-creation-container">
    <h2>👤 創建新使用者</h2>

    <form on:submit|preventDefault={handleSubmit}>
        <label>
            姓名 (Name):
            <input type="text" bind:value={name} required />
        </label>
        
        <label>
            電子郵件 (Email):
            <input type="email" bind:value={email} required />
        </label>
        
        <label>
            密碼 Hash (Password Hash):
            <input type="password" bind:value={passwordHash} required />
        </label>
        
        <button type="submit" disabled={loading}>
            {loading ? '創建中...' : '創建使用者'}
        </button>
    </form>

    {#if errorMessage}
        <p class="message error">錯誤: {errorMessage}</p>
    {/if}

    {#if successMessage}
        <p class="message success">{successMessage}</p>
    {/if}
</div>

<style>
    .user-creation-container {
        max-width: 400px;
        margin: 0 auto; /* 移除頂部 margin，讓 App.svelte 控制 */
        padding: 20px;
        border-radius: 8px;
        background-color: #2a2a2a;
    }
    h2 {
        color: #fff;
    }
    label {
        display: block;
        margin-bottom: 15px;
        font-weight: bold;
        color: #e0e0e0;
    }
    input[type="text"], input[type="email"], input[type="password"] {
        width: 100%;
        padding: 10px;
        margin-top: 5px;
        box-sizing: border-box;
        border: 1px solid #555;
        border-radius: 4px;
        background-color: #333;
        color: #eee;
    }
    button {
        width: 100%;
        padding: 10px 15px;
        background-color: #007bff;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
        font-weight: bold;
        transition: background-color 0.2s;
    }
    button:hover:not(:disabled) {
        background-color: #0056b3;
    }
    button:disabled {
        background-color: #5a91d8;
        cursor: not-allowed;
    }
    .message {
        margin-top: 20px;
        padding: 10px;
        border-radius: 4px;
        font-weight: bold;
    }
    .error {
        background-color: #440000;
        color: #ff6b6b;
        border: 1px solid #dc3545;
    }
    .success {
        background-color: #004400;
        color: #8bc34a;
        border: 1px solid #28a745;
    }
</style>