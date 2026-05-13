<script>
  import { onMount } from "svelte";
  import MediaGallery from "../components/MediaGallery.svelte";
  import UploadMediaModal from "../components/UploadMediaModal.svelte";
  import {
    GetItemByID,
    GetLoanHistoryByItemID,
    createBorrowingRecord,
    deleteBorrowingRecord,
    getCurrentUserFromToken,
    getFullImageUrl,
    updateBorrowingRecordTime,
    uploadItemImage,
  } from "../stores/api";

  export let itemId;

  let productData = null;
  let history = [];
  let loading = true;
  let error = "";
  let imageFile = null;
  let coverUploading = false;
  let canMutateData = false;
  let isUploadModalOpen = false;
  let isRecordModalOpen = false;
  let isEditingRecords = false;
  let recordSaving = false;
  let recordError = "";
  let recordEdits = {};
  let deletedRecordIds = new Set();
  let newRecord = {
    borrowerName: "",
    startTime: "",
    endTime: "",
  };

  $: visibleHistory = history.filter(hasLoanRecord);

  onMount(async () => {
    canMutateData = Boolean(getCurrentUserFromToken()?.user_id);
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
    if (!canMutateData) return;
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

  async function reloadHistory() {
    history = await GetLoanHistoryByItemID(itemId);
  }

  function getCurrentUserId() {
    return getCurrentUserFromToken()?.user_id || 0;
  }

  function setDefaultNewRecordTime() {
    const start = new Date();
    const end = new Date(start);
    end.setHours(end.getHours() + 1);
    newRecord = {
      borrowerName: "",
      startTime: toDateTimeLocal(start),
      endTime: toDateTimeLocal(end),
    };
  }

  function openCreateRecordModal() {
    if (!canMutateData) return;
    recordError = "";
    setDefaultNewRecordTime();
    isRecordModalOpen = true;
  }

  async function handleCreateRecord() {
    const userId = getCurrentUserId();
    if (!userId) {
      recordError = "請先登入，才能新增借閱紀錄。";
      return;
    }

    if (!newRecord.borrowerName.trim() || !newRecord.startTime || !newRecord.endTime) {
      recordError = "請填寫使用者/用途、開始時間與結束時間。";
      return;
    }

    if (new Date(newRecord.startTime) >= new Date(newRecord.endTime)) {
      recordError = "開始時間必須早於結束時間。";
      return;
    }

    recordSaving = true;
    recordError = "";
    try {
      await createBorrowingRecord({
        userId,
        itemId,
        borrowerName: newRecord.borrowerName.trim(),
        startTime: fromDateTimeLocal(newRecord.startTime),
        endTime: fromDateTimeLocal(newRecord.endTime),
      });
      await reloadHistory();
      isRecordModalOpen = false;
    } catch (err) {
      recordError = err.message || "新增借閱紀錄失敗。";
    } finally {
      recordSaving = false;
    }
  }

  function toggleRecordEditMode() {
    if (!canMutateData) return;
    recordError = "";
    if (isEditingRecords) {
      isEditingRecords = false;
      recordEdits = {};
      deletedRecordIds = new Set();
      return;
    }

    recordEdits = Object.fromEntries(
      visibleHistory
        .filter((record) => record.order_id)
        .map((record) => [
          record.order_id,
          {
            startTime: toDateTimeLocal(record.start_time),
            endTime: toDateTimeLocal(record.end_time),
          },
        ]),
    );
    deletedRecordIds = new Set();
    isEditingRecords = true;
  }

  function updateRecordEdit(orderId, field, value) {
    recordEdits = {
      ...recordEdits,
      [orderId]: {
        ...recordEdits[orderId],
        [field]: value,
      },
    };
  }

  function toggleDeleteRecord(orderId) {
    const next = new Set(deletedRecordIds);
    if (next.has(orderId)) {
      next.delete(orderId);
    } else {
      next.add(orderId);
    }
    deletedRecordIds = next;
  }

  async function handleSubmitRecordChanges() {
    const userId = getCurrentUserId();
    if (!userId) {
      recordError = "請先登入，才能修改借閱紀錄。";
      return;
    }

    const editableRecords = visibleHistory.filter((record) => record.order_id);
    for (const record of editableRecords) {
      if (deletedRecordIds.has(record.order_id)) continue;

      const edit = recordEdits[record.order_id];
      if (!edit?.startTime || !edit?.endTime) {
        recordError = "每筆紀錄都需要開始與結束時間。";
        return;
      }

      if (new Date(edit.startTime) >= new Date(edit.endTime)) {
        recordError = "開始時間必須早於結束時間。";
        return;
      }
    }

    recordSaving = true;
    recordError = "";
    try {
      for (const orderId of deletedRecordIds) {
        await deleteBorrowingRecord(orderId, userId);
      }

      for (const record of editableRecords) {
        if (deletedRecordIds.has(record.order_id)) continue;

        const edit = recordEdits[record.order_id];
        const startChanged = edit.startTime !== toDateTimeLocal(record.start_time);
        const endChanged = edit.endTime !== toDateTimeLocal(record.end_time);
        if (!startChanged && !endChanged) continue;

        await updateBorrowingRecordTime(record.order_id, {
          userId,
          startTime: fromDateTimeLocal(edit.startTime),
          endTime: fromDateTimeLocal(edit.endTime),
        });
      }

      await reloadHistory();
      isEditingRecords = false;
      recordEdits = {};
      deletedRecordIds = new Set();
    } catch (err) {
      recordError = err.message || "修改借閱紀錄失敗。";
    } finally {
      recordSaving = false;
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

  function hasLoanRecord(record) {
    return Boolean(record?.order_id || record?.name || record?.status || record?.start_time || record?.end_time);
  }

  function toDateTimeLocal(value) {
    if (!value || value === "0001-01-01T00:00:00Z") return "";

    const date = value instanceof Date ? value : new Date(value);
    if (Number.isNaN(date.getTime())) return "";

    const pad = (number) => String(number).padStart(2, "0");
    return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  function fromDateTimeLocal(value) {
    return new Date(value).toISOString();
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

        {#if canMutateData}
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
        {/if}
      </aside>
    </section>

    <section class="detail-section">
      <div class="section-title record-title">
        <div class="record-heading">
          <h2>借閱紀錄</h2>
          <span>- {visibleHistory.length} 筆</span>
        </div>
        {#if canMutateData}
          <div class="record-actions" aria-label="借閱紀錄操作">
            <button class="record-action-button" type="button" aria-label="新增" title="新增" on:click={openCreateRecordModal}>
              <span aria-hidden="true">+</span>
            </button>
            <button class:active={isEditingRecords} class="record-action-button" type="button" aria-label="修改" title="修改" on:click={toggleRecordEditMode}>
              <span aria-hidden="true">⚙</span>
            </button>
          </div>
        {/if}
      </div>
      {#if canMutateData && recordError}
        <p class="record-error">{recordError}</p>
      {/if}
      {#if visibleHistory.length > 0}
        <div class:editing={isEditingRecords} class="timeline">
          {#each visibleHistory as record}
            <div class:marked-delete={deletedRecordIds.has(record.order_id)} class="record-row">
              <div class="record-main">
                <strong>{record.name || "使用者"}</strong>
                {#if canMutateData && isEditingRecords && record.order_id}
                  <div class="record-time-editor">
                    <label>
                      <span>開始</span>
                      <input
                        type="datetime-local"
                        value={recordEdits[record.order_id]?.startTime || ""}
                        disabled={deletedRecordIds.has(record.order_id)}
                        on:input={(event) => updateRecordEdit(record.order_id, "startTime", event.currentTarget.value)}
                      />
                    </label>
                    <label>
                      <span>結束</span>
                      <input
                        type="datetime-local"
                        value={recordEdits[record.order_id]?.endTime || ""}
                        disabled={deletedRecordIds.has(record.order_id)}
                        on:input={(event) => updateRecordEdit(record.order_id, "endTime", event.currentTarget.value)}
                      />
                    </label>
                  </div>
                {:else}
                  <span>{record.status || "N/A"} · {formatDate(record.start_time)} - {formatDate(record.end_time)}</span>
                {/if}
              </div>
              {#if canMutateData && isEditingRecords && record.order_id}
                <button class="delete-record-button" type="button" on:click={() => toggleDeleteRecord(record.order_id)}>
                  {deletedRecordIds.has(record.order_id) ? "復原" : "刪除"}
                </button>
              {/if}
            </div>
          {/each}
        </div>
      {/if}
      {#if canMutateData && isEditingRecords}
        <div class="record-submit-row">
          <button class="primary-button" type="button" disabled={recordSaving} on:click={handleSubmitRecordChanges}>
            {recordSaving ? "送出中..." : "確定修改"}
          </button>
        </div>
      {/if}
    </section>

    <section class="detail-section media-section">
      <MediaGallery {itemId} />
    </section>
  </main>

  {#if canMutateData && isUploadModalOpen}
    <UploadMediaModal
      objectId={itemId}
      on:close={() => (isUploadModalOpen = false)}
      on:success={() => {
        isUploadModalOpen = false;
        window.location.reload();
      }}
    />
  {/if}

  {#if canMutateData && isRecordModalOpen}
    <div class="record-modal-backdrop" role="presentation">
      <div class="record-modal" role="dialog" aria-modal="true" aria-labelledby="record-modal-title">
        <div class="record-modal-header">
          <h2 id="record-modal-title">新增借閱紀錄</h2>
          <button class="record-modal-close" type="button" aria-label="關閉" on:click={() => (isRecordModalOpen = false)}>×</button>
        </div>
        <div class="record-form">
          <label>
            <span>使用者 / 用途</span>
            <input bind:value={newRecord.borrowerName} placeholder="輸入使用者或用途" />
          </label>
          <label>
            <span>開始借閱時間</span>
            <input type="datetime-local" bind:value={newRecord.startTime} />
          </label>
          <label>
            <span>結束借閱時間</span>
            <input type="datetime-local" bind:value={newRecord.endTime} />
          </label>
          {#if recordError}
            <p class="record-error">{recordError}</p>
          {/if}
        </div>
        <div class="record-modal-actions">
          <button class="secondary-button" type="button" disabled={recordSaving} on:click={() => (isRecordModalOpen = false)}>
            取消
          </button>
          <button class="primary-button" type="button" disabled={recordSaving} on:click={handleCreateRecord}>
            {recordSaving ? "新增中..." : "新增"}
          </button>
        </div>
      </div>
    </div>
  {/if}
{/if}
