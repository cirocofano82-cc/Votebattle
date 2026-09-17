"use client";

import { useRef, useState } from "react";
import { uploadImage } from "@/lib/api";

// An image input that accepts either a pasted URL or a file upload. On a
// successful upload it fills the field with the served image URL. Shows a
// small preview and hides it gracefully if the image can't be loaded.
export default function ImageUploadField({ value, onChange, label = "Image", placeholder }) {
  const inputRef = useRef(null);
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState(null);

  async function handleFile(e) {
    const file = e.target.files?.[0];
    if (!file) return;
    setBusy(true);
    setError(null);
    try {
      const url = await uploadImage(file);
      onChange(url);
    } catch (err) {
      setError(err.status === 403 ? "Only admins can upload images." : err.message || "Upload failed.");
    } finally {
      setBusy(false);
      if (inputRef.current) inputRef.current.value = "";
    }
  }

  return (
    <div className="flex flex-col gap-2">
      <div className="flex gap-2">
        <input
          className="input flex-1"
          placeholder={placeholder || `${label} URL (optional)`}
          value={value}
          onChange={(e) => onChange(e.target.value)}
        />
        <button
          type="button"
          className="btn btn-ghost whitespace-nowrap"
          disabled={busy}
          onClick={() => inputRef.current?.click()}
        >
          {busy ? "Uploading…" : "Upload"}
        </button>
        <input
          ref={inputRef}
          type="file"
          accept="image/png,image/jpeg,image/webp,image/gif"
          hidden
          onChange={handleFile}
        />
      </div>
      {error && <p className="text-sideb text-sm">{error}</p>}
      {value && (
        // eslint-disable-next-line @next/next/no-img-element
        <img
          src={value}
          alt="preview"
          className="w-16 h-16 rounded-lg object-cover border border-line"
          onError={(e) => {
            e.currentTarget.style.display = "none";
          }}
        />
      )}
    </div>
  );
}
