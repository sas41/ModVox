# AGENTS.md

## Mission
Build ModVox: a fast website and API for discovering mods hosted on public git repositories, using Thunderstore-compatible data and workflows.

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
- Render remote `README.md` files.
- Find images from configured repository folders.
- Link directly to release artifacts from provider URLs.

## Architecture Boundaries
- Keep host-specific behavior behind provider interfaces.
- Keep domain and application services provider-agnostic.
- Keep endpoint classes thin and explicit.
- Use explicit DTO mapping in code (no reflection-heavy mappers).

## Provider Abstraction Contract
Each provider implementation must support:
1. Fetch markdown/raw file by repo + ref + path.
2. List files in a folder (for image discovery).
3. List releases and release artifacts.
4. Resolve public/canonical URLs for files and artifacts.

Rules:
- No `git clone` operations.
- Use remote API endpoints only.
- Use conditional requests (`ETag`, `If-Modified-Since`) where available.
- Handle upstream rate limits as a first-class concern.

## Thunderstore Compatibility
Use Thunderstore schema as an upstream-compatible source contract where possible.

Primary source of truth for integration work:
- Docs UI: `https://thunderstore.io/api/docs/`
- OpenAPI schema: `https://thunderstore.io/api/docs/?format=openapi`
- Local saved API config for agents: `docs/thunderstore-api-config.json`

Compatibility notes:
- Schema currently reports OpenAPI/Swagger `2.0`.
- Host: `thunderstore.io`
- Base path: `/`
- Schemes: `https`
- Default consumes/produces: `application/json`
- Security definition present: `Basic` auth (some endpoints are public, some are auth-gated).

Important endpoint groups to model around:
- `/api/v1/package/` and community-scoped variants for package listing.
- `/api/v1/package-metrics/...` for package/version metrics.
- `/api/experimental/package-index/` for newline-delimited JSON index stream.
- `/api/experimental/package/{namespace}/{name}/{version}/readme/` for markdown retrieval.

Treat `experimental` endpoints as unstable and protect usage behind adapter boundaries.

When refreshing Thunderstore integration assumptions, update `docs/thunderstore-api-config.json` from the OpenAPI URL above.

## Caching Rules (Valkey) - Mandatory
Use cache-aside for all read paths.

Every endpoint/service must define:
- cache key schema
- TTL
- invalidation strategy
- stale behavior

Required cache behaviors:
1. Key schema includes provider + owner + repo + ref + path (or equivalent unique identifiers).
2. Negative caching for not-found responses.
3. Stale-while-revalidate for high-traffic read pages.
4. Stampede prevention using lock/single-flight semantics.

## Persistence Rules (PostgreSQL)
Persist durable mod data, including:
- normalized package/mod metadata
- source repository mapping
- release artifact references
- fetch metadata (`etag`, `last_modified`, `checked_at`, failure state)
- maintainer key metadata and rotation history

Use Postgres as the durable source of truth and Valkey as acceleration.

## Domain Entities and Ownership
Core entities:
- `games`
- `users`
- `mods`

Ownership rules:
- A mod belongs to exactly one game.
- A mod belongs to exactly one user (its maintainer owner).
- Mod ownership and game association are required for create/update flows.

## Access Control and Accounts
Use role-based access control with these roles:
- `admin`
- `moderator`
- `maintainer`
- `user` (normal user role, currently minimal feature usage)

Account model rules:
1. Only maintainers require accounts today, but the `user` role must exist.
2. Login exists but is not publicly advertised.
3. No public registration page or self-signup flow.
4. Admins create user accounts.
5. Admins add/manage games.

Moderation and visibility rules:
1. Moderators can approve mods.
2. Moderators can hide mods.
3. Hidden mods are hidden site-wide for non-staff users.
4. Only admins and moderators can view hidden mods.
5. Admins can permanently delete hidden mods or remove the hidden flag.
6. Maintainers with flagged/hidden mods cannot add or update mods until resolved.

User enforcement rules:
1. Admins can apply temporary bans.
2. Admins can apply permanent bans.
3. Banned users cannot perform authenticated write actions.

Reports:
- All logged-in users can report mods.
- Required report types:
  - `rule_violation`
  - `malicious_code`
  - `not_working`
