"use client";

import { useState } from "react";
import Link from "next/link";
import { useAuth } from "@/context/AuthContext";
import AdminOverview from "@/components/admin/AdminOverview";
import AdminBattles from "@/components/admin/AdminBattles";
import AdminReports from "@/components/admin/AdminReports";
import AdminUsers from "@/components/admin/AdminUsers";
import AdminPackages from "@/components/admin/AdminPackages";

const TABS = [
  ["overview", "Overview"],
  ["battles", "Battles"],
  ["reports", "Reports"],
  ["users", "Users"],
  ["packages", "Vote Packages"],
];

export default function AdminPage() {
  const { user, loading } = useAuth();
  const [tab, setTab] = useState("overview");

  if (loading) return <div className="container-page py-12 text-muted">Loading…</div>;

  if (!user || !user.roles?.includes("Admin")) {
    return (
      <div className="container-page py-12 text-center">
        <h1 className="font-display text-4xl mb-2">Admin</h1>
        <p className="text-muted mb-5">You need admin access to view this page.</p>
        <Link href="/" className="btn btn-ink">Back home</Link>
      </div>
    );
  }

  return (
    <div className="container-page py-8">
      <h1 className="font-display text-4xl mb-5">Admin Dashboard</h1>

      <div className="flex gap-2 flex-wrap mb-6 border-b border-line pb-3">
        {TABS.map(([value, label]) => (
          <button
            key={value}
            className={`chip ${tab === value ? "!bg-ink !text-onink" : ""}`}
            onClick={() => setTab(value)}
          >
            {label}
          </button>
        ))}
      </div>

      {tab === "overview" && <AdminOverview />}
      {tab === "battles" && <AdminBattles />}
      {tab === "reports" && <AdminReports />}
      {tab === "users" && <AdminUsers />}
      {tab === "packages" && <AdminPackages />}
    </div>
  );
}
