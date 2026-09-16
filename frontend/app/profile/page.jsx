"use client";

import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";
import { formatNumber, initials, timeAgo } from "@/lib/format";

export default function ProfilePage() {
  const { user, loading: authLoading } = useAuth();

  const { data: profile } = useQuery({
    queryKey: ["profile"],
    queryFn: async () => (await apiFetch("/users/me/profile")).data,
    enabled: !!user,
  });

  const { data: votes } = useQuery({
    queryKey: ["my-votes"],
    queryFn: async () => (await apiFetch("/users/me/votes?pageSize=10")).data,
    enabled: !!user,
  });

  if (!authLoading && !user) {
    return (
      <div className="container-page py-12 text-center">
        <p className="text-muted mb-4">Sign in to view your profile.</p>
        <Link href="/login" className="btn btn-ink">Sign in</Link>
      </div>
    );
  }

  if (!profile) {
    return <div className="container-page py-12 text-muted">Loading…</div>;
  }

  const stats = [
    { label: "Vote Credits", value: `⚡ ${formatNumber(profile.voteCredits)}` },
    { label: "Total Votes", value: formatNumber(profile.totalVotes) },
    { label: "Comments", value: formatNumber(profile.totalComments) },
    { label: "Total Spent", value: `$${profile.totalSpent}` },
  ];

  return (
    <div className="container-page py-8 max-w-[760px] mx-auto">
      <div className="card p-6 flex items-center gap-4">
        <div className="w-16 h-16 rounded-full bg-surface2 grid place-items-center font-display text-2xl text-muted overflow-hidden">
          {profile.avatarUrl ? (
            // eslint-disable-next-line @next/next/no-img-element
            <img src={profile.avatarUrl} alt={profile.username} className="w-full h-full object-cover" />
          ) : (
            initials(profile.username)
          )}
        </div>
        <div>
          <h1 className="font-display text-3xl leading-none">{profile.username}</h1>
          <p className="text-muted text-sm mt-1">
            Member since {new Date(profile.createdAt).toLocaleDateString()} · {profile.status}
          </p>
        </div>
        <div className="flex-1" />
        <Link href="/credits" className="btn btn-gold">Get Credits</Link>
      </div>

      <div className="grid grid-cols-2 sm:grid-cols-4 gap-4 mt-6">
        {stats.map((s) => (
          <div key={s.label} className="card p-4">
            <div className="text-xs font-semibold uppercase tracking-wide text-muted">{s.label}</div>
            <div className="font-display text-2xl mt-1">{s.value}</div>
          </div>
        ))}
      </div>

      <h2 className="font-display text-2xl mt-8 mb-3">Recent Votes</h2>
      <div className="card divide-y divide-line">
        {(votes?.items || []).length === 0 ? (
          <p className="text-muted p-5">No votes yet. <Link href="/battles" className="text-sidea no-underline">Find a battle</Link>.</p>
        ) : (
          votes.items.map((v) => (
            <Link key={v.id} href={`/battle/${v.battleSlug}`} className="flex items-center justify-between px-5 py-3 no-underline text-text hover:bg-surface2">
              <div>
                <div className="font-medium">
                  Voted for <span className="text-sidea">{v.participantName}</span>
                </div>
                <div className="text-muted text-sm">{v.battleTitle}</div>
              </div>
              <div className="text-muted text-sm">{timeAgo(v.createdAt)}</div>
            </Link>
          ))
        )}
      </div>
    </div>
  );
}