- Reporting infrastructure and moderation workflow must exist in API/domain design.

## Maintainer Authentication Model
Authentication is hybrid:
- account-based auth for product UX/admin/moderation actions
- platform-issued mod key auth for CI refresh flows

Rules:
1. Exactly one active secret key per mod.
2. Generate a random key when a maintainer adds/registers a mod.
3. Show plaintext key once at creation/rotation time only.
4. Store only a secure hash of the key in Postgres.
5. Maintainer can rotate key at any time.
6. No public self-signup flow is required.
7. No GitHub auth, OIDC, webhook signing, or account delegation is required.

## Maintainer-Triggered Update Flow
Maintain two ways to refresh mod content:
1. Manual request by maintainer in product UX or admin action.
2. API request from CI workflows (for example GitHub Actions) using mod key.

### Refresh Throttling Policy
- Allow at most one accepted refresh per mod every configurable `N` minutes.
- If called before cooldown expires, return `429 Too Many Requests`.
- Include `retry_after_seconds` in `429` responses.

### Refresh API Requirements
- Endpoint auth: `Authorization: Bearer <mod_key>`.
- Refresh processing is asynchronous:
  - API validates scope and cooldown.
  - API enqueues job and returns `202 Accepted` with job id.
  - Worker executes fetch -> upsert -> cache invalidation/warm.
- Support idempotency keys for retry-safe CI runs.
- Coalesce duplicate jobs for same mod/repo within short windows.

### Required Endpoint Set (One Class Per Endpoint)
- `POST /api/v1/mods` (register mod and issue key)
- `POST /api/v1/mods/{modId}/keys/rotate`
- `POST /api/v1/refresh/mod`
- `GET /api/v1/refresh/jobs/{jobId}`

### Access and Moderation Endpoint Requirements (One Class Per Endpoint)
Minimum endpoint coverage:
- `POST /api/v1/auth/login` (not publicly advertised in UX navigation)
- `POST /api/v1/admin/users` (admin-created accounts only)
- `POST /api/v1/admin/games`
- `POST /api/v1/mods/{modId}/moderation/approve`
- `POST /api/v1/mods/{modId}/moderation/hide`
- `POST /api/v1/mods/{modId}/moderation/unhide`
- `DELETE /api/v1/admin/mods/{modId}` (permanent delete)
- `POST /api/v1/admin/users/{userId}/ban` (temporary or permanent)
- `POST /api/v1/admin/users/{userId}/unban`
- `POST /api/v1/mods/{modId}/reports`
- `GET /api/v1/moderation/reports`
- `POST /api/v1/moderation/reports/{reportId}/resolve`

Optional if needed:
- `POST /api/v1/mods/{modId}/keys/revoke`
- `POST /api/v1/refresh/repository`

## Markdown Rendering Rules
- Render remote `README.md` and local static `.md` content.
- Sanitize rendered HTML to prevent XSS.
- Rewrite relative links and image paths through provider-aware resolvers.
- Disallow unsafe HTML/script payloads.

## Images and Release Artifacts
- Support configured image folder discovery conventions per repository.
- Resolve image URLs directly from provider APIs/CDN endpoints.
- Link directly to release artifact URLs without proxying by default.
- Validate artifact metadata before storage/indexing.

## Docker Compose Requirements
Single compose stack includes:
- `web` (ASP.NET app)
- `postgres`
- `valkey`

Required runtime standards:
- health checks for all services
- persistent volume for Postgres
- environment-driven configuration
- local development defaults with override-friendly env vars

## Reliability and Observability
- Structured logs for provider calls, cache hit/miss, throttle decisions, and refresh jobs.
- Timeouts and retry/backoff for upstream API calls.
- Metrics for endpoint latency, cache effectiveness, queue depth, and refresh outcomes.
- Graceful degradation when upstream providers are unavailable.

## Configuration Contract
Minimum required settings:
- `Refresh:MinIntervalMinutes` (global cooldown for per-mod refresh acceptance)
- cache TTLs by data type (readme, images, releases, listing, page)
- provider API timeout/retry settings
- queue worker parallelism and retry policy
- auth token/session settings for account login
- moderation/report queue page sizes and retention policy
- user ban defaults (temporary duration, enforcement toggles)

