<script>
    import ItemOverview from './components/ItemOverview.svelte';
    import UserCreation from './routes/UserCreation.svelte';
    import UserLoans from './routes/UserLoans.svelte';
    // --- 新增 ItemManagement 元件 ---
    // import ItemManagement from './routes/ItemManagement.svelte'; 

    // 使用 Map 來儲存視圖和元件的映射
    const views = {
        'home': ItemOverview,
        'creation': UserCreation,
        'loans': UserLoans,
        // 'item_manage': ItemManagement, // <-- 註冊 Item 管理視圖
    }
    
    let currentView = 'home';
    let currentItemId = null; // <-- 新增：用於儲存當前正在編輯的 Item ID

    // 處理 ItemOverview 傳遞的導航事件
    function handleNavigate(event) {
        const { view, id } = event.detail;
        currentView = view;
        currentItemId = id; // <-- 將 Item ID 儲存起來
        console.log(`切換到 ${view}, ID: ${id}`);
    }

    // 導航欄點擊處理器：重設 Item ID
    function navigateTo(view) {
        currentView = view;
        // 如果切換到非 Item 管理頁面，清除 Item ID
        if (view !== 'item_manage') {
            currentItemId = null;
        }
    }
</script>

<main>
    <h1>物品借閱管理系統</h1>

    <nav>
        <button 
            on:click={() => navigateTo('home')} 
            class:active={currentView === 'home'}
        >
            🏠 系統概覽
        </button>
        <button 
            on:click={() => navigateTo('creation')} 
            class:active={currentView === 'creation'}
        >
            👤 創建使用者
        </button>
        <button 
            on:click={() => navigateTo('loans')} 
            class:active={currentView === 'loans'}
        >
            📖 查詢借閱記錄
        </button>
        <button 
            on:click={() => navigateTo('item_manage')} 
            class:active={currentView === 'item_manage'}
        >
            🛠️ 物品管理
        </button>
    </nav>
    
    <div class="content">
        <svelte:component 
            this={views[currentView]} 
            on:navigate={handleNavigate}      itemId={currentItemId}            /> 
    </div>
</main>

<style>
    /* 這裡定義應用程式的主題樣式 */
    :global(body) {
        background-color: #1a1a1a; 
        color: #e0e0e0;
        font-family: Arial, sans-serif;
        margin: 0;
        padding: 0;
    }
    
    main {
        max-width: 1200px; /* 增加最大寬度 */
        margin: 0 auto;
        padding: 20px;
    }

    /* 導航欄樣式保持不變或優化 */
    nav {
        margin-bottom: 20px;
        border-bottom: 2px solid #333;
        padding-bottom: 10px;
        display: flex;
        gap: 10px;
    }
    nav button {
        padding: 10px 15px;
        cursor: pointer;
        border: none;
        background-color: #333; /* 按鈕背景 */
        color: #fff;
        border-radius: 4px;
        transition: background-color 0.2s, color 0.2s;
    }
    nav button.active {
        background-color: #007bff;
        color: white;
    }
    
    .content {
        min-height: 500px;
        padding: 20px;
        background-color: #1f1f1f; /* 內容區塊的背景 */
        border-radius: 8px;
    }
</style>