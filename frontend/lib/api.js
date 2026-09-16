// Central API client. Talks to the ASP.NET Core backend and always sends the
// auth cookie (credentials: 'include'). Returns the ApiResponse envelope
// { success, message, errors, data } and throws on non-2xx with { status, body }.

export const API_BASE_URL =
  process.env.NEXT_PUBLIC_API_BASE_URL || "http://localhost:5000/api";

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
