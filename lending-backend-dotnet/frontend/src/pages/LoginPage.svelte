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
  let isRegistering = false;
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

<main class="mx-auto w-[min(1180px,calc(100vw-2rem))] px-0 py-4 pb-12 text-slate-900">
  <header class="flex min-h-[72px] items-center justify-between gap-4">
    <a class="inline-flex items-center gap-3 text-[1.05rem] font-black text-inherit no-underline" href="/">
      <span class="grid h-[42px] w-[42px] place-items-center rounded-lg bg-slate-900 font-black text-white">LS</span>
      <span>物品借閱系統</span>
    </a>
    <a
      class="min-h-[42px] rounded-lg border-2 border-slate-900 bg-white px-4 py-2.5 font-black text-slate-900 no-underline hover:bg-slate-100 focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
      href="/"
    >
      返回首頁
    </a>
  </header>

  <section class="grid items-end gap-5 border-b-[3px] border-slate-900 py-[clamp(2rem,5vw,4rem)] pb-6 md:grid-cols-[minmax(0,1fr)_auto]">
    <div>
      <p class="mb-1.5 text-xs font-black uppercase text-slate-700">Admin Console</p>
      <h1 class="mb-4 text-[clamp(2.4rem,6vw,5.25rem)] font-black leading-[0.95] text-slate-900">管理登入</h1>
      <p class="mb-0 max-w-[680px] text-lg leading-7 text-slate-700">
        登入後可建立使用者、登錄借閱、歸還物品與新增物品。
      </p>
    </div>
    {#if token}
      <button
        class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-white px-4 py-2.5 font-black text-slate-900 hover:bg-slate-100 focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
        type="button"
        on:click={logout}
      >
        登出
      </button>
    {/if}
  </section>

  {#if !token}
    <section
      class="mx-auto mt-5 grid w-[min(520px,100%)] gap-4 rounded-lg border-2 border-slate-900 bg-white p-5 shadow-[8px_8px_0_#111827]"
    >
      {#if isRegistering}
        <label class="grid gap-2 font-black text-slate-900">
          <span>姓名</span>
          <input
            class="w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15"
            bind:value={userForm.name}
            placeholder="王小明"
            autocomplete="name"
          />
        </label>
        <label class="grid gap-2 font-black text-slate-900">
          <span>Email</span>
          <input
            class="w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15"
            bind:value={userForm.email}
            placeholder="user@example.com"
            autocomplete="email"
          />
        </label>
        <label class="grid gap-2 font-black text-slate-900">
          <span>Password</span>
          <input
            class="w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15"
            bind:value={userForm.password}
            placeholder="Password"
            type="password"
            autocomplete="new-password"
          />
        </label>
        <button
          class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-slate-900 px-4 py-2.5 font-black text-white hover:bg-black focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
          type="button"
          on:click={handleRegister}
        >
          建立帳號
        </button>
        <button
          class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-white px-4 py-2.5 font-black text-slate-900 hover:bg-slate-100 focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
          type="button"
          on:click={() => (isRegistering = false)}
        >
          返回登入
        </button>
      {:else}
        <label class="grid gap-2 font-black text-slate-900">
          <span>Email</span>
          <input
            class="w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15"
            bind:value={loginForm.email}
            placeholder="admin@example.com"
            autocomplete="username"
          />
        </label>
        <label class="grid gap-2 font-black text-slate-900">
          <span>Password</span>
          <input
            class="w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15"
            bind:value={loginForm.password}
            placeholder="Password"
            type="password"
            autocomplete="current-password"
          />
        </label>
        <button
          class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-slate-900 px-4 py-2.5 font-black text-white hover:bg-black focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
          type="button"
          on:click={handleLogin}
        >
          登入管理後台
        </button>
        <button
          class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-white px-4 py-2.5 font-black text-slate-900 hover:bg-slate-100 focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
          type="button"
          on:click={() => (isRegistering = true)}
        >
          註冊
        </button>
      {/if}
    </section>
  {:else}
    <section class="mt-5 grid gap-4 md:grid-cols-2">
      <section class="grid gap-3 rounded-lg border-2 border-slate-900 bg-white p-4 shadow-[8px_8px_0_#111827]">
        <div class="flex items-center justify-between gap-3">
          <h2 class="mb-0 text-xl font-black text-slate-900">使用者</h2>
        </div>
        <div class="flex items-center gap-3">
          <input
            class="w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15"
            bind:value={userQuery}
            placeholder="姓名搜尋"
            on:keydown={(event) => event.key === "Enter" && handleUserSearch()}
          />
          <button
            type="button"
            class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-slate-900 px-4 py-2.5 font-black text-white hover:bg-black focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
            aria-label="搜尋使用者"
            on:click={handleUserSearch}
          >
            ⌕
          </button>
        </div>
        {#if selectedUser}
          <div class="grid gap-0.5 rounded-lg border-2 border-slate-900 bg-slate-100 p-3">
            <strong>{selectedUser.name}</strong>
            <span class="text-sm font-bold text-slate-600">{selectedUser.email}</span>
          </div>
        {/if}
        <details class="grid gap-3">
          <summary class="mt-1 mb-3 cursor-pointer font-black text-slate-900">新增使用者</summary>
          <input class="mb-3 w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15" bind:value={userForm.name} placeholder="姓名" />
          <input class="mb-3 w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15" bind:value={userForm.email} placeholder="Email" />
          <input class="mb-3 w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15" bind:value={userForm.password} placeholder="初始密碼" type="password" />
          <button
            type="button"
            class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-white px-4 py-2.5 font-black text-slate-900 hover:bg-slate-100 focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
            on:click={handleCreateUser}
          >
            建立
          </button>
        </details>
      </section>

      <section class="grid gap-3 rounded-lg border-2 border-slate-900 bg-white p-4 shadow-[8px_8px_0_#111827]">
        <div class="flex items-center justify-between gap-3">
          <h2 class="mb-0 text-xl font-black text-slate-900">建立借閱</h2>
          <span class="mb-0 text-sm font-extrabold text-slate-600">{selectedItemIds.length} 件</span>
        </div>
        {#if loading}
          <p class="font-bold text-slate-600">載入物品中</p>
        {:else}
          <div class="grid max-h-[220px] grid-cols-[repeat(auto-fill,minmax(170px,1fr))] gap-2 overflow-auto">
            {#each availableItems as item (item.object_id)}
              <label class="flex min-w-0 items-center gap-2 rounded-lg border-2 border-slate-300 p-2.5 font-extrabold text-slate-900">
                <input
                  class="w-auto"
                  type="checkbox"
                  checked={selectedItemIds.includes(item.object_id)}
                  on:change={() => toggleBorrowItem(item.object_id)}
                />
                <span>{item.object_name}</span>
              </label>
            {/each}
          </div>
        {/if}
        <div class="flex items-center gap-3">
          <input class="w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15" bind:value={durationHours} type="number" min="1" max="720" />
          <button
            type="button"
            class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-slate-900 px-4 py-2.5 font-black text-white hover:bg-black focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
            on:click={handleCreateBorrowing}
          >
            送出借閱
          </button>
        </div>
      </section>

      <section class="grid gap-3 rounded-lg border-2 border-slate-900 bg-white p-4 shadow-[8px_8px_0_#111827]">
        <div class="flex items-center justify-between gap-3">
          <h2 class="mb-0 text-xl font-black text-slate-900">新增物品</h2>
        </div>
        <input class="w-full rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15" bind:value={itemForm.objectName} placeholder="物品名稱" />
        <textarea class="min-h-24 w-full resize-y rounded-lg border-2 border-slate-400 bg-white px-3.5 py-3 text-slate-900 outline-none focus:border-slate-900 focus:ring-4 focus:ring-slate-900/15" bind:value={itemForm.description} placeholder="描述"></textarea>
        <button
          type="button"
          class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-white px-4 py-2.5 font-black text-slate-900 hover:bg-slate-100 focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
          on:click={handleCreateItem}
        >
          建立物品
        </button>
      </section>

      <section class="grid gap-3 rounded-lg border-2 border-slate-900 bg-white p-4 shadow-[8px_8px_0_#111827]">
        <div class="flex items-center justify-between gap-3">
          <h2 class="mb-0 text-xl font-black text-slate-900">目前借閱</h2>
          <span class="mb-0 text-sm font-extrabold text-slate-600">{activeBorrowings.length}</span>
        </div>
        {#each activeBorrowings as loan}
          <div class="grid gap-3 border-l-4 border-slate-900 pl-3">
            <strong class="block text-sm text-slate-600">#{loan.order_id} · {formatDate(loan.end_time)}</strong>
            {#each loan.items as item}
              <div class="flex items-center justify-between gap-3">
                <span>{item.object_name}</span>
                <button
                  type="button"
                  class="min-h-[42px] whitespace-nowrap rounded-lg border-2 border-slate-900 bg-white px-4 py-2.5 font-black text-slate-900 hover:bg-slate-100 focus-visible:border-slate-900 focus-visible:outline-none focus-visible:ring-4 focus-visible:ring-slate-900/15"
                  on:click={() => handleReturn(loan.order_id, item.object_id)}
                >
                  歸還
                </button>
              </div>
            {/each}
          </div>
        {:else}
          <p class="font-bold text-slate-600">尚未載入或沒有進行中的借閱。</p>
        {/each}
      </section>
    </section>
  {/if}
</main>

{#if message || error}
  <div
    class={`fixed right-4 bottom-4 max-w-[min(420px,calc(100vw-2rem))] rounded-lg border-2 border-slate-900 px-4 py-3 font-extrabold text-white shadow-[8px_8px_0_rgba(0,0,0,0.18)] ${error ? "bg-red-800" : "bg-slate-900"}`}
  >
    {error || message}
  </div>
{/if}
