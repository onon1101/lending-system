<script>
    export let title;
    export let value;
    export let colorClass;
    export let backgroundColor;
</script>

<div class="bg-white shadow-lg p-6 rounded-xl border border-gray-100 flex items-center justify-between">
    <div>
        <div class="text-sm font-medium text-gray-500 mb-1">
            {title}
        </div>

        <div class="text-3xl font-bold {colorClass}">
            {value}
        </div>
    </div>

    <div class="h-10 w-10 flex items-center justify-center rounded-full {backgroundColor}">
    <svg xmlns="http://www.w3.org/2000/svg" class="h-6 w-6 {colorClass}" fill="none" viewBox="0 0 24 24" stroke="currentColor" stroke-width="2">
        <path stroke-linecap="round" stroke-linejoin="round" d="M8.228 9.247a3.75 3.75 0 100 5.506h.001v.001h.001l-.001.001zM12 21a9 9 0 100-18 9 9 0 000 18z" />
    </svg>
  </div>
</div>