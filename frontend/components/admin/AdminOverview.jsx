"use client";

import { useQuery } from "@tanstack/react-query";
import { apiFetch } from "@/lib/api";
import { formatNumber } from "@/lib/format";

export default function AdminOverview() {
  const { data: d } = useQuery({
    queryKey: ["admin-dashboard"],
    queryFn: async () => (await apiFetch("/admin/analytics/dashboard")).data,
  });
  const { data: sec } = useQuery({
    queryKey: ["admin-security"],
    queryFn: async () => (await apiFetch("/admin/analytics/security")).data,
  });

  if (!d) return <p className="text-muted">Loading…</p>;

  const tiles = [
    { label: "Total Revenue", value: `$${d.totalRevenue}` },
    { label: "Revenue Today", value: `$${d.revenueToday}` },
    { label: "Total Users", value: formatNumber(d.totalUsers) },
    { label: "Paying Users", value: formatNumber(d.payingUsers) },
    { label: "Total Votes", value: formatNumber(d.totalVotes) },
    { label: "Votes Today", value: formatNumber(d.votesToday) },
    { label: "Free→Paid", value: `${d.freeToPaidConversionRate}%` },
    { label: "Avg Order Value", value: `$${d.averageOrderValue}` },
  ];

  return (
    <div>
      <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
        {tiles.map((t) => (
          <div key={t.label} className="card p-4">
            <div className="text-xs font-semibold uppercase tracking-wide text-muted">{t.label}</div>
            <div className="font-display text-2xl mt-1">{t.value}</div>
          </div>
        ))}
      </div>

      <div className="grid md:grid-cols-2 gap-6 mt-6">
        <div className="card p-5">
          <h3 className="font-display text-xl mb-3">Top Battles</h3>
          <div className="divide-y divide-line">
            {(d.topBattles || []).map((b) => (
              <div key={b.slug} className="flex justify-between py-2">
                <span className="truncate mr-2">{b.title}</span>
                <span className="text-muted tnum">{formatNumber(b.totalVotes)} votes</span>
              </div>
            ))}
          </div>
        </div>

        <div className="card p-5">
          <h3 className="font-display text-xl mb-3">Security &amp; Abuse</h3>
          {sec ? (
            <div className="grid grid-cols-2 gap-3 text-sm">
              <Stat label="Banned" value={sec.bannedUsers} />
              <Stat label="Suspended" value={sec.suspendedUsers} />
              <Stat label="Suspicious" value={sec.suspiciousUsers} />
              <Stat label="New (7d)" value={sec.recentRegistrations} />
              <Stat label="Failed logins (24h)" value={sec.failedLogins24h} />
              <Stat label="Bonuses granted" value={sec.registrationBonusesGranted} />
            </div>
          ) : (
            <p className="text-muted">Loading…</p>
          )}
        </div>
      </div>
    </div>
  );
}

function Stat({ label, value }) {
  return (
    <div className="flex justify-between border-b border-line pb-1">
      <span className="text-muted">{label}</span>
      <span className="font-semibold tnum">{formatNumber(value)}</span>
    </div>
  );
}
