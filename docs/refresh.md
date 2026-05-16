# Content Refresh Flow

Content refresh updates cached README and images from the source repository.

## Triggers

1. **Manual** — maintainer clicks "Trigger Content Refresh" on the manage mod page.
2. **CI** — `POST /api/v1/refresh/mod` with `Authorization: Bearer <mod_key>`.

Content refresh is blocked for `unverified` mods.

## Throttling

- At most one accepted refresh per mod every `Refresh:MinIntervalMinutes` minutes.
- Returns `429` with `retry_after_seconds` if cooldown is active.
- Idempotency key support for retry-safe CI runs; duplicate jobs within the cooldown window are coalesced (not double-enqueued).

## Job Lifecycle

- Processing is async: the API enqueues the job and returns `202 Accepted` with a job ID.
- Job status is polled at `GET /api/v1/refresh/jobs/{jobId}`.

## Manifest Refresh

Separate from content refresh. Triggered via `POST /api/v1/mods/{modId}/manifest/refresh`.  
Re-reads the manifest, updates metadata, and checks the verify token (transitions `unverified` → `pending` if token matches).
