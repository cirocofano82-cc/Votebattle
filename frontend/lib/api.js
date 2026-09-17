// Central API client. Talks to the ASP.NET Core backend and always sends the
// auth cookie (credentials: 'include'). Returns the ApiResponse envelope
// { success, message, errors, data } and throws on non-2xx with { status, body }.

export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5000/api";

// Base URL for server-side (SSR) fetches. In Docker the browser reaches the API
// at localhost:5000, but the web container reaches it internally at
// http://api:8080/api. Set INTERNAL_API_BASE_URL as a runtime env var for that.
export const SERVER_API_BASE_URL =
  process.env.INTERNAL_API_BASE_URL || API_BASE_URL;

export async function apiFetch(path, options = {}) {
  const { method = "GET", body, headers, cache, next } = options;

  const res = await fetch(`${API_BASE_URL}${path}`, {
    method,
    headers: {
      "Content-Type": "application/json",
      ...(headers || {}),
    },
    body: body !== undefined ? JSON.stringify(body) : undefined,
    credentials: "include",
    cache,
    next,
  });

  let data = null;
  try {
    data = await res.json();
  } catch {
    // no JSON body
  }

  if (!res.ok) {
    const error = new Error(data?.message || `Request failed (${res.status})`);
    error.status = res.status;
    error.body = data;
    throw error;
  }

  return data;
}

// Uploads an image file (multipart) and returns the served URL. Admin-only.
export async function uploadImage(file) {
  const form = new FormData();
  form.append("file", file);

  // Note: don't set Content-Type — the browser adds the multipart boundary.
  const res = await fetch(`${API_BASE_URL}/uploads/image`, {
    method: "POST",
    body: form,
    credentials: "include",
  });

  let data = null;
  try {
    data = await res.json();
  } catch {
    // no JSON body
  }

  if (!res.ok) {
    const error = new Error(data?.message || `Upload failed (${res.status})`);
    error.status = res.status;
    throw error;
  }

  return data.data.url;
}
