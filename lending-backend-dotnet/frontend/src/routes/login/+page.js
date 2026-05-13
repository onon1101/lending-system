import { redirect } from "@sveltejs/kit";

export function load() {
  if (import.meta.env.PROD) {
    throw redirect(302, "/developing");
  }
}
