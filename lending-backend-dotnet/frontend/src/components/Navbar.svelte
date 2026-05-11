<script>
    import { createEventDispatcher } from 'svelte';
    const dispatch = createEventDispatcher();
</script>

<nav class="bg-neutral-800 border-b border-neutral-700 p-4 sticky top-0 z-50">
    <div class="max-w-6xl mx-auto flex justify-between items-center">
        <h1 class="text-xl font-bold text-blue-400 cursor-pointer" on:click={() => dispatch('nav', {view: 'home'})}>
            物品借閱系統
        </h1>
        <div class="space-x-4">
            <button class="hover:text-blue-400 transition" on:click={() => dispatch('nav', {view: 'creation'})}>新增使用者</button>
            <button class="hover:text-blue-400 transition" on:click={() => dispatch('nav', {view: 'loans'})}>借閱查詢</button>
        </div>
    </div>
</nav>