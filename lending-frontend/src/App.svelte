<script>
// 狀態管理
let count = 0;
let taskInput = '';
let tasks = [
{ id: 1, text: '學習 Svelte 基礎語法', completed: true },
{ id: 2, text: '設定 Tailwind CSS 環境', completed: false },
];

// 響應式聲明
$: nextCount = count + 1;
$: pendingTasks = tasks.filter(t => !t.completed).length;

// 函式：增加計數
function increment() {
count += 1;
}

// 函式：新增任務
function addTask() {
if (taskInput.trim()) {
tasks = [...tasks, {
id: Date.now(),
text: taskInput.trim(),
completed: false
}];
taskInput = ''; // 清空輸入框
}
}

// 函式：切換任務完成狀態
function toggleTask(taskId) {
tasks = tasks.map(task =>
task.id === taskId ? { ...task, completed: !task.completed } : task
);
}
</script>

<div class="min-h-screen bg-gray-50 flex items-center justify-center p-4 sm:p-8">

<!-- 主容器：寬度調整為 max-w-xl 以容納列表 -->

<div class="bg-white shadow-2xl rounded-xl p-6 sm:p-10 w-full max-w-xl transition duration-500 border-t-4 border-indigo-500">

<!-- 標題與計數器 -->
<h1 class="text-3xl font-extrabold text-gray-800 mb-2 text-center">
  Svelte + Tailwind 任務列表
</h1>
<p class="text-sm text-gray-500 mb-6 text-center">
  待完成任務數：<span class="font-bold text-indigo-600">{pendingTasks}</span>
</p>

<!-- 任務輸入區 -->
<div class="flex gap-3 mb-6">
  <input
    type="text"
    bind:value={taskInput}
    on:keydown={(e) => { if (e.key === 'Enter') addTask(); }}
    placeholder="新增一項任務..."
    class="flex-grow p-3 border border-gray-300 rounded-lg focus:ring-indigo-500 focus:border-indigo-500 shadow-sm"
  />
  <button
    on:click={addTask}
    class="bg-indigo-600 text-white p-3 rounded-lg font-semibold shadow-md hover:bg-indigo-700 transition duration-200 focus:outline-none focus:ring-4 focus:ring-indigo-500 focus:ring-opacity-50"
  >
    新增
  </button>
</div>

<!-- 任務列表 -->
<ul class="space-y-3">
  {#each tasks as task (task.id)}
    <li 
      class="flex items-center justify-between p-4 bg-gray-50 rounded-lg shadow-sm cursor-pointer hover:bg-gray-100 transition duration-150"
      on:click={() => toggleTask(task.id)}
    >
      <!-- 任務文字：根據完成狀態套用不同樣式 -->
      <span 
        class="text-gray-700 text-base flex-grow {task.completed ? 'line-through text-gray-400' : ''}"
      >
        {task.text}
      </span>
      
      <!-- 狀態標籤 -->
      <span class="text-xs font-semibold px-2 py-0.5 rounded-full {task.completed ? 'bg-green-100 text-green-700' : 'bg-yellow-100 text-yellow-700'}">
        {task.completed ? '完成' : '待辦'}
      </span>
    </li>
  {:else}
    <p class="text-center text-gray-400 py-4">目前沒有待辦事項！</p>
  {/each}
</ul>

<!-- 獨立計數器範例 -->
<div class="mt-8 pt-6 border-t border-gray-200 flex justify-between items-center">
    <span class="text-lg font-medium text-gray-700">總點擊次數: <span class="text-indigo-600 font-bold">{count}</span></span>
    <button
        on:click={increment}
        class="py-2 px-4 bg-blue-500 text-white rounded-full text-sm hover:bg-blue-600 transition duration-200"
    >
        點擊 ({nextCount})
    </button>
</div>


</div>
</div>

<style>
/* 這裡可以放置您專門為這個組件編寫的 CSS。
注意：所有 Tailwind 的類別都寫在 HTML 結構中。
*/
</style>