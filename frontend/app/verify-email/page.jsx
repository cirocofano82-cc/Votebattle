"use client";

import { Suspense, useEffect, useRef, useState } from "react";
import Link from "next/link";
import { useSearchParams } from "next/navigation";
import AuthCard from "@/components/AuthCard";
import { apiFetch } from "@/lib/api";

function VerifyEmailInner() {
  const params = useSearchParams();
  const token = params.get("token");
  const [status, setStatus] = useState("loading"); // loading | success | error | resend
  const [message, setMessage] = useState("");
  const [resendEmail, setResendEmail] = useState("");
  const [resent, setResent] = useState(false);
  const ran = useRef(false);

  useEffect(() => {
    if (ran.current) return;
    ran.current = true;

    if (!token) {
      setStatus("resend");
      return;
    }
    (async () => {
      try {
        const res = await apiFetch("/auth/verify-email", { method: "POST", body: { token } });
        setMessage(res.message || "");
        setStatus("success");
      } catch (err) {
        setMessage(err.message || "This verification link is invalid or has expired.");
        setStatus("error");
      }
    })();
  }, [token]);

  async function resend(e) {
    e.preventDefault();
    try {
      await apiFetch("/auth/resend-verification", { method: "POST", body: { email: resendEmail } });
    } catch {
      // generic response either way
    }
    setResent(true);
  }

  if (status === "loading") {
    return <AuthCard title="Verifying…" subtitle="One moment while we confirm your email." />;
  }

  if (status === "success") {
    return (
      <AuthCard title="Welcome to VoteBattle! 🎉" subtitle="Your email is verified.">
        <p className="text-muted">
          You've received <strong className="text-text">5 free Vote Credits</strong>. Sign in to
          make your voice count.
        </p>
        <div className="mt-5 flex flex-col gap-2">
          <Link href="/login" className="btn btn-ink w-full">Sign in & start voting</Link>
          <Link href="/battles" className="btn btn-ghost w-full">Browse battles</Link>
        </div>
      </AuthCard>
    );
  }

  // error or resend
  return (
    <AuthCard title={status === "error" ? "Link not valid" : "Verify your email"} subtitle={status === "error" ? message : "Enter your email to get a new verification link."}>
      {resent ? (
        <p className="text-muted">
          If an unverified account exists for that email, a new verification link has been sent.
        </p>
      ) : (
        <form onSubmit={resend} className="flex flex-col gap-3">
          <div>
            <label className="label" htmlFor="email">Email</label>
            <input id="email" type="email" className="input" value={resendEmail} onChange={(e) => setResendEmail(e.target.value)} required />
          </div>
          <button className="btn btn-ink w-full">Resend verification link</button>
        </form>
      )}
      <div className="mt-4 text-center">
        <Link href="/login" className="text-muted text-sm no-underline hover:text-text">Back to sign in</Link>
      </div>
    </AuthCard>
  );
}

export default function VerifyEmailPage() {
  return (
    <Suspense fallback={<AuthCard title="Verifying…" />}>
      <VerifyEmailInner />
    </Suspense>
  );
}
