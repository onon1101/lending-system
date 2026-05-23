<script>
  import { onMount, createEventDispatcher } from "svelte";
  import { getAllItems, getFullImageUrl, getItemKey, getItemDetailPath } from "../stores/api"; //

  const dispatch = createEventDispatcher();
  let items = [];
  let loading = true;
  let error = null;

  onMount(async () => {
    try {
      items = await getAllItems(); //
    } catch (err) {
      error = "無法載入物品清單，請稍後再試。";
      console.error(err);
    } finally {
      loading = false;
    }
  });

  function viewDetail(item) {
    window.location.href = getItemDetailPath(item);
    dispatch("navigate", { view: "item_detail", id: getItemKey(item) }); //
  }
</script>

<div class="space-y-8 text-white">
  <header>
    <h2 class="text-3xl font-black">系統物品概覽</h2>
    <p class="text-neutral-500 mt-1">目前共有 {items.length} 件物品可供借閱</p>
  </header>

  {#if loading}
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
      {#each Array(6) as _}
        <div
          class="h-64 bg-neutral-800 animate-pulse rounded-2xl border border-neutral-700"
        ></div>
      {/each}
    </div>
  {:else if error}
    <div
      class="bg-red-900/20 border border-red-500/50 p-6 rounded-2xl text-red-400 text-center"
    >
      {error}
    </div>
  {:else}
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
      {#each items as item (getItemKey(item))}
        <div
          class="group bg-neutral-800 rounded-2xl overflow-hidden border border-neutral-700 hover:border-blue-500/50 transition-all cursor-pointer shadow-lg"
          on:click={() => viewDetail(item)}
          aria-hidden="true"
        >
          <div class="relative h-48 bg-neutral-900">
            {#if item.image_url}
              <img
                src={getFullImageUrl(item.image_url)}
                alt={item.object_name}
                class="w-full h-full object-cover opacity-90 group-hover:opacity-100 transition"
              />
            {:else}
              <div
                class="flex items-center justify-center h-full text-neutral-700 font-bold uppercase italic"
              >
                No Preview
              </div>
            {/if}
            <div class="absolute top-4 right-4">
              <span
                class="px-3 py-1 text-[10px] font-black uppercase rounded-full border
                {item.current_status === 'Available'
                  ? 'bg-green-500/10 text-green-400 border-green-500/30'
                  : 'bg-yellow-500/10 text-yellow-400 border-yellow-500/30'}"
              >
                {item.current_status}
              </span>
            </div>
          </div>
          <div class="p-6">
            <h3
              class="text-xl font-bold group-hover:text-blue-400 transition-colors"
            >
              {item.object_name}
            </h3>
            <p class="mt-2 text-neutral-400 text-sm line-clamp-2">
              {item.description || "尚無物品描述。"}
            </p>
          </div>
        </div>
      {/each}
    </div>
  {/if}
</div>
