"use client";

import { useState } from "react";
import Link from "next/link";
import ContenderAvatar from "./ContenderAvatar";
import Modal from "./Modal";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";
import { formatNumber } from "@/lib/format";

export default function VotePanel({ battle }) {
  const { user, loading: authLoading, refresh } = useAuth();

  const MAX_PER_VOTE = 1000;
  const initial = [...(battle.participants || [])].sort((a, b) => a.position - b.position);
  const [participants, setParticipants] = useState(initial);
  const [totalVotes, setTotalVotes] = useState(battle.totalVotes);
  const [confirmFor, setConfirmFor] = useState(null); // participant pending confirmation
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

  // The two "Back X" buttons in the arena route to the right next step by state.
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
    <div className="card p-0 overflow-hidden">
      {/* Clash arena — the screen is the fight */}
      <div className="arena">
        <div className="halfA" />
        <div className="halfB" />
        <div className="seam" />

        <div className="a-top">
          <span className="a-round">Round 01 · {battle.categoryName}</span>
          <span className="a-live"><span className="live-dot" /> Live · {formatNumber(totalVotes)} voting</span>
        </div>

        <div className="a-score">
          <div className="a-vs">VS</div>
          <div className="a-tally"><b>{pa}</b><s>:</s><i>{pb}</i></div>
          <span className="a-cast">{formatNumber(totalVotes)} votes cast</span>
        </div>

        <ArenaFighter p={a} side="left" glow="rgba(79,139,255,.6)" />
        <ArenaFighter p={b} side="right" glow="rgba(255,86,111,.6)" />

        <button className="a-vote left" disabled={voting} onClick={() => pick(a)}>
          Back {a?.name}
        </button>
        <button className="a-vote right" disabled={voting} onClick={() => pick(b)}>
          Back {b?.name}
        </button>
      </div>

      {/* Control strip: quantity + balance, or sign-in / out-of-credits */}
      <div className="p-4 border-t border-line">
        {toast && (
          <div
            className={`mb-3 text-sm rounded-[10px] px-3 py-2 ${
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
            <div className="inline-flex items-center rounded-[10px] border border-line-strong overflow-hidden">
              <button
                type="button"
                className="px-3 py-2 font-bold text-lg leading-none hover:bg-surface2 disabled:opacity-40"
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
                className="w-16 text-center bg-transparent font-bold tnum py-2 outline-none border-x border-line-strong [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                aria-label="Number of votes"
              />
              <button
                type="button"
                className="px-3 py-2 font-bold text-lg leading-none hover:bg-surface2 disabled:opacity-40"
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

function ArenaFighter({ p, side, glow }) {
  return (
    <div className={`fighter ${side}`}>
      <div style={{ filter: `drop-shadow(0 0 18px ${glow})` }}>
        <ContenderAvatar name={p?.name} imageUrl={p?.imageUrl} side={side === "right" ? "b" : "a"} size="lg" />
      </div>
      <div className="nm">{p?.name}</div>
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
