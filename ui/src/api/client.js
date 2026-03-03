const BASE_URL = import.meta.env.VITE_API_BASE_URL;

export async function apiFetch(path, options = {}) {
  const res = await fetch(`${BASE_URL}${path}`, {
    headers: {
      "Content-Type": "application/json",
      ...(options.headers || {}),
    },
    ...options,
  });

  const text = await res.text();
  const payload = text ? safeJsonParse(text) : null;

  if (!res.ok) {
    const msg =
      payload?.error?.message || payload?.message || `HTTP ${res.status}`;
    throw new Error(msg);
  }

  if (payload && payload.success === false) {
    throw new Error(payload?.error?.message || "Request failed");
  }

  return payload; // { success, data, error }
}

function safeJsonParse(text) {
  try {
    return JSON.parse(text);
  } catch {
    return { raw: text };
  }
}
