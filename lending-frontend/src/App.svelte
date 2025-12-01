<script>
  import ItemList from "./pages/ItemPage.svelte";
  import ItemDetailPage from "./pages/ItemDetailPage.svelte";
  const product = {
    id: "CAM-001",
    name: "Sony Alpha a7 III Kit",
    status: "可借出",
    description:
      "包含 28-70mm 鏡頭的全片幅無反光鏡相機，適合活動攝影與錄影使用。附帶兩顆電池與充電器。",
  };
</script>

<!-- <ItemList /> -->
<ItemDetailPage productData={product} />
