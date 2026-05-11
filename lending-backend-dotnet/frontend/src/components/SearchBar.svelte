<script>
    export let placeholder = 'Search...'
    export let value = ''
    export let customClass = ''

    import { createEventDispatcher } from 'svelte';
  const dispatch = createEventDispatcher();

  // 3. 處理輸入變化的函數
  function handleInput(event) {
    // 更新本地的 value
    value = event.target.value;
    
    // 發送 'search' 事件給父元件，並附帶新的搜尋值
    dispatch('search', {
      searchTerm: value
    });
  }
</script>

<div class={customClass}>
    <div class="relative">
        <div class="absolute inset-y-0 left-0 flex items-center pl-3 pointer-events-none">
            <svg xmlns="http://www.w3.org/2000/svg" class="h-5 w-5 text-gray-400" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
            <path stroke-linecap="round" stroke-linejoin="round" d="M21 21l-6-6m2-5a7 7 0 11-14 0 7 7 0 0114 0z" />
            </svg>
        </div>

      <input 
        type="search"
        bind:value={value}
        on:input={handleInput}
        {placeholder}
        class="block bg-white w-full py-3 pl-10 pr-4 rounded-4xl text-gray-900 border border-gray-300 focus:ring-indigo-500 focus:border-indigo-500 shadow-sm transition duration-150 ease-in-out"    
        />
    </div>
</div>