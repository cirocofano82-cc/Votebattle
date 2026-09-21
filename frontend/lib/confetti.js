// Tiny dependency-free confetti burst. Fires colored paper bits from a point
// (defaults to upper-middle of the viewport). Respects reduced-motion.
export function confettiBurst(opts = {}) {
  if (typeof window === "undefined") return;
  if (window.matchMedia?.("(prefers-reduced-motion: reduce)")?.matches) return;

  const colors = opts.colors || ["#4f7bf7", "#ff5c8a", "#8b5cf6", "#ffc53d", "#22c55e"];
  const count = opts.count ?? 110;
  const originX = opts.x ?? window.innerWidth / 2;
  const originY = opts.y ?? window.innerHeight / 3;

  const canvas = document.createElement("canvas");
  canvas.style.cssText = "position:fixed;inset:0;width:100%;height:100%;pointer-events:none;z-index:9999";
  const w = (canvas.width = window.innerWidth);
  const h = (canvas.height = window.innerHeight);
  document.body.appendChild(canvas);
  const ctx = canvas.getContext("2d");

  const parts = Array.from({ length: count }, () => {
    const angle = Math.random() * Math.PI * 2;
    const speed = 4 + Math.random() * 9;
    return {
      x: originX,
      y: originY,
      vx: Math.cos(angle) * speed,
      vy: Math.sin(angle) * speed - 4,
      g: 0.32 + Math.random() * 0.18,
      size: 6 + Math.random() * 7,
      color: colors[(Math.random() * colors.length) | 0],
      rot: Math.random() * Math.PI,
      vr: (Math.random() - 0.5) * 0.35,
      life: 0,
      ttl: 90 + Math.random() * 50,
    };
  });

  let raf;
  const start = performance.now();
  function frame(now) {
    ctx.clearRect(0, 0, w, h);
    let alive = false;
    for (const p of parts) {
      p.life++;
      if (p.life > p.ttl || p.y > h + 20) continue;
      alive = true;
      p.vy += p.g;
      p.x += p.vx;
      p.y += p.vy;
      p.rot += p.vr;
      ctx.save();
      ctx.globalAlpha = Math.max(0, 1 - p.life / p.ttl);
      ctx.translate(p.x, p.y);
      ctx.rotate(p.rot);
      ctx.fillStyle = p.color;
      ctx.fillRect(-p.size / 2, -p.size / 2, p.size, p.size * 0.55);
      ctx.restore();
    }
    if (alive && now - start < 4000) {
      raf = requestAnimationFrame(frame);
    } else {
      cancelAnimationFrame(raf);
      canvas.remove();
    }
  }
  raf = requestAnimationFrame(frame);
}
