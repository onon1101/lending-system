<script>
  import { onMount } from "svelte";
  import { getItemMedia } from "../stores/api";
  import MediaModal from "./MediaModal.svelte";

  export let item;
  let mediaList = [];
  let selectedMedia = null;
  let loading = true;

  onMount(async () => {
    try {
      mediaList = (await getItemMedia(item)) ?? [];
    } catch (e) {
      console.error("媒體加載失敗", e);
    } finally {
      loading = false;
    }
  });
</script>

<section class="mt-12 border-t border-neutral-800 pt-8">
  <div class="flex items-center justify-between mb-6">
    <h2
      class="text-2xl font-black text-white italic tracking-tighter uppercase"
    >
      相關內容
    </h2>
    <span class="text-neutral-500 text-sm font-mono"
      >{mediaList.length} ITEMS</span
    >
  </div>

  {#if loading}
    <div class="grid grid-cols-2 md:grid-cols-4 gap-4">
      {#each Array(4) as _}
        <div
          class="aspect-square bg-neutral-800 animate-pulse rounded-xl"
        ></div>
      {/each}
    </div>
  {:else if mediaList.length > 0}
    <div class="grid grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-4">
      {#each mediaList as item}
        <button
          type="button"
          class="group relative aspect-square bg-neutral-900 rounded-xl overflow-hidden cursor-pointer border border-neutral-800 hover:border-blue-500/50 transition-all focus:outline-none focus:ring-2 focus:ring-blue-500"
          on:click={() => (selectedMedia = item)}
          aria-label="查看媒體 {item.name}"
        >
          <div
            class="w-full h-full flex items-center justify-center bg-neutral-950"
          >
            {#if item.type === "video"}
              <div
                class="absolute inset-0 z-10 flex items-center justify-center bg-black/20 group-hover:bg-black/40 transition-colors"
              >
                <svg
                  class="w-12 h-12 text-white/80 group-hover:scale-110 transition-transform"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                >
                  <path
                    d="M6.3 2.841A1.5 1.5 0 004 4.11V15.89a1.5 1.5 0 002.3 1.269l9.344-5.89a1.5 1.5 0 000-2.538L6.3 2.841z"
                  />
                </svg>
              </div>
            {/if}
            <img
              src={item.url}
              alt={item.name}
              class="w-full h-full object-cover opacity-80 group-hover:opacity-100 transition-opacity"
            />
          </div>

          <div
            class="absolute bottom-0 inset-x-0 p-3 bg-gradient-to-t from-black/80 to-transparent translate-y-full group-hover:translate-y-0 transition-transform"
          >
            <p class="text-xs font-bold text-white truncate text-left">
              {item.name}
            </p>
          </div>
        </button>
      {/each}
    </div>
  {:else}
    <div
      class="text-center py-12 bg-neutral-900/50 rounded-2xl border border-dashed border-neutral-800"
    >
      <p class="text-neutral-500">此物品尚無影音媒體紀錄。</p>
    </div>
  {/if}
</section>

<MediaModal media={selectedMedia} on:close={() => (selectedMedia = null)} />
