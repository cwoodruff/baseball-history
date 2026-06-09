---
name: "explicit-usd-display-format"
description: "Render salary values with a stable dollar sign in Razor views without depending on host culture."
domain: "frontend"
confidence: "high"
source: "earned"
---

## Context
Use this when baseball salary amounts must always display as USD in Razor pages, partials, or view-model-backed UI.

## Patterns
- Prefer a shared helper near the salary view model so payroll cards and row values format the same way.
- Use explicit output like `$` plus `N0` grouping for whole-dollar salary displays.
- Bind Razor markup to formatted properties instead of repeating formatting logic inline.
- Cover both full-page and non-boosted htmx responses when the same partial renders in both paths.

## Examples
- `baseball-history-web/ViewModels/SalaryViewModel.cs`
- `baseball-history-web/Pages/Salaries/_SalaryList.cshtml`
- `baseball-history-tests/Pages/PageRoutingIntegrationTests.cs`

## Anti-Patterns
- Do not rely on `"C0"` when the UI contract requires a literal dollar sign.
- Do not duplicate salary formatting strings across multiple Razor fragments.
