const numberFormatter = new Intl.NumberFormat("en-US");

export function formatNumber(value) {
  return numberFormatter.format(value ?? 0);
}

export function formatPercent(value) {
  return `${(value ?? 0).toFixed(1)}%`;
}

export function initials(name) {
  if (!name) return "?";
  return name.trim().charAt(0).toUpperCase();
}

export function timeAgo(dateInput) {
  const date = new Date(dateInput);
  const seconds = Math.floor((Date.now() - date.getTime()) / 1000);
  if (seconds < 60) return "just now";
  const minutes = Math.floor(seconds / 60);
  if (minutes < 60) return `${minutes}m ago`;
  const hours = Math.floor(minutes / 60);
  if (hours < 24) return `${hours}h ago`;
  const days = Math.floor(hours / 24);
  if (days < 30) return `${days}d ago`;
  return date.toLocaleDateString();
}
