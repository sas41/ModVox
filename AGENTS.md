# AGENTS.md

## Mission
Build ModVox: a fast website and API for discovering mods hosted on public git repositories.

## Core Constraints (Do Not Violate)
1. Stack: ASP.NET with C# 10, PostgreSQL, Valkey.
2. Deployment: the full stack runs from one `docker-compose.yml`.
3. Content model: markdown-first. Static pages are `.md` files.
4. Remote repository access: never clone repositories.
5. API style: one endpoint per class. No MediatR pattern. No AutoMapper pattern.
6. Performance model: cache-first architecture for all read paths.

## Product Scope
- Discover mods from public git repository hosts.
- Start with GitHub provider support.
- Keep provider integrations isolated so GitLab, Codeberg, and Gitea can be added without rewriting domain logic.
- Render remote README files and images from configured repository folders.
- Mod downloads are served via GitHub releases — link directly to release artifact URLs, never proxy.
- Mod metadata (name, description, tags, credits, paths) lives in a manifest file in the repository (`modvox.json` by default, configurable per instance).
- Expose a Thunderstore-compatible API so existing Thunderstore mod manager clients can discover and install mods without modification.

## Architecture Boundaries
- Keep host-specific behavior behind provider interfaces.
- Keep domain and application services provider-agnostic.
- Keep endpoint classes thin and explicit.
- Use explicit DTO mapping in code (no reflection-heavy mappers).

## Provider Abstraction Contract
Each provider implementation must support:
1. Fetch a raw file by repo + ref + path.
2. List files in a folder (for image discovery).
3. List releases and release artifacts.
4. Resolve public/canonical URLs for files and artifacts.

Rules:
- No `git clone` operations.
- Use remote API endpoints only.
- Use conditional requests (`ETag`, `If-Modified-Since`) where available.
- Handle upstream rate limits as a first-class concern.

## Manifest System
Mod metadata is declared in a manifest file committed to the repository root.

### Manifest filename
Configured per instance via `Manifest:FileName` (default: `modvox.json`).

### Manifest schema
```json
{
  "verify": "",
  "name": "My Mod Name",
  "description": "Optional description.",
  "default_ref": "main",
  "readme": "README.md",
  "changelog": "CHANGELOG.md",
  "images": "images",
  "tags": ["tag-label"],
  "credits": {
    "contributor-name": "What they did"
  }
}
```

Required fields: `name`, `default_ref`, `readme`, `changelog`, `images`, `tags` (at least one label matching a server tag).
Optional fields: `description`, `credits`, `verify`.

### Manifest rules
- `tags` labels are matched case-insensitively against server tags. Unknown labels are silently ignored. At least one must resolve.
- `credits` keys are free-form strings (e.g. GitHub usernames). No server-side validation.
- `verify` is a server-generated ownership token. When present and matching the stored token the mod transitions from `unverified` to `pending`.
- Manifest is read at registration time and on every **Refresh Manifest** action.
- `default_ref` in the manifest sets the branch used for all subsequent content fetches. It can be overridden per-request via an explicit `ref` parameter on the Refresh Manifest endpoint.

### Registration flow
1. Maintainer submits: game, repo URL (parsed for provider/owner/repo), optional initial ref.
2. Server fetches the manifest at `initial_ref` (defaults to `HEAD` if not supplied).
3. If the manifest is missing or invalid → `422 Unprocessable Entity`.
4. Mod is created in `unverified` state with fields populated from the manifest.
5. Mod key and verify token are both issued immediately and shown once.
6. Maintainer adds the verify token to the `verify` field and commits.
7. Maintainer clicks **Refresh Manifest** on the edit page. If the token matches, the mod transitions to `pending` (visible to moderators for approval).

### Scaffold download
`GET /api/v1/manifest/scaffold` — no auth required. Returns a pre-filled scaffold JSON as a file download using the configured filename. The `verify` field is present at the top, set to `""`.

## Mod Moderation Statuses
| Status | Visible to public | Visible to moderators/admins | Notes |
|---|---|---|---|
| `unverified` | No | No | Newly registered; awaiting verify token match |
| `pending` | No | Yes | Verified; awaiting moderator approval |
| `approved` | Yes | Yes | Publicly visible |
| `hidden` | No | Yes | Hidden by moderator action |

## Caching Rules (Valkey) — Mandatory
Use cache-aside for all read paths.

Every endpoint/service must define:
- cache key schema
- TTL
- invalidation strategy
- stale behavior

Required cache behaviors:
1. Key schema: `provider:owner:repository:ref:path` (or equivalent unique identifiers).
2. Negative caching for not-found responses.
3. Stale-while-revalidate for high-traffic read pages.
4. Stampede prevention using lock/single-flight semantics.

## Persistence Rules (PostgreSQL)
Persist durable mod data, including:
- normalised mod metadata (name, description, paths, tags, credits)
- source repository coordinates (provider, owner, repository)
- verify token and moderation status
- release artifact references
- fetch metadata (`etag`, `last_modified`, `checked_at`, failure state)
- maintainer key hash and rotation history

