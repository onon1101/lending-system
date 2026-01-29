<script>
  import ItemOverview from "./components/ItemOverview.svelte";
  import ItemDetailPage from "./pages/ItemDetailPage.svelte";
  // import UserCreation from "./routes/UserCreation.svelte";
  // import UserLoans from "./routes/UserLoans.svelte";

  const views = {
    home: ItemOverview,
    item_detail: ItemDetailPage,
    // creation: UserCreation,
    // loans: UserLoans,
  };

  let currentView = "home";
  let currentItemId = null;

  function handleNavigate(event) {
    const { view, id } = event.detail;
    currentView = view;
    currentItemId = id || null; // 點擊時保存 ID
    window.scrollTo(0, 0);
  }
</script>

<nav class="bg-black text-white p-4 flex items-center justify-between border-b border-neutral-800 sticky top-0 z-50">
  <button 
    type="button"
    class="text-2xl font-black text-blue-500 tracking-tighter" 
    on:click={() => handleNavigate({ detail: { view: 'home' } })}>
    LENDING.SYS
  </button>
  
  <div class="flex gap-4">
    <button class="px-4 py-2 hover:bg-neutral-800 rounded-lg transition" on:click={() => currentView = "creation"}>👤 註冊</button>
    <button class="px-4 py-2 hover:bg-neutral-800 rounded-lg transition" on:click={() => currentView = "loans"}>📖 查詢</button>
  </div>
</nav>

<main class="content">
  <svelte:component
    this={views[currentView]}
    itemId={currentItemId}
    on:navigate={handleNavigate}
  />
</main>

<style>
  :global(body) {
    margin: 0;
    background-color: #0a0a0a;
    font-family: 'Inter', system-ui, -apple-system, sans-serif;
  }
  .content {
    min-height: calc(100vh - 73px);
  }
</style>