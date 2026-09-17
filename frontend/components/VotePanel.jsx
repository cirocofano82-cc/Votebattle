"use client";

import { useState } from "react";
import Link from "next/link";
import ContenderAvatar from "./ContenderAvatar";
import VersusBar from "./VersusBar";
import Modal from "./Modal";
import { useAuth } from "@/context/AuthContext";
import { apiFetch } from "@/lib/api";
import { formatNumber, formatPercent } from "@/lib/format";

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
  // A single request may spend at most the smaller of the balance and the server cap.
  const maxQuantity = Math.max(1, Math.min(balance, MAX_PER_VOTE));

  function clampQuantity(n) {
    if (Number.isNaN(n)) return 1;
    return Math.min(Math.max(1, Math.round(n)), maxQuantity);
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
    <div className="card p-5">
      {/* Arena */}
      <div className="grid grid-cols-[1fr_auto_1fr] items-start gap-3">
        <Contender p={a} side="a" />
        <span className="font-display text-muted text-2xl self-center">VS</span>
        <Contender p={b} side="b" />
      </div>

      <div className="mt-4">
        <VersusBar pa={a?.percentage ?? 0} empty={totalVotes === 0} />
      </div>
      <div className="flex justify-between mt-2 font-bold">
        <span className="text-sidea">{formatPercent(a?.percentage)}</span>
        <span className="text-sideb">{formatPercent(b?.percentage)}</span>
      </div>
      <div className="text-center text-muted text-sm mt-3 tnum">
        {formatNumber(totalVotes)} total votes
      </div>

      {/* Voting area */}
      <div className="mt-5 border-t border-line pt-5">
        {toast && (
          <div
            className={`mb-3 text-sm rounded-[10px] px-3 py-2 ${
              toast.kind === "ok"
                ? "bg-sidea-soft text-sidea"
                : "bg-sideb-soft text-sideb"
            }`}
          >
            {toast.text}
          </div>
        )}

        {authLoading ? null : !user ? (
          <Link href="/login" className="btn btn-ink w-full">Sign in to vote</Link>
        ) : user.voteCredits <= 0 ? (
          <OutOfCredits />
        ) : (
          <>
            {/* Quantity selector: how many credits to spend per vote action. */}
            <div className="flex items-center justify-center gap-3 mb-4">
              <span className="text-muted text-sm">Votes</span>
              <div className="inline-flex items-center rounded-[10px] border border-line overflow-hidden">
                <button
                  type="button"
                  className="px-3 py-2 font-bold text-lg leading-none hover:bg-line disabled:opacity-40"
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
                  className="w-16 text-center bg-transparent font-bold tnum py-2 outline-none border-x border-line [appearance:textfield] [&::-webkit-outer-spin-button]:appearance-none [&::-webkit-inner-spin-button]:appearance-none"
                  aria-label="Number of votes"
                />
                <button
                  type="button"
                  className="px-3 py-2 font-bold text-lg leading-none hover:bg-line disabled:opacity-40"
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
            </div>

            <div className="grid grid-cols-1 sm:grid-cols-2 gap-3">
              <button
                className="btn btn-a"
                disabled={voting}
                onClick={() => setConfirmFor(a)}
              >
                Vote for {a?.name}
              </button>
              <button
                className="btn btn-b"
                disabled={voting}
                onClick={() => setConfirmFor(b)}
              >
                Vote for {b?.name}
              </button>
            </div>
            <p className="text-center text-muted text-sm mt-3">
              Each vote uses 1 Vote Credit · Balance:{" "}
              <strong className="text-text tnum">{formatNumber(balance)}</strong>
            </p>
          </>
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

function Contender({ p, side }) {
  return (
    <div className="flex flex-col items-center text-center gap-2">
      <ContenderAvatar name={p?.name} imageUrl={p?.imageUrl} side={side} size="lg" />
      <span className="font-semibold">{p?.name}</span>
      <span className={`font-display text-3xl ${side === "b" ? "text-sideb" : "text-sidea"}`}>
        {formatPercent(p?.percentage)}
      </span>
      <span className="text-muted text-sm tnum">{formatNumber(p?.voteCount)} votes</span>
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