## Contribution Checklist (Required)
Before merge:
1. Endpoint follows one-class-per-endpoint rule.
2. Provider-specific logic remains behind abstraction.
3. Cache strategy is defined and implemented.
4. No repository cloning is introduced.
5. Markdown rendering remains sanitized.
6. Maintainer key scope and cooldown policy are enforced.
8. Hidden-mod visibility is correctly enforced for staff vs public access.
9. `docker-compose.yml` remains single-file and functional.

## Implementation Status (Current In-Dev Snapshot)
This section tracks what is already implemented in code vs what remains.

## Auth Hardening Plan (In-Progress Checklist)
- [x] Add secure cookie-based account auth (`HttpOnly`, `Secure`, `SameSite`) for website/API account flows.
- [x] Add durable session model with per-user session version and revoke-all semantics.
- [x] Add `POST /api/v1/auth/logout` and `POST /api/v1/auth/logout-all`.
- [x] Move public login page to `GET /login` and keep it out of public nav.
- [x] Add role-gated `GET /staff` page that requires `admin` or `moderator`.
- [x] Add user settings/account self-service for password, username, display name updates.
- [x] Add account delete endpoint and UI action.
- [x] Add admin user management endpoints for role/email/password changes and revoke-all sessions.
- [x] Add global tags model and enforce at least one tag per mod.
- [x] Add startup seed for default tags from config if tag list is empty.
- [x] Add public user profile route `GET /user/{userId}` with username/display-name separation.
- [x] Add public discovery routes:
  - `GET /` game search/listing (default name sort)
  - `GET /{gameId}` game mod search/listing (default download sort)
  - `GET /{gameId}/{modId}` mod detail
- [x] Add download counters on mods and use in listing sort.
- [x] Add append-only audit/mod log with admin export and purge actions.
- [x] Add cookie notice banner.

### Completed Change Log
- 2026-05-16: Added this auth hardening and product expansion checklist to track implementation as work lands.
- 2026-05-16: Implemented cookie-based account sessions with secure cookie defaults, durable in-memory session store abstraction, and session-version revoke-all semantics.
- 2026-05-16: Added account auth endpoints `POST /api/v1/auth/logout`, `POST /api/v1/auth/logout-all`, and `GET /api/v1/auth/me`.
- 2026-05-16: Moved login route to `GET /login`, added `GET /staff` role-gated page, and kept `GET /staff/login` as redirect shim.
- 2026-05-16: Added account settings page `GET /settings` plus self-service actions for credential changes, display name, password, logout-all, and account delete.
- 2026-05-16: Expanded user model with `display_name`, `email`, `session_version`, `is_deleted`; added public profile page `GET /user/{userId}`.
- 2026-05-16: Added admin user-management endpoints for list, role/email/password updates, and revoke-all sessions.
- 2026-05-16: Added global tags model/repository/endpoints, required at least one tag on mod registration, and startup tag seeding from config.
- 2026-05-16: Added public discovery routes for game listing, game mod listing, and mod detail with default sorting behavior.
- 2026-05-16: Added mod download count tracking endpoint and integrated download-based default sort.
- 2026-05-16: Added append-only audit log model/repository/service and admin export/purge endpoints.
- 2026-05-16: Added cookie notice banner component and rendered it on public/staff pages.
- 2026-05-16: Completed web UI cutover from minimal API HTML string endpoints to Razor Pages and removed superseded page endpoint classes.
- 2026-05-16: Added/finished Razor Pages for `/staff/moderation`, `/settings`, `/staff/login` redirect shim, `/user/{userId}`, `/{gameId}`, and `/{gameId}/{modId}`.
- 2026-05-16: Updated app startup to use Razor Pages (`AddRazorPages` + `MapRazorPages`) while keeping one-class-per-endpoint API controllers.
- 2026-05-16: Reorganized `ModVox.Web` source layout to top-level folders: `Core`, `Controllers`, `Pages`, `Models`, `wwwroot`.
- 2026-05-16: Moved static markdown content from `Content/Pages/*` to `Pages/Content/*` and updated page/include services accordingly.

