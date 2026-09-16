"use client";

import { useEffect, useState } from "react";
import { useQuery } from "@tanstack/react-query";
import BattleCard from "./BattleCard";
import { apiFetch } from "@/lib/api";

const SORTS = [
  ["trending", "Trending"],
  ["most-voted", "Most Voted"],
  ["newest", "Newest"],
  ["ending-soon", "Ending Soon"],
];

export default function BattlesExplorer() {
  const [sort, setSort] = useState("trending");
  const [category, setCategory] = useState("");
  const [searchInput, setSearchInput] = useState("");
  const [search, setSearch] = useState("");
  const [page, setPage] = useState(1);

  // Debounce the search box.
  useEffect(() => {
    const t = setTimeout(() => {
      setSearch(searchInput);
      setPage(1);
    }, 400);
    return () => clearTimeout(t);
  }, [searchInput]);

  const { data: catData } = useQuery({
    queryKey: ["categories"],
    queryFn: async () => (await apiFetch("/battles/categories")).data,
  });
  const categories = catData || [];

  const { data, isLoading, isError } = useQuery({
    queryKey: ["battles", { sort, category, search, page }],
    queryFn: async () => {
      const params = new URLSearchParams({ sort, page: String(page), pageSize: "12" });
      if (category) params.set("category", category);
      if (search) params.set("search", search);
      return (await apiFetch(`/battles?${params.toString()}`)).data;
    },
    placeholderData: (prev) => prev,
  });

  const items = data?.items || [];
  const totalPages = data?.totalPages || 1;

  return (
    <div>
      <div className="flex flex-wrap gap-3 items-center mb-6">
        <div className="flex gap-2 flex-wrap">
          {SORTS.map(([value, label]) => (
            <button
              key={value}
              onClick={() => {
                setSort(value);
                setPage(1);
              }}
              className={`chip ${sort === value ? "!bg-ink !text-onink" : ""}`}
            >
              {label}
            </button>
          ))}
        </div>
        <div className="flex-1" />
        <select
          className="input max-w-[180px]"
          value={category}
          onChange={(e) => {
            setCategory(e.target.value);
            setPage(1);
          }}
        >
          <option value="">All categories</option>
          {categories.map((c) => (
            <option key={c.id} value={c.slug}>{c.name}</option>
          ))}
        </select>
        <input
          className="input max-w-[220px]"
          placeholder="Search battles…"
          value={searchInput}
          onChange={(e) => setSearchInput(e.target.value)}
        />
      </div>

      {isLoading ? (
        <p className="text-muted">Loading…</p>
      ) : isError ? (
        <p className="text-muted">Could not load battles. Is the API running?</p>
      ) : items.length === 0 ? (
        <p className="text-muted">No battles match your filters.</p>
      ) : (
        <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
          {items.map((b) => (
            <BattleCard key={b.id} battle={b} />
          ))}
        </div>
      )}

      {totalPages > 1 && (
        <div className="flex items-center justify-center gap-3 mt-8">
          <button className="btn btn-ghost" disabled={page <= 1} onClick={() => setPage((p) => p - 1)}>
            ← Prev
          </button>
          <span className="text-muted text-sm">Page {page} of {totalPages}</span>
          <button
            className="btn btn-ghost"
            disabled={page >= totalPages}
            onClick={() => setPage((p) => p + 1)}
          >
            Next →
          </button>
        </div>
      )}
    </div>
  );
}
