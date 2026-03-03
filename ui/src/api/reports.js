import { apiFetch } from "./client";

export async function getOpenTasksByUser() {
  // apiFetch: { success, data, error } döndürür
  return apiFetch("/reports/open-tasks-by-user");
}
