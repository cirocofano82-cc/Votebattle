import BattlesExplorer from "@/components/BattlesExplorer";

export const metadata = {
  title: "Explore Battles — VoteBattle",
  description: "Browse, filter and search head-to-head battles across every category.",
};

export default function BattlesPage() {
  return (
    <div className="container-page py-8">
      <div className="text-xs font-bold tracking-[0.16em] uppercase text-muted">Discover</div>
      <h1 className="font-display text-4xl mb-6">Explore Battles</h1>
      <BattlesExplorer />
    </div>
  );
}
