"use client";

import { useState } from "react";
import Link from "next/link";
import ContenderAvatar from "./ContenderAvatar";
import VersusBar from "./VersusBar";
import Modal from "./Modal";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";
import { formatNumber, categoryEmoji } from "@/lib/format";
import { confettiBurst } from "@/lib/confetti";

export default function VotePanel({ battle }) {
  const { user, loading: authLoading, refresh } = useAuth();

  const MAX_PER_VOTE = 1000;
  const initial = [...(battle.participants || [])].sort((a, b) => a.position - b.position);
  const [participants, setParticipants] = useState(initial);
  const [totalVotes, setTotalVotes] = useState(battle.totalVotes);
  const [confirmFor, setConfirmFor] = useState(null);
  const [quantity, setQuantity] = useState(1);
  const [voting, setVoting] = useState(false);
  const [toast, setToast] = useState(null);

  const [a, b] = participants;

  const balance = user?.voteCredits ?? 0;
  const maxQuantity = Math.max(1, Math.min(balance, MAX_PER_VOTE));

  const empty = totalVotes === 0;
  const pa = empty ? 50 : Math.round(a?.percentage ?? 0);
  const pb = empty ? 50 : Math.round(b?.percentage ?? 0);

  function clampQuantity(n) {
    if (Number.isNaN(n)) return 1;
    return Math.min(Math.max(1, Math.round(n)), maxQuantity);
  }

  function pick(participant) {
    if (authLoading || voting) return;
    if (!user) {
      setToast({ kind: "err", text: "Please sign in to vote." });
      return;
    }
    if (balance <= 0) {
      setToast({ kind: "err", text: "You're out of Vote Credits." });
      return;
    }
    setConfirmFor(participant);
  }

  async function castVote(participant) {
    const qty = clampQuantity(quantity);
    setConfirmFor(null);
    setVoting(true);
    setToast(null);
    try {
      const res = await apiFetch("/votes", {
        method: "POST",
        body: { battleId: battle.id, battleParticipantId: participant.id, quantity: qty },
      });
      const d = res.data;
      setParticipants([...d.participants].sort((x, y) => x.position - y.position));
      setTotalVotes(d.battleTotalVotes);
      setQuantity(1);
      await refresh();
      confettiBurst();
      setToast({
        kind: "ok",
        text:
          qty === 1
            ? `Vote counted for ${participant.name}!`
            : `${qty} votes counted for ${participant.name}!`,
      });
    } catch (e) {
      await refresh();
      if (e.status === 402) {
        setToast({ kind: "err", text: "You don't have enough Vote Credits." });
      } else if (e.status === 401) {
        setToast({ kind: "err", text: "Please sign in to vote." });
      } else {
        setToast({ kind: "err", text: e.message || "Something went wrong." });
      }
    } finally {
      setVoting(false);
    }
  }

  return (
    <div className="card p-5 sm:p-6">
      {/* Header */}
      <div className="flex items-center justify-between mb-5">
        <span className="chip">{categoryEmoji(battle.categoryName)} {battle.categoryName}</span>
        <span className="inline-flex items-center gap-2 text-[11px] font-bold uppercase tracking-wider text-muted">
          <span className="live-dot" /> Live · <span className="tnum text-text">{formatNumber(totalVotes)}</span> votes
        </span>
      </div>

      {/* Fighters + scoreboard */}
      <div className="grid grid-cols-[1fr_auto_1fr] items-center gap-2 sm:gap-5">
        <Fighter p={a} side="a" />
        <div className="text-center px-1">
          <div className="font-display text-muted text-xs tracking-[0.3em]">VS</div>
          <div className="font-display leading-none text-4xl sm:text-6xl mt-1 tnum">
            <span className="text-sidea">{pa}</span>
            <span className="text-muted mx-1">:</span>
            <span className="text-sideb">{pb}</span>
          </div>
        </div>
        <Fighter p={b} side="b" />
      </div>

      <div className="mt-5">
        <VersusBar pa={a?.percentage ?? 0} empty={empty} />
      </div>

      {/* Vote buttons */}
      <div className="grid grid-cols-1 sm:grid-cols-2 gap-3 mt-5">
        <button className="btn btn-a" disabled={voting} onClick={() => pick(a)}>
          Vote {a?.name}
        </button>
        <button className="btn btn-b" disabled={voting} onClick={() => pick(b)}>
          Vote {b?.name}
        </button>
      </div>

      {/* Control strip */}
      <div className="mt-5 border-t border-line pt-5">
        {toast && (
          <div
            className={`mb-3 text-sm rounded-[14px] px-3 py-2 ${
              toast.kind === "ok" ? "bg-sidea-soft text-sidea" : "bg-sideb-soft text-sideb"
            }`}
          >
            {toast.text}
          </div>
        )}

        {authLoading ? null : !user ? (
          <Link href="/login" className="btn btn-ink w-full">Sign in to vote</Link>
        ) : balance <= 0 ? (
          <OutOfCredits />
        ) : (
          <div className="flex items-center justify-center gap-3 flex-wrap">
            <span className="text-muted text-sm">Votes per tap</span>
            <div className="inline-flex items-center rounded-full border border-line-strong overflow-hidden bg-surface">
              <button
                type="button"
                className="px-3.5 py-2 font-bold text-lg leading-none hover:bg-surface2 disabled:opacity-40"
                disabled={voting || quantity <= 1}
                onClick={() => setQuantity((q) => clampQuantity(q - 1))}
                aria-label="Decrease votes"
              >
                −
              </button>
              <input
                type="number"
                min={1}
                max={maxQuantity}
                value={quantity}
                disabled={voting}
                onChange={(e) => setQuantity(clampQuantity(Number(e.target.value)))}
                className="w-14 text-center bg-transparent font-bold tnum py-2 outline-none border-x border-line-strong [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                aria-label="Number of votes"
              />
              <button
                type="button"
                className="px-3.5 py-2 font-bold text-lg leading-none hover:bg-surface2 disabled:opacity-40"
                disabled={voting || quantity >= maxQuantity}
                onClick={() => setQuantity((q) => clampQuantity(q + 1))}
                aria-label="Increase votes"
              >
                +
              </button>
            </div>
            <button
              type="button"
              className="text-sm text-muted underline hover:text-text disabled:opacity-40"
              disabled={voting || quantity >= maxQuantity}
              onClick={() => setQuantity(maxQuantity)}
            >
              Max
            </button>
            <span className="text-muted text-sm">· Balance <strong className="text-text tnum">{formatNumber(balance)}</strong></span>
          </div>
        )}
      </div>

      <Modal
        open={!!confirmFor}
        onClose={() => setConfirmFor(null)}
        title={
          clampQuantity(quantity) === 1
            ? "Use 1 Vote Credit?"
            : `Use ${clampQuantity(quantity)} Vote Credits?`
        }
      >
        <p className="text-muted mb-4">
          You're casting{" "}
          <strong className="text-text">{clampQuantity(quantity)}</strong>{" "}
          {clampQuantity(quantity) === 1 ? "vote" : "votes"} for{" "}
          <strong className="text-text">{confirmFor?.name}</strong>.<br />
          Your balance: <strong className="text-text">{formatNumber(balance)}</strong> credits.
        </p>
        <div className="flex gap-3">
          <button className="btn btn-ink flex-1" disabled={voting} onClick={() => castVote(confirmFor)}>
            {clampQuantity(quantity) === 1 ? "Vote" : `Cast ${clampQuantity(quantity)} votes`}
          </button>
          <button className="btn btn-ghost flex-1" onClick={() => setConfirmFor(null)}>
            Cancel
          </button>
        </div>
      </Modal>
    </div>
  );
}

function Fighter({ p, side }) {
  const color = side === "b" ? "text-sideb" : "text-sidea";
  return (
    <div className="flex flex-col items-center text-center gap-2">
      <ContenderAvatar name={p?.name} imageUrl={p?.imageUrl} side={side} size="lg" />
      <span className={`font-display text-base sm:text-xl leading-tight ${color}`}>{p?.name}</span>
      <span className="text-muted text-xs tnum">{formatNumber(p?.voteCount)} votes</span>
    </div>
  );
}

function OutOfCredits() {
  return (
    <div className="text-center">
      <p className="font-display text-2xl">You're out of Vote Credits</p>
      <p className="text-muted mt-1 mb-4">
        You've used all your votes. Get more credits to keep backing your side.
      </p>
      <Link href="/credits" className="btn btn-gold">Get More Credits</Link>
    </div>
  );
}
