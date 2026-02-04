<script>
  import { createEventDispatcher } from "svelte";
  import { uploadItemMedia } from "../stores/api"; // 引入封裝後的 API

  const dispatch = createEventDispatcher();

  export let objectId;
  let fileInput;
  let description = "";
  let isUploading = false;
  let uploadProgress = 0;
  let statusMessage = "";

  async function handleUpload() {
    const file = fileInput.files[0];
    if (!file) {
      statusMessage = "請選擇檔案";
      return;
    }

    isUploading = true;
    statusMessage = "正在準備上傳...";
    uploadProgress = 0;

    try {
      // 呼叫封裝後的 API，並傳入進度處理回呼
      await uploadItemMedia(file, objectId, description, (percent) => {
        uploadProgress = percent;
        statusMessage = `上傳中: ${percent}%`;
      });

      statusMessage = "上傳成功！";
      setTimeout(() => {
        dispatch("success");
        dispatch("close");
      }, 1000);
    } catch (err) {
      statusMessage = "上傳失敗：" + err.message;
      isUploading = false;
    }
  }
</script>

<div
  class="fixed inset-0 z-50 flex items-center justify-center bg-black bg-opacity-50 backdrop-blur-sm"
>
  <div class="bg-white rounded-xl shadow-2xl w-full max-w-md p-6 m-4">
    <div class="flex justify-between items-center mb-4">
      <h2 class="text-xl font-bold text-gray-800">上傳媒體檔案</h2>
      <button
        on:click={() => dispatch("close")}
        class="text-gray-400 hover:text-gray-600">✕</button
      >
    </div>

    <div class="space-y-4">
      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1"
          >選擇檔案 (圖片或影片)</label
        >
        <input
          type="file"
          bind:this={fileInput}
          accept="image/*,video/*"
          class="block w-full text-sm text-gray-500 file:mr-4 file:py-2 file:px-4 file:rounded-full file:border-0 file:text-sm file:font-semibold file:bg-indigo-50 file:text-indigo-700 hover:file:bg-indigo-100"
        />
      </div>

      <div>
        <label class="block text-sm font-medium text-gray-700 mb-1"
          >描述 (選填)</label
        >
        <textarea
          bind:value={description}
          class="w-full border border-gray-300 rounded-lg p-2 focus:ring-2 focus:ring-indigo-500 focus:border-indigo-500 outline-none"
          rows="3"
        ></textarea>
      </div>

      {#if isUploading}
        <div class="w-full bg-gray-200 rounded-full h-2.5 mt-4">
          <div
            class="bg-indigo-600 h-2.5 rounded-full transition-all duration-300"
            style="width: {uploadProgress}%"
          ></div>
        </div>
      {/if}

      <p
        class="text-sm text-center {statusMessage.includes('失敗')
          ? 'text-red-500'
          : 'text-indigo-600'}"
      >
        {statusMessage}
      </p>

      <div class="flex gap-3 mt-6">
        <button
          on:click={() => dispatch("close")}
          class="flex-1 py-2 px-4 border border-gray-300 rounded-lg text-gray-700 hover:bg-gray-50 transition-colors"
          disabled={isUploading}
        >
          取消
        </button>
        <button
          on:click={handleUpload}
          disabled={isUploading}
          class="flex-1 py-2 px-4 bg-indigo-600 text-white rounded-lg hover:bg-indigo-700 disabled:bg-indigo-300 transition-colors font-semibold"
        >
          {isUploading ? "處理中..." : "開始上傳"}
        </button>
      </div>
    </div>
  </div>
</div>
