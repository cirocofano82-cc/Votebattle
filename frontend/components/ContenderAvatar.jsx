"use client";

import { useState } from "react";
import { initials } from "@/lib/format";

const SIZES = {
  sm: "w-14 h-14 text-xl rounded-xl",
  md: "w-16 h-16 text-2xl rounded-2xl",
  lg: "w-24 h-24 text-4xl rounded-2xl",
};

// Renders a contender's photo/logo (imageUrl) when available, falling back to a
// colored tile with the contender's initial when there is no image or it fails to load.
export default function ContenderAvatar({ name, imageUrl, side = "a", size = "md" }) {
  const [failed, setFailed] = useState(false);
  const base = `${SIZES[size]} grid place-items-center font-display text-white overflow-hidden shrink-0`;
  const gradient =
    side === "b"
      ? "linear-gradient(150deg,#fb7185,#f43f5e)"
      : "linear-gradient(150deg,#4f86f7,#2563eb)";

  if (imageUrl && !failed) {
    return (
      <div className={base} style={{ background: "var(--surface-2)" }}>
        {/* eslint-disable-next-line @next/next/no-img-element */}
        <img
          src={imageUrl}
          alt={name || "contender"}
          className="w-full h-full object-cover"
          onError={() => setFailed(true)}
        />
      </div>
    );
  }

  return (
    <div className={base} style={{ background: gradient }}>
      {initials(name)}
    </div>
  );
}
