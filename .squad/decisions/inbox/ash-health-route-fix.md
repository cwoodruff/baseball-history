# Ash Decision: Resolve `/Health` route ambiguity

- **Date:** 2026-06-09
- **Owner:** Ash
- **Status:** Accepted

## Context

The web app now exposes a Razor support page at `/Health`, while the shared service-defaults package also mapped a machine health-check endpoint at `/health`. ASP.NET Core route matching is case-insensitive, so requests to `/Health` matched both endpoints and failed with `AmbiguousMatchException` during integration tests.

## Decision

Move the machine-readable readiness endpoint from `/health` to `/healthz` in `baseball-history-servicedefaults`, keep `/alive` as the liveness endpoint, and preserve the existing Razor support page at `/Health`.

## Rationale

- Preserves the intended human-readable `/Health` page contract already used by the app and tests.
- Keeps a dedicated readiness endpoint for platform probes without adding Aspire-specific logic to the web project.
- Avoids unrelated route changes and remains compatible with local, App Service, and Aspire scenarios where probe paths can target a machine endpoint explicitly.

## Consequences

- Human-facing health diagnostics remain at `/Health`.
- Machine readiness probes should use `/healthz`.
- Liveness probes remain on `/alive`.
