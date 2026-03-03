import { apiFetch } from "./client";

export function getUsers() {
  return apiFetch("/users");
}
