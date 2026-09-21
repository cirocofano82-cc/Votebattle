"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import BattleCard from "./BattleCard";
import ContenderAvatar from "./ContenderAvatar";
import VersusBar from "./VersusBar";
import Spinner from "./Spinner";
import { apiFetch } from "@/lib/api";
import { formatNumber, formatPercent, categoryEmoji } from "@/lib/format";

// Shared trending query. Seeded with the SSR result when it has data (good for
// SEO / instant paint). When the SSR fetch came back empty — typically because
// the API was still starting up — we leave it unseeded so the client fetches,
// shows a spinner, and keeps retrying until the API answers.
function useTrending(initialBattles) {
  const seed = initialBattles?.length ? initialBattles : undefined;
  return useQuery({
    queryKey: ["home-trending"],
    queryFn: async () =>
      (await apiFetch("/battles?sort=trending&pageSize=7")).data?.items ?? [],
    // Seed with the SSR result for an instant first paint / SEO, but treat it as
    // stale so the client immediately refetches fresh data (e.g. newly uploaded
    // images) instead of showing a cached snapshot.
    initialData: seed,
    initialDataUpdatedAt: 0,
    staleTime: 0,
    refetchOnMount: "always",
    retry: 3,
    retryDelay: 1500,
    // Keep polling while the API is unreachable; stop once a request succeeds.
    refetchInterval: (query) => (query.state.status === "error" ? 3000 : false),
  });
}

export function HomeFeatured({ initialBattles }) {
  const { data = [], isSuccess } = useTrending(initialBattles);
  const featured = data[0];

  if (featured) return <FeaturedCard battle={featured} />;
  if (isSuccess) return <EmptyFeatured />; // fetched OK, nothing to show yet

  return (
    <div className="card p-8 flex flex-col items-center justify-center gap-3 min-h-[280px]">
      <Spinner size={32} />
      <p className="text-muted">Loading battles…</p>
    </div>
  );
}

export function HomeTrending({ initialBattles }) {
  const { data = [], isSuccess } = useTrending(initialBattles);
  const rest = data.slice(1);

  if (rest.length > 0) {
    return (
      <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
        {rest.map((b) => (
          <BattleCard key={b.id} battle={b} />
        ))}
      </div>
    );
  }

  if (isSuccess) {
    return <p className="text-muted">No battles to show yet.</p>;
  }

  return (
    <div className="flex flex-col items-center justify-center gap-3 py-10">
      <Spinner size={32} />
      <p className="text-muted">Loading battles…</p>
    </div>
  );
}

function FeaturedCard({ battle }) {
  const [a, b] = battle.participants || [];
  return (
    <div className="card overflow-hidden">
      <div className="flex items-center justify-between px-4 pt-4">
        <span className="chip">{categoryEmoji(battle.categoryName)} {battle.categoryName}</span>
        <span className="chip">🔥 Trending #1</span>
      </div>
      <div className="grid grid-cols-[1fr_auto_1fr] items-center gap-2 p-4">
        <div className="flex flex-col items-center text-center gap-2">
          <ContenderAvatar name={a?.name} imageUrl={a?.imageUrl} side="a" size="lg" />
          <span className="font-semibold">{a?.name}</span>
        </div>
        <span className="font-display text-muted text-xl">VS</span>
        <div className="flex flex-col items-center text-center gap-2">
          <ContenderAvatar name={b?.name} imageUrl={b?.imageUrl} side="b" size="lg" />
          <span className="font-semibold">{b?.name}</span>
        </div>
      </div>
      <div className="px-4"><VersusBar pa={a?.percentage ?? 0} /></div>
      <div className="flex justify-between px-4 pt-2 font-bold">
        <span className="text-sidea">{formatPercent(a?.percentage)}</span>
        <span className="text-sideb">{formatPercent(b?.percentage)}</span>
      </div>
      <div className="flex justify-between px-4 pt-1 text-sm text-muted tnum">
        <span>{formatNumber(a?.voteCount)} votes</span>
        <span>{formatNumber(b?.voteCount)} votes</span>
      </div>
      <div className="p-4">
        <Link href={`/battle/${battle.slug}`} className="btn btn-ink w-full">
          Vote in this battle →
        </Link>
      </div>
    </div>
  );
}

function EmptyFeatured() {
  return (
    <div className="card p-8 text-center text-muted">
      <p className="font-display text-2xl text-text mb-2">No featured battle</p>
      <p>Battles will appear here once they go live.</p>
    </div>
  );
}
