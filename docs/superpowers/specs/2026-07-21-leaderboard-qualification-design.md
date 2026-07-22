# Leaderboard Qualification — Shared Query Layer Design

**Issue:** #63 (closes #75 as a side effect)
**Date:** 2026-07-21
**Status:** Approved

## Problem

Rate-stat leaderboards (batting: `avg`, `obp`, `slg`, `ops`; pitching: `era`, `whip`, `k9`, `bb9`, `wpct`) default to no qualification minimum. The default career AVG board returns 124 players batting 1.000 on 1–2 at-bats; Ty Cobb ranks 584th. The same unqualified results are served by three **separate** query implementations:

| Path | File |
|---|---|
| Website | `baseball-history-web/Pages/Stats/Batting.cshtml.cs`, `Pitching.cshtml.cs` |
| API | `baseball-history-web/Api/Endpoints/LeaderEndpoints.cs` |
| MCP | `baseball-history-mcp/Querying/LeaderboardReadService.cs` |

A flat 3,000-AB career floor is not an acceptable fix: it erases Josh Gibson (2,768 documented AB) because Negro Leagues teams played 60–80 game seasons with partial surviving records.

Audit evidence: issue #62 (closed).

## Solution overview

One shared query layer in a new project, `baseball-history-data`, implementing **season-relative qualification**, consumed by all three paths. The three duplicate implementations are deleted.

## 1. New project: `baseball-history-data`

- Target `net10.0`, nullable enabled, matching solution conventions.
- **Moved in:** all EF entities from `baseball-history-web/Models/` and `BaseballDbContext`. Namespaces swept to `BaseballHistory.Data.Models` / `BaseballHistory.Data`.
- **New:** `Querying/` — leaderboard query service, request/response records, stat catalog, qualification rules.
- **References:** `baseball-history-web` → `data`; `baseball-history-mcp` → `data`. The existing `mcp` → `web` project reference is **removed**.
- DI: `AddDataServices(...)` extension in the data project registers `BaseballDbContext` (Npgsql) and `ILeaderboardQueryService`; composed in each host's `Program.cs`.
- The MCP project's non-leaderboard read services (players, franchises, HoF, salaries, teams) keep working against the moved context; they are **not** restructured in #63 beyond fixing namespaces.

## 2. Qualification rules

Single static class `QualificationRules` — the only place the rule exists.

### Batting

- Plate appearances (NULL-safe): `PA = AB + BB + COALESCE(HBP,0) + COALESCE(SH,0) + COALESCE(SF,0)`
- Season threshold: `3.1 × Teams.G` for the player's team-season.
- Career / year-range threshold: `SUM(3.1 × Teams.G)` over the player's Batting rows inside the active year/league filter. Traded players contribute one term per stint — the same joined rows that supply their stats.

### Pitching

- Season threshold: `3 × Teams.G` outs (1 IP per team game; IP stored as `IPouts`).
- Career / range: `SUM(3 × Teams.G)` outs, same stint semantics as batting.

### Semantics

- Qualification applies **only to rate stats**, and only when `Qualified = true` (the default). Counting stats never filter.
- An explicit `MinAtBats` / `MinInningsPitched` value **replaces** season-relative qualification with a flat floor (researcher override).
- Missing HBP/SH/SF in early-era rows make PA slightly conservative (smaller), i.e. qualification slightly stricter. Accepted.
- Validation (production data): Gibson career PA 3,211 vs derived threshold 3,001 → qualifies. Cobb, Hornsby, Charleston, Stearnes, Suttles all qualify. All 122 one-or-two-AB 1.000 hitters excluded.

## 3. Service surface

```csharp
public sealed record LeaderboardRequest(
    string Stat,
    int? FromYear = null,
    int? ToYear = null,
    string? League = null,
    bool SingleSeason = false,
    bool Qualified = true,
    int? MinAtBats = null,
    int? MinInningsPitched = null,
    int Page = 1,
    int PageSize = 25);

public interface ILeaderboardQueryService
{
    Task<PagedResult<BattingLeaderRow>> GetBattingLeadersAsync(LeaderboardRequest request, CancellationToken cancellationToken);
    Task<PagedResult<PitchingLeaderRow>> GetPitchingLeadersAsync(LeaderboardRequest request, CancellationToken cancellationToken);
}
```

