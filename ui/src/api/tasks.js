import { apiFetch } from "./client";

export function getTasks({ status, assignee, q, page, pageSize } = {}) {
  const params = new URLSearchParams();

  if (status) params.set("status", status);
  if (assignee) params.set("assignee", String(assignee));
  if (q) params.set("q", q);

  if (page) params.set("page", String(page));
  if (pageSize) params.set("pageSize", String(pageSize));

  const qs = params.toString();
  return apiFetch(`/tasks${qs ? `?${qs}` : ""}`);
}

export function createTask(payload) {
  return apiFetch("/tasks", { method: "POST", body: JSON.stringify(payload) });
}

export function updateTask(id, payload) {
  return apiFetch(`/tasks/${id}`, {
    method: "PUT",
    body: JSON.stringify(payload),
  });
}

export function patchTaskStatus(id, status) {
  return apiFetch(`/tasks/${id}/status`, {
    method: "PATCH",
    body: JSON.stringify({ status }),
  });
}
