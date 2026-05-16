# Implementation Status

## Implemented

### Infrastructure
- One-class-per-endpoint pattern across the full API surface (52 endpoints).
- Razor Pages for all website routes (public, staff, account, maintainer).
- ASP.NET cookie-based account sessions with `HttpOnly`, `Secure`, `SameSite=Lax`, 8-hour TTL.
- Per-user `session_version` with revoke-all semantics.
- RBAC: `admin`, `moderator`, `maintainer`, `user`.
- Ban model (temporary/permanent) enforced at login and write actions.
- EF Core (code-first) with Npgsql for all entity persistence.
- Valkey-backed cache store via `StackExchange.Redis`.
- Async refresh queue/worker with idempotency key and coalescing support.
- Cache coordinator and key factory (`provider:owner:repo:ref:path` schema).
- Structured source layout: `Core/`, `Controllers/`, `Pages/`, `Models/`, `Infrastructure/`, `wwwroot/`.

### Manifest System
- `modvox.json` manifest schema with required and optional fields.
- `ManifestService` reads, parses, and validates manifests via the provider API.
- `ManifestOptions` config section (`Manifest:FileName`).
- Tag label resolution (case-insensitive, unknown labels silently dropped).
- Supports `credits` (`Guid->string`) and `external_credits` (`string->string`).
- `GET /api/v1/manifest/scaffold` — scaffold file download, no auth.
- `POST /api/v1/mods/{modId}/refresh` (plus legacy alias `.../manifest/refresh`) — unified refresh pipeline.

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
- Unified refresh updates manifest metadata, verifies token, persists README/CHANGELOG markdown+HTML snapshots, and upserts releases/artifacts.
- Verify token mismatch/missing hard-fails refresh.

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
- `POST /api/v1/mods/{modId:guid}/verify-token/rotate` for moderator/admin (`unverified`/`pending` only).

### Public Discovery
- `GET /api/v1/games/{gameId}/mods`, `GET /api/v1/games/{gameId}/mods/{modId}`
- `POST /api/v1/games/{gameId}/mods/{modId}/download` (download counter)
- `unverified` and `hidden` mods excluded from all public queries.

### Thunderstore Compatibility (MVP)
- `GET /api/v1/package/`
- `GET /c/{community_identifier}/api/v1/package/`
- `GET /api/experimental/package-index/` (NDJSON)
- `GET /api/experimental/package/{namespace}/{name}/`
- `GET /api/experimental/package/{namespace}/{name}/{version}/`
- `GET /api/experimental/package/{namespace}/{name}/{version}/readme/`
- `GET /api/experimental/package/{namespace}/{name}/{version}/changelog/`
- Response shapes are now explicit DTOs under `Models/Thunderstore/`.

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
| `GET /staff/moderation` | Manage Mods page (approve/hide/unhide) |
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
- Refresh Mod button on the manage mod page; reloads on successful refresh.

---

## Not Yet Implemented

- **Cache invalidation** — not fully wired for moderation/hide/unhide actions.
- **ETag / conditional requests** — provider calls do not yet use `If-Modified-Since` / `ETag` headers.
- **Rate limit handling** — upstream GitHub API rate limits are not handled with backoff/retry.
- **Thunderstore completeness** — current implementation is mod-manager-focused MVP; broader endpoint parity is still pending.