Use Postgres as the durable source of truth and Valkey as acceleration.

## Domain Entities and Ownership
Core entities: `games`, `users`, `mods`, `tags`.

Ownership rules:
- A mod belongs to exactly one game.
- A mod belongs to exactly one user (its maintainer).
- Mod ownership and game association are required for registration.

## Access Control and Accounts
Roles: `admin`, `moderator`, `maintainer`, `user`.

Account model rules:
1. No public registration. Admins create accounts.
2. Login exists at `GET /login` but is not in public navigation.
3. Admins add/manage games and user accounts.
4. Maintainers and admins can register mods.
5. Maintainers with hidden mods cannot register new mods until resolved.

Moderation rules:
1. Moderators can approve, hide, and unhide mods.
2. Hidden and unverified mods are invisible site-wide to non-staff.
3. Only admins and moderators can view hidden/pending mods.
4. Admins can permanently delete mods.
5. Admins apply temporary or permanent bans. Banned users cannot perform authenticated write actions.

Reports:
- All logged-in users can report mods.
- Required report types: `rule_violation`, `malicious_code`, `not_working`.

## Maintainer Authentication Model
Authentication is hybrid:
- Cookie-based account session for UI and admin/moderation actions.
- Mod key bearer token for CI refresh flows (`Authorization: Bearer <mod_key>`).

Rules:
1. Exactly one active mod key per mod.
2. Key is issued at mod registration (and on rotate). Shown in plaintext once only.
3. Only the SHA-256 hash of the key is stored.
4. Maintainer can rotate or revoke the key at any time from the manage mod page.
5. No GitHub OAuth, OIDC, webhook signing, or account delegation.

## Content Refresh Flow
Two ways to trigger a content refresh (README, images):
1. **Manual** — maintainer clicks Trigger Content Refresh on the manage mod page.
2. **CI** — `POST /api/v1/refresh/mod` with `Authorization: Bearer <mod_key>`.

Content refresh is blocked for `unverified` mods.

Throttling:
- At most one accepted refresh per mod every `Refresh:MinIntervalMinutes` minutes.
- Returns `429` with `retry_after_seconds` if cooldown is active.
- Idempotency key support for retry-safe CI runs; duplicate jobs within the window are coalesced.
- Processing is async: API enqueues job and returns `202 Accepted` with job ID. Status polled at `GET /api/v1/refresh/jobs/{jobId}`.

## Markdown Rendering Rules
- Render remote README and local static `.md` content.
- Sanitise rendered HTML to prevent XSS.
- Rewrite relative links and image paths through provider-aware resolvers.
- Disallow unsafe HTML/script payloads.

## Docker Compose Requirements
Single compose stack: `web` (ASP.NET), `postgres`, `valkey`.

Required runtime standards:
- Health checks for all services.
- Persistent volume for Postgres.
- Environment-driven configuration with local development defaults.

## Reliability and Observability
- Structured logs for provider calls, cache hit/miss, throttle decisions, and refresh jobs.
- Timeouts and retry/backoff for upstream API calls.
- Metrics for endpoint latency, cache effectiveness, queue depth, and refresh outcomes.
- Graceful degradation when upstream providers are unavailable.

## Configuration Contract
Required settings:
- `Manifest:FileName` — manifest filename (default: `modvox.json`)
- `Refresh:MinIntervalMinutes` — per-mod content refresh cooldown
- `Tags:DefaultSeedLabels` — tag labels to seed on first startup
- Cache TTLs by resource type (readme, images, releases, listing, page)
- Provider API timeout/retry settings
- Auth cookie/session settings
- Moderation report page sizes and retention policy

## Contribution Checklist (Required Before Merge)
1. Endpoint follows one-class-per-endpoint rule.
2. Provider-specific logic remains behind the provider abstraction.
3. Cache strategy is defined and implemented for any new read path.
4. No repository cloning is introduced.
5. Markdown rendering remains sanitised.
6. Mod key scope and cooldown policy are enforced on all refresh paths.
7. Unverified and hidden mod visibility is correctly enforced for all public queries.
8. `docker-compose.yml` remains single-file and functional.

---

## Implementation Status

### Implemented

**Infrastructure**
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

**Manifest system**
- `modvox.json` manifest schema with required and optional fields.
- `ManifestService` reads, parses, and validates manifests via the provider API.
- `ManifestOptions` config section (`Manifest:FileName`).
- Tag label resolution (case-insensitive, unknown labels silently dropped).
- `GET /api/v1/manifest/scaffold` — scaffold file download, no auth.
- `POST /api/v1/mods/{modId}/manifest/refresh` — re-reads manifest, updates metadata, verifies token.

