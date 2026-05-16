# Access Control and Accounts

## Roles

`admin`, `moderator`, `maintainer`, `user`.

## Account Model

1. No public registration. Admins create accounts.
2. Login exists at `GET /login` but is not in public navigation.
3. Admins add/manage games and user accounts.
4. Maintainers and admins can register mods.
5. Maintainers with hidden mods cannot register new mods until resolved.

## Authentication Model

Authentication is hybrid:

- **Cookie-based session** — for UI and admin/moderation actions. `HttpOnly`, `Secure`, `SameSite=Lax`, 8-hour TTL.
- **Mod key bearer token** — for CI refresh flows (`Authorization: Bearer <mod_key>`).

Mod key rules:
1. Exactly one active mod key per mod.
2. Key is issued at registration (and on rotate). Shown in plaintext once only.
3. Only the SHA-256 hash is stored.
4. Maintainer can rotate or revoke at any time from the manage mod page.
5. No GitHub OAuth, OIDC, webhook signing, or account delegation.

## Moderation

| Status | Visible to public | Visible to moderators/admins | Notes |
|---|---|---|---|
| `unverified` | No | No | Newly registered; awaiting verify token match |
| `pending` | No | Yes | Verified; awaiting moderator approval |
| `approved` | Yes | Yes | Publicly visible |
| `hidden` | No | Yes | Hidden by moderator action |

Rules:
1. Moderators can approve, hide, and unhide mods.
2. Hidden and unverified mods are invisible site-wide to non-staff.
3. Only admins and moderators can view hidden/pending mods.
4. Admins can permanently delete mods.
5. Admins apply temporary or permanent bans. Banned users cannot perform authenticated write actions.

## Reports

- All logged-in users can report mods.
- Required report types: `rule_violation`, `malicious_code`, `not_working`.
