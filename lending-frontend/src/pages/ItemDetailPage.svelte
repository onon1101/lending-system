<script>
  // 假設從 API 或 Store 載入了單個產品的數據
  export let productData = {
    name: "Sony Alpha a7 III Kit",
    id: "CAM-001",
    status: "AVAILABLE",
    // 模擬最近的借還紀錄數據，包含圖示所需的顏色和註記
  };

  const history = [
    {
      type: "歸還成功",
      user: "陳小明 (Dev)",
      date: "2小時前",
      icon: "bg-green-600",
      note: "電池已充飽，鏡頭清潔完畢。",
      action: "歸還",
    },
    {
      type: "借出",
      user: "陳小明 (Dev)",
      date: "2023/11/28",
      icon: "bg-yellow-600",
      expectedReturn: "2023/12/01",
      action: "借用",
    },
    {
      type: "歸還成功",
      user: "林雅婷 (Design)",
      date: "2023/11/20",
      icon: "bg-indigo-600",
      action: "歸還",
    },
    {
      type: "借出",
      user: "林雅婷 (Design)",
      date: "2023/11/15",
      icon: "bg-indigo-600",
      action: "借用",
    },
  ];
  // 狀態顏色動態計算
  $: statusColor =
    productData.status === "AVAILABLE" ? "bg-green-600" : "bg-yellow-600";

  /**
   * @param {string} action
   * @returns {string} Tailwind Icon Class (SVG Placeholder)
   */
  function getIcon(action) {
    // 使用 Tailwind 顏色和簡單的 SVG 佔位符模擬圖示
    if (action === "借用") {
      // 購物袋圖示 (Bag)
      return `<svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 11H5a2 2 0 00-2 2v7a2 2 0 002 2h14a2 2 0 002-2v-7a2 2 0 00-2-2z"></path><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M11 7V5a2 2 0 012-2h0a2 2 0 012 2v2"></path></svg>`;
    } else if (action === "歸還") {
      // 返回箭頭圖示 (Back/Check)
      return `<svg class="w-6 h-6 text-white" fill="none" stroke="currentColor" viewBox="0 0 24 24" xmlns="http://www.w3.org/2000/svg"><path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M10 19l-7-7 7-7m4 14l-7-7 7-7"></path></svg>`;
    }
    return "";
  }
</script>

<div class="grid grid-cols-3 gap-6 bg-gray-200 px-6 py-6">
  <!-- Letf Section -->
  <section class="col-span-1">
    <!-- Back Button -->
    <a
      href="/"
      class="text-gray-600 hover:text-blue-600 flex items-center mb-4 text-base font-medium transition duration-150"
    >
      <svg
        class="w-5 h-5 mr-1"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
        xmlns="http://www.w3.org/2000/svg"
        ><path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M15 19l-7-7 7-7"
        ></path></svg
      >
      返回列表
    </a>

    <!-- Title -->
    <h1 class="text-3xl font-bold text-gray-800 mb-3">{productData.name}</h1>

    <!-- Status -->
    <span
      class="bg-green-600/20 text-white text-sm rounded-md px-3 py-1 uppercase tracking-wider {statusColor}"
      >{productData.status}</span
    >

    <!-- slash -->
    <div class="h-px w-full bg-gray-800/20 mt-5 mb-5"></div>

    <!-- history -->
    <div class="mt-8">
      <div class="space-y-8 relative pl-6">
        <!-- <div class="w-0.5 bg-gray-200 top-0 bottom-0 left-6 relative"></div> -->
        {#each history as record, i}
          <div class="relative min-h-20">
            <!-- left icon  -->

            <!-- {#if i !== 0}
                <div
                  class="absolute bottom-full -translate-x-1/2 w-px h-10 bg-gray-600/40"
                ></div>
              {/if} -->

            <!-- icon 完整置中 -->
            <div
              class="absolute -translate-x-1/2 top-0 w-12 h-12 rounded-full
                {record.icon} flex items-center justify-center z-10 shadow-md"
            >
              {@html getIcon(record.action)}
            </div>

            <!-- {#if i !== history.length - 1}
                <div
                  class="absolute top-full -translate-x-1/2 w-px h-24 bg-gray-600/40"
                ></div>
              {/if} -->

            <div class="ml-16 pt-1">
              <h3 class="text-gray-800 flex items-center justify-between mb-1">
                <span class="text-lg font-bold">{record.type}</span>
                <span class="text-sm font-medium text-gray-500"
                  >{record.date}</span
                >
              </h3>
              <p class="text-base text-gray-600">
                {record.type === "借出" ? "被" : "由"}
                {record.user}
                {record.action}
              </p>
              {#if record.note}
                <div class="mt-3 text-gray-500">
                  {record.note}
                </div>
              {/if}
              {#if record.expectedReturn}
                <p class="text-sm text-yellow-600 mt-1 font-medium">
                  預計歸還： {record.expectedReturn}
                </p>
              {/if}
            </div>
            {#if i !== history.length - 1}
              <div class="absolute top-full w-px h-16 bg-gray-600/40"></div>
            {/if}
          </div>
        {/each}
      </div>
      <div class="flex justify-center mt-8">
        <button
          class="text-shadow-gray-600 text-base font-medium hover:text-shadow-gray-800"
          >查看更多紀錄
        </button>
      </div>
    </div>

    <!-- slash -->
    <div class="h-px w-full bg-gray-800/20 mt-5 mb-5"></div>
  </section>

  <!-- Right Section -->
  <div class="col-span-2 bg-gray-800 flex items-center justify-center">
    <span class="text-gray-400">產品圖片佔位符</span>
  </div>
</div>
