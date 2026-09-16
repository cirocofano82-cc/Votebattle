"use client";

import { useEffect, useState } from "react";
import Link from "next/link";
import { useRouter, useParams } from "next/navigation";
import { useQuery } from "@tanstack/react-query";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";

export default function EditBattlePage() {
  const router = useRouter();
  const { id } = useParams();
  const { user, loading: authLoading } = useAuth();

  const { data: categories } = useQuery({
    queryKey: ["categories"],
    queryFn: async () => (await apiFetch("/battles/categories")).data,
  });

  const [form, setForm] = useState(null);
  const [loadError, setLoadError] = useState(null);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);
  const [errors, setErrors] = useState([]);

  useEffect(() => {
    if (authLoading || !user) return;
    (async () => {
      try {
        const res = await apiFetch(`/battles/${id}/edit`);
        const b = res.data;
        const a = b.participants.find((p) => p.position === 1) || b.participants[0];
        const bb = b.participants.find((p) => p.position === 2) || b.participants[1];
        setForm({
          slug: b.slug,
          title: b.title,
          categoryId: String(b.categoryId),
          description: b.description || "",
          aName: a?.name || "",
          aImage: a?.imageUrl || "",
          bName: bb?.name || "",
          bImage: bb?.imageUrl || "",
        });
      } catch (e) {
        setLoadError(e.message || "Could not load this battle.");
      }
    })();
  }, [id, user, authLoading]);

  const set = (k) => (e) => setForm((f) => ({ ...f, [k]: e.target.value }));

  async function submit(e) {
    e.preventDefault();
    setError(null);
    setErrors([]);
    setSaving(true);
    try {
      const res = await apiFetch(`/battles/${id}`, {
        method: "PUT",
        body: {
          title: form.title,
          categoryId: Number(form.categoryId),
          description: form.description || null,
          competitorA: { name: form.aName, imageUrl: form.aImage || null },
          competitorB: { name: form.bName, imageUrl: form.bImage || null },
        },
      });
      router.push(`/battle/${res.data}`);
    } catch (err) {
      setError(err.message || "Could not save the battle.");
      setErrors(err.body?.errors || []);
      setSaving(false);
    }
  }

  if (!authLoading && !user) {
    return (
      <div className="container-page py-12 text-center">
        <p className="text-muted mb-4">Sign in to edit a battle.</p>
        <Link href="/login" className="btn btn-ink">Sign in</Link>
      </div>
    );
  }

  if (loadError) {
    return (
      <div className="container-page py-12 text-center">
        <p className="text-muted mb-4">{loadError}</p>
        <Link href="/battles" className="btn btn-ghost">Back to battles</Link>
      </div>
    );
  }

  if (!form) return <div className="container-page py-12 text-muted">Loading…</div>;

  return (
    <div className="container-page py-8 max-w-[680px] mx-auto">
      <h1 className="font-display text-4xl mb-1">Edit Battle</h1>
      <p className="text-muted mb-6">Fix details or update the contenders' images.</p>

      <form onSubmit={submit} className="flex flex-col gap-4">
        <div>
          <label className="label" htmlFor="title">Title</label>
          <input id="title" className="input" value={form.title} onChange={set("title")} required minLength={5} maxLength={200} />
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
            <input className="input" placeholder="Image/logo URL (optional)" value={form.aImage} onChange={set("aImage")} />
            {form.aImage ? <img src={form.aImage} alt="" className="w-16 h-16 rounded-xl object-cover" /> : null}
          </div>
          <div className="card p-4 flex flex-col gap-3 border-t-4 !border-t-sideb">
            <div className="font-display text-xl text-sideb">Contender B</div>
            <input className="input" placeholder="Name" value={form.bName} onChange={set("bName")} required maxLength={200} />
            <input className="input" placeholder="Image/logo URL (optional)" value={form.bImage} onChange={set("bImage")} />
            {form.bImage ? <img src={form.bImage} alt="" className="w-16 h-16 rounded-xl object-cover" /> : null}
          </div>
        </div>

        {error && (
          <div className="text-sideb text-sm">
            {error}
            {errors.length > 0 && <ul className="list-disc ml-5 mt-1">{errors.map((x, i) => <li key={i}>{x}</li>)}</ul>}
          </div>
        )}

        <div className="flex gap-3">
          <button className="btn btn-ink flex-1" disabled={saving}>{saving ? "Saving…" : "Save changes"}</button>
          <Link href={`/battle/${form.slug}`} className="btn btn-ghost">Cancel</Link>
        </div>
      </form>
    </div>
  );
}
