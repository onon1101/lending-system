<script>
  import { page } from "$app/stores";
  import UserItemsPage from "../../../pages/UserItemsPage.svelte";
</script>

<svelte:head>
  <title>使用者物品 | Lending System</title>
</svelte:head>

<UserItemsPage username={$page.params.username} />