- `PagedResult<T>` carries rows plus `Page`, `PageSize`, `TotalCount`, `TotalPages` (superset of what all three consumers return today).
- Row records carry the same fields the API DTOs expose today (rank, playerId, playerName, isHallOfFamer, counting stats, computed rates).
- **Stat catalog** moves from MCP's `LeaderboardStatCatalog` into the data project: canonical stat key, aliases, display name, sort direction, `IsRateStat` flag. One list feeds UI dropdowns, API validation, and MCP tool metadata.

### Stat formulas (defined once, here)

- `AVG = H / AB`
- `OBP = (H + BB + COALESCE(HBP,0)) / (AB + BB + COALESCE(HBP,0) + COALESCE(SF,0))` — **corrects #75**; current implementations wrongly use `(H+BB)/(AB+BB)`.
- `SLG = TB / AB`; `OPS = OBP + SLG` (inherits the OBP fix).
- `ERA = ER × 27 / IPouts`; `WHIP = (BB + H) × 3 / IPouts`; `K9 = SO × 27 / IPouts`; `BB9 = BB × 27 / IPouts`; `WPCT = W / (W + L)`.
- All computed in SQL via EF expression translation; division guarded against zero denominators.

## 4. Consumer rewiring

| Consumer | Change |
|---|---|
| `Pages/Stats/Batting.cshtml.cs`, `Pitching.cshtml.cs` | Page models build a `LeaderboardRequest` from query params and call the service. Dynamic expression helpers (`DynExpr`, `DynComputedExpr`, `DynSlgExpr`, …) deleted. Existing `minAb`/`minIp` query params keep working as overrides. |
| `Api/Endpoints/LeaderEndpoints.cs` | Endpoints call the service. Existing route shape and response contract preserved; a `qualified` query param is plumbed through (documentation and MCP tool description work stays in #65). |
| MCP `Querying/LeaderboardReadService.cs` | Deleted; `BaseballReferenceTools` leaderboard tools call the shared service. `minAtBats`/`minInningsPitched` tool params keep working as overrides. |

Behavior change shipped by #63: **rate-stat leaderboards are qualified by default** on all three paths. UI override control polish and explanatory note remain #64; API/MCP parameter documentation remains #65.

## 5. Error handling

- Unknown stat key → catalog lookup failure surfaces as today: web falls back to default stat, API returns 400 problem details, MCP throws `BaseballMcpUsageException`. The service itself throws `ArgumentException` with the invalid key; hosts translate.
- Negative/oversized paging values clamped or rejected at host edges exactly as today (MCP request policy, API validation) — the service additionally clamps defensively.
- Zero-denominator players (0 AB, 0 IPouts) are excluded from rate-stat boards regardless of qualification flag (they are today via `> 0` guards; preserved).

## 6. Testing

Unit (no DB):

- PA NULL-safety: missing HBP/SH/SF treated as 0.
- Stint summing: two stints in one season produce summed threshold.
- Outs conversion: 1 IP/game rule = `3 × G` outs.
- Override semantics: explicit `MinAtBats` disables season-relative rule.
- Counting stats ignore `Qualified`.

Integration (against the production-schema DB):

- Default career AVG page one contains Cobb and Hornsby; contains no player below their derived threshold.
- Gibson appears on the default career AVG board.
- `Qualified=false` restores the 1.000 crowd (override intact).
- OBP spot-check: Ted Williams career OBP ≈ .482 (formula fix verified).
- Default career ERA board contains no 0.00 sub-threshold pitchers.

Full regression-pin suite (Bonds 762 HR etc.) remains issue #66.

## 7. Risks

- **Model move breadth:** moving entities + context touches most files in web. Mechanical, compiler-verified, no behavior change. Do it as the first commit, isolated from logic changes.
- **EF/Npgsql translation:** the grouped join (`SUM` over joined `Teams.G` per player) must translate to SQL. Validated shape exists as raw SQL from the #62 audit. If EF fails to translate, fallback is a keyless entity mapped to a hand-written SQL view — decision deferred until proven necessary.
- **Contract drift:** API response fields preserved verbatim to avoid breaking existing clients; asserted by integration tests.

## Out of scope

- UI default dropdown, override control styling, explanatory note (#64).
- API docs and MCP tool description updates (#65).
- Full aggregation-total regression suite (#66).
- Park factors / era adjustment (explicitly a non-goal; see #70).
