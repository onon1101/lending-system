<script>
  import CatalogView from "./components/CatalogView.svelte";
  import { getAllItems } from "./stores/api";
</script>

<CatalogView loadItems={getAllItems} />
