"use client";

import Link from "next/link";
import { useAuth } from "@/context/AuthContext";

// Shows edit controls to the battle's creator or an admin.
export default function OwnerActions({ battle }) {
  const { user } = useAuth();
  if (!user) return null;

  const canEdit = user.id === battle.createdByUserId || user.roles?.includes("Admin");
  if (!canEdit) return null;

  return (
    <Link href={`/edit-battle/${battle.id}`} className="btn btn-ghost">
      Edit battle
    </Link>
  );
}
