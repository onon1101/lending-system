<script>
  import Navbar from "../components/Navbar.svelte";
  import UploadMediaModal from "../components/UploadMediaModal.svelte";
  import MediaGallery from "../components/MediaGallery.svelte";
  import { onMount, createEventDispatcher } from "svelte";
  import {
    GetItemByID,
    GetLoanHistoryByItemID,
    getFullImageUrl,
  } from "../stores/api";

  const dispatch = createEventDispatcher();

  // 核心修正：接收 App.svelte 傳入的 ID
  export let itemId;

  let productData = null;
  let history = [];
  let loading = true;

  onMount(async () => {
    if (!itemId) return;
    try {
      // 同時抓取物品資訊與歷史紀錄
      const [item, loanHistory] = await Promise.all([
        GetItemByID(itemId),
        GetLoanHistoryByItemID(itemId),
      ]);
      productData = item;
      history = loanHistory || [];
    } catch (err) {
      console.error("載入失敗:", err);
    } finally {
      loading = false;
    }
  });

  // 動態狀態顏色 (Available -> 綠色, 其他 -> 黃色)
  $: statusColor =
    productData?.current_status === "Available"
      ? "bg-green-600"
      : "bg-yellow-600";

  function getIcon(status) {
    // 如果是借用中 (On Loan)
    if (status === "On Loan") {
      // 顯示時鐘圖示 (代表進行中)
      return `<svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M12 8v4l3 3m6-3a9 9 0 11-18 0 9 9 0 0118 0z"></path></svg>`;
    }

    // 如果是已歸還 (Returned / 完成狀態)
    // 顯示打勾圖示
    return `<svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 13l4 4L19 7"></path></svg>`;
  }

  /** 格式化日期為 YYYY 年 MM 月 DD 日 */
  function formatDate(isoString) {
    if (!isoString || isoString === "0001-01-01T00:00:00Z") return "N/A";

    const date = new Date(isoString);
    return date.toLocaleDateString("zh-TW", {
      year: "numeric",
      month: "long",
      day: "numeric",
    });
  }
  let isUploadModalOpen = false;
  // const params = useParams();
  // const itemId = $params.ID;

  function openUploadModal() {
    isUploadModalOpen = true;
  }
</script>

{#if loading}
  <div
    class="flex justify-center items-center h-screen bg-neutral-900 text-white"
  >
    載入中...
  </div>
{:else if productData}
  <div
    class="grid grid-cols-1 md:grid-cols-3 gap-6 bg-neutral-900 min-h-screen p-6 text-white"
  >
    <section
      class="col-span-1 bg-neutral-800 p-8 rounded-2xl shadow-xl border border-neutral-700"
    >
      <div class="flex items-center justify-between">
        <button
          class="text-blue-400 hover:text-blue-300 flex items-center mb-6 font-bold"
          on:click={() => dispatch("navigate", { view: "home" })}
        >
          ← 返回列表
        </button>

        <button
          on:click={openUploadModal}
          class="p-2 text-gray-500 hover:text-indigo-400 transition-colors flex items-center justify-center mb-6"
          title="管理媒體資源"
        >
          <i class="fa-solid fa-gear text-2xl"></i>
        </button>
      </div>

      <h1 class="text-4xl font-black mb-4">{productData.object_name}</h1>
      <span
        class="inline-block px-4 py-1 rounded-full text-sm font-bold {statusColor} shadow-lg mb-6"
      >
        {productData.current_status}
      </span>
      <p class="text-neutral-400 leading-relaxed mb-8">
        {productData.description}
      </p>

      <div class="space-y-8 relative pl-6 border-l-2 border-neutral-700">
        <h2 class="text-xl font-bold mb-4">📜 借閱紀錄</h2>
        {#each history as record}
          <div class="relative pb-4">
            <div
              class="absolute -left-[35px] top-0 w-8 h-8 rounded-full flex items-center justify-center border-4 border-neutral-800 transition-colors duration-300
              {record.status === 'On Loan' ? 'bg-yellow-500' : 'bg-blue-600'}"
            >
              {@html getIcon(record.status)}
            </div>
            <div class="ml-4">
              <h3 class="font-bold text-white text-lg">
                {record.name || "使用者"}
              </h3>
              <p class="text-sm text-neutral-500">
                結束：{formatDate(record.end_time)}
              </p>
              <p class="text-sm text-neutral-500">
                開始：{formatDate(record.start_time)}
              </p>
            </div>
          </div>
        {:else}
          <p class="text-neutral-500 italic">尚無借閱歷史</p>
        {/each}
      </div>
    </section>

    <div
      class="col-span-2 bg-neutral-950 rounded-2xl border border-neutral-800 flex items-center justify-center overflow-hidden"
    >
      {#if productData.image_url}
        <img
          src={getFullImageUrl(productData.image_url)}
          alt="product"
          class="max-w-full max-h-full object-contain shadow-2xl"
        />
      {:else}
        <p class="text-neutral-600 font-bold uppercase tracking-widest">
          No Image Available
        </p>
      {/if}
    </div>
  </div>

  <div class="detail-container">
    <MediaGallery {itemId} />
  </div>
  {#if isUploadModalOpen}<UploadMediaModal
      objectId={itemId}
      on:close={() => (isUploadModalOpen = false)}
      on:success={() => {
        isUploadModalOpen = false;
        window.location.reload();
      }}
    />{/if}
{/if}
