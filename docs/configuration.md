# Configuration Contract

All settings are environment-driven with local development defaults.

## Required Settings

| Key | Default | Description |
|---|---|---|
| `Manifest:FileName` | `modvox.json` | Manifest filename |
| `Refresh:MinIntervalMinutes` | — | Per-mod content refresh cooldown |
| `Tags:DefaultSeedLabels` | — | Tag labels to seed on first startup |
| `ConnectionStrings:Postgres` | `Host=postgres;Port=5432;Database=modvox;Username=modvox;Password=modvox` | PostgreSQL connection string |
| `Valkey:ConnectionString` | `valkey:6379` | Valkey connection string |
| `Cache:ReadmeTtlMinutes` | `30` | TTL for README content |
| `Cache:ChangelogTtlMinutes` | `30` | TTL for CHANGELOG content |
| `Cache:ImagesTtlMinutes` | `15` | TTL for image listings |
| `Cache:ReleasesTtlMinutes` | `10` | TTL for release data |
| `Cache:ListingTtlMinutes` | `5` | TTL for listing responses |
| `Cache:PageTtlMinutes` | `3` | TTL for rendered pages |
| `Cache:NegativeTtlMinutes` | `2` | TTL for negative cache entries |
| `Cache:StaleWindowMinutes` | `5` | Stale-while-revalidate window |
| `Providers:GitHub:ApiBaseUrl` | `https://api.github.com` | GitHub API base URL |
| `Providers:GitHub:RawBaseUrl` | `https://raw.githubusercontent.com` | GitHub raw content base URL |
| `Providers:GitHub:TimeoutSeconds` | `15` | GitHub request timeout |
| `Thunderstore:OpenApiUrl` | `https://thunderstore.io/api/docs/?format=openapi` | Upstream Thunderstore OpenAPI reference |
| `Thunderstore:PackageIndexUrl` | `https://thunderstore.io/api/experimental/package-index/` | Upstream package-index URL reference |
| Provider API timeout | — | Timeout for upstream API calls |
| Provider API retry | — | Retry/backoff settings |
| Auth cookie settings | — | Cookie TTL, domain, SameSite policy |
| Moderation report page size | — | Results per page for report queues |
| Moderation report retention | — | How long resolved reports are kept |
