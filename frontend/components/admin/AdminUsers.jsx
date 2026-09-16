"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api";
import { formatNumber } from "@/lib/format";

export default function AdminUsers() {
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [busy, setBusy] = useState(null);

  const { data, refetch, isLoading } = useQuery({
    queryKey: ["admin-users", search],
    queryFn: async () => {
      const params = new URLSearchParams({ pageSize: "50" });
      if (search) params.set("search", search);
      return (await apiFetch(`/admin/users?${params.toString()}`)).data;
    },
  });

  async function setStatus(id, action) {
    setBusy(id + action);
    try {
      await apiFetch(`/admin/users/${id}/status`, { method: "POST", body: { action } });
      await refetch();
    } finally {
      setBusy(null);
    }
  }

  const items = data?.items || [];

  return (
    <div>
      <form
        className="flex gap-2 mb-4"
        onSubmit={(e) => { e.preventDefault(); setSearch(searchInput); }}
      >
        <input className="input max-w-[280px]" placeholder="Search username or email…" value={searchInput} onChange={(e) => setSearchInput(e.target.value)} />
        <button className="btn btn-ghost">Search</button>
      </form>

      {isLoading ? <p className="text-muted">Loading…</p> : (
        <div className="card divide-y divide-line">
          {items.map((u) => (
            <div key={u.id} className="flex items-center justify-between gap-3 px-4 py-3 flex-wrap">
              <div className="min-w-0">
                <div className="font-semibold truncate">{u.username} <span className="text-muted font-normal">· {u.email}</span></div>
                <div className="text-muted text-sm">{u.status} · ⚡ {formatNumber(u.voteCredits)}</div>
              </div>
              <div className="flex gap-2">
                {u.status === "Active" && (
                  <>
                    <button className="btn btn-ghost" disabled={busy === u.id + "Suspend"} onClick={() => setStatus(u.id, "Suspend")}>Suspend</button>
                    <button className="btn btn-b" disabled={busy === u.id + "Ban"} onClick={() => setStatus(u.id, "Ban")}>Ban</button>
                  </>
                )}
                {(u.status === "Suspended" || u.status === "Banned") && (
                  <button className="btn btn-a" disabled={busy === u.id + "Activate"} onClick={() => setStatus(u.id, "Activate")}>Reactivate</button>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}
