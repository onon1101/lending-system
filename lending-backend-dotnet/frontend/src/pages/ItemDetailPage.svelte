<script>
  import { onMount } from "svelte";
  import MediaGallery from "../components/MediaGallery.svelte";
  import UploadMediaModal from "../components/UploadMediaModal.svelte";
  import {
    GetItemByID,
    GetLoanHistoryByItemID,
    getFullImageUrl,
    uploadItemImage,
  } from "../stores/api";

  export let itemId;

  let productData = null;
  let history = [];
  let loading = true;
  let error = "";
  let imageFile = null;
  let coverUploading = false;
  let isUploadModalOpen = false;

  onMount(async () => {
    itemId = itemId || new URLSearchParams(window.location.search).get("id");
    if (!itemId) {
      error = "缺少物品 ID。";
      loading = false;
      return;
    }

    try {
      const [item, loanHistory] = await Promise.all([
        GetItemByID(itemId),
        GetLoanHistoryByItemID(itemId),
      ]);
      productData = item;
      history = loanHistory || [];
    } catch (err) {
      error = err.message || "載入失敗。";
    } finally {
      loading = false;
    }
  });

  async function handleCoverUpload() {
    if (!itemId || !imageFile) return;

    coverUploading = true;
    try {
      await uploadItemImage(itemId, imageFile);
      productData = await GetItemByID(itemId);
      imageFile = null;
    } catch (err) {
      error = err.message || "封面圖片更新失敗。";
    } finally {
      coverUploading = false;
    }
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

{#if loading}
  <main class="detail-page">
    <div class="empty-state">載入物品中</div>
  </main>
{:else if error}
  <main class="detail-page">
    <div class="empty-state error-state">{error}</div>
  </main>
{:else if productData}
  <main class="detail-page">
    <header class="topbar">
      <a class="brand" href="/">
        <span class="brand-mark">LS</span>
        <span>物品借閱系統</span>
      </a>
      <a class="subtle-link" href="/">返回首頁</a>
    </header>

    <section class="detail-layout">
      <div class="detail-visual">
        {#if productData.image_url}
          <img src={getFullImageUrl(productData.image_url)} alt={productData.object_name} />
        {:else}
          <span>{productData.object_name}</span>
        {/if}
      </div>

      <aside class="detail-panel">
        <span class:available={productData.current_status === "Available"} class="status-pill">
          {productData.current_status}
        </span>
        <h1>{productData.object_name}</h1>
        <p>{productData.description || "尚無物品描述。"}</p>

        <div class="media-actions">
          <label>
            <span>封面圖片</span>
            <input
              type="file"
              accept="image/*"
              class="bg-blue-500"
              on:change={(event) => (imageFile = event.currentTarget.files?.[0])}
            />
          </label>
          <button
            type="button"
            class="secondary-button"
            disabled={!imageFile || coverUploading}
            on:click={handleCoverUpload}
          >
            {coverUploading ? "更新中..." : "更新封面"}
          </button>
          <button type="button" class="primary-button" on:click={() => (isUploadModalOpen = true)}>
            管理媒體
          </button>
        </div>
      </aside>
    </section>

    <section class="detail-section">
      <div class="section-title">
        <h2>借閱紀錄</h2>
        <span>{history.length} 筆</span>
      </div>
      <div class="timeline">
        {#each history as record}
          <div>
            <strong>{record.name || "使用者"}</strong>
            <span>{record.status || "N/A"} · {formatDate(record.start_time)} - {formatDate(record.end_time)}</span>
          </div>
        {:else}
          <p class="muted">尚無借閱歷史。</p>
        {/each}
      </div>
    </section>

    <section class="detail-section media-section">
      <MediaGallery {itemId} />
    </section>
  </main>

  {#if isUploadModalOpen}
    <UploadMediaModal
      objectId={itemId}
      on:close={() => (isUploadModalOpen = false)}
      on:success={() => {
        isUploadModalOpen = false;
        window.location.reload();
      }}
    />
  {/if}
{/if}
