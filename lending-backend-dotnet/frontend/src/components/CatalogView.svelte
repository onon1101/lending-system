<script>
  import { onMount } from "svelte";
  import { getFullImageUrl, getItemDetailPath, getItemKey } from "../stores/api";

  export let title = "物品總覽";
  export let eyebrow = "Item Catalog";
  export let heroCopy = "快速瀏覽設備、器材與資源目前狀態，點選卡片可查看完整物品資訊與借閱紀錄。";
  export let statLabel = "全部物品";
  export let loadItems;
  export let loadingText = "載入物品中";
  export let emptyPrefix = "目前沒有符合條件的";

  let items = [];
  let loading = true;
  let error = "";
  let search = "";
  let statusFilter = "all";

  const statusFilters = [
    { key: "all", label: "全部" },
    { key: "available", label: "可借閱" },
    { key: "borrowed", label: "借閱中" },
    { key: "unavailable", label: "不可借閱" },
  ];

  $: availableItems = items.filter((item) => getStatusGroup(item.current_status) === "available");
  $: borrowedItems = items.filter((item) => getStatusGroup(item.current_status) === "borrowed");
  $: unavailableItems = items.filter((item) => getStatusGroup(item.current_status) === "unavailable");
  $: statusFilteredItems = items.filter((item) => statusFilter === "all" || getStatusGroup(item.current_status) === statusFilter);
  $: filteredItems = statusFilteredItems.filter((item) => {
    const text = `${item.object_name} ${item.description} ${item.owner_name ?? ""}`.toLowerCase();
    return text.includes(search.trim().toLowerCase());
  });
  $: activeStatusLabel = statusFilters.find((filter) => filter.key === statusFilter)?.label ?? "全部";

  onMount(async () => {
    try {
      items = await loadItems();
    } catch (err) {
      error = err.message || "無法載入物品清單。";
    } finally {
      loading = false;
    }
  });

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
</script>

<main class="home-page">
  <header class="topbar">
    <a class="brand" href="/">
      <span class="brand-mark">LS</span>
      <span>物品借閱系統</span>
    </a>
    <a class="login-link" href="/login">登入</a>
  </header>

  <section class="home-hero">
    <div>
      <p class="eyebrow">{eyebrow}</p>
      <h1>{title}</h1>
      <p class="hero-copy">{heroCopy}</p>
    </div>
    <div class="hero-stat">
      <span>{items.length}</span>
      <small>{statLabel}</small>
    </div>
  </section>

  <section class="catalog-section">
    <div class="catalog-header">
      <div>
        <h2>{activeStatusLabel}物品</h2>
        <p>
          {filteredItems.length} 件符合條件 · 可借閱 {availableItems.length} · 借閱中 {borrowedItems.length} · 不可借閱 {unavailableItems.length}
        </p>
      </div>
      <div class="catalog-tools">
        <div class="status-tabs" aria-label="物品狀態篩選">
          {#each statusFilters as filter}
            <button
              type="button"
              class:active={statusFilter === filter.key}
              on:click={() => (statusFilter = filter.key)}
            >
              {filter.label}
            </button>
          {/each}
        </div>
        <input class="search-input" bind:value={search} placeholder="搜尋物品、描述或保管人" />
      </div>
    </div>

    {#if loading}
      <div class="empty-state">{loadingText}</div>
    {:else if error}
      <div class="empty-state error-state">{error}</div>
    {:else if filteredItems.length === 0}
      <div class="empty-state">{emptyPrefix}{activeStatusLabel}物品</div>
    {:else}
      <div class="overview-grid">
        {#each filteredItems as item (getItemKey(item))}
          <a class="overview-card" href={getItemDetailPath(item)}>
            <div class="overview-image">
              {#if item.image_url}
                <img src={getFullImageUrl(item.image_url)} alt={item.object_name} />
              {:else}
                <span>{item.object_name.slice(0, 2).toUpperCase()}</span>
              {/if}
            </div>
            <div class="overview-content">
              <span class="status-pill" class:available={getStatusGroup(item.current_status) === "available"} class:borrowed={getStatusGroup(item.current_status) === "borrowed"}>
                {getStatusLabel(item.current_status)}
              </span>
              <h3>{item.object_name}</h3>
              <p>{item.description || "尚無物品描述。"}</p>
            </div>
          </a>
        {/each}
      </div>
    {/if}
  </section>
</main>
