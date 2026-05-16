# Implementation Status

## Implemented

### Infrastructure
- One-class-per-endpoint pattern across the full API surface (48 endpoints).
- Razor Pages for all website routes (public, staff, account, maintainer).
- ASP.NET cookie-based account sessions with `HttpOnly`, `Secure`, `SameSite=Lax`, 8-hour TTL.
- Per-user `session_version` with revoke-all semantics.
- RBAC: `admin`, `moderator`, `maintainer`, `user`.
- Ban model (temporary/permanent) enforced at login and write actions.
- In-memory repositories for all entities (pending Postgres migration).
- In-memory cache store (pending Valkey migration).
- Async refresh queue/worker with idempotency key and coalescing support.
- Cache coordinator and key factory (`provider:owner:repo:ref:path` schema).
- Structured source layout: `Core/`, `Controllers/`, `Pages/`, `Models/`, `wwwroot/`.

### Manifest System
- `modvox.json` manifest schema with required and optional fields.
- `ManifestService` reads, parses, and validates manifests via the provider API.
- `ManifestOptions` config section (`Manifest:FileName`).
- Tag label resolution (case-insensitive, unknown labels silently dropped).
- `GET /api/v1/manifest/scaffold` — scaffold file download, no auth.
- `POST /api/v1/mods/{modId}/manifest/refresh` — re-reads manifest, updates metadata, verifies token.

### Mod Registration and Lifecycle
- `POST /api/v1/mods` — register mod; reads manifest; issues mod key and verify token; creates mod in `unverified` state.
- Mod moderation statuses: `unverified`, `pending`, `approved`, `hidden`.
- `POST /api/v1/mods/{modId}/keys/rotate`
- `POST /api/v1/mods/{modId}/keys/revoke`
- Maintainer restriction: hidden mods block new registrations.
- Content refresh blocked for `unverified` mods.

### Content Refresh
- `POST /api/v1/refresh/mod` — CI bearer-token refresh, async job enqueue.
- `GET /api/v1/refresh/jobs/{jobId}` — job status polling.
- Refresh cooldown enforced via `Refresh:MinIntervalMinutes`.
- `429` response with `retry_after_seconds`.

### Games and Tags
- `POST /api/v1/admin/games`, `GET /api/v1/admin/games`, `GET /api/v1/admin/games/{gameId}`, `POST /api/v1/admin/games/{gameId}`
- `GET /api/v1/games`
- `POST /api/v1/admin/tags`, `GET /api/v1/tags`, `POST /api/v1/admin/tags/{tagId}`, `DELETE /api/v1/admin/tags/{tagId}`
- Startup tag seeding from `Tags:DefaultSeedLabels`.

### User and Account Management
- `POST /api/v1/auth/login`, `POST /api/v1/auth/logout`, `POST /api/v1/auth/logout-all`, `GET /api/v1/auth/me`
- `POST /api/v1/auth/change-credentials`, `POST /api/v1/account/change-display-name`, `POST /api/v1/account/change-password`, `POST /api/v1/account/delete`
- `POST /api/v1/admin/users`, `GET /api/v1/admin/users`, `GET /api/v1/admin/users/{userId}`
- `POST /api/v1/admin/users/{userId}/role`, `/email`, `/password`, `/username`, `/display-name`, `/revoke-all-tokens`
- `POST /api/v1/admin/users/{userId}/ban`, `/unban`
- `POST /api/v1/admin/users/{userId}/mods/keys/revoke-all`
- `GET /api/v1/admin/users/{userId}/mods`
- Default admin account seeded on startup.

### Moderation
- `POST /api/v1/mods/{modId}/moderation/approve`, `/hide`, `/unhide`
- `DELETE /api/v1/admin/mods/{modId}`
- `POST /api/v1/mods/{modId}/reports`, `GET /api/v1/moderation/reports`, `POST /api/v1/moderation/reports/{reportId}/resolve`
- Report types: `rule_violation`, `malicious_code`, `not_working`.

### Public Discovery
- `GET /api/v1/games/{gameId}/mods`, `GET /api/v1/games/{gameId}/mods/{modId}`
- `POST /api/v1/games/{gameId}/mods/{modId}/download` (download counter)
- `unverified` and `hidden` mods excluded from all public queries.

### Audit Log
- Append-only audit log model and service.
- `GET /api/v1/admin/audit/export`, `POST /api/v1/admin/audit/purge`

### Web UI Routes

| Route | Description |
|---|---|
| `GET /` | Public game listing |
| `GET /{gameId}` | Public mod listing for a game |
| `GET /{gameId}/{modId}` | Public mod detail |
| `GET /user/{userId}` | Public user profile |
| `GET /login` | Staff login (not in public nav) |
| `GET /settings` | Account self-service |
| `GET /staff` | Staff dashboard (admin + moderator) |
| `GET /staff/moderation` | Moderation info page |
| `GET /staff/users` | Admin user management |
| `GET /staff/games` | Admin game management |
| `GET /staff/edit/user/{userId}` | Admin user edit |
| `GET /staff/edit/game/{gameId}` | Admin game edit |
| `GET /maintainer` | Maintainer mod list |
| `GET /maintainer/register` | Register new mod |
| `GET /maintainer/edit/{modId}` | Manage mod (manifest refresh, key rotation, content refresh) |

### UI Features
- Role-conditional navigation (My Mods for maintainer/admin, Staff for admin/moderator).
- Cookie notice banner.
- Status badges for all moderation statuses including `unverified`.
- Manifest scaffold download button on the register mod page.
- Process explanation with numbered steps on the register mod page.
- Verify token and mod key reveal panels with copy-to-clipboard.
- Refresh Manifest button on the manage mod page; reloads on successful verification.

---

## Not Yet Implemented

- **PostgreSQL-backed repositories** — all repositories are currently in-memory.
- **Valkey-backed cache store** — current cache store is in-memory.
- **Durable session storage** — sessions are in-memory; lost on restart.
- **Durable audit log** — audit log is in-memory; lost on restart.
- **Cache invalidation** — not fully wired for moderation/hide/unhide actions.
- **ETag / conditional requests** — provider calls do not yet use `If-Modified-Since` / `ETag` headers.
- **Rate limit handling** — upstream GitHub API rate limits are not handled with backoff/retry.
- **Thunderstore-compatible API** — not yet implemented. Goal: expose endpoints matching the Thunderstore package API schema so existing Thunderstore mod manager clients work without modification.
  - Thunderstore API reference: `https://thunderstore.io/api/docs/` (OpenAPI 2.0).
  - Key endpoint groups: `/api/v1/package/` and community-scoped variants; `/api/experimental/package-index/` (newline-delimited JSON stream); `/api/experimental/package/{namespace}/{name}/{version}/readme/`.
  - Keep Thunderstore adapter code behind its own abstraction boundary — must not bleed into core domain logic.
