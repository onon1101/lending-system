<script>
  import { onMount } from "svelte";
  import {
    clearAccessToken,
    createBorrowing,
    createItem,
    createUser,
    getAccessToken,
    getActiveBorrowings,
    getAllItems,
    login,
    returnBorrowedItem,
    searchUserByName,
  } from "../stores/api";

  let items = [];
  let activeBorrowings = [];
  let selectedItemIds = [];
  let loading = true;
  let message = "";
  let error = "";
  let token = getAccessToken();
  let userQuery = "";
  let selectedUser = null;
  let durationHours = 24;
  let loginForm = { email: "", password: "" };
  let userForm = { name: "", email: "", password: "" };
  let itemForm = { objectName: "", description: "" };

  $: availableItems = items.filter((item) => item.current_status === "Available");

  onMount(loadItems);

  async function run(action, successText = "") {
    error = "";
    message = "";
    try {
      const result = await action();
      message = successText;
      return result;
    } catch (err) {
      error = err.message || "操作失敗";
      throw err;
    }
  }

  async function loadItems() {
    loading = true;
    await run(async () => {
      items = await getAllItems();
    }).catch(() => {});
    loading = false;
  }

  async function handleLogin() {
    await run(async () => {
      const result = await login(loginForm.email, loginForm.password);
      token = result.access_token;
      loginForm = { email: "", password: "" };
    }, "已登入，可執行管理操作。");
  }

  function logout() {
    clearAccessToken();
    token = "";
    selectedUser = null;
    activeBorrowings = [];
    message = "已登出。";
  }

  async function handleUserSearch() {
    if (!userQuery.trim()) return;
    await run(async () => {
      selectedUser = await searchUserByName(userQuery.trim());
      activeBorrowings = await getActiveBorrowings(selectedUser.user_id);
    }, "已載入使用者借閱資料。");
  }

  async function handleCreateUser() {
    await run(async () => {
      selectedUser = await createUser(userForm);
      userQuery = selectedUser.name;
      userForm = { name: "", email: "", password: "" };
      activeBorrowings = [];
    }, "使用者已建立。");
  }

  async function handleCreateItem() {
    const created = await run(() => createItem(itemForm), "物品已新增。");
    itemForm = { objectName: "", description: "" };
    await loadItems();
    window.location.href = `/item.html?id=${created.object_id}`;
  }

  function toggleBorrowItem(itemId) {
    selectedItemIds = selectedItemIds.includes(itemId)
      ? selectedItemIds.filter((id) => id !== itemId)
      : [...selectedItemIds, itemId];
  }

  async function handleCreateBorrowing() {
    if (!selectedUser) {
      error = "請先查詢並選取使用者。";
      return;
    }

    if (selectedItemIds.length === 0) {
      error = "請至少選擇一件可借物品。";
      return;
    }

    await run(async () => {
      await createBorrowing(selectedUser.user_id, selectedItemIds, durationHours);
      selectedItemIds = [];
      activeBorrowings = await getActiveBorrowings(selectedUser.user_id);
      await loadItems();
    }, "借閱已建立。");
  }

  async function handleReturn(orderId, objectId) {
    await run(async () => {
      await returnBorrowedItem(orderId, objectId);
      activeBorrowings = selectedUser ? await getActiveBorrowings(selectedUser.user_id) : [];
      await loadItems();
    }, "物品已歸還。");
  }

  function formatDate(value) {
    if (!value) return "N/A";
    return new Date(value).toLocaleString("zh-TW", {
      month: "2-digit",
      day: "2-digit",
      hour: "2-digit",
      minute: "2-digit",
    });
  }
</script>

<main class="admin-page">
  <header class="topbar">
    <a class="brand" href="/">
      <span class="brand-mark">LS</span>
      <span>物品借閱系統</span>
    </a>
    <a class="subtle-link" href="/">返回首頁</a>
  </header>

  <section class="admin-hero">
    <div>
      <p class="eyebrow">Admin Console</p>
      <h1>管理登入</h1>
      <p class="hero-copy">登入後可建立使用者、登錄借閱、歸還物品與新增物品。</p>
    </div>
    {#if token}
      <button class="ghost-button" type="button" on:click={logout}>登出</button>
    {/if}
  </section>

  {#if !token}
    <section class="login-card">
      <label>
        <span>Email</span>
        <input bind:value={loginForm.email} placeholder="admin@example.com" autocomplete="username" />
      </label>
      <label>
        <span>Password</span>
        <input bind:value={loginForm.password} placeholder="Password" type="password" autocomplete="current-password" />
      </label>
      <button class="primary-button" type="button" on:click={handleLogin}>登入管理後台</button>
    </section>
  {:else}
    <section class="admin-grid">
      <section class="panel compact">
        <div class="section-title">
          <h2>使用者</h2>
        </div>
        <div class="inline-form">
          <input bind:value={userQuery} placeholder="姓名搜尋" on:keydown={(event) => event.key === "Enter" && handleUserSearch()} />
          <button type="button" class="icon-button" aria-label="搜尋使用者" on:click={handleUserSearch}>⌕</button>
        </div>
        {#if selectedUser}
          <div class="selected-user light">
            <strong>{selectedUser.name}</strong>
            <span>{selectedUser.email}</span>
          </div>
        {/if}
        <details>
          <summary>新增使用者</summary>
          <input bind:value={userForm.name} placeholder="姓名" />
          <input bind:value={userForm.email} placeholder="Email" />
          <input bind:value={userForm.password} placeholder="初始密碼" type="password" />
          <button type="button" class="secondary-button" on:click={handleCreateUser}>建立</button>
        </details>
      </section>

      <section class="panel compact">
        <div class="section-title">
          <h2>建立借閱</h2>
          <span>{selectedItemIds.length} 件</span>
        </div>
        {#if loading}
          <p class="muted">載入物品中</p>
        {:else}
          <div class="borrow-list">
            {#each availableItems as item (item.object_id)}
              <label>
                <input
                  type="checkbox"
                  checked={selectedItemIds.includes(item.object_id)}
                  on:change={() => toggleBorrowItem(item.object_id)}
                />
                <span>{item.object_name}</span>
              </label>
            {/each}
          </div>
        {/if}
        <div class="inline-form">
          <input bind:value={durationHours} type="number" min="1" max="720" />
          <button type="button" class="primary-button" on:click={handleCreateBorrowing}>送出借閱</button>
        </div>
      </section>

      <section class="panel compact">
        <div class="section-title">
          <h2>新增物品</h2>
        </div>
        <input bind:value={itemForm.objectName} placeholder="物品名稱" />
        <textarea bind:value={itemForm.description} placeholder="描述"></textarea>
        <button type="button" class="secondary-button" on:click={handleCreateItem}>建立物品</button>
      </section>

      <section class="panel compact">
        <div class="section-title">
          <h2>目前借閱</h2>
          <span>{activeBorrowings.length}</span>
        </div>
        {#each activeBorrowings as loan}
          <div class="loan-card">
            <strong>#{loan.order_id} · {formatDate(loan.end_time)}</strong>
            {#each loan.items as item}
              <div class="loan-row">
                <span>{item.object_name}</span>
                <button type="button" class="ghost-button" on:click={() => handleReturn(loan.order_id, item.object_id)}>歸還</button>
              </div>
            {/each}
          </div>
        {:else}
          <p class="muted">尚未載入或沒有進行中的借閱。</p>
        {/each}
      </section>
    </section>
  {/if}
</main>

{#if message || error}
  <div class:error class="toast">{error || message}</div>
{/if}
