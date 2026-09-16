# Deployment — VoteBattle

Guida al deploy economico dell'MVP. L'obiettivo è **costi molto bassi** (sotto i
~10 $/mese per partire) senza infrastruttura complessa.

## 1. Stack completo in locale (Docker)

```bash
cp .env.example .env      # compila i valori
docker compose up --build
```

- Web → http://localhost:3000
- API → http://localhost:5000 (Swagger su `/swagger`)
- Mailhog (email di test) → http://localhost:8025

L'API applica automaticamente le migrazioni e fa il seed all'avvio. In locale
gira con `ASPNETCORE_ENVIRONMENT=Development` (HTTP semplice, CAPTCHA disattivo,
Swagger attivo).

## 2. Architettura di produzione

Monolite in due processi + un database:

```
[Browser] → [Reverse proxy TLS: Caddy/nginx] → [Web (Next.js)]  + [API (ASP.NET Core)]
                                                                        │
                                                                   [PostgreSQL]
```

Il reverse proxy termina l'HTTPS e inoltra al web e all'API. L'API in produzione
usa `ASPNETCORE_ENVIRONMENT=Production` (HTTPS redirect, CAPTCHA obbligatorio,
niente Swagger, niente stack trace).

## 3. Opzioni di deploy

### Opzione A — Un solo VPS (la più economica) ✅ consigliata per iniziare
Tutto su una piccola VPS con `docker compose` + Caddy per il TLS automatico.

- **Hetzner CX22** (2 vCPU, 4 GB) ≈ **€4–5/mese**, oppure una VPS simile.
- PostgreSQL nel compose (volume su disco) → **0 €** aggiuntivi.
- Caddy come reverse proxy con HTTPS Let's Encrypt automatico.
- Costo totale indicativo: **~5 €/mese**.

Passi:
1. Installa Docker + Docker Compose sulla VPS.
2. Clona il repo, crea `.env` con i valori di produzione (vedi sotto).
3. Metti un `Caddyfile` che inoltra `votebattle.com` → web:3000 e
   `api.votebattle.com` → api:8080 (o `/api` sullo stesso dominio).
4. `docker compose up -d --build`.

### Opzione B — Frontend gestito + backend VPS
- **Frontend**: Vercel (free tier) — ottimo per Next.js e SEO. **0 €**.
- **Backend**: piccola VPS o Fly.io/Railway (piani da pochi $).
- **Database**: Neon o Supabase (free tier iniziale). **0 €** all'inizio.
- Costo: **~0–5 $/mese** finché resti nei free tier.

> Nota: con frontend e backend su domini diversi il cookie di sessione deve essere
> `SameSite=None; Secure` (cross-site). Su singolo dominio (`/api` dietro lo stesso
> host) resta `Lax`. Regola `ConfigureApplicationCookie` di conseguenza.

## 4. Variabili d'ambiente di produzione

Vedi `.env.example` per l'elenco completo. In produzione, in più:

- `ASPNETCORE_ENVIRONMENT=Production`
- `DATABASE_CONNECTION_STRING` → database di produzione
- `JWT_SECRET` → stringa random lunga (≥ 32 char)
- `STRIPE_SECRET_KEY` / `STRIPE_PUBLISHABLE_KEY` → chiavi **live**
- `STRIPE_WEBHOOK_SECRET` → dalla dashboard Stripe (vedi sotto)
- `TURNSTILE_ENABLED=true` + `TURNSTILE_SITE_KEY` / `TURNSTILE_SECRET_KEY`
- SMTP di un provider reale (es. Resend/Brevo, free tier iniziale)
- `SEED_ADMIN_EMAIL` / `SEED_ADMIN_PASSWORD` → cambiali dai default!
- Frontend: `NEXT_PUBLIC_API_BASE_URL` deve puntare all'URL pubblico dell'API
  (viene "cotto" nel bundle al momento della build).

## 5. Webhook Stripe (fonte di verità dei pagamenti)

1. Stripe Dashboard → Developers → Webhooks → Add endpoint.
2. URL: `https://<tuo-dominio-api>/api/payments/webhook`
3. Evento: `checkout.session.completed` (aggiungi refund in seguito).
4. Copia il **Signing secret** in `STRIPE_WEBHOOK_SECRET`.

## 6. Costi indicativi

| Voce | Servizio | Costo iniziale |
|---|---|---|
| Server | Hetzner VPS (o free tier Vercel + VPS) | ~5 €/mese (o ~0 €) |
| Database | PostgreSQL su VPS o Neon/Supabase free | 0 € |
| Email | Resend/Brevo free tier | 0 € |
| CAPTCHA | Cloudflare Turnstile | 0 € |
| Pagamenti | Stripe | solo fee per transazione (~2.9% + 0,30 $) |

Redis, broker, Kubernetes: **non necessari** per l'MVP.

## 7. Checklist pre-lancio

- [ ] `ASPNETCORE_ENVIRONMENT=Production` e HTTPS attivo (reverse proxy TLS)
- [ ] `TURNSTILE_ENABLED=true` con chiavi reali
- [ ] Chiavi Stripe **live** + webhook configurato e testato
- [ ] SMTP di produzione verificato
- [ ] Password admin di seed cambiata
- [ ] Backup del database pianificati
- [ ] Pagine legali (`/terms`, `/privacy`, `/refunds`, `/cookies`,
      `/community-guidelines`) **revisionate da un professionista**
