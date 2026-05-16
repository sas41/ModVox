# Manifest System

Mod metadata is declared in a manifest file committed to the repository root.

## Filename

Configured per instance via `Manifest:FileName` (default: `modvox.json`).

## Schema

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
    "00000000-0000-0000-0000-000000000000": "What they did"
  }
}
```

Required fields: `name`, `default_ref`, `readme`, `changelog`, `images`, `tags` (at least one label matching a server tag).  
Optional fields: `description`, `credits`, `verify`.

## Rules

- `tags` labels are matched case-insensitively against server tags. Unknown labels are silently ignored. At least one must resolve.
- `credits` keys are ModVox user IDs (GUIDs). Any key that is not a valid GUID is silently ignored.
- `verify` is a server-generated ownership token. When present and matching the stored token the mod transitions from `unverified` to `pending`.
- Manifest is read at registration time and on every **Refresh Manifest** action.
- `default_ref` sets the branch for all subsequent content fetches. It can be overridden per-request via an explicit `ref` parameter on the Refresh Manifest endpoint.

## Registration Flow

1. Maintainer submits: game, repo URL (parsed for provider/owner/repo), optional initial ref.
2. Server fetches the manifest at `initial_ref` (defaults to `HEAD` if not supplied).
3. If the manifest is missing or invalid → `422 Unprocessable Entity`.
4. Mod is created in `unverified` state with fields populated from the manifest.
5. Mod key and verify token are both issued immediately and shown once.
6. Maintainer adds the verify token to the `verify` field and commits.
7. Maintainer clicks **Refresh Manifest** on the edit page. If the token matches, the mod transitions to `pending` (visible to moderators for approval).

## Scaffold Endpoint

`GET /api/v1/manifest/scaffold` — no auth required. Returns a pre-filled scaffold JSON as a file download using the configured filename. The `verify` field is present at the top, set to `""`.
