import Link from "next/link";
import BattleCard from "@/components/BattleCard";
import ContenderAvatar from "@/components/ContenderAvatar";
import VersusBar from "@/components/VersusBar";
import { formatNumber, formatPercent } from "@/lib/format";
import { SERVER_API_BASE_URL } from "@/lib/api";

async function getTrending() {
  try {
    const res = await fetch(`${SERVER_API_BASE_URL}/battles?sort=trending&pageSize=7`, {
      next: { revalidate: 60 },
    });
    if (!res.ok) return [];
    const json = await res.json();
    return json.data?.items ?? [];
  } catch {
    return [];
  }
}

export default async function Home() {
  const battles = await getTrending();
  const featured = battles[0];
  const rest = battles.slice(1);

  return (
    <>
      <section className="container-page pt-10 md:pt-16">
        <div className="grid md:grid-cols-2 gap-8 md:gap-12 items-center">
          <div>
            <span className="chip">🥊 Head-to-head, decided by the crowd</span>
            <h1 className="font-display text-5xl md:text-7xl leading-[0.92] mt-4">
              PROVE<br />YOUR <span className="text-sidea">SI</span>
              <span className="text-sideb">DE</span>.
            </h1>
            <p className="text-muted text-lg mt-4 max-w-[52ch]">
              Put two contenders head-to-head and back the one you believe in — one Vote
              Credit at a time. New players get <strong>5 free credits</strong>.
            </p>
            <div className="flex gap-3 mt-6 flex-wrap">
              <Link href="/battles" className="btn btn-ink">Explore Battles →</Link>
              <Link href="/create-battle" className="btn btn-ghost">Create a Battle</Link>
            </div>
          </div>

          {featured ? <FeaturedCard battle={featured} /> : <EmptyFeatured />}
        </div>
      </section>

      <section className="container-page py-12 md:py-20">
        <div className="flex items-end justify-between gap-4 mb-6 flex-wrap">
          <div>
            <div className="text-xs font-bold tracking-[0.16em] uppercase text-muted">Discover</div>
            <h2 className="font-display text-3xl md:text-4xl">Trending Battles</h2>
          </div>
          <Link href="/battles" className="btn btn-ghost">View all</Link>
        </div>

        {rest.length > 0 ? (
          <div className="grid gap-4 sm:grid-cols-2 lg:grid-cols-3">
            {rest.map((b) => (
              <BattleCard key={b.id} battle={b} />
            ))}
          </div>
        ) : (
          <p className="text-muted">No battles to show yet. The API may still be starting up.</p>
        )}
      </section>
    </>
  );
}

function FeaturedCard({ battle }) {
  const [a, b] = battle.participants || [];
  return (
    <div className="card overflow-hidden">
      <div className="flex items-center justify-between px-4 pt-4">
        <span className="chip">{battle.categoryName}</span>
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
      <p>Start the backend API to see live battles here.</p>
    </div>
  );
}