### Implemented so far
- One-class-per-endpoint pattern is in place across API surface.
- Razor Pages are now the website rendering path for public/staff/account pages.
- Core refresh flow exists with async queue/worker and idempotency key support.
- Cache coordinator and key factory exist with readme/images/releases cache writes.
- Account login exists at `POST /api/v1/auth/login`.
- Hidden, non-public staff UI routes exist:
  - `GET /login`
  - `GET /staff/login` (redirect shim)
  - `GET /staff`
  - `GET /settings`
- Account credential update endpoint exists: `POST /api/v1/auth/change-credentials`.
- Account session/auth endpoints exist:
  - `GET /api/v1/auth/me`
  - `POST /api/v1/auth/logout`
  - `POST /api/v1/auth/logout-all`
- RBAC model is implemented in domain with roles:
  - `admin`
  - `moderator`
  - `maintainer`
  - `user`
- Ban model is implemented (temporary/permanent) and enforced in authenticated write/login flows.
- User account model includes:
  - `display_name`
  - `email`
  - `session_version`
  - `is_deleted`
- Mod ownership model now includes:
  - `game_id`
  - `maintainer_user_id`
- Mod model includes:
  - `changelog_path`
  - `tag_ids`
  - `download_count`
  - nullable `key_hash` for key revoke
- Game creation endpoint exists: `POST /api/v1/admin/games`.
- Game listing endpoint exists: `GET /api/v1/games`.
- Admin account creation endpoint exists: `POST /api/v1/admin/users`.
- Admin user-management endpoints exist:
  - `GET /api/v1/admin/users`
  - `POST /api/v1/admin/users/{userId}/role`
  - `POST /api/v1/admin/users/{userId}/email`
  - `POST /api/v1/admin/users/{userId}/password`
  - `POST /api/v1/admin/users/{userId}/revoke-all-tokens`
- User ban/unban endpoints exist:
  - `POST /api/v1/admin/users/{userId}/ban`
  - `POST /api/v1/admin/users/{userId}/unban`
- Moderation endpoints exist:
  - `POST /api/v1/mods/{modId}/moderation/approve`
  - `POST /api/v1/mods/{modId}/moderation/hide`
  - `POST /api/v1/mods/{modId}/moderation/unhide`
  - `DELETE /api/v1/admin/mods/{modId}`
- Report infrastructure exists with required types:
  - `rule_violation`
  - `malicious_code`
  - `not_working`
- Report endpoints exist:
  - `POST /api/v1/mods/{modId}/reports`
  - `GET /api/v1/moderation/reports`
  - `POST /api/v1/moderation/reports/{reportId}/resolve`
- Tag endpoints exist:
  - `POST /api/v1/admin/tags`
  - `GET /api/v1/tags`
  - `POST /api/v1/admin/tags/{tagId}`
  - `DELETE /api/v1/admin/tags/{tagId}`
- Key revoke endpoint exists:
  - `POST /api/v1/mods/{modId}/keys/revoke`
- Public discovery APIs and pages exist:
  - `GET /api/v1/games/{gameId}/mods`
  - `GET /api/v1/games/{gameId}/mods/{modId}`
  - `POST /api/v1/games/{gameId}/mods/{modId}/download`
  - `GET /` (games)
  - `GET /{gameId}` (mods)
  - `GET /{gameId}/{modId}` (mod detail)
  - `GET /user/{userId}` (public profile)
- Audit log endpoints exist:
  - `GET /api/v1/admin/audit/export`
  - `POST /api/v1/admin/audit/purge`
- Maintainer restriction implemented: maintainers with hidden mods cannot add/update mods.
- Mod registration enforces at least one valid global tag.
- Cookie notice banner is rendered on public and staff pages.
- Source layout is standardized under `Core`, `Controllers`, `Pages`, `Models`, and `wwwroot`.

### Not done yet
- PostgreSQL-backed repositories are not implemented yet (current implementation is in-memory repositories).
- Valkey-backed cache store is not implemented yet (current store is in-memory).
- Staff/public hidden-mod visibility filtering for read/list endpoints exists for game mod list/detail, but broader search/feed coverage is still incomplete.
- Durable audit log exists in-memory, but PostgreSQL-backed permanent audit storage is not implemented yet.
- Production-grade auth cookie/session persistence (Valkey/Postgres) is not implemented yet.
- Cache invalidation matrix for moderation/report actions is not fully implemented yet.
