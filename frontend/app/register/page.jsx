"use client";

import { useState } from "react";
import Link from "next/link";
import AuthCard from "@/components/AuthCard";
import Turnstile from "@/components/Turnstile";
import { apiFetch } from "@/lib/api";

const SITE_KEY = process.env.NEXT_PUBLIC_TURNSTILE_SITE_KEY;

export default function RegisterPage() {
  const [email, setEmail] = useState("");
  const [username, setUsername] = useState("");
  const [password, setPassword] = useState("");
  const [captchaToken, setCaptchaToken] = useState("");
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [errors, setErrors] = useState([]);
  const [done, setDone] = useState(false);

  async function submit(e) {
    e.preventDefault();
    setError(null);
    setErrors([]);
    setLoading(true);
    try {
      await apiFetch("/auth/register", {
        method: "POST",
        body: { email, username, password, captchaToken: captchaToken || null },
      });
      setDone(true);
    } catch (err) {
      setError(err.message || "Registration failed.");
      setErrors(err.body?.errors || []);
    } finally {
      setLoading(false);
    }
  }

  if (done) {
    return (
      <AuthCard title="Check your email 📬" subtitle="You're almost in.">
        <p className="text-muted">
          We've sent a verification link to <strong className="text-text">{email}</strong>. Click it
          to activate your account and claim your <strong className="text-text">5 free Vote Credits</strong>.
        </p>
        <div className="mt-5">
          <Link href="/login" className="btn btn-ghost w-full">Back to sign in</Link>
        </div>
      </AuthCard>
    );
  }

  return (
    <AuthCard
      title="Join VoteBattle"
      subtitle="Create an account and get 5 free Vote Credits."
      footer={<>Already have an account? <Link href="/login" className="text-sidea no-underline font-semibold">Sign in</Link></>}
    >
      <form onSubmit={submit} className="flex flex-col gap-3">
        <div>
          <label className="label" htmlFor="username">Username</label>
          <input id="username" className="input" value={username} onChange={(e) => setUsername(e.target.value)} required minLength={3} maxLength={30} />
        </div>
        <div>
          <label className="label" htmlFor="email">Email</label>
          <input id="email" type="email" className="input" value={email} onChange={(e) => setEmail(e.target.value)} required />
        </div>
        <div>
          <label className="label" htmlFor="password">Password</label>
          <input id="password" type="password" className="input" value={password} onChange={(e) => setPassword(e.target.value)} required minLength={8} />
          <p className="text-muted text-xs mt-1">At least 8 characters.</p>
        </div>

        <Turnstile siteKey={SITE_KEY} onToken={setCaptchaToken} />

        {error && (
          <div className="text-sideb text-sm">
            {error}
            {errors.length > 0 && (
              <ul className="list-disc ml-5 mt-1">{errors.map((x, i) => <li key={i}>{x}</li>)}</ul>
            )}
          </div>
        )}

        <button className="btn btn-ink w-full mt-1" disabled={loading}>
          {loading ? "Creating account…" : "Create account"}
        </button>
      </form>
    </AuthCard>
  );
}
