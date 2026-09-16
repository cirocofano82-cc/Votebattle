"use client";

import { useState } from "react";
import Link from "next/link";
import AuthCard from "@/components/AuthCard";
import { apiFetch } from "@/lib/api";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [loading, setLoading] = useState(false);
  const [sent, setSent] = useState(false);

  async function submit(e) {
    e.preventDefault();
    setLoading(true);
    try {
      await apiFetch("/auth/forgot-password", { method: "POST", body: { email } });
    } catch {
      // generic response either way
    } finally {
      setLoading(false);
      setSent(true);
    }
  }

  if (sent) {
    return (
      <AuthCard title="Check your email" subtitle="If an account exists, a reset link is on its way.">
        <Link href="/login" className="btn btn-ghost w-full">Back to sign in</Link>
      </AuthCard>
    );
  }

  return (
    <AuthCard
      title="Forgot password"
      subtitle="We'll email you a link to reset it."
      footer={<><Link href="/login" className="text-sidea no-underline font-semibold">Back to sign in</Link></>}
    >
      <form onSubmit={submit} className="flex flex-col gap-3">
        <div>
          <label className="label" htmlFor="email">Email</label>
          <input id="email" type="email" className="input" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <button className="btn btn-ink w-full" disabled={loading}>
          {loading ? "Sending…" : "Send reset link"}
        </button>
      </form>
    </AuthCard>
  );
}
