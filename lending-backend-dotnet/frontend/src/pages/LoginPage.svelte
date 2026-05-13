<svelte:head>
  <script src="https://accounts.google.com/gsi/client" async defer></script>
</svelte:head>

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
    getCurrentUserFromToken,
    getFullImageUrl,
    getItemsByUserId,
    googleLogin,
    login,
    returnBorrowedItem,
    searchUserByName,
    updateItem,
  } from "../stores/api";

  export let initialRegistering = false;

  let items = [];
  let ownedItems = [];
  let activeBorrowings = [];
  let selectedItemIds = [];
  let loading = false;
  let ownedLoading = false;
  let message = "";
  let error = "";
  let token = getAccessToken();
  let currentUser = token ? getCurrentUserFromToken(token) : null;
  let activeTab = "owned";
  let userQuery = "";
  let selectedUser = null;
  let durationHours = 24;
  let isRegistering = initialRegistering;
  let isCreateItemModalOpen = false;
  let isEditItemModalOpen = false;
  let creatingItem = false;
  let updatingItem = false;
  let editingItem = null;
  let loginForm = { email: "", password: "" };
  let userForm = { name: "", email: "", password: "" };
  let itemForm = { objectName: "", maker: "", material: "", description: "", cover: null };
  let editItemForm = { objectName: "", maker: "", material: "", description: "" };

  $: role = String(currentUser?.role || "").toLowerCase();
  $: isAdmin = isAdminRole(role);
  $: canUseWorkspace = role === "admin" || role === "user";
  $: availableItems = items.filter((item) => getStatusGroup(item.current_status) === "available");
  $: sidebarItems = [
    { key: "owned", label: "我的抱枕", visible: canUseWorkspace },
    { key: "overview", label: "紀錄總覽", href: "/my-pillows", visible: canUseWorkspace },
    { key: "users", label: "使用者", visible: isAdmin },
    { key: "borrow", label: "建立借閱", visible: isAdmin },
    { key: "items", label: "物品管理", visible: isAdmin },
    { key: "active", label: "目前借閱", visible: isAdmin },
  ].filter((item) => item.visible);

  onMount(async () => {
    if (!token) return;
    await initializeWorkspace();
  });

  const GOOGLE_CLIENT_ID = import.meta.env.VITE_GOOGLE_CLIENT_ID;

  onMount(() => {
    if (!GOOGLE_CLIENT_ID || token) return;

    let cancelled = false;
    let timeoutId;
    const intervalId = window.setInterval(() => {
      if (cancelled || !window.google?.accounts?.id) return;

      const button = document.getElementById("google-signin-button");
      if (!button) return;

      window.google.accounts.id.initialize({
        client_id: GOOGLE_CLIENT_ID,
        callback: async (response) => {
          try {
            await googleLogin(response.credential);
            window.location.href = "/login";
          } catch (err) {
            error = err?.message || "Google 登入失敗";
          }
        },
      });

      window.google.accounts.id.renderButton(button, { theme: "outline", size: "large", width: 280 });
      window.clearInterval(intervalId);
      window.clearTimeout(timeoutId);
    }, 100);

    timeoutId = window.setTimeout(() => window.clearInterval(intervalId), 5000);

    return () => {
      cancelled = true;
      window.clearInterval(intervalId);
      window.clearTimeout(timeoutId);
    };
  });

  async function run(action, successText = "") {
    error = "";
    message = "";
    try {
      const result = await action();
      if (successText) message = successText;
      return result;
    } catch (err) {
      error = err.message || "操作失敗";
      throw err;
    }
  }

  async function initializeWorkspace() {
    currentUser = getCurrentUserFromToken(token);
    if (!currentUser?.user_id) {
      error = "無法從登入資訊取得使用者 ID，請重新登入。";
      return;
    }

    await loadOwnedItems();
    if (isAdminRole(currentUser.role)) await loadItems();
  }

  function isAdminRole(value) {
    return String(value || "").toLowerCase() === "admin";
  }

  async function loadOwnedItems() {
    if (!currentUser?.user_id) return;
    ownedLoading = true;
    await run(async () => {
      ownedItems = await getItemsByUserId(currentUser.user_id);
    }).catch(() => {});
    ownedLoading = false;
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
      currentUser = getCurrentUserFromToken(token);
      loginForm = { email: "", password: "" };
      activeTab = "owned";
      await initializeWorkspace();
    }, "已登入。");
  }

  async function handleRegister() {
    await run(async () => {
      await createUser(userForm);
      loginForm = { email: userForm.email, password: "" };
      userForm = { name: "", email: "", password: "" };
      isRegistering = false;
    }, "註冊成功，請使用新帳號登入。");
  }

  function logout() {
    clearAccessToken();
    token = "";
    currentUser = null;
    selectedUser = null;
    ownedItems = [];
    items = [];
    activeBorrowings = [];
    selectedItemIds = [];
    activeTab = "owned";
    message = "已登出。";
  }

  async function handleUserSearch() {
    if (!userQuery.trim()) return;
    await run(async () => {
      selectedUser = await searchUserByName(userQuery.trim());
      activeBorrowings = await getActiveBorrowings(selectedUser.user_id);
      activeTab = "active";
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
    const trimmedName = itemForm.objectName.trim();
    const trimmedDescription = itemForm.description.trim();

    if (!trimmedName || !trimmedDescription) {
      error = "請填寫物品名稱與描述。";
      message = "";
      return;
    }

    creatingItem = true;
    await run(async () => {
      await createItem({
        ...itemForm,
        objectName: trimmedName,
        maker: itemForm.maker.trim(),
        material: itemForm.material.trim(),
        description: trimmedDescription,
      });
      resetItemForm();
      isCreateItemModalOpen = false;
      await Promise.all([loadItems(), loadOwnedItems()]);
    }, "物品已新增。").catch(() => {});
    creatingItem = false;
  }

  async function handleUpdateItem() {
    if (!editingItem) return;

    const trimmedName = editItemForm.objectName.trim();
    const trimmedDescription = editItemForm.description.trim();

    if (!trimmedName || !trimmedDescription) {
      error = "請填寫物品名稱與描述。";
      message = "";
      return;
    }

    updatingItem = true;
    await run(async () => {
      await updateItem(editingItem.object_id, {
        objectName: trimmedName,
        maker: editItemForm.maker.trim(),
        material: editItemForm.material.trim(),
        description: trimmedDescription,
        currentStatus: editingItem.current_status,
        imageUrl: editingItem.image_url,
      });
      closeEditItemModal();
      await Promise.all([loadItems(), loadOwnedItems()]);
    }, "物品資訊已更新。").catch(() => {});
    updatingItem = false;
  }

  function openCreateItemModal() {
    error = "";
    message = "";
    isCreateItemModalOpen = true;
  }

  function closeCreateItemModal() {
    if (creatingItem) return;
    isCreateItemModalOpen = false;
    resetItemForm();
  }

  function resetItemForm() {
    itemForm = { objectName: "", maker: "", material: "", description: "", cover: null };
  }

  function openEditItemModal(item) {
    error = "";
    message = "";
    editingItem = item;
    editItemForm = {
      objectName: item.object_name || "",
      maker: item.maker || "",
      material: item.material || "",
      description: item.description || "",
    };
    isEditItemModalOpen = true;
  }

  function closeEditItemModal() {
    if (updatingItem) return;
    isEditItemModalOpen = false;
    editingItem = null;
    editItemForm = { objectName: "", maker: "", material: "", description: "" };
  }

  function handleCoverChange(event) {
    itemForm = { ...itemForm, cover: event.currentTarget.files?.[0] || null };
  }

  function toggleBorrowItem(itemId) {
    selectedItemIds = selectedItemIds.includes(itemId)
      ? selectedItemIds.filter((id) => id !== itemId)
      : [...selectedItemIds, itemId];
  }

  async function handleCreateBorrowing() {
    if (!selectedUser) {
      error = "請先查詢並選取使用者。";
      activeTab = "users";
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
      await Promise.all([loadItems(), loadOwnedItems()]);
      activeTab = "active";
    }, "借閱已建立。");
  }

  async function handleReturn(orderId, objectId) {
    await run(async () => {
      await returnBorrowedItem(orderId, objectId);
      activeBorrowings = selectedUser ? await getActiveBorrowings(selectedUser.user_id) : [];
      await Promise.all([loadItems(), loadOwnedItems()]);
    }, "物品已歸還。");
  }

  function getStatusGroup(status) {
    const normalized = String(status ?? "").trim().toLowerCase();
    if (["available", "可借閱", "可借出"].includes(normalized)) return "available";
    if (["on loan", "borrowed", "borrowing", "借閱中", "借出中"].includes(normalized)) return "borrowed";
    return "unavailable";
  }

  function getStatusLabel(status) {
    const group = getStatusGroup(status);
    if (group === "available") return "可借閱";
    if (group === "borrowed") return "借閱中";
    return "不可借閱";
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

<main class:workspace={token} class="login-shell">
  {#if !token}
    <header class="topbar">
      <a class="brand" href="/">
        <span class="brand-mark">LS</span>
        <span>物品借閱系統</span>
      </a>
      <a class="subtle-link" href="/">返回首頁</a>
    </header>

    <section class="login-hero">
      <p class="eyebrow">Account</p>
      <h1>登入</h1>
      <p>登入後可查看目前自己持有的抱枕；管理者可從側欄執行使用者、借閱與物品管理。</p>
    </section>

    <section class="login-card">
      {#if isRegistering}
        <label>
          <span>姓名</span>
          <input bind:value={userForm.name} placeholder="王小明" autocomplete="name" />
        </label>
        <label>
          <span>Email</span>
          <input bind:value={userForm.email} placeholder="user@example.com" autocomplete="email" />
        </label>
        <label>
          <span>Password</span>
          <input bind:value={userForm.password} placeholder="Password" type="password" autocomplete="new-password" />
        </label>
        <button class="primary-button" type="button" on:click={handleRegister}>建立帳號</button>
        <button class="secondary-button" type="button" on:click={() => (isRegistering = false)}>返回登入</button>
      {:else}
        <label>
          <span>Email</span>
          <input bind:value={loginForm.email} placeholder="admin@example.com" autocomplete="username" />
        </label>
        <label>
          <span>Password</span>
          <input bind:value={loginForm.password} placeholder="Password" type="password" autocomplete="current-password" />
        </label>
        <button class="primary-button" type="button" on:click={handleLogin}>登入</button>
        <button class="secondary-button" type="button" on:click={() => (isRegistering = true)}>註冊</button>
      {/if}
      {#if GOOGLE_CLIENT_ID}
        <div class="auth-divider" aria-hidden="true"></div>
        <div class="google-auth-area">
          <div id="google-signin-button" class="google-signin-button"></div>
        </div>
      {/if}
    </section>
  {:else}
    <aside class="leftbar">
      <a class="leftbar-brand" href="/" aria-label="物品借閱系統">
        <span>LS</span>
      </a>
      <nav aria-label="登入後功能">
        {#each sidebarItems as item}
          {#if item.href}
            <a href={item.href}>{item.label}</a>
          {:else}
            <button
              class:active={activeTab === item.key}
              type="button"
              on:click={() => (activeTab = item.key)}
            >
              {item.label}
            </button>
          {/if}
        {/each}
      </nav>
    </aside>

    <section class="workspace-main">
      <header class="workspace-header">
        <div>
          <p class="eyebrow">{isAdmin ? "Admin Console" : "User Console"}</p>
          <h1>{activeTab === "owned" ? "我的抱枕" : "管理操作"}</h1>
          <p>目前登入身分：{role || "未知"}</p>
        </div>
        <button class="secondary-button" type="button" on:click={logout}>登出</button>
      </header>

      {#if activeTab === "owned"}
        <section class="panel owned-panel">
          <div class="section-title">
            <div>
              <h2>目前持有</h2>
              <span>{ownedItems.length} 件抱枕</span>
            </div>
            <button class="secondary-button" type="button" on:click={openCreateItemModal}>新增</button>
          </div>

          {#if ownedLoading}
            <div class="empty-state">載入抱枕中</div>
          {:else if ownedItems.length === 0}
            <div class="empty-state">目前沒有持有中的抱枕。</div>
          {:else}
            <div class="owned-grid">
              {#each ownedItems as item (item.object_id)}
                <article class="owned-card">
                  <a class="owned-card-link" href={`/items/${item.object_id}`} aria-label={`查看 ${item.object_name}`}>
                    <div class="owned-image">
                      {#if item.image_url}
                        <img src={getFullImageUrl(item.image_url)} alt={item.object_name} />
                      {:else}
                        <span>{item.object_name.slice(0, 2).toUpperCase()}</span>
                      {/if}
                    </div>
                    <div class="owned-copy">
                      <span class:available={getStatusGroup(item.current_status) === "available"} class:borrowed={getStatusGroup(item.current_status) === "borrowed"} class="status-pill">
                        {getStatusLabel(item.current_status)}
                      </span>
                      <h3>{item.object_name}</h3>
                      <dl class="owned-meta">
                        <div>
                          <dt>作者</dt>
                          <dd>{item.maker || "未填寫"}</dd>
                        </div>
                        <div>
                          <dt>材質</dt>
                          <dd>{item.material || "未填寫"}</dd>
                        </div>
                      </dl>
                      <p>{item.description || "尚無抱枕描述。"}</p>
                    </div>
                  </a>
                  {#if isAdmin}
                    <button class="edit-card-button" type="button" on:click={() => openEditItemModal(item)}>
                      編輯
                    </button>
                  {/if}
                </article>
              {/each}
            </div>
          {/if}
        </section>
      {:else if activeTab === "users"}
        <section class="panel tool-panel">
          <div class="section-title">
            <h2>使用者</h2>
          </div>
          <div class="inline-form">
            <input
              bind:value={userQuery}
              placeholder="姓名搜尋"
              on:keydown={(event) => event.key === "Enter" && handleUserSearch()}
            />
            <button class="icon-button" type="button" aria-label="搜尋使用者" on:click={handleUserSearch}>⌕</button>
          </div>
          {#if selectedUser}
            <div class="selected-user">
              <strong>{selectedUser.name}</strong>
              <span>{selectedUser.email}</span>
            </div>
          {/if}
          <details>
            <summary>新增使用者</summary>
            <input bind:value={userForm.name} placeholder="姓名" />
            <input bind:value={userForm.email} placeholder="Email" />
            <input bind:value={userForm.password} placeholder="初始密碼" type="password" />
            <button class="secondary-button" type="button" on:click={handleCreateUser}>建立</button>
          </details>
        </section>
      {:else if activeTab === "borrow"}
        <section class="panel tool-panel">
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
              {:else}
                <div class="empty-state">目前沒有可借物品。</div>
              {/each}
            </div>
          {/if}
          <div class="inline-form">
            <input bind:value={durationHours} type="number" min="1" max="720" />
            <button class="primary-button" type="button" on:click={handleCreateBorrowing}>送出借閱</button>
          </div>
        </section>
      {:else if activeTab === "items"}
        <section class="panel tool-panel">
          <div class="section-title">
            <div>
              <h2>物品管理</h2>
              <span>{items.length} 件物品</span>
            </div>
            <button class="secondary-button" type="button" on:click={openCreateItemModal}>新增</button>
          </div>
          {#if loading}
            <div class="empty-state">載入物品中</div>
          {:else}
            <div class="admin-item-list">
              {#each items as item (item.object_id)}
                <article class="admin-item-row">
                  <div class="admin-item-thumb">
                    {#if item.image_url}
                      <img src={getFullImageUrl(item.image_url)} alt={item.object_name} />
                    {:else}
                      <span>{item.object_name.slice(0, 2).toUpperCase()}</span>
                    {/if}
                  </div>
                  <div>
                    <span class:available={getStatusGroup(item.current_status) === "available"} class:borrowed={getStatusGroup(item.current_status) === "borrowed"} class="status-pill">
                      {getStatusLabel(item.current_status)}
                    </span>
                    <h3>{item.object_name}</h3>
                    <p>{item.description || "尚無物品描述。"}</p>
                    <dl class="admin-item-meta">
                      <div>
                        <dt>作者</dt>
                        <dd>{item.maker || "未填寫"}</dd>
                      </div>
                      <div>
                        <dt>材質</dt>
                        <dd>{item.material || "未填寫"}</dd>
                      </div>
                    </dl>
                  </div>
                  <button class="secondary-button" type="button" on:click={() => openEditItemModal(item)}>編輯</button>
                </article>
              {:else}
                <div class="empty-state">目前沒有物品。</div>
              {/each}
            </div>
          {/if}
        </section>
      {:else if activeTab === "active"}
        <section class="panel tool-panel">
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
                  <button class="secondary-button" type="button" on:click={() => handleReturn(loan.order_id, item.object_id)}>
                    歸還
                  </button>
                </div>
              {/each}
            </div>
          {:else}
            <div class="empty-state">尚未載入或沒有進行中的借閱。</div>
          {/each}
        </section>
      {/if}
    </section>
  {/if}
</main>

{#if message || error}
  <div class:error class="toast">
    {error || message}
  </div>
{/if}

{#if isCreateItemModalOpen}
  <div class="modal-backdrop" role="presentation" on:click={closeCreateItemModal}>
    <div
      class="item-modal"
      role="dialog"
      aria-modal="true"
      aria-labelledby="create-item-title"
      tabindex="-1"
      on:click|stopPropagation
      on:keydown|stopPropagation
    >
      <div class="section-title">
        <div>
          <p class="eyebrow">Catalog</p>
          <h2 id="create-item-title">新增物品</h2>
        </div>
        <button class="ghost-button" type="button" on:click={closeCreateItemModal} disabled={creatingItem}>關閉</button>
      </div>

      <form class="item-form" on:submit|preventDefault={handleCreateItem}>
        <label>
          <span>物品名稱</span>
          <input bind:value={itemForm.objectName} placeholder="例：雪ちゃん" disabled={creatingItem} />
        </label>

        <div class="form-grid">
          <label>
            <span>作者</span>
            <input bind:value={itemForm.maker} placeholder="可留空" disabled={creatingItem} />
          </label>
          <label>
            <span>材質</span>
            <input bind:value={itemForm.material} placeholder="可留空" disabled={creatingItem} />
          </label>
        </div>

        <label>
          <span>描述</span>
          <textarea bind:value={itemForm.description} placeholder="輸入物品描述" disabled={creatingItem}></textarea>
        </label>

        <label>
          <span>封面圖片</span>
          <input type="file" accept="image/*" on:change={handleCoverChange} disabled={creatingItem} />
        </label>

        <div class="modal-actions">
          <button class="ghost-button" type="button" on:click={closeCreateItemModal} disabled={creatingItem}>取消</button>
          <button class="primary-button" type="submit" disabled={creatingItem}>
            {creatingItem ? "建立中" : "建立物品"}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}

{#if isEditItemModalOpen}
  <div class="modal-backdrop" role="presentation" on:click={closeEditItemModal}>
    <div
      class="item-modal"
      role="dialog"
      aria-modal="true"
      aria-labelledby="edit-item-title"
      tabindex="-1"
      on:click|stopPropagation
      on:keydown|stopPropagation
    >
      <div class="section-title">
        <div>
          <p class="eyebrow">Catalog</p>
          <h2 id="edit-item-title">修改物品資訊</h2>
        </div>
        <button class="ghost-button" type="button" on:click={closeEditItemModal} disabled={updatingItem}>關閉</button>
      </div>

      <form class="item-form" on:submit|preventDefault={handleUpdateItem}>
        <label>
          <span>物品名稱</span>
          <input bind:value={editItemForm.objectName} placeholder="例：雪ちゃん" disabled={updatingItem} />
        </label>

        <div class="form-grid">
          <label>
            <span>作者</span>
            <input bind:value={editItemForm.maker} placeholder="可留空" disabled={updatingItem} />
          </label>
          <label>
            <span>材質</span>
            <input bind:value={editItemForm.material} placeholder="可留空" disabled={updatingItem} />
          </label>
        </div>

        <label>
          <span>描述</span>
          <textarea bind:value={editItemForm.description} placeholder="輸入物品描述" disabled={updatingItem}></textarea>
        </label>

        <div class="modal-actions">
          <button class="ghost-button" type="button" on:click={closeEditItemModal} disabled={updatingItem}>取消</button>
          <button class="primary-button" type="submit" disabled={updatingItem}>
            {updatingItem ? "儲存中" : "儲存修改"}
          </button>
        </div>
      </form>
    </div>
  </div>
{/if}

<style>
  .login-shell {
    width: min(1180px, calc(100vw - 2rem));
    margin: 0 auto;
    padding: 1rem 0 3rem;
  }

  .login-shell.workspace {
    width: 100%;
    min-height: 100vh;
    display: grid;
    grid-template-columns: 116px minmax(0, 1fr);
    gap: 0;
    padding: 0;
  }

  .login-hero {
    padding: clamp(2rem, 5vw, 4rem) 0 1.5rem;
    border-bottom: 3px solid #111827;
  }

  .login-hero p:last-child,
  .workspace-header p:last-child {
    max-width: 680px;
    color: #374151;
    font-size: 1.08rem;
    line-height: 1.7;
    margin-bottom: 0;
  }

  .auth-divider {
    width: 100%;
    height: 3px;
    background: #111827;
    margin: 0.1rem 0 0.05rem;
  }

  .google-auth-area {
    display: flex;
    justify-content: center;
    width: 100%;
    padding-top: 0.1rem;
  }

  .google-signin-button {
    display: flex;
    justify-content: center;
    width: 280px;
    max-width: 100%;
    min-height: 40px;
  }

  .leftbar {
    position: sticky;
    top: 0;
    min-height: 100vh;
    display: grid;
    grid-template-rows: auto 1fr;
    gap: 1rem;
    border-right: 3px solid #111827;
    background: #f9fafb;
    padding: 1rem 0.65rem;
  }

  .leftbar-brand {
    width: 54px;
    height: 54px;
    display: grid;
    place-items: center;
    justify-self: center;
    border: 2px solid #111827;
    border-radius: 8px;
    background: #111827;
    color: #ffffff;
    font-weight: 950;
    text-decoration: none;
  }

  .leftbar nav {
    display: grid;
    align-content: start;
    gap: 0.5rem;
  }

  .leftbar button,
  .leftbar a {
    min-height: 48px;
    display: grid;
    place-items: center;
    border: 2px solid transparent;
    border-radius: 8px;
    background: transparent;
    color: #374151;
    font-weight: 950;
    text-align: center;
    text-decoration: none;
  }

  .leftbar button:hover,
  .leftbar button.active,
  .leftbar a:hover {
    border-color: #111827;
    background: #ffffff;
    color: #111827;
    box-shadow: 4px 4px 0 #111827;
  }

  .workspace-main {
    min-width: 0;
    padding: 1.1rem clamp(1rem, 4vw, 3.5rem) 3rem;
  }

  .workspace-header {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    align-items: end;
    gap: 1rem;
    padding: clamp(2rem, 5vw, 4rem) 0 1.5rem;
    border-bottom: 3px solid #111827;
  }

  .owned-panel,
  .tool-panel {
    margin-top: 1.25rem;
    display: grid;
    gap: 1rem;
  }

  .owned-grid {
    display: grid;
    grid-template-columns: repeat(auto-fill, minmax(230px, 1fr));
    gap: 1rem;
  }

  .owned-card {
    position: relative;
    min-width: 0;
    overflow: hidden;
    border: 2px solid #111827;
    border-radius: 8px;
    background: #f9fafb;
    color: inherit;
    transition: transform 160ms ease, box-shadow 160ms ease;
  }

  .owned-card:hover,
  .owned-card:focus-within {
    transform: translate(-3px, -3px);
    box-shadow: 7px 7px 0 #111827;
  }

  .owned-card-link {
    min-height: 100%;
    display: grid;
    grid-template-rows: 190px 1fr;
    color: inherit;
    text-decoration: none;
  }

  .owned-image {
    display: grid;
    place-items: center;
    overflow: hidden;
    background: #d1d5db;
    color: #111827;
    font-size: 2rem;
    font-weight: 950;
  }

  .owned-image img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .owned-copy {
    padding: 1rem 1rem 4.3rem;
  }

  .owned-copy h3 {
    padding-right: 4.5rem;
  }

  .owned-meta {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 0.6rem;
    margin: 0.6rem 0;
  }

  .owned-meta div {
    min-width: 0;
  }

  .owned-meta dt {
    color: #6b7280;
    font-size: 0.78rem;
    font-weight: 900;
  }

  .owned-meta dd {
    margin: 0.15rem 0 0;
    overflow-wrap: anywhere;
    color: #111827;
    font-weight: 900;
  }

  .owned-copy p {
    color: #374151;
    line-height: 1.55;
    margin-bottom: 0;
  }

  .edit-card-button {
    position: absolute;
    right: 0.75rem;
    bottom: 0.75rem;
    min-height: 38px;
    border: 2px solid #111827;
    border-radius: 8px;
    background: #ffffff;
    color: #111827;
    font-weight: 950;
    box-shadow: 3px 3px 0 #111827;
    cursor: pointer;
  }

  .edit-card-button:hover,
  .edit-card-button:focus-visible {
    background: #facc15;
  }

  .tool-panel {
    max-width: 860px;
  }

  .admin-item-list {
    display: grid;
    gap: 0.8rem;
  }

  .admin-item-row {
    display: grid;
    grid-template-columns: 96px minmax(0, 1fr) auto;
    align-items: center;
    gap: 1rem;
    border: 2px solid #111827;
    border-radius: 8px;
    background: #f9fafb;
    padding: 0.75rem;
  }

  .admin-item-thumb {
    width: 96px;
    aspect-ratio: 1;
    display: grid;
    place-items: center;
    overflow: hidden;
    border: 2px solid #111827;
    border-radius: 8px;
    background: #d1d5db;
    color: #111827;
    font-weight: 950;
  }

  .admin-item-thumb img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .admin-item-row h3 {
    margin: 0.5rem 0 0.35rem;
  }

  .admin-item-row p {
    margin: 0;
    color: #374151;
    line-height: 1.45;
  }

  .admin-item-meta {
    display: flex;
    flex-wrap: wrap;
    gap: 0.4rem 1rem;
    margin: 0.55rem 0 0;
  }

  .admin-item-meta div {
    display: flex;
    gap: 0.35rem;
  }

  .admin-item-meta dt {
    color: #6b7280;
    font-weight: 900;
  }

  .admin-item-meta dd {
    margin: 0;
    color: #111827;
    font-weight: 900;
  }

  .modal-backdrop {
    position: fixed;
    inset: 0;
    z-index: 40;
    display: grid;
    place-items: center;
    padding: 1rem;
    background: rgba(17, 24, 39, 0.55);
  }

  .item-modal {
    width: min(640px, 100%);
    max-height: min(760px, calc(100vh - 2rem));
    display: grid;
    gap: 1rem;
    overflow: auto;
    border: 2px solid #111827;
    border-radius: 8px;
    background: #ffffff;
    box-shadow: 8px 8px 0 #111827;
    padding: 1rem;
  }

  .item-form,
  .item-form label {
    display: grid;
    gap: 0.55rem;
  }

  .item-form label {
    color: #111827;
    font-weight: 900;
  }

  .form-grid {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 0.8rem;
  }

  .modal-actions {
    display: flex;
    justify-content: flex-end;
    gap: 0.8rem;
    padding-top: 0.25rem;
  }

  @media (max-width: 760px) {
    .login-shell.workspace {
      grid-template-columns: 1fr;
    }

    .leftbar {
      position: static;
      min-height: 0;
      border-right: 0;
      border-bottom: 3px solid #111827;
      grid-template-columns: auto 1fr;
      grid-template-rows: 1fr;
      align-items: center;
    }

    .leftbar nav {
      display: flex;
      overflow-x: auto;
      padding-bottom: 0.25rem;
    }

    .leftbar button,
    .leftbar a {
      padding: 0 0.8rem;
      white-space: nowrap;
    }

    .workspace-header {
      grid-template-columns: 1fr;
    }

    .form-grid,
    .modal-actions,
    .admin-item-row {
      grid-template-columns: 1fr;
    }

    .modal-actions {
      display: grid;
    }

    .admin-item-thumb {
      width: 100%;
      max-height: 180px;
    }
  }
</style>
