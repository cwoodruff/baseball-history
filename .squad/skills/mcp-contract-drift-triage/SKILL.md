---
name: "mcp-contract-drift-triage"
description: "Triage MCP test drift by separating stale assertions from real surface regressions after a partial metadata/tool refactor."
domain: "mcp-review"
confidence: "high"
source: "earned"
---

## When to use

Use this when MCP integration tests start failing after metadata, limits, or tool-surface changes and it is unclear whether the problem is the test, the contract, or a bad merge.

## Checklist

1. **Build first.** If the MCP project does not compile, do not treat assertion failures as the top-level issue yet.
2. Find the last known green MCP commit and rerun the MCP-focused slice there.
3. Diff current MCP files against that green commit, especially:
   - options/config models
   - metadata documents
   - resource URIs
   - tool registrations
   - request-policy clamp logic
4. Classify each failing assertion explicitly:
   - **stale test expectation**: same contract, new value
   - **response-shape mismatch**: renamed/removed JSON field or resource shape
   - **real regression**: shipped tool/metadata behavior no longer coherent
5. Watch for partial removals: if a tool disappears from `Tools/*` but its limits or metadata remain elsewhere, treat that as hidden contract drift.
6. Gate approval on end-to-end coherence, not on a single passing test file.

## Proven pattern in this repo

- MCP milestone commit `9cb62c8` was a reliable green baseline.
- A later broken tree mixed old and new MCP contracts in `BaseballMcpOptions`, metadata models/services, and Hall of Fame reads.
- The right review move was to prove the old slice still passed, then classify the new failures as stale expectations vs surface-shape drift instead of assuming a pure product regression.

## Validation

- `dotnet build baseball-history.sln --nologo`
- `dotnet test baseball-history-tests --no-build --nologo --filter Mcp --logger "console;verbosity=minimal"`
- Re-run the same MCP slice on the last known green MCP commit for comparison when drift classification is unclear.
