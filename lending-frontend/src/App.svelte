<script>
  import UserLoans from "./routes/UserLoans.svelte";

  let currentView = 'loads';
</script>

<main>
  <h1>Object 借閱系統</h1>

  <nav>
    <button
      on:click={()=>currentView = 'loans'}
      class:active={currentView === 'loans'}>
      查詢借閱紀錄   
      </button> 
  </nav>

  <div class="content">
    {#if currentView === 'loans'}
      <UserLoans/>
    {/if}
  </div>
</main>

<style>
    :global(body) {
        background-color: #1a1a1a; /* 全局深色背景 */
        color: #e0e0e0; /* 全局文字顏色 */
        font-family: sans-serif;
    }

    main {
        padding: 20px;
    }
    nav {
        margin-bottom: 20px;
        border-bottom: 2px solid #eee;
        padding-bottom: 10px;
    }
    nav button {
        padding: 10px 15px;
        margin-right: 10px;
        cursor: pointer;
        border: 1px solid #ccc;
        background-color: #fff;
        border-radius: 4px;
        transition: background-color 0.2s;
    }
    nav button.active {
        background-color: #007bff;
        color: white;
        border-color: #007bff;
    }
    .content {
        border: 1px dashed #ccc;
        padding: 20px;
    }
</style>