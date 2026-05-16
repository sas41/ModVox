# Content Refresh Flow

Refresh is unified across maintainer and CI paths and always executes the same pipeline.

## Triggers

1. **Manual** — maintainer clicks "Refresh Mod" on the manage mod page.
2. **CI** — `POST /api/v1/refresh/mod` with `Authorization: Bearer <mod_key>`.

Refresh is blocked for `unverified` mods.

## Unified Refresh Pipeline

Each accepted refresh executes the following steps, in order:

1. Read and validate `modvox.json`.
2. Verify manifest `verify` token against stored token.
3. Update normalized manifest metadata (`name`, `description`, refs/paths, tags, credits/external credits).
4. Fetch and persist README + CHANGELOG markdown/html.
5. Fetch and upsert releases + artifacts.

If verify token is missing or mismatched, refresh fails hard and no partial update is considered successful.

## Throttling

- At most one accepted refresh per mod every `Refresh:MinIntervalMinutes` minutes.
- Returns `429` with `retry_after_seconds` if cooldown is active.
- Idempotency key support for retry-safe CI runs; duplicate jobs within the cooldown window are coalesced (not double-enqueued).

## Job Lifecycle (CI endpoint)

- Processing is async: the CI API enqueues the job and returns `202 Accepted` with a job ID.
- Job status is polled at `GET /api/v1/refresh/jobs/{jobId}`.

## Maintainer Refresh Endpoint

Maintainer/admin page action uses:

- `POST /api/v1/mods/{modId}/refresh`

The legacy alias remains available:

- `POST /api/v1/mods/{modId}/manifest/refresh`
