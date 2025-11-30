<script>
    import { API_BASE_URL, getAllItems } from '../stores/api';
    import { createEventDispatcher, onMount } from 'svelte';

    let items = [];
    let loading = true;
    let error = null;

    const dispatch = createEventDispatcher();

    onMount(async () => {
        try {
            items = await getAllItems();
        } catch (e) {
            error = e.message;
        } finally {
            loading = false;
        }
    });

    // 計算物品狀態的總結
    $: totalItems = items.length;
    $: availableItems = items.filter(item => item.current_status === 'Available').length;
    $: onLoanItems = totalItems - availableItems;

    /** 根據狀態獲取顏色 */
    function getStatusColor(status) {
        if (status === 'Available') return 'var(--color-success)';
        if (status === 'On Loan') return 'var(--color-warning)';
        return 'var(--color-danger)';
    }

    function navigateToItem(itemId) {
        dispatch('navigate', { view: 'item_manage', id: itemId})
    }
</script>

<div class="overview-container">
    <h2>📦 物品庫存概覽</h2>

    {#if loading}
        <p class="loading">正在載入物品數據...</p>
    {:else if error}
        <p class="message error">
            載入失敗: {error}
            {#if error.includes('NetworkError') || error.includes('Failed to fetch')}
                <br>請檢查後端服務 (API) 是否運行。
            {/if}
        </p>
    {:else}
        <div class="stats-cards">
            <div class="card total">
                <h3>總物品數</h3>
                <p>{totalItems}</p>
            </div>
            <div class="card available">
                <h3>可借出</h3>
                <p>{availableItems}</p>
            </div>
            <div class="card on-loan">
                <h3>借出中</h3>
                <p>{onLoanItems}</p>
            </div>
        </div>

        <h3>所有物品清單</h3>
        <ul class="item-list">
        {#each items as item (item.object_id)}
            <button class="item-card-button" on:click={() => navigateToItem(item.object_id)}>
                
                {#if item.image_url}
                {
                    console.log(item.image_url)
                }
                    <img src="{item.image_url.replace("localhost:9000", "192.168.2.110:9000")}"
                     alt="{item.object_name}"
                    class="item-image"/>
                {:else}
                    <div class="image-placeholder no-image">暫無圖片</div>
                {/if}

                <div class="item-details">
                    <div class="item-header">
                        <strong>{item.object_name} (ID: {item.object_id})</strong>
                        <span class="status-badge" style="background-color: {getStatusColor(item.current_status)};">
                            {item.current_status}
                        </span>
                    </div>
                    <p>{item.description || '無描述'}</p>
                </div>
            </button>
        {:else}
            <p class="info">目前系統中沒有物品。</p>
        {/each}
    </ul>
    {/if}
</div>
<style>
    /* 定義顏色變數 (與您的深色主題匹配) */
    .overview-container {
        color: #e0e0e0;
        --color-primary: #007bff;
        --color-success: #28a745;
        --color-warning: #ffc107;
        --color-danger: #dc3545;
        padding: 20px 0;
    }

    /* 狀態概覽卡片 */
    .stats-cards {
        display: flex;
        gap: 20px;
        margin-bottom: 30px;
    }
    .card {
        flex: 1;
        background-color: #2a2a2a;
        padding: 15px;
        border-radius: 8px;
        text-align: center;
        border-left: 5px solid var(--border-color);
    }
    .total { --border-color: var(--color-primary); }
    .available { --border-color: var(--color-success); }
    .on-loan { --border-color: var(--color-warning); }

    .card h3 { color: #fff; margin-top: 0; font-size: 1.1em; }
    .card p { font-size: 2em; font-weight: bold; margin: 5px 0 0; }
    
    /* 物品清單 */
    .item-list {
        list-style: none;
        padding: 0;
        display: grid;
        grid-template-columns: repeat(auto-fit, minmax(300px, 1fr));
        gap: 20px; /* 增加間距 */
    }


    /* 圖片區域 */
    .item-image {
        width: 100%;
        height: 150px; 
        object-fit: cover;
        /* 移除上圓角，因為已經在卡片層級設定 */
    }

    .image-placeholder.no-image {
        background-color: #333;
        color: #999;
        height: 150px; /* 與圖片高度一致 */
        display: flex;
        align-items: center;
        justify-content: center;
        font-size: 1.2em;
    }
    
    .item-details {
        padding: 15px; /* 確保內容有內邊距 */
    }

    .item-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 5px;
    }
    .status-badge {
        padding: 4px 8px;
        border-radius: 12px;
        font-size: 0.8em;
        color: #1a1a1a;
        font-weight: bold;
    }

    /* 錯誤訊息優化 */
    .message.error {
        background-color: #330000;
        padding: 15px;
        border: 1px solid var(--color-danger);
        border-radius: 4px;
        color: #ff6b6b;
    }
    .info {
        color: #a0a0a0;
        text-align: center;
    }
</style>