"use client";

import { useState } from "react";
import Link from "next/link";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";
import ImageUploadField from "@/components/ImageUploadField";

export default function CreateBattlePage() {
  const { user, loading: authLoading } = useAuth();
  const { data: categories } = useQuery({
    queryKey: ["categories"],
    queryFn: async () => (await apiFetch("/battles/categories")).data,
  });

  const [form, setForm] = useState({
    title: "",
    categoryId: "",
    description: "",
    aName: "",
    aImage: "",
    bName: "",
    bImage: "",
  });
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState(null);
  const [errors, setErrors] = useState([]);
  const [done, setDone] = useState(null);

  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.value }));

  async function submit(e) {
    e.preventDefault();
    setError(null);
    setErrors([]);
    setLoading(true);
    try {
      const res = await apiFetch("/battles", {
        method: "POST",
        body: {
          title: form.title,
          categoryId: Number(form.categoryId),
          description: form.description || null,
          competitorA: { name: form.aName, imageUrl: form.aImage || null },
          competitorB: { name: form.bName, imageUrl: form.bImage || null },
        },
      });
      setDone({ message: res.message || "Battle created.", slug: res.data });
    } catch (err) {
      setError(err.message || "Could not create the battle.");
      setErrors(err.body?.errors || []);
    } finally {
      setLoading(false);
    }
  }

  if (!authLoading && !user) {
    return (
      <div className="container-page py-12 text-center">
        <h1 className="font-display text-4xl mb-2">Create a Battle</h1>
        <p className="text-muted mb-5">Sign in to create a battle.</p>
        <Link href="/login" className="btn btn-ink">Sign in</Link>
      </div>
    );
  }

  // Battle creation is currently restricted to admins.
  if (!authLoading && user && !user.roles?.includes("Admin")) {
    return (
      <div className="container-page py-12 text-center">
        <h1 className="font-display text-4xl mb-2">Battle creation is closed</h1>
        <p className="text-muted mb-5">
          New battles aren&apos;t open to players right now. Explore the ones already live.
        </p>
        <Link href="/battles" className="btn btn-ink">Browse battles</Link>
      </div>
    );
  }

  if (done) {
    return (
      <div className="container-page py-12 max-w-[560px] mx-auto text-center">
        <h1 className="font-display text-4xl mb-2">Battle created! 🎉</h1>
        <p className="text-muted mb-5">{done.message}</p>
        <div className="flex gap-3 justify-center">
          {done.slug && (
            <Link href={`/battle/${done.slug}`} className="btn btn-ink">View battle</Link>
          )}
          <Link href="/battles" className="btn btn-ghost">Browse battles</Link>
          <button className="btn btn-ghost" onClick={() => { setDone(null); setForm({ title: "", categoryId: "", description: "", aName: "", aImage: "", bName: "", bImage: "" }); }}>
            Create another
          </button>
        </div>
      </div>
    );
  }

  return (
    <div className="container-page py-8 max-w-[680px] mx-auto">
      <h1 className="font-display text-4xl mb-1">Create a Battle</h1>
      <p className="text-muted mb-6">Pit two contenders head-to-head. As an admin, your battle goes live immediately.</p>

      <form onSubmit={submit} className="flex flex-col gap-4">
        <div>
          <label className="label" htmlFor="title">Title</label>
          <input id="title" className="input" placeholder="e.g. iPhone vs Android" value={form.title} onChange={set("title")} required minLength={5} maxLength={200} />
        </div>
        <div>
          <label className="label" htmlFor="category">Category</label>
          <select id="category" className="input" value={form.categoryId} onChange={set("categoryId")} required>
            <option value="">Select a category…</option>
            {(categories || []).map((c) => <option key={c.id} value={c.id}>{c.name}</option>)}
          </select>
        </div>
        <div>
          <label className="label" htmlFor="description">Description (optional)</label>
          <textarea id="description" className="input min-h-[70px]" value={form.description} onChange={set("description")} maxLength={4000} />
        </div>

        <div className="grid sm:grid-cols-2 gap-4">
          <div className="card p-4 flex flex-col gap-3 border-t-4 !border-t-sidea">
            <div className="font-display text-xl text-sidea">Contender A</div>
            <input className="input" placeholder="Name" value={form.aName} onChange={set("aName")} required maxLength={200} />
            <ImageUploadField
              label="Contender A image"
              value={form.aImage}
              onChange={(v) => setForm((f) => ({ ...f, aImage: v }))}
            />
          </div>
          <div className="card p-4 flex flex-col gap-3 border-t-4 !border-t-sideb">
            <div className="font-display text-xl text-sideb">Contender B</div>
            <input className="input" placeholder="Name" value={form.bName} onChange={set("bName")} required maxLength={200} />
            <ImageUploadField
              label="Contender B image"
              value={form.bImage}
              onChange={(v) => setForm((f) => ({ ...f, bImage: v }))}
            />
          </div>
        </div>

        {error && (
          <div className="text-sideb text-sm">
            {error}
            {errors.length > 0 && <ul className="list-disc ml-5 mt-1">{errors.map((x, i) => <li key={i}>{x}</li>)}</ul>}
          </div>
        )}

        <button className="btn btn-ink" disabled={loading}>
          {loading ? "Creating…" : "Create battle"}
        </button>
      </form>
    </div>
  );
}
