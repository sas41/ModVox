# Persistence Rules (PostgreSQL)

Postgres is the durable source of truth. Valkey is the acceleration layer. Never use Valkey as the primary store.

## What to Persist

- Normalised mod metadata: `name`, `description`, `paths`, `tags`, `credits`
- Source repository coordinates: `provider`, `owner`, `repository`
- Verify token and moderation status
- Release artifact references
- Fetch metadata: `etag`, `last_modified`, `checked_at`, failure state
- Maintainer key hash and rotation history

## Domain Entities

Core entities: `games`, `users`, `mods`, `tags`.

Ownership rules:
- A mod belongs to exactly one game.
- A mod belongs to exactly one user (its maintainer).
- Mod ownership and game association are required for registration.

## Mod Domain Model Fields

| Field | Notes |
|---|---|
| `provider` | e.g. `github` |
| `owner` | repo owner/org |
| `repository` | repo name |
| `default_ref` | branch for content fetches |
| `name` | from manifest |
| `description` | from manifest, optional |
| `credits` | free-form string dictionary |
| `readme_path` | from manifest |
| `changelog_path` | from manifest |
| `images_folder` | from manifest |
| `tag_ids` | resolved server tag IDs |
| `download_count` | incremented on download |
| `moderation_status` | `unverified`, `pending`, `approved`, `hidden` |
| `verify_token` | server-generated; compared against manifest `verify` field |
| `key_hash` | SHA-256 of mod key; `null` means revoked |
| `key_version` | incremented on rotate/revoke |

## Current State

All repositories are currently in-memory (pending Postgres migration).  
Sessions and audit log are also in-memory and lost on restart.
