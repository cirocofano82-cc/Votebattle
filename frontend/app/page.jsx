import Link from "next/link";
import { HomeFeatured, HomeTrending } from "@/components/HomeBattles";
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
              Put two contenders head-to-head and back the one you believe in — cast as
              many Vote Credits as you want. New players get <strong>5 free credits</strong>.
            </p>
            <div className="flex gap-3 mt-6 flex-wrap">
              <Link href="/battles" className="btn btn-ink">Explore Battles →</Link>
            </div>
          </div>

          <HomeFeatured initialBattles={battles} />
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

        <HomeTrending initialBattles={battles} />
      </section>
    </>
  );
}
