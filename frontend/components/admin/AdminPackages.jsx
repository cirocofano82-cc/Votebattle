"use client";

import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import Modal from "@/components/Modal";
import { apiFetch } from "@/lib/api";

const EMPTY = { name: "", price: "", currency: "USD", credits: "", isActive: true, isPopular: false, displayOrder: 0 };

export default function AdminPackages() {
  const { data, refetch, isLoading } = useQuery({
    queryKey: ["admin-packages"],
    queryFn: async () => (await apiFetch("/admin/vote-packages")).data,
  });

  const [editing, setEditing] = useState(null); // package or {} for new
  const [form, setForm] = useState(EMPTY);
  const [saving, setSaving] = useState(false);
  const [error, setError] = useState(null);

  function open(pkg) {
    setError(null);
    if (pkg) {
      setEditing(pkg);
      setForm({ ...pkg, price: String(pkg.price), credits: String(pkg.credits) });
    } else {
      setEditing({});
      setForm(EMPTY);
    }
  }

  const set = (k) => (e) =>
    setForm((f) => ({ ...f, [k]: e.target.type === "checkbox" ? e.target.checked : e.target.value }));

  async function save() {
    setSaving(true);
    setError(null);
    const body = {
      name: form.name,
      price: Number(form.price),
      currency: form.currency,
      credits: Number(form.credits),
      isActive: !!form.isActive,
      isPopular: !!form.isPopular,
      displayOrder: Number(form.displayOrder) || 0,
    };
    try {
      if (editing.id) {
        await apiFetch(`/admin/vote-packages/${editing.id}`, { method: "PUT", body });
      } else {
        await apiFetch("/admin/vote-packages", { method: "POST", body });
      }
      setEditing(null);
      await refetch();
    } catch (e) {
      setError(e.message || "Could not save.");
    } finally {
      setSaving(false);
    }
  }

  async function deactivate(id) {
    await apiFetch(`/admin/vote-packages/${id}`, { method: "DELETE" });
    await refetch();
  }

  const items = data || [];

  return (
    <div>
      <div className="flex justify-between items-center mb-4">
        <p className="text-muted text-sm">Editing a package never changes historical payments.</p>
        <button className="btn btn-ink" onClick={() => open(null)}>+ New package</button>
      </div>

      {isLoading ? <p className="text-muted">Loading…</p> : (
        <div className="card divide-y divide-line">
          {items.map((p) => (
            <div key={p.id} className="flex items-center justify-between gap-3 px-4 py-3 flex-wrap">
              <div>
                <div className="font-semibold">
                  {p.name} {p.isPopular && <span className="chip !bg-gold-soft !text-gold-ink ml-1">Popular</span>}
                  {!p.isActive && <span className="chip ml-1">Inactive</span>}
                </div>
                <div className="text-muted text-sm">${p.price} · {p.credits} credits · order {p.displayOrder}</div>
              </div>
              <div className="flex gap-2">
                <button className="btn btn-ghost" onClick={() => open(p)}>Edit</button>
                {p.isActive && <button className="btn btn-ghost" onClick={() => deactivate(p.id)}>Deactivate</button>}
              </div>
            </div>
          ))}
        </div>
      )}

      <Modal open={!!editing} onClose={() => setEditing(null)} title={editing?.id ? "Edit package" : "New package"}>
        <div className="flex flex-col gap-3">
          <div><label className="label">Name</label><input className="input" value={form.name} onChange={set("name")} /></div>
          <div className="grid grid-cols-2 gap-3">
            <div><label className="label">Price</label><input className="input" type="number" step="0.01" value={form.price} onChange={set("price")} /></div>
            <div><label className="label">Credits</label><input className="input" type="number" value={form.credits} onChange={set("credits")} /></div>
          </div>
          <div className="grid grid-cols-2 gap-3">
            <div><label className="label">Currency</label><input className="input" value={form.currency} onChange={set("currency")} maxLength={3} /></div>
            <div><label className="label">Display order</label><input className="input" type="number" value={form.displayOrder} onChange={set("displayOrder")} /></div>
          </div>
          <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.isActive} onChange={set("isActive")} /> Active</label>
          <label className="flex items-center gap-2 text-sm"><input type="checkbox" checked={form.isPopular} onChange={set("isPopular")} /> Most popular</label>
          {error && <p className="text-sideb text-sm">{error}</p>}
          <div className="flex gap-3 mt-1">
            <button className="btn btn-ink flex-1" disabled={saving} onClick={save}>{saving ? "Saving…" : "Save"}</button>
            <button className="btn btn-ghost flex-1" onClick={() => setEditing(null)}>Cancel</button>
          </div>
        </div>
      </Modal>
    </div>
  );
}
