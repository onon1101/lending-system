<svelte:head>
  <title>功能開發中 | Lending System</title>
</svelte:head>

<main class="developing-page">
  <header class="topbar">
    <a class="brand" href="/">
      <span class="brand-mark">LS</span>
      <span>物品借閱系統</span>
    </a>
    <a class="subtle-link" href="/">返回首頁</a>
  </header>

  <section class="developing-panel">
    <p class="eyebrow">In Progress</p>
    <h1>此功能正在開發階段，盡請見諒</h1>
    <p>這個頁面目前暫不開放使用，完成後會重新啟用。</p>
  </section>
</main>

<style>
  .developing-page {
    width: min(1180px, calc(100vw - 2rem));
    min-height: 100vh;
    display: grid;
    grid-template-rows: auto 1fr;
    margin: 0 auto;
    padding: 1rem 0 3rem;
  }

  .developing-panel {
    align-self: center;
    max-width: 880px;
    border-top: 3px solid #111827;
    border-bottom: 3px solid #111827;
    padding: clamp(2.5rem, 7vw, 5rem) 0;
  }

  .developing-panel p:last-child {
    max-width: 620px;
    color: #374151;
    font-size: 1.08rem;
    font-weight: 800;
    line-height: 1.7;
    margin-bottom: 0;
  }
</style>