**Mod registration and lifecycle**
- `POST /api/v1/mods` — register mod; reads manifest at registration time; issues mod key and verify token; creates mod in `unverified` state.
- Mod moderation statuses: `unverified`, `pending`, `approved`, `hidden`.
- Mod key: issued at registration, rotate at any time, revoke to disable CI.
- `POST /api/v1/mods/{modId}/keys/rotate`
- `POST /api/v1/mods/{modId}/keys/revoke`
- Maintainer restriction: hidden mods block new registrations.
- Content refresh blocked for `unverified` mods.

**Mod domain model fields**
- `provider`, `owner`, `repository`, `default_ref`
- `name`, `description`, `credits` (free-form string dictionary)
- `readme_path`, `changelog_path`, `images_folder`
- `tag_ids`, `download_count`, `moderation_status`
- `verify_token`, `key_hash` (nullable — null means revoked), `key_version`

**Content refresh**
- `POST /api/v1/refresh/mod` — CI bearer-token refresh, async job enqueue.
- `GET /api/v1/refresh/jobs/{jobId}` — job status polling.
- Refresh cooldown enforced via `Refresh:MinIntervalMinutes`.
- `429` response with `retry_after_seconds`.

**Games and tags**
- `POST /api/v1/admin/games`, `GET /api/v1/admin/games`, `GET /api/v1/admin/games/{gameId}`, `POST /api/v1/admin/games/{gameId}`
- `GET /api/v1/games`
- `POST /api/v1/admin/tags`, `GET /api/v1/tags`, `POST /api/v1/admin/tags/{tagId}`, `DELETE /api/v1/admin/tags/{tagId}`
- Startup tag seeding from `Tags:DefaultSeedLabels`.

**User and account management**
- `POST /api/v1/auth/login`, `POST /api/v1/auth/logout`, `POST /api/v1/auth/logout-all`, `GET /api/v1/auth/me`
- `POST /api/v1/auth/change-credentials`, `POST /api/v1/account/change-display-name`, `POST /api/v1/account/change-password`, `POST /api/v1/account/delete`
- `POST /api/v1/admin/users`, `GET /api/v1/admin/users`, `GET /api/v1/admin/users/{userId}`
- `POST /api/v1/admin/users/{userId}/role`, `/email`, `/password`, `/username`, `/display-name`, `/revoke-all-tokens`
- `POST /api/v1/admin/users/{userId}/ban`, `/unban`
- `POST /api/v1/admin/users/{userId}/mods/keys/revoke-all`
- `GET /api/v1/admin/users/{userId}/mods`
- Default admin account seeded on startup.

**Moderation**
- `POST /api/v1/mods/{modId}/moderation/approve`, `/hide`, `/unhide`
- `DELETE /api/v1/admin/mods/{modId}`
- `POST /api/v1/mods/{modId}/reports`, `GET /api/v1/moderation/reports`, `POST /api/v1/moderation/reports/{reportId}/resolve`
- Report types: `rule_violation`, `malicious_code`, `not_working`.

**Public discovery**
- `GET /api/v1/games/{gameId}/mods`, `GET /api/v1/games/{gameId}/mods/{modId}`
- `POST /api/v1/games/{gameId}/mods/{modId}/download` (download counter)
- Public routes: `GET /`, `GET /{gameId}`, `GET /{gameId}/{modId}`, `GET /user/{userId}`
- `unverified` and `hidden` mods excluded from all public queries.

**Audit log**
- Append-only audit log model and service.
- `GET /api/v1/admin/audit/export`, `POST /api/v1/admin/audit/purge`

**Web UI routes**
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

**UI features**
- Role-conditional navigation in site header (My Mods for maintainer/admin, Staff for admin/moderator).
- Cookie notice banner.
- Status badges for all moderation statuses including `unverified`.
- Manifest scaffold download button on the register mod page.
- Process explanation with numbered steps on the register mod page.
- Verify token and mod key reveal panels with copy-to-clipboard.
- Refresh Manifest button on the manage mod page; reloads on successful verification.

---

### Not yet implemented
- **PostgreSQL-backed repositories** — all repositories are currently in-memory.
- **Valkey-backed cache store** — current cache store is in-memory.
- **Durable session storage** — sessions are in-memory; lost on restart.
- **Durable audit log** — audit log is in-memory; lost on restart.
- **Cache invalidation** — cache invalidation on moderation/hide/unhide actions is not fully wired.
- **ETag / conditional requests** — provider calls do not yet use `If-Modified-Since` / `ETag` headers.
- **Rate limit handling** — upstream GitHub API rate limits are not handled with backoff/retry.
- **Thunderstore-compatible API** — not yet implemented. Goal is to expose endpoints that match the Thunderstore package API schema closely enough for existing Thunderstore mod manager clients to work without modification. Thunderstore API reference: `https://thunderstore.io/api/docs/` (OpenAPI 2.0). Key endpoint groups to model against:
  - `/api/v1/package/` and community-scoped variants for package listing.
  - `/api/experimental/package-index/` for newline-delimited JSON index stream.
  - `/api/experimental/package/{namespace}/{name}/{version}/readme/` for README retrieval.
  - Keep Thunderstore adapter code behind its own abstraction boundary so it does not bleed into core domain logic.
