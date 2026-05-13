import { mount } from "svelte";
import "../app.css";
import MyPillowsPage from "./MyPillowsPage.svelte";

const app = mount(MyPillowsPage, {
  target: document.getElementById("app"),
});

export default app;
