# ModVox

ModVox is a light-weight mod discovery platform for games utilizing public git repositories.

It provides:

- A website for browsing games, mods, releases, README/CHANGELOG content, and credits.
- A JSON API for registration, moderation, refresh, and discovery workflows.
- Thunderstore-compatible endpoints (mod-manager-focused MVP) for existing clients.

## Tech Stack

- ASP.NET Core + C#
- PostgreSQL (durable data)
- Valkey (active cache backend)
- Docker Compose (single-file local stack)

## Quick Start

1. Start the stack:

```bash
docker compose up --build
```

2. Open the app:

- Website: `http://localhost:8080`
- Website: `http://localhost:8080/login`
- Health check: `http://localhost:8080/healthz`

3. Log in with seeded account: admin/admin (created at startup, please change username and password) and configure games/tags/users from staff pages.

## Core Website Flow

1. Maintainer registers a mod (`/maintainer/register`) using a public repository URL.
2. ModVox reads `modvox.json`, creates the mod, and issues:
   - mod key (for CI refresh)
   - verify token (for ownership verification in manifest)
3. Maintainer commits verify token into manifest and runs **Refresh Mod**.
4. Unified refresh validates manifest + verify token, then updates metadata, README/CHANGELOG, and releases.
5. Moderator approves mod from **Manage Mods** (`/staff/moderation`).

## Thunderstore API Compatibility for Mod Managers (MVP)

Implemented endpoints:

- `GET /api/v1/package/`
- `GET /c/{community_identifier}/api/v1/package/`
- `GET /api/experimental/package-index/`
- `GET /api/experimental/package/{namespace}/{name}/`
- `GET /api/experimental/package/{namespace}/{name}/{version}/`
- `GET /api/experimental/package/{namespace}/{name}/{version}/readme/`
- `GET /api/experimental/package/{namespace}/{name}/{version}/changelog/`

## Configuration

Primary settings live in `ModVox.Web/appsettings.json` and can be overridden with environment variables.

Important sections:

- `ConnectionStrings:Postgres`
- `Valkey:ConnectionString`
- `Manifest:FileName`
- `Refresh:MinIntervalMinutes`
- `Cache:*TtlMinutes` and stale/negative windows
- `Providers:GitHub:*`
- `Tags:DefaultSeedLabels`

## Documentation

- `AGENTS.md`
- `docs/manifest.md`
- `docs/providers.md`
- `docs/caching.md`
- `docs/persistence.md`
- `docs/access-control.md`
- `docs/refresh.md`
- `docs/configuration.md`
- `docs/implementation-status.md`

## Current Notes

- Remote repository access is API-based only (no cloning).
- Cache coordinator is implemented and backed by Valkey.
- Markdown is rendered and sanitized before serving.
- EF Core migrations are applied automatically on startup.
