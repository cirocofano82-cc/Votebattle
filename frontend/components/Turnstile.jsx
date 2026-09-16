"use client";

import { useEffect, useRef } from "react";

// Lightweight Cloudflare Turnstile widget. Renders only when a site key is
// provided (production); in development, with no key, it renders nothing and the
// backend accepts the request because CAPTCHA is disabled there.
export default function Turnstile({ siteKey, onToken }) {
  const containerRef = useRef(null);
  const widgetId = useRef(null);

  useEffect(() => {
    if (!siteKey) return;

    let cancelled = false;
    const render = () => {
      if (cancelled) return;
      if (window.turnstile && containerRef.current && widgetId.current === null) {
        widgetId.current = window.turnstile.render(containerRef.current, {
          sitekey: siteKey,
          callback: (token) => onToken(token),
          "error-callback": () => onToken(""),
          "expired-callback": () => onToken(""),
        });
      }
    };

    if (window.turnstile) {
      render();
      return () => {
        cancelled = true;
      };
    }

    const scriptId = "cf-turnstile-script";
    let poll;
    if (!document.getElementById(scriptId)) {
      const s = document.createElement("script");
      s.id = scriptId;
      s.src = "https://challenges.cloudflare.com/turnstile/v0/api.js";
      s.async = true;
      s.defer = true;
      s.onload = render;
      document.head.appendChild(s);
    } else {
      poll = setInterval(() => {
        if (window.turnstile) {
          clearInterval(poll);
          render();
        }
      }, 200);
    }

    return () => {
      cancelled = true;
      if (poll) clearInterval(poll);
    };
  }, [siteKey, onToken]);

  if (!siteKey) return null;
  return <div ref={containerRef} className="my-2" />;
}
