"use client";

import { useCallback, useEffect, useState } from "react";
import Link from "next/link";
import Modal from "./Modal";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";
import { initials, timeAgo } from "@/lib/format";

const REPORT_REASONS = ["Spam", "Harassment", "HateSpeech", "MisleadingContent", "Copyright", "Other"];
const PAGE_SIZE = 20;

export default function Comments({ battleId }) {
  const { user } = useAuth();
  const [items, setItems] = useState([]);
  const [total, setTotal] = useState(0);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(true);
  const [content, setContent] = useState("");
  const [posting, setPosting] = useState(false);
  const [error, setError] = useState(null);
  const [reportFor, setReportFor] = useState(null);

  const load = useCallback(
    async (pageToLoad) => {
      setLoading(true);
      try {
        const res = await apiFetch(
          `/battles/${battleId}/comments?page=${pageToLoad}&pageSize=${PAGE_SIZE}`
        );
        const d = res.data;
        setTotal(d.totalCount);
        setItems((prev) => (pageToLoad === 1 ? d.items : [...prev, ...d.items]));
        setPage(pageToLoad);
      } catch {
        // leave list as-is
      } finally {
        setLoading(false);
      }
    },
    [battleId]
  );

  useEffect(() => {
    load(1);
  }, [load]);

  async function submit(e) {
    e.preventDefault();
    setError(null);
    const text = content.trim();
    if (text.length < 3 || text.length > 1000) {
      setError("Comments must be between 3 and 1000 characters.");
      return;
    }
    setPosting(true);
    try {
      const res = await apiFetch(`/battles/${battleId}/comments`, {
        method: "POST",
        body: { content: text },
      });
      setItems((prev) => [res.data, ...prev]);
      setTotal((t) => t + 1);
      setContent("");
    } catch (e) {
      setError(e.message || "Could not post your comment.");
    } finally {
      setPosting(false);
    }
  }

  async function toggleLike(comment) {
    if (!user) return;
    try {
      const res = await apiFetch(`/comments/${comment.id}/like`, { method: "POST" });
      const d = res.data;
      setItems((prev) =>
        prev.map((c) =>
          c.id === comment.id ? { ...c, likeCount: d.likeCount, likedByMe: d.liked } : c
        )
      );
    } catch {
      // ignore
    }
  }

  const hasMore = items.length < total;

  return (
    <section className="card p-5">
      <h2 className="font-display text-2xl mb-4">
        Comments {total > 0 && <span className="text-muted">· {total}</span>}
      </h2>

      {user ? (
        <form onSubmit={submit} className="mb-5">
          <textarea
            className="input min-h-[80px] resize-y"
            placeholder="Share your take (3–1000 characters, no spam)…"
            value={content}
            maxLength={1000}
            onChange={(e) => setContent(e.target.value)}
          />
          {error && <p className="text-sideb text-sm mt-1">{error}</p>}
          <div className="flex justify-end mt-2">
            <button className="btn btn-ink" disabled={posting}>
              {posting ? "Posting…" : "Post comment"}
            </button>
          </div>
        </form>
      ) : (
        <p className="text-muted mb-5">
          <Link href="/login" className="text-sidea no-underline font-semibold">Sign in</Link> to
          join the discussion.
        </p>
      )}

      <div className="flex flex-col divide-y divide-line">
        {items.map((c) => (
          <div key={c.id} className="flex gap-3 py-4">
            <div className="w-9 h-9 rounded-full bg-surface2 grid place-items-center font-bold text-muted shrink-0">
              {initials(c.username)}
            </div>
            <div className="flex-1 min-w-0">
              <div className="text-sm text-muted">
                <span className="font-semibold text-text">{c.username}</span> · {timeAgo(c.createdAt)}
              </div>
              <p className="my-1 break-words">{c.content}</p>
              <div className="flex gap-4 text-sm text-muted">
                <button
                  className={`inline-flex items-center gap-1 ${c.likedByMe ? "text-sideb" : ""} ${
                    user ? "" : "opacity-60 cursor-not-allowed"
                  }`}
                  onClick={() => toggleLike(c)}
                  disabled={!user}
                >
                  {c.likedByMe ? "♥" : "♡"} {c.likeCount}
                </button>
                {user && (
                  <button className="hover:text-text" onClick={() => setReportFor(c)}>
                    Report
                  </button>
                )}
              </div>
            </div>
          </div>
        ))}
        {!loading && items.length === 0 && (
          <p className="text-muted py-4">No comments yet. Be the first to weigh in.</p>
        )}
      </div>

      {hasMore && (
        <div className="flex justify-center mt-4">
          <button className="btn btn-ghost" disabled={loading} onClick={() => load(page + 1)}>
            {loading ? "Loading…" : "Load more"}
          </button>
        </div>
      )}

      <ReportModal
        comment={reportFor}
        onClose={() => setReportFor(null)}
      />
    </section>
  );
}

function ReportModal({ comment, onClose }) {
  const [reason, setReason] = useState("Spam");
  const [details, setDetails] = useState("");
  const [sending, setSending] = useState(false);
  const [done, setDone] = useState(false);

  async function send() {
    setSending(true);
    try {
      await apiFetch(`/comments/${comment.id}/report`, {
        method: "POST",
        body: { reason, details: details.trim() || null },
      });
      setDone(true);
    } catch {
      setDone(true); // reporting is best-effort from the UI's perspective
    } finally {
      setSending(false);
    }
  }

  return (
    <Modal open={!!comment} onClose={onClose} title="Report comment">
      {done ? (
        <>
          <p className="text-muted mb-4">Thanks — our team will review this.</p>
          <button className="btn btn-ink w-full" onClick={() => { setDone(false); onClose(); }}>
            Close
          </button>
        </>
      ) : (
        <>
          <label className="label">Reason</label>
          <select className="input mb-3" value={reason} onChange={(e) => setReason(e.target.value)}>
            {REPORT_REASONS.map((r) => (
              <option key={r} value={r}>{r.replace(/([A-Z])/g, " $1").trim()}</option>
            ))}
          </select>
          <label className="label">Details (optional)</label>
          <textarea
            className="input min-h-[70px] mb-4"
            value={details}
            maxLength={2000}
            onChange={(e) => setDetails(e.target.value)}
          />
          <div className="flex gap-3">
            <button className="btn btn-ink flex-1" disabled={sending} onClick={send}>
              {sending ? "Sending…" : "Submit report"}
            </button>
            <button className="btn btn-ghost flex-1" onClick={onClose}>Cancel</button>
          </div>
        </>
      )}
    </Modal>
  );
}
