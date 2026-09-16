"use client";

import { useEffect, useState } from "react";

export default function ShareButtons({ title }) {
  const [url, setUrl] = useState("");
  const [copied, setCopied] = useState(false);

  useEffect(() => {
    setUrl(window.location.href);
  }, []);

  const text = `I just voted in ${title}. Which one would you choose?`;
  const enc = encodeURIComponent;
  const links = {
    x: `https://twitter.com/intent/tweet?text=${enc(text)}&url=${enc(url)}`,
    facebook: `https://www.facebook.com/sharer/sharer.php?u=${enc(url)}`,
    whatsapp: `https://wa.me/?text=${enc(`${text} ${url}`)}`,
  };

  async function copy() {
    try {
      await navigator.clipboard.writeText(url);
      setCopied(true);
      setTimeout(() => setCopied(false), 1500);
    } catch {
      // clipboard unavailable
    }
  }

  return (
    <div className="card p-4 flex items-center gap-2 flex-wrap">
      <span className="font-semibold text-sm mr-1">Share:</span>
      <a className="btn btn-ghost" href={links.x} target="_blank" rel="noopener noreferrer">X</a>
      <a className="btn btn-ghost" href={links.facebook} target="_blank" rel="noopener noreferrer">Facebook</a>
      <a className="btn btn-ghost" href={links.whatsapp} target="_blank" rel="noopener noreferrer">WhatsApp</a>
      <button className="btn btn-ghost" onClick={copy}>{copied ? "Copied!" : "Copy link"}</button>
    </div>
  );
}
