# Configuration Contract

All settings are environment-driven with local development defaults.

## Required Settings

| Key | Default | Description |
|---|---|---|
| `Manifest:FileName` | `modvox.json` | Manifest filename |
| `Refresh:MinIntervalMinutes` | — | Per-mod content refresh cooldown |
| `Tags:DefaultSeedLabels` | — | Tag labels to seed on first startup |
| `Cache:Ttl:Readme` | — | TTL for cached README content |
| `Cache:Ttl:Images` | — | TTL for cached image listings |
| `Cache:Ttl:Releases` | — | TTL for cached release data |
| `Cache:Ttl:Listing` | — | TTL for mod listing pages |
| `Cache:Ttl:Page` | — | TTL for general static pages |
| Provider API timeout | — | Timeout for upstream API calls |
| Provider API retry | — | Retry/backoff settings |
| Auth cookie settings | — | Cookie TTL, domain, SameSite policy |
| Moderation report page size | — | Results per page for report queues |
| Moderation report retention | — | How long resolved reports are kept |
