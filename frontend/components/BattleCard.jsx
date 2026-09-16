import Link from "next/link";
import ContenderAvatar from "./ContenderAvatar";
import VersusBar from "./VersusBar";
import { formatNumber, formatPercent } from "@/lib/format";

export default function BattleCard({ battle }) {
  const [a, b] = battle.participants || [];

  return (
    <Link
      href={`/battle/${battle.slug}`}
      className="card overflow-hidden no-underline text-text hover:border-line-strong"
    >
      <div className="flex items-center justify-between px-4 pt-3">
        <span className="chip">{battle.categoryName}</span>
        <span className="chip tnum">{formatNumber(battle.totalVotes)} votes</span>
      </div>

      <div className="grid grid-cols-[1fr_auto_1fr] items-center gap-2 p-4">
        <div className="flex flex-col items-center text-center gap-2">
          <ContenderAvatar name={a?.name} imageUrl={a?.imageUrl} side="a" size="sm" />
          <span className="font-semibold text-sm leading-tight">{a?.name}</span>
        </div>
        <span className="font-display text-muted text-lg">VS</span>
        <div className="flex flex-col items-center text-center gap-2">
          <ContenderAvatar name={b?.name} imageUrl={b?.imageUrl} side="b" size="sm" />
          <span className="font-semibold text-sm leading-tight">{b?.name}</span>
        </div>
      </div>

      <div className="px-4">
        <VersusBar pa={a?.percentage ?? 0} />
      </div>
      <div className="flex justify-between px-4 py-3 font-semibold text-sm">
        <span className="text-sidea">{formatPercent(a?.percentage)}</span>
        <span className="text-sideb">{formatPercent(b?.percentage)}</span>
      </div>
    </Link>
  );
}
