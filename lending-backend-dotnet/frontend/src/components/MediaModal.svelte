<script>
  import { createEventDispatcher } from "svelte";

  // 1. 解決 TypeScript 錯誤：給予初始值 null 並明確標註型別為 Object 或 null
  /** @type {any} */
  export let media = null;

  const dispatch = createEventDispatcher();

  function close() {
    dispatch("close");
  }

  // 處理鍵盤事件以符合無障礙規範 (a11y)
  function handleKeydown(event) {
    if (event.key === "Escape") close();
  }
</script>

<svelte:window on:keydown={handleKeydown} />

{#if media}
  <div
    class="fixed inset-0 z-50 flex items-center justify-center bg-black/90 p-4 backdrop-blur-sm"
    role="dialog"
    aria-modal="true"
    tabindex="-1"
    on:click|self={close}
    on:keydown={handleKeydown}
  >
    <button
      type="button"
      class="absolute top-6 right-6 text-white hover:text-red-500 transition-colors"
      aria-label="關閉視窗"
      on:click={close}
    >
      <svg
        class="w-10 h-10"
        fill="none"
        stroke="currentColor"
        viewBox="0 0 24 24"
      >
        <path
          stroke-linecap="round"
          stroke-linejoin="round"
          stroke-width="2"
          d="M6 18L18 6M6 6l12 12"
        />
      </svg>
    </button>

    <div class="max-w-5xl w-full flex flex-col items-center">
      {#if media.type === "video"}
        <video
          controls
          autoplay
          playsinline
          class="max-h-[80vh] w-auto rounded-lg shadow-2xl"
        >
          <source src={media.url} type="video/mp4" />
          您的瀏覽器不支援影片播放。
        </video>
      {:else}
        <img
          src={media.url}
          alt={media.name || "媒體內容"}
          class="max-h-[80vh] w-auto object-contain rounded-lg shadow-2xl"
        />
      {/if}

      <div class="mt-6 text-center text-white">
        <h3 class="text-2xl font-bold">{media.name || "未命名媒體"}</h3>
        {#if media.description}
          <p class="text-neutral-400 mt-2">{media.description}</p>
        {/if}
        {#if media.link}
          <a
            href={media.link}
            target="_blank"
            rel="noopener noreferrer"
            class="text-blue-400 text-sm mt-2 inline-block hover:underline"
          >
            原始連結 ↗
          </a>
        {/if}
      </div>
    </div>
  </div>
{/if}
