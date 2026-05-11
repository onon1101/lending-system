import { mount } from "svelte";
import "../app.css";
import ItemDetailPage from "./ItemDetailPage.svelte";

const app = mount(ItemDetailPage, {
  target: document.getElementById("app"),
});

export default app;
