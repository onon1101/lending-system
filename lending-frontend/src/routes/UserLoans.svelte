<script>
    import { getActiveLoans } from '../stores/api';

    // 表單數據
    let userId = null;
    let loans = [];
    let loading = false;
    let errorMessage = '';
    let fetched = false; // 標記是否已經嘗試過查詢

    /** 格式化日期時間 */
    function formatTime(isoString) {
        if (!isoString || isoString === '0001-01-01T00:00:00Z') {
            return 'N/A';
        }
        return new Date(isoString).toLocaleString();
    }

    async function handleSearch() {
        // 確保輸入的是有效的 ID
        if (!userId || isNaN(parseInt(userId))) {
            errorMessage = '請輸入有效的使用者 ID。';
            loans = [];
            fetched = true;
            return;
        }

        loading = true;
        errorMessage = '';
        loans = [];
        fetched = false;

        try {
            const data = await getActiveLoans(parseInt(userId));
            loans = data;
            
        } catch (error) {
            console.error('查詢失敗:', error);
            errorMessage = `查詢借閱記錄失敗: ${error.message}`;
        } finally {
            loading = false;
            fetched = true;
        }
    }
</script>

<div class="loans-container">
    <h2>📖 借閱記錄查詢</h2>

    <form on:submit|preventDefault={handleSearch} class="search-form">
        <label>
            使用者 ID:
            <input type="number" bind:value={userId} required min="1" />
        </label>
        
        <button type="submit" disabled={loading}>
            {loading ? '查詢中...' : '查詢進行中的借閱'}
        </button>
    </form>

    {#if errorMessage}
        <p class="message error">錯誤: {errorMessage}</p>
    {:else if fetched}
        {#if loans.length === 0}
            <p class="message info">使用者 ID {userId} 目前沒有進行中的借閱記錄。</p>
        {:else}
            <h3>結果：找到 {loans.length} 筆進行中的訂單</h3>
            {#each loans as loan (loan.order_id)}
                <div class="loan-card">
                    <p><strong>訂單 ID:</strong> {loan.order_id} (狀態: {loan.order_status})</p>
                    <p><strong>應歸還時間:</strong> {formatTime(loan.end_time)}</p>

                    <h4>包含物品 ({loan.items.length} 項):</h4>
                    <table class="item-table">
                        <thead>
                            <tr>
                                <th>物品 ID</th>
                                <th>名稱</th>
                                <th>狀態</th>
                                <th>實際歸還</th>
                            </tr>
                        </thead>
                        <tbody>
                            {#each loan.items as item (item.object_id)}
                                <tr>
                                    <td>{item.object_id}</td>
                                    <td>{item.object_name}</td>
                                    <td>{item.detail_status}</td>
                                    <td>{formatTime(item.actual_return_time)}</td>
                                </tr>
                            {/each}
                        </tbody>
                    </table>
                </div>
            {/each}
        {/if}
    {/if}
</div>

<style>
    /* 確保所有文字和背景的對比度 */
    .loans-container {
        max-width: 800px;
        margin: 50px auto;
        padding: 20px;
        /* 將容器內所有文字預設為白色/淺灰色，以適應深色背景 */
        color: #e0e0e0; 
    }
    
    /* 修正輸入框和按鈕樣式，確保它們與整體主題協調 */
    .search-form {
        display: flex;
        gap: 10px;
        align-items: flex-end;
        margin-bottom: 20px;
    }
    
    input[type="number"] {
        padding: 8px;
        border: 1px solid #555; /* 深色主題下的邊框 */
        border-radius: 4px;
        background-color: #333; /* 輸入框背景色 */
        color: #eee; /* 輸入框文字顏色 */
    }
    
    /* 借閱卡片樣式修正 */
    .loan-card {
        border: 1px solid #444; /* 深色邊框 */
        padding: 15px;
        margin-bottom: 20px;
        border-radius: 6px;
        background-color: #2a2a2a; /* 卡片背景色 */
        box-shadow: 0 4px 8px rgba(0, 0, 0, 0.3);
    }
    
    /* 標題顏色修正 */
    h3, h4, p strong {
        color: #ffffff; /* 確保標題和粗體字是純白色 */
    }

    /* 表格樣式修正 */
    .item-table {
        width: 100%;
        border-collapse: collapse;
        margin-top: 10px;
    }
    
    .item-table th, .item-table td {
        border: 1px solid #555; /* 表格邊框使用淺色 */
        padding: 8px;
        text-align: left;
        color: #e0e0e0; /* 表格文字顏色 */
    }

    .item-table th {
        background-color: #383838; /* 表格頭部背景色 */
    }

    /* 錯誤和資訊訊息顏色 */
    .message.error { color: #ff6b6b; } /* 淺紅色 */
    .message.info { color: #6bbaff; }  /* 淺藍色 */

    /* 按鈕顏色可以與導航欄保持一致或自定義 */
    button {
        padding: 10px 15px;
        background-color: #007bff;
        color: white;
        border: none;
        border-radius: 4px;
        cursor: pointer;
    }
    button:disabled {
        background-color: #5a91d8;
    }
</style>