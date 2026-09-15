# VoteBattle

Piattaforma dove gli utenti confrontano due contendenti (es. _Samsung Galaxy Fold 8_ vs _iPhone Duo_)
e sostengono il proprio preferito usando i **Vote Credits**. Il posizionamento è competitivo /
fandom ("**show your support**"), non un sondaggio d'opinione neutrale.

> ⚠️ Progetto in sviluppo (MVP). Questo README è documentazione tecnica interna.

## Stack

| Livello | Tecnologia |
|---|---|
| Frontend | Next.js (App Router) + JavaScript, Tailwind CSS, TanStack Query |
| Backend | ASP.NET Core Web API (C#), Entity Framework Core, ASP.NET Core Identity |
| Database | PostgreSQL |
| Pagamenti | Stripe (Checkout + Webhook) |
| Anti-bot | Cloudflare Turnstile |
| Auth | Cookie HttpOnly (sessione firmata) |

## Modello di business

- Ogni nuovo utente **verificato** riceve **5 Vote Credits gratuiti** (una sola volta per account).
- Ogni voto consuma **1 Vote Credit**.
- I crediti si acquistano a **pacchetti** tramite Stripe (gestiti da database, non hardcoded).
- Un utente può votare **più volte** la stessa battle (intenzionale).
- Fonte di verità dei pagamenti = **Stripe Webhook** (verificato + idempotente).
- Fonte di verità dei crediti = **ledger** (`VoteCreditTransaction`), append-only.

## Struttura del repository

```
backend/    ASP.NET Core (Api / Core / Infrastructure) + tests
frontend/   Next.js (JavaScript)
docker-compose.yml   PostgreSQL + Mailhog (sviluppo locale)
.env.example         Variabili d'ambiente (copiare in .env)
```

## Avvio ambiente di sviluppo

### 1. Prerequisiti
- Docker + Docker Compose
- .NET SDK 8
- Node.js 20+

### 2. Variabili d'ambiente
```bash
cp .env.example .env
# modifica i valori se necessario
```

### 3. Servizi locali (database + email)
```bash
docker compose up -d
```
- PostgreSQL → `localhost:5432`
- Mailhog (cattura le email di verifica) → http://localhost:8025

### 4. Backend
```bash
cd backend
dotnet restore

# Genera la migrazione iniziale (solo la prima volta).
# Le migrazioni EF Core sono generate dal tool, non scritte a mano.
dotnet tool install --global dotnet-ef   # se non hai già dotnet-ef
dotnet ef migrations add InitialCreate \
  --project src/VoteBattle.Infrastructure \
  --startup-project src/VoteBattle.Api \
  --output-dir Data/Migrations

# Applica le migrazioni al database.
dotnet ef database update \
  --project src/VoteBattle.Infrastructure \
  --startup-project src/VoteBattle.Api

dotnet run --project src/VoteBattle.Api
```
API → http://localhost:5000 · Swagger (dev) → http://localhost:5000/swagger

> La connection string viene letta da `DATABASE_CONNECTION_STRING` (se impostata),
> altrimenti da `ConnectionStrings:Default` in `appsettings.Development.json`.

All'avvio l'app **applica automaticamente le migrazioni e fa il seed** (ruoli,
utente admin, categorie, vote packages, blocklist email e 10 battle demo).

**Utente admin di sviluppo** (cambiare in produzione via `SEED_ADMIN_EMAIL` /
`SEED_ADMIN_PASSWORD`):
- email: `admin@votebattle.local`
- password: `Admin123!`

### 5. Frontend
```bash
cd frontend
npm install
npm run dev
```
App → http://localhost:3000

## Sicurezza e note

- I **secret non vanno mai committati**: solo `.env.example` è versionato.
- In **sviluppo**: Stripe in test mode, Turnstile disattivabile (`TURNSTILE_ENABLED=false`).
- In **produzione**: HTTPS, Stripe live, Turnstile obbligatorio, verifica email obbligatoria.

## Pagine legali

Le pagine `/terms`, `/privacy`, `/refunds`, `/cookies`, `/community-guidelines` sono
**placeholder** e **devono essere revisionate da un professionista** prima del go-live.
Non costituiscono consulenza legale.
