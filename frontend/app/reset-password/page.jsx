"use client";

import { Suspense, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import AuthCard from "@/components/AuthCard";
import { apiFetch } from "@/lib/api";

function ResetPasswordInner() {
  const params = useSearchParams();
  const email = params.get("email") || "";
  const token = params.get("token") || "";

  const [newPassword, setNewPassword] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [done, setDone] = useState(false);

  async function submit(e) {
    e.preventDefault();
    setError(null);
    setLoading(true);
    try {
      await apiFetch("/auth/reset-password", {
        method: "POST",
        body: { email, token, newPassword },
      });
      setDone(true);
    } catch (err) {
      setError(err.message || "This reset link is invalid or has expired.");
    } finally {
      setLoading(false);
    }
  }

  if (done) {
    return (
      <AuthCard title="Password reset" subtitle="You can now sign in with your new password.">
        <Link href="/login" className="btn btn-ink w-full">Sign in</Link>
      </AuthCard>
    );
  }

  if (!token || !email) {
    return (
      <AuthCard title="Invalid reset link" subtitle="This link is missing information.">
        <Link href="/forgot-password" className="btn btn-ghost w-full">Request a new link</Link>
      </AuthCard>
    );
  }

  return (
    <AuthCard title="Set a new password" subtitle={`For ${email}`}>
      <form onSubmit={submit} className="flex flex-col gap-3">
        <div>
          <label className="label" htmlFor="newPassword">New password</label>
          <input id="newPassword" type="password" className="input" value={newPassword} onChange={(e) => setNewPassword(e.target.value)} required minLength={8} />
          <p className="text-muted text-xs mt-1">At least 8 characters.</p>
        </div>
        {error && <p className="text-sideb text-sm">{error}</p>}
        <button className="btn btn-ink w-full" disabled={loading}>
          {loading ? "Saving…" : "Reset password"}
        </button>
      </form>
    </AuthCard>
  );
}

export default function ResetPasswordPage() {
  return (
    <Suspense fallback={<AuthCard title="Loading…" />}>
      <ResetPasswordInner />
    </Suspense>
  );
}
