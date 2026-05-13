<script>
  import { onMount } from "svelte";
  import {
    clearAccessToken,
    getBorrowingHistory,
    getCurrentUserFromToken,
    getFullImageUrl,
    getItemDetailPath,
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
    window.location.href = "/login";
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

  function getStatusClass(status) {
    const group = getStatusGroup(status);
    if (group === "available") return "bg-green-500";
    if (group === "borrowed") return "bg-blue-400";
    return "bg-yellow-400";
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

<main class="grid min-h-screen grid-cols-[116px_minmax(0,1fr)] bg-gray-200 max-md:grid-cols-1">
  <aside class="sticky top-0 grid min-h-screen grid-rows-[auto_1fr] gap-4 border-r-[3px] border-gray-900 bg-gray-50 px-3 py-4 max-md:static max-md:min-h-0 max-md:grid-cols-[auto_1fr] max-md:grid-rows-1 max-md:items-center max-md:border-r-0 max-md:border-b-[3px]">
    <a class="grid h-[54px] w-[54px] place-items-center justify-self-center rounded-lg border-2 border-gray-900 bg-gray-900 font-black text-white no-underline" href="/" aria-label="物品借閱系統">
      <span>LS</span>
    </a>
    <nav class="grid content-start gap-2 max-md:flex max-md:overflow-x-auto" aria-label="使用者功能">
      <a class="grid min-h-12 place-items-center rounded-lg border-2 border-gray-900 bg-white px-3 text-center font-black text-gray-900 no-underline shadow-[4px_4px_0_#111827]" href="/my-pillows">我的抱枕</a>
      <a class="grid min-h-12 place-items-center rounded-lg border-2 border-transparent px-3 text-center font-black text-gray-700 no-underline hover:border-gray-900 hover:bg-white hover:text-gray-900 hover:shadow-[4px_4px_0_#111827]" href="/login">工作台</a>
    </nav>
  </aside>

  <section class="min-w-0 px-[clamp(1rem,4vw,3.5rem)] py-5">
    <header class="grid grid-cols-[minmax(0,1fr)_auto] items-end gap-4 border-b-[3px] border-gray-900 py-[clamp(2rem,5vw,4rem)] pb-6 max-md:grid-cols-1">
      <div>
        <p class="mb-1 text-xs font-black uppercase text-gray-700">User Console</p>
        <h1 class="mb-4 text-[clamp(3rem,8vw,6rem)] font-black leading-none text-gray-900">我的抱枕</h1>
        <p class="mb-0 text-lg font-bold leading-7 text-gray-700">目前登入身分：{role || "未知"}</p>
      </div>
      <button class="min-h-[42px] rounded-lg border-2 border-gray-900 bg-white px-4 py-2 font-black text-gray-900 hover:bg-gray-100" type="button" on:click={logout}>登出</button>
    </header>

    <section class="mt-5 grid gap-4 rounded-lg border-2 border-gray-900 bg-white p-4 shadow-[8px_8px_0_#111827]">
      <div class="flex items-center justify-between gap-3">
        <div>
          <h2 class="mb-1 text-xl font-black text-gray-900">目前持有</h2>
          <span class="text-sm font-extrabold text-gray-600">{items.length} 件抱枕</span>
        </div>
        <a class="inline-flex min-h-[42px] items-center rounded-lg border-2 border-gray-900 bg-white px-4 py-2 font-black text-gray-900 no-underline hover:bg-gray-100" href="/login">新增</a>
      </div>

      {#if error}
        <div class="grid min-h-44 place-items-center rounded-lg border-2 border-dashed border-red-800 bg-red-50 font-extrabold text-red-800">{error}</div>
      {:else if loading}
        <div class="grid min-h-44 place-items-center rounded-lg border-2 border-dashed border-gray-500 bg-gray-50 font-extrabold text-gray-700">載入抱枕中</div>
      {:else if items.length === 0}
        <div class="grid min-h-44 place-items-center rounded-lg border-2 border-dashed border-gray-500 bg-gray-50 font-extrabold text-gray-700">目前沒有持有中的抱枕。</div>
      {:else}
        <div class="grid gap-4">
          {#each items as item (item.object_id)}
            <article class={`overflow-hidden rounded-lg border-2 border-gray-900 ${expandedItemId === item.object_id ? "bg-white" : "bg-gray-50"}`}>
              <button class="grid w-full grid-cols-[172px_minmax(0,1fr)_56px] items-stretch gap-4 border-0 bg-transparent p-0 text-left text-inherit max-md:grid-cols-1" type="button" on:click={() => toggleItem(item.object_id)}>
                <div class="grid min-h-[172px] place-items-center overflow-hidden bg-gray-300 text-3xl font-black text-gray-900">
                  {#if item.image_url}
                    <img class="h-full w-full object-cover" src={getFullImageUrl(item.image_url)} alt={item.object_name} />
                  {:else}
                    <span>{item.object_name?.slice(0, 2).toUpperCase() || "LS"}</span>
                  {/if}
                </div>
                <div class="min-w-0 py-4 max-md:px-4 max-md:pb-4 max-md:pt-0">
                  <span class={`mb-3 inline-flex w-fit items-center rounded-full border-2 border-gray-900 px-3 py-1 text-xs font-black text-gray-900 ${getStatusClass(item.current_status)}`}>
                    {getStatusLabel(item.current_status)}
                  </span>
                  <h3 class="mb-2 text-2xl font-black leading-tight text-gray-900">{item.object_name}</h3>
                  <p class="mb-3 leading-6 text-gray-700">{item.description || "尚無抱枕描述。"}</p>
                  <dl class="grid grid-cols-2 gap-3 max-md:grid-cols-1">
                    <div class="min-w-0">
                      <dt class="text-xs font-black text-gray-500">作者</dt>
                      <dd class="m-0 font-black text-gray-900">{item.maker || "未填寫"}</dd>
                    </div>
                    <div class="min-w-0">
                      <dt class="text-xs font-black text-gray-500">材質</dt>
                      <dd class="m-0 font-black text-gray-900">{item.material || "未填寫"}</dd>
                    </div>
                  </dl>
                </div>
                <span class="grid place-items-center border-l-2 border-gray-900 text-2xl font-black max-md:min-h-10 max-md:border-l-0 max-md:border-t-2" aria-hidden="true">{expandedItemId === item.object_id ? "−" : "+"}</span>
              </button>

              {#if expandedItemId === item.object_id}
                <div class="border-t-2 border-gray-900 p-4">
                  <div class="mb-3 flex items-center justify-between gap-4">
                    <h4 class="m-0 text-base font-black text-gray-900">借閱歷史</h4>
                    <a class="font-black text-gray-900" href={getItemDetailPath(item)}>查看細節</a>
                  </div>
                  {#if historyLoadingId === item.object_id}
                    <p class="font-bold text-gray-600">載入借閱歷史中</p>
                  {:else if (histories[item.object_id] || []).length === 0}
                    <p class="font-bold text-gray-600">此抱枕尚無借閱紀錄。</p>
                  {:else}
                    <div class="grid gap-3">
                      {#each histories[item.object_id] as record}
                        <div class="border-l-4 border-gray-900 pl-3">
                          <strong class="block text-gray-900">{record.name || "使用者"}</strong>
                          <span class="block text-sm font-bold text-gray-600">{record.status || "N/A"} · {formatDate(record.start_time)} - {formatDate(record.end_time)}</span>
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
