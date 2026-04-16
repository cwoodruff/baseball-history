# htmxRazor Baseline Integration

## When to use

Use this when a Razor Pages app needs the smallest safe htmxRazor foundation before broader UI migration.

## File map

1. `baseball-history-web/baseball-history-web.csproj` — package reference
2. `baseball-history-web/Program.cs` — `AddhtmxRazor()` and `UsehtmxRazor()`
3. `baseball-history-web/Pages/_ViewImports.cshtml` — Tag Helper registration
4. `baseball-history-web/Pages/Shared/_Layout.cshtml` — shared asset strategy
5. `baseball-history-web/Pages/About.cshtml` — safest proof page for a first `rhx-*` component

## Heuristic

Prove the integration on a low-risk support page first. Do **not** use the shared shell, filters, or modal flows as the first proof because those areas already carry global htmx and Bootstrap coupling.

## Watchouts

- `Pages/Shared/_Layout.cshtml` is a singleton shell: it owns `hx-boost`, nav dropdown behavior, search dropdown hosting, and modal cleanup.
- Preserve existing `/Search` and `#modal-container` flows while Bootstrap and htmxRazor coexist.
- Keep component CSS/JS imports centralized in layout until page groups settle on stable primitives.
