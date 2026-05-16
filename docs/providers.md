# Provider Abstraction Contract

Each provider implementation must support:

1. Fetch a raw file by repo + ref + path.
2. List files in a folder (for image discovery).
3. List releases and release artifacts.
4. Resolve public/canonical URLs for files and artifacts.

## Rules

- No `git clone` operations. Use remote API endpoints only.
- Use conditional requests (`ETag`, `If-Modified-Since`) where available.
- Handle upstream rate limits as a first-class concern (backoff/retry).
- Keep all host-specific behavior behind the provider interface — domain logic must remain provider-agnostic.

## Current Providers

- **GitHub** — initial implementation.
- GitLab, Codeberg, Gitea — planned; must be addable without rewriting domain logic.

## Markdown Rendering

- Render remote README and local static `.md` content.
- Sanitise rendered HTML to prevent XSS.
- Rewrite relative links and image paths through provider-aware resolvers.
- Disallow unsafe HTML/script payloads.
