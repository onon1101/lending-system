<script>
  import { page } from "$app/stores";
  import ItemDetailPage from "../../../../../pages/ItemDetailPage.svelte";
</script>

<svelte:head>
  <title>物品細節 | Lending System</title>
</svelte:head>

<ItemDetailPage username={$page.params.username} objectName={$page.params.objectName} />
