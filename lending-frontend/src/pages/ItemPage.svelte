<script>
  import StatCard from "../components/StatCard.svelte";
  import SearchBar from "../components/SearchBar.svelte";
  import ProductCard from "../components/ProductCard.svelte";

  const totalItems = 8;
  const availableItems = 4;
  const loanedItems = 3;

  let currentSearchTerm = "";

  // 這是綁定到 SearchBar 上的事件處理函數 (可選，如果只用 bind:value 也可以)
  function handleSearchEvent(event) {
    // 當 SearchBar 元件發出 search 事件時，更新 currentSearchTerm
    // 雖然我們已經使用了 bind:value，但使用事件可以處理更複雜的邏輯
    console.log("來自元件的搜尋詞:", event.detail.searchTerm);
    // currentSearchTerm = event.detail.searchTerm; // 由於 bind:value，這行可以省略
  }

  const products = [
    {
      id: "CAM-001",
      name: "Sony Alpha a7 III Kit",
      status: "可借出",
      description:
        "包含 28-70mm 鏡頭的全片幅無反光鏡相機，適合活動攝影與錄影使用。附帶兩顆電池與充電器。",
    },
    {
      id: "PRJ-102",
      name: "Epson 高亮度投影機",
      status: "借出中",
      description:
        "3600 流明高亮度，支援 HDMI 與無線投影，適合大型會議室使用。",
    },
    {
      id: "VR-005",
      name: "Meta Quest 3",
      status: "可借出",
      description:
        "最新款混合實境頭戴裝置，包含左右手控制器，已預裝開發者測試應用。",
    },
    // 更多產品...
  ];
</script>

<div class="bg-gray-50 min-h-screen p-6 md:px-32 md:py-28">
  <div class="mx-auto max-w-6xl">
    <div class="flex justify-center mb-10">
      <SearchBar
        customClass="max-w-sm w-full"
        bind:value={currentSearchTerm}
        on:search={handleSearchEvent}
      />
    </div>

    <div class="grid gird-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-12">
      <StatCard
        title="物品總數"
        value={totalItems}
        colorClass="text-indigo-600"
        backgroundColor="bg-indigo-100"
      />
      <StatCard
        title="可借出"
        value={availableItems}
        colorClass="text-green-600"
        backgroundColor="bg-green-100"
      />
      <StatCard
        title="借出中"
        value={loanedItems}
        colorClass="text-yellow-600"
        backgroundColor="bg-yellow-100"
      />
    </div>

    <div class="bg-gray-50 min-h-screen shadow-gray-200">
      <div class="max-w-6xl mx-auto">
        <div
          class="grid gird-cols-1 sm:grid-cols-2 lg:grid-cols-3 gap-12 mb-10"
        ></div>

        <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-12">
          {#each products as product (product.id)}
            <ProductCard {product} />
          {/each}
        </div>
      </div>
    </div>
  </div>
</div>
