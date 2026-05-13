<script>
  import CatalogView from "../components/CatalogView.svelte";
  import { getItemsByUserName } from "../stores/api";

  export let username;

  async function loadUserItems() {
    return getItemsByUserName(username);
  }
</script>

<CatalogView
  title={`使用者 ${username} 的所有物品`}
  statLabel="使用者物品"
  loadItems={loadUserItems}
/>
