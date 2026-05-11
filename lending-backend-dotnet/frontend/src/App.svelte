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
    getBorrowingHistory,
    getFullImageUrl,
    getItem,
    getItemMedia,
    login,
    returnBorrowedItem,
    searchUserByName,
    uploadItemImage,
    uploadItemMedia,
  } from "./stores/api";

  let items = [];
  let selectedItem = null;
  let history = [];
  let media = [];
  let activeBorrowings = [];
  let selectedItemIds = [];

  let loading = true;
  let detailLoading = false;
  let message = "";
  let error = "";
  let token = getAccessToken();

  let search = "";
  let statusFilter = "all";
  let userQuery = "";
  let selectedUser = null;
  let durationHours = 24;

  let loginForm = { email: "", password: "" };
  let userForm = { name: "", email: "", password: "" };
  let itemForm = { objectName: "", description: "" };
  let mediaForm = { description: "", link: "", file: null };
  let imageFile = null;
  let uploadProgress = 0;

  $: availableItems = items.filter((item) => item.current_status === "Available");
  $: filteredItems = items.filter((item) => {
    const text = `${item.object_name} ${item.description} ${item.owner_name ?? ""}`.toLowerCase();
    const matchesText = text.includes(search.trim().toLowerCase());
    const matchesStatus = statusFilter === "all" || item.current_status === statusFilter;
    return matchesText && matchesStatus;
  });
  $: totalAvailable = items.filter((item) => item.current_status === "Available").length;
  $: totalOnLoan = items.filter((item) => item.current_status === "On Loan").length;

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
      if (!selectedItem && items.length > 0) {
        await selectItem(items[0].object_id);
      } else if (selectedItem) {
        const exists = items.some((item) => item.object_id === selectedItem.object_id);
        if (exists) await selectItem(selectedItem.object_id);
      }
    }).catch(() => {});
    loading = false;
  }

  async function selectItem(itemId) {
    detailLoading = true;
    await run(async () => {
      const [item, itemHistory, itemMedia] = await Promise.all([
        getItem(itemId),
        getBorrowingHistory(itemId),
        getItemMedia(itemId),
      ]);
      selectedItem = item;
      history = itemHistory.filter((record) => record.start_time);
      media = itemMedia;
    }).catch(() => {});
    detailLoading = false;
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
    await selectItem(created.object_id);
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

  async function handleUploadImage() {
    if (!selectedItem || !imageFile) return;
    await run(async () => {
      await uploadItemImage(selectedItem.object_id, imageFile);
      imageFile = null;
      await loadItems();
    }, "封面圖片已更新。");
  }

  async function handleUploadMedia() {
    if (!selectedItem || !mediaForm.file) return;
    uploadProgress = 0;
    await run(async () => {
      await uploadItemMedia(
        mediaForm.file,
        selectedItem.object_id,
        mediaForm.description,
        mediaForm.link,
        (percent) => (uploadProgress = percent),
      );
      mediaForm = { description: "", link: "", file: null };
      uploadProgress = 0;
      await selectItem(selectedItem.object_id);
    }, "媒體已上傳。");
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

<main class="app-shell">
  <aside class="sidebar">
    <div>
      <p class="eyebrow">Lending System</p>
      <h1>借閱管理工作台</h1>
    </div>

    <div class="metrics">
      <div>
        <span>{items.length}</span>
        <small>總物品</small>
      </div>
      <div>
        <span>{totalAvailable}</span>
        <small>可借</small>
      </div>
      <div>
        <span>{totalOnLoan}</span>
        <small>借出</small>
      </div>
    </div>

    <section class="panel compact">
      <div class="section-title">
        <h2>管理登入</h2>
        {#if token}
          <button class="ghost-button" type="button" on:click={logout}>登出</button>
        {/if}
      </div>
      {#if token}
        <p class="muted">已取得管理權杖。</p>
      {:else}
        <input bind:value={loginForm.email} placeholder="Email" autocomplete="username" />
        <input bind:value={loginForm.password} placeholder="Password" type="password" autocomplete="current-password" />
        <button class="primary-button" type="button" on:click={handleLogin}>登入</button>
      {/if}
    </section>

    <section class="panel compact">
      <div class="section-title">
        <h2>使用者</h2>
      </div>
      <div class="inline-form">
        <input bind:value={userQuery} placeholder="姓名搜尋" on:keydown={(event) => event.key === "Enter" && handleUserSearch()} />
        <button type="button" class="icon-button" aria-label="搜尋使用者" on:click={handleUserSearch}>⌕</button>
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
        <button type="button" class="secondary-button" on:click={handleCreateUser}>建立</button>
      </details>
    </section>
  </aside>

  <section class="catalog">
    <div class="toolbar">
      <input class="search-input" bind:value={search} placeholder="搜尋物品、描述或保管人" />
      <div class="segmented">
        <button class:active={statusFilter === "all"} type="button" on:click={() => (statusFilter = "all")}>全部</button>
        <button class:active={statusFilter === "Available"} type="button" on:click={() => (statusFilter = "Available")}>可借</button>
        <button class:active={statusFilter === "On Loan"} type="button" on:click={() => (statusFilter = "On Loan")}>借出</button>
      </div>
    </div>

    {#if loading}
      <div class="empty-state">載入物品中</div>
    {:else}
      <div class="item-grid">
        {#each filteredItems as item (item.object_id)}
          <button
            type="button"
            class="item-card"
            class:selected={selectedItem?.object_id === item.object_id}
            on:click={() => selectItem(item.object_id)}
          >
            <span class:available={item.current_status === "Available"} class="status-dot"></span>
            <div class="thumb">
              {#if item.image_url}
                <img src={getFullImageUrl(item.image_url)} alt={item.object_name} />
              {:else}
                <span>{item.object_name.slice(0, 2).toUpperCase()}</span>
              {/if}
            </div>
            <div class="item-copy">
              <strong>{item.object_name}</strong>
              <small>{item.description || "尚無描述"}</small>
            </div>
          </button>
        {/each}
      </div>
    {/if}

    <section class="panel">
      <div class="section-title">
        <h2>建立借閱</h2>
        <span>{selectedItemIds.length} 件</span>
      </div>
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
      <div class="inline-form">
        <input bind:value={durationHours} type="number" min="1" max="720" />
        <button type="button" class="primary-button" on:click={handleCreateBorrowing}>送出借閱</button>
      </div>
    </section>
  </section>

  <section class="detail">
    {#if selectedItem}
      <div class="hero-media">
        {#if selectedItem.image_url}
          <img src={getFullImageUrl(selectedItem.image_url)} alt={selectedItem.object_name} />
        {:else}
          <div>{selectedItem.object_name}</div>
        {/if}
      </div>

      <section class="detail-header">
        <span class="status-pill">{selectedItem.current_status}</span>
        <h2>{selectedItem.object_name}</h2>
        <p>{selectedItem.description || "尚無物品描述。"}</p>
      </section>

      <section class="panel compact">
        <div class="section-title">
          <h2>借閱紀錄</h2>
          {#if detailLoading}<span>更新中</span>{/if}
        </div>
        <div class="timeline">
          {#each history as record}
            <div>
              <strong>{record.name || "使用者"}</strong>
              <span>{record.status || "N/A"} · {formatDate(record.start_time)} - {formatDate(record.end_time)}</span>
            </div>
          {:else}
            <p class="muted">尚無紀錄。</p>
          {/each}
        </div>
      </section>

      <section class="panel compact">
        <div class="section-title">
          <h2>影音媒體</h2>
          <span>{media.length}</span>
        </div>
        <div class="media-grid">
          {#each media as asset}
            <a href={asset.url} target="_blank" rel="noreferrer">
              {#if asset.type === "video"}
                <video src={asset.url} muted></video>
              {:else}
                <img src={asset.url} alt={asset.description || asset.name || "media"} />
              {/if}
              <span>{asset.description || asset.name || asset.type}</span>
            </a>
          {:else}
            <p class="muted">尚無媒體。</p>
          {/each}
        </div>
      </section>

      <section class="panel compact">
        <div class="section-title">
          <h2>物品媒體</h2>
        </div>
        <input type="file" accept="image/*" on:change={(event) => (imageFile = event.currentTarget.files?.[0])} />
        <button type="button" class="secondary-button" disabled={!imageFile} on:click={handleUploadImage}>更新封面</button>
        <input bind:value={mediaForm.description} placeholder="媒體描述" />
        <input bind:value={mediaForm.link} placeholder="原始連結" />
        <input type="file" accept="image/*,video/*" on:change={(event) => (mediaForm.file = event.currentTarget.files?.[0])} />
        {#if uploadProgress > 0}<progress value={uploadProgress} max="100"></progress>{/if}
        <button type="button" class="secondary-button" disabled={!mediaForm.file} on:click={handleUploadMedia}>上傳媒體</button>
      </section>
    {:else}
      <div class="empty-state">選取一件物品查看細節</div>
    {/if}

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
</main>

{#if message || error}
  <div class:error class="toast">{error || message}</div>
{/if}
