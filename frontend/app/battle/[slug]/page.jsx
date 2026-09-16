import Link from "next/link";
import { notFound } from "next/navigation";
import VotePanel from "@/components/VotePanel";
import Comments from "@/components/Comments";
import ShareButtons from "@/components/ShareButtons";
import { API_BASE_URL } from "@/lib/api";

async function getBattle(slug) {
  try {
    const res = await fetch(`${API_BASE_URL}/battles/${slug}`, { next: { revalidate: 30 } });
    if (!res.ok) return null;
    const json = await res.json();
    return json.data;
  } catch {
    return null;
  }
}

export async function generateMetadata({ params }) {
  const battle = await getBattle(params.slug);
  if (!battle) return { title: "Battle not found — VoteBattle" };

  const description =
    battle.metaDescription || battle.description || `${battle.title} — vote and see who wins.`;

  return {
    title: `${battle.title} — VoteBattle`,
    description,
    alternates: { canonical: `/battle/${battle.slug}` },
    openGraph: {
      title: battle.title,
      description,
      type: "website",
      images: battle.ogImageUrl ? [battle.ogImageUrl] : [],
    },
  };
}

export default async function BattlePage({ params }) {
  const battle = await getBattle(params.slug);
  if (!battle) notFound();

  const [a, b] = battle.participants || [];

  return (
    <div className="container-page py-8">
      <div className="max-w-[760px] mx-auto flex flex-col gap-6">
        <div>
          <Link href="/battles" className="text-muted text-sm no-underline hover:text-text">
            ← All battles
          </Link>
          <div className="mt-2">
            <span className="chip">{battle.categoryName}</span>
          </div>
          <h1 className="font-display text-3xl md:text-5xl mt-3 leading-none">
            <span className="text-sidea">{a?.name}</span>{" "}
            <span className="text-muted">vs</span>{" "}
            <span className="text-sideb">{b?.name}</span>
          </h1>
          {battle.description && (
            <p className="text-muted mt-3 max-w-[65ch]">{battle.description}</p>
          )}
        </div>

        <VotePanel battle={battle} />
        <ShareButtons title={battle.title} />
        <Comments battleId={battle.id} />
      </div>
    </div>
  );
}
