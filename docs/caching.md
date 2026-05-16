# Caching Rules (Valkey)

Use cache-aside for all read paths. This is mandatory — every endpoint and service must define its cache key schema, TTL, invalidation strategy, and stale behavior before implementation.

## Key Schema

`provider:owner:repository:ref:path` (or equivalent unique identifiers per resource type).

## Required Behaviors

1. **Negative caching** — cache not-found responses to avoid hammering upstream APIs.
2. **Stale-while-revalidate** — serve stale data while revalidating in the background for high-traffic read pages.
3. **Stampede prevention** — use lock/single-flight semantics so concurrent requests for a cold key do not all hit the upstream simultaneously.

## TTLs

Configure per resource type via app settings:

- `Cache:ReadmeTtlMinutes`
- `Cache:ChangelogTtlMinutes`
- `Cache:ImagesTtlMinutes`
- `Cache:ReleasesTtlMinutes`
- `Cache:ListingTtlMinutes`
- `Cache:PageTtlMinutes`
- `Cache:NegativeTtlMinutes`
- `Cache:StaleWindowMinutes`

## Invalidation

- Moderation actions (approve, hide, unhide) must invalidate affected listing and detail cache entries.
- Manifest refresh must invalidate cached metadata for the affected mod.
- Cache invalidation must be wired at the service layer, not in endpoint handlers.

## Current State

- Cache coordinator and cache key factory are active in the read path.
- Cache backing store is Valkey (`ValkeyCacheStore`) via `StackExchange.Redis`.
- Single-flight locks are persisted in Valkey using key-level lock entries.
- Prefix invalidation scans keys by prefix and deletes matching entries.
