# C# Merge Repair for Duplicated Services

## Context
Use when a .NET build suddenly fails with clustered syntax errors across a few files after parallel feature work or branch merges.

## Pattern
1. **Look for duplicate fragments first.** Constructor parameter lists, whole method bodies, and adjacent `return` blocks are common merge-damage hotspots.
2. **Restore one authoritative implementation per contract.** If two versions of the same service/type are interleaved, pick the active contract used by the shipped tool/API surface and delete the dead duplicate path.
3. **Resolve catalog/type duplication at the namespace level.** Ambiguous references usually mean two versions of the same helper now exist; keep one and repoint callers.
4. **Rebuild incrementally.** Fix the first syntax cluster, rebuild the target project, then the full solution, because later semantic errors are often hidden until syntax is clean.
5. **Only then touch tests.** Constructor/helper test fixes should be limited to signature drift required by the repaired production contracts.

## Baseball History Example
- `HallOfFameReadService`, `FranchiseReadService`, `PlayerReadService`, and `BaseballMcpMetadataService` had interleaved old/new code blocks.
- `TeamSeasonReadModel` existed in two files; the shipped MCP surface already used `ITeamReadService`, so the unused duplicate `ITeamSeasonReadService` path was removed.
- Rebuild sequence: MCP project first, then full solution.
