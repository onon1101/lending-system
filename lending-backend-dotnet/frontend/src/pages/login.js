import { mount } from "svelte";
import "../app.css";
import LoginPage from "./LoginPage.svelte";

const app = mount(LoginPage, {
  target: document.getElementById("app"),
});

export default app;
