<script>
    // 接受父元件傳入的產品數據物件
    export let product;

    // 根據產品狀態返回 Tailwind 顏色類別
    $: statusColor = product.status === '可借出' 
        ? 'bg-green-600' 
        : 'bg-yellow-600';
</script>

<div class="rounded-xl overflow-hidden shadow-lg bg-white border border-gray-100 transition-transform duration-300 hover:scale-[1.02] hover:shadow-xl">
    <div class="h-48 bg-gray-100 flex items-center justify-center">
        <span class="text-gray-400 text-lg">產品圖片佔位符</span>
    </div>

    <div class="p-4 sm:p6">

    <div class="flex items-start justify-between mb-3">
        <h3 class="text-xl font-bold mr-4 text-gray-900">
            {product.name}
        </h3>

        <span class="px-3 py-1 text-xs font-medium rounded-full {statusColor} text-white whitespace-nowrap">
            {product.status}
        </span>
    </div>

    <p class="text-sm text-gray-500 mb-6 ">
        {product.description}
    </p>

    </div>
</div>