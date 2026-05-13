<script>
  import { onMount } from "svelte";
  import {
    clearAccessToken,
    getBorrowingHistory,
    getCurrentUserFromToken,
    getFullImageUrl,
    getItemsByUserId,
  } from "../stores/api";

  let currentUser = null;
  let items = [];
  let histories = {};
  let expandedItemId = null;
  let loading = true;
  let historyLoadingId = null;
  let error = "";

  $: role = String(currentUser?.role || "").toLowerCase();

  onMount(async () => {
    currentUser = getCurrentUserFromToken();
    if (!currentUser?.user_id) {
      loading = false;
      error = "請先登入後再查看我的抱枕總覽。";
      return;
    }

    try {
      items = await getItemsByUserId(currentUser.user_id);
    } catch (err) {
      error = err.message || "載入抱枕總覽失敗。";
    } finally {
      loading = false;
    }
  });

  async function toggleItem(itemId) {
    expandedItemId = expandedItemId === itemId ? null : itemId;
    if (!expandedItemId || histories[itemId]) return;

    historyLoadingId = itemId;
    error = "";
    try {
      histories = {
        ...histories,
        [itemId]: (await getBorrowingHistory(itemId)).filter(hasLoanRecord),
      };
    } catch (err) {
      error = err.message || "載入借閱歷史失敗。";
    } finally {
      historyLoadingId = null;
    }
  }

  function logout() {
    clearAccessToken();
    window.location.href = "/login.html";
  }

  function hasLoanRecord(record) {
    return Boolean(record?.order_id || record?.name || record?.status || record?.start_time || record?.end_time);
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
    if (!value || value === "0001-01-01T00:00:00Z") return "N/A";
    return new Date(value).toLocaleDateString("zh-TW", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  }
</script>

<main class="pillow-console">
  <aside class="pillow-sidebar">
    <a class="leftbar-brand" href="/" aria-label="物品借閱系統">
      <span>LS</span>
    </a>
    <nav aria-label="使用者功能">
      <a class="active" href="/my-pillows.html">我的抱枕</a>
      <a href="/login.html">工作台</a>
    </nav>
  </aside>

  <section class="pillow-main">
    <header class="pillow-header">
      <div>
        <p class="eyebrow">User Console</p>
        <h1>我的抱枕</h1>
        <p>目前登入身分：{role || "未知"}</p>
      </div>
      <button class="secondary-button" type="button" on:click={logout}>登出</button>
    </header>

    <section class="panel pillow-overview-panel">
      <div class="section-title">
        <div>
          <h2>目前持有</h2>
          <span>{items.length} 件抱枕</span>
        </div>
        <a class="secondary-button pillow-add-link" href="/login.html">新增</a>
      </div>

      {#if error}
        <div class="empty-state error-state">{error}</div>
      {:else if loading}
        <div class="empty-state">載入抱枕中</div>
      {:else if items.length === 0}
        <div class="empty-state">目前沒有持有中的抱枕。</div>
      {:else}
        <div class="pillow-list">
          {#each items as item (item.object_id)}
            <article class:expanded={expandedItemId === item.object_id} class="pillow-row">
              <button class="pillow-summary" type="button" on:click={() => toggleItem(item.object_id)}>
                <div class="pillow-thumb">
                  {#if item.image_url}
                    <img src={getFullImageUrl(item.image_url)} alt={item.object_name} />
                  {:else}
                    <span>{item.object_name?.slice(0, 2).toUpperCase() || "LS"}</span>
                  {/if}
                </div>
                <div class="pillow-copy">
                  <span class:available={getStatusGroup(item.current_status) === "available"} class:borrowed={getStatusGroup(item.current_status) === "borrowed"} class="status-pill">
                    {getStatusLabel(item.current_status)}
                  </span>
                  <h3>{item.object_name}</h3>
                  <p>{item.description || "尚無抱枕描述。"}</p>
                  <dl>
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
                <span class="expand-indicator" aria-hidden="true">{expandedItemId === item.object_id ? "−" : "+"}</span>
              </button>

              {#if expandedItemId === item.object_id}
                <div class="pillow-history">
                  <div class="pillow-history-title">
                    <h4>借閱歷史</h4>
                    <a href={`/item.html?id=${item.object_id}`}>查看細節</a>
                  </div>
                  {#if historyLoadingId === item.object_id}
                    <p class="muted">載入借閱歷史中</p>
                  {:else if (histories[item.object_id] || []).length === 0}
                    <p class="muted">此抱枕尚無借閱紀錄。</p>
                  {:else}
                    <div class="pillow-history-list">
                      {#each histories[item.object_id] as record}
                        <div>
                          <strong>{record.name || "使用者"}</strong>
                          <span>{record.status || "N/A"} · {formatDate(record.start_time)} - {formatDate(record.end_time)}</span>
                        </div>
                      {/each}
                    </div>
                  {/if}
                </div>
              {/if}
            </article>
          {/each}
        </div>
      {/if}
    </section>
  </section>
</main>

<style>
  .pillow-console {
    min-height: 100vh;
    display: grid;
    grid-template-columns: 116px minmax(0, 1fr);
    background: #e5e7eb;
  }

  .pillow-sidebar {
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

  .pillow-sidebar nav {
    display: grid;
    align-content: start;
    gap: 0.5rem;
  }

  .pillow-sidebar a {
    min-height: 48px;
    display: grid;
    place-items: center;
    border: 2px solid transparent;
    border-radius: 8px;
    color: #374151;
    font-weight: 950;
    text-align: center;
    text-decoration: none;
  }

  .pillow-sidebar a:hover,
  .pillow-sidebar a.active {
    border-color: #111827;
    background: #ffffff;
    color: #111827;
    box-shadow: 4px 4px 0 #111827;
  }

  .pillow-main {
    min-width: 0;
    padding: 1.1rem clamp(1rem, 4vw, 3.5rem) 3rem;
  }

  .pillow-header {
    display: grid;
    grid-template-columns: minmax(0, 1fr) auto;
    align-items: end;
    gap: 1rem;
    padding: clamp(2rem, 5vw, 4rem) 0 1.5rem;
    border-bottom: 3px solid #111827;
  }

  .pillow-header p:last-child {
    color: #374151;
    font-size: 1.08rem;
    line-height: 1.7;
    margin-bottom: 0;
  }

  .pillow-overview-panel {
    display: grid;
    gap: 1rem;
    margin-top: 1.25rem;
  }

  .pillow-add-link {
    display: inline-flex;
    align-items: center;
    color: #111827;
    text-decoration: none;
  }

  .pillow-list {
    display: grid;
    gap: 0.9rem;
  }

  .pillow-row {
    border: 2px solid #111827;
    border-radius: 8px;
    background: #f9fafb;
    overflow: hidden;
  }

  .pillow-row.expanded {
    background: #ffffff;
  }

  .pillow-summary {
    width: 100%;
    display: grid;
    grid-template-columns: 172px minmax(0, 1fr) auto;
    align-items: stretch;
    gap: 1rem;
    border: 0;
    background: transparent;
    color: inherit;
    padding: 0;
    text-align: left;
  }

  .pillow-thumb {
    min-height: 172px;
    display: grid;
    place-items: center;
    background: #d1d5db;
    color: #111827;
    font-size: 2rem;
    font-weight: 950;
    overflow: hidden;
  }

  .pillow-thumb img {
    width: 100%;
    height: 100%;
    object-fit: cover;
  }

  .pillow-copy {
    min-width: 0;
    padding: 1rem 0;
  }

  .pillow-copy h3 {
    font-size: 1.45rem;
  }

  .pillow-copy p {
    color: #374151;
    line-height: 1.55;
    margin-bottom: 0.8rem;
  }

  .pillow-copy dl {
    display: grid;
    grid-template-columns: repeat(2, minmax(0, 1fr));
    gap: 0.7rem;
    margin: 0;
  }

  .pillow-copy dl div {
    min-width: 0;
  }

  .pillow-copy dt {
    color: #6b7280;
    font-size: 0.8rem;
    font-weight: 900;
  }

  .pillow-copy dd {
    color: #111827;
    font-weight: 900;
    margin: 0;
  }

  .expand-indicator {
    width: 56px;
    display: grid;
    place-items: center;
    border-left: 2px solid #111827;
    font-size: 1.5rem;
    font-weight: 950;
  }

  .pillow-history {
    border-top: 2px solid #111827;
    padding: 1rem;
  }

  .pillow-history-title {
    display: flex;
    align-items: center;
    justify-content: space-between;
    gap: 1rem;
    margin-bottom: 0.8rem;
  }

  .pillow-history-title h4 {
    color: #111827;
    font-size: 1rem;
    margin: 0;
  }

  .pillow-history-title a {
    color: #111827;
    font-weight: 900;
  }

  .pillow-history-list {
    display: grid;
    gap: 0.65rem;
  }

  .pillow-history-list div {
    border-left: 4px solid #111827;
    padding-left: 0.8rem;
  }

  .pillow-history-list strong,
  .pillow-history-list span {
    display: block;
  }

  .pillow-history-list span {
    color: #4b5563;
    font-size: 0.92rem;
    font-weight: 700;
  }

  @media (max-width: 760px) {
    .pillow-console {
      grid-template-columns: 1fr;
    }

    .pillow-sidebar {
      position: static;
      min-height: 0;
      grid-template-columns: auto 1fr;
      grid-template-rows: 1fr;
      align-items: center;
      border-right: 0;
      border-bottom: 3px solid #111827;
    }

    .pillow-sidebar nav {
      display: flex;
      overflow-x: auto;
    }

    .pillow-sidebar a {
      min-width: max-content;
      padding: 0 0.8rem;
    }

    .pillow-header {
      grid-template-columns: 1fr;
    }

    .pillow-summary {
      grid-template-columns: 1fr;
    }

    .pillow-copy {
      padding: 0 1rem 1rem;
    }

    .expand-indicator {
      width: 100%;
      min-height: 42px;
      border-top: 2px solid #111827;
      border-left: 0;
    }

    .pillow-copy dl {
      grid-template-columns: 1fr;
    }
  }
</style>
