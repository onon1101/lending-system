<script>
  import { onMount } from "svelte";
  import { getAllItems, getFullImageUrl } from "./stores/api";

  let items = [];
  let loading = true;
  let error = "";
  let search = "";

  $: availableItems = items.filter((item) => item.current_status === "Available");
  $: filteredItems = availableItems.filter((item) => {
    const text = `${item.object_name} ${item.description} ${item.owner_name ?? ""}`.toLowerCase();
    return text.includes(search.trim().toLowerCase());
  });

  onMount(async () => {
    try {
      items = await getAllItems();
    } catch (err) {
      error = err.message || "無法載入物品清單。";
    } finally {
      loading = false;
    }
  });
</script>

<main class="home-page">
  <header class="topbar">
    <a class="brand" href="/">
      <span class="brand-mark">LS</span>
      <span>物品借閱系統</span>
    </a>
    <a class="login-link" href="/login.html">登入</a>
  </header>

  <section class="home-hero">
    <div>
      <p class="eyebrow">Available Catalog</p>
      <h1>可借閱物品總覽</h1>
      <p class="hero-copy">快速瀏覽目前可借出的設備、器材與資源，點選卡片可查看完整物品資訊與借閱紀錄。</p>
    </div>
    <div class="hero-stat">
      <span>{availableItems.length}</span>
      <small>目前可借</small>
    </div>
  </section>

  <section class="catalog-section">
    <div class="catalog-header">
      <div>
        <h2>所有可借物品</h2>
        <p>{filteredItems.length} 件符合條件</p>
      </div>
      <input class="search-input" bind:value={search} placeholder="搜尋物品、描述或保管人" />
    </div>

    {#if loading}
      <div class="empty-state">載入物品中</div>
    {:else if error}
      <div class="empty-state error-state">{error}</div>
    {:else if filteredItems.length === 0}
      <div class="empty-state">目前沒有符合條件的可借物品</div>
    {:else}
      <div class="overview-grid">
        {#each filteredItems as item (item.object_id)}
          <a class="overview-card" href={`/item.html?id=${item.object_id}`}>
            <div class="overview-image">
              {#if item.image_url}
                <img src={getFullImageUrl(item.image_url)} alt={item.object_name} />
              {:else}
                <span>{item.object_name.slice(0, 2).toUpperCase()}</span>
              {/if}
            </div>
            <div class="overview-content">
              <span class="status-pill available">可借閱</span>
              <h3>{item.object_name}</h3>
              <p>{item.description || "尚無物品描述。"}</p>
            </div>
          </a>
        {/each}
      </div>
    {/if}
  </section>
</main>
