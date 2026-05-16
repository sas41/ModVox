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
- Discover mods from public git repository hosts (GitHub first; GitLab, Codeberg, Gitea later).
- Render remote README/CHANGELOG content and images from configured repository folders.
- Mod downloads link directly to release artifact URLs — never proxy.
- Mod metadata lives in a manifest file in the repository (`modvox.json` by default).
- Expose a Thunderstore-compatible API for existing mod manager clients (MVP endpoints are now implemented).

## Architecture Boundaries
- Keep host-specific behavior behind provider interfaces.
- Keep domain and application services provider-agnostic.
- Keep endpoint classes thin and explicit.
- Use explicit DTO mapping in code (no reflection-heavy mappers).

## Contribution Checklist (Required Before Merge)
1. Endpoint follows one-class-per-endpoint rule.
2. Provider-specific logic remains behind the provider abstraction.
3. Cache strategy is defined and implemented for any new read path.
4. No repository cloning is introduced.
5. Markdown rendering remains sanitised.
6. Mod key scope and cooldown policy are enforced on all refresh paths.
7. Unverified and hidden mod visibility is correctly enforced for all public queries.
8. `docker-compose.yml` remains single-file and functional.

## Current Baseline Notes
- Database: EF Core + PostgreSQL is active and migrations are applied at startup.
- Cache: read-path cache coordinator is active and uses Valkey as the backing store.
- Refresh: unified refresh path is the source of truth for maintainer and CI refresh actions.
- Thunderstore: mod-manager-focused compatibility endpoints are available under `/api/v1/package/` and `/api/experimental/*`.

---

## Reference Docs

| Topic | File |
|---|---|
| Manifest schema, registration flow | [docs/manifest.md](docs/manifest.md) |
| Provider abstraction contract | [docs/providers.md](docs/providers.md) |
| Caching rules (Valkey) | [docs/caching.md](docs/caching.md) |
| Persistence rules (PostgreSQL) | [docs/persistence.md](docs/persistence.md) |
| Access control, auth, moderation | [docs/access-control.md](docs/access-control.md) |
| Content refresh flow | [docs/refresh.md](docs/refresh.md) |
| Configuration contract | [docs/configuration.md](docs/configuration.md) |
| Implementation status | [docs/implementation-status.md](docs/implementation-status.md) |
