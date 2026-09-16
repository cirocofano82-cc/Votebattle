"use client";

import { useState } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import AuthCard from "@/components/AuthCard";
import { useAuth } from "@/context/AuthContext";

export default function LoginPage() {
  const router = useRouter();
  const { login } = useAuth();

  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [rememberMe, setRememberMe] = useState(true);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [needsVerification, setNeedsVerification] = useState(false);

  async function submit(e) {
    e.preventDefault();
    setError(null);
    setNeedsVerification(false);
    setLoading(true);
    try {
      await login(email, password, rememberMe);
      router.push("/");
    } catch (err) {
      if (err.status === 403) {
        setNeedsVerification(true);
        setError(err.message || "Please verify your email before signing in.");
      } else {
        setError(err.message || "Invalid email or password.");
      }
    } finally {
      setLoading(false);
    }
  }

  return (
    <AuthCard
      title="Welcome back"
      subtitle="Sign in to keep backing your side."
      footer={<>New here? <Link href="/register" className="text-sidea no-underline font-semibold">Create an account</Link></>}
    >
      <form onSubmit={submit} className="flex flex-col gap-3">
        <div>
          <label className="label" htmlFor="email">Email</label>
          <input id="email" type="email" className="input" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <div>
          <label className="label" htmlFor="password">Password</label>
          <input id="password" type="password" className="input" value={password} onChange={(e) => setPassword(e.target.value)} required />
        </div>

        <label className="flex items-center gap-2 text-sm text-muted">
          <input type="checkbox" checked={rememberMe} onChange={(e) => setRememberMe(e.target.checked)} />
          Remember me
        </label>

        {error && (
          <div className="text-sideb text-sm">
            {error}
            {needsVerification && (
              <div className="mt-1">
                <Link href="/verify-email" className="text-sidea no-underline font-semibold">Resend verification</Link>
              </div>
            )}
          </div>
        )}

        <button className="btn btn-ink w-full mt-1" disabled={loading}>
          {loading ? "Signing in…" : "Sign in"}
        </button>

        <div className="text-center">
          <Link href="/forgot-password" className="text-muted text-sm no-underline hover:text-text">
            Forgot your password?
          </Link>
        </div>
      </form>
    </AuthCard>
  );
}
