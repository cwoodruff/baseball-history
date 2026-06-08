# SQLite to Postgres Insert Export

## Context

Use this when a SQLite dataset must be handed off as replayable Postgres seed scripts without changing the source schema files already tracked in the repo.

## Pattern

1. Read the active SQLite database directly, not older checked-in SQL dumps.
2. Create a dedicated output directory for generated Postgres artifacts so legacy SQL files remain untouched.
3. Emit one `INSERT` per row and one `.sql` file per table for easier replay, diffing, and partial reruns.
4. Double-quote all table and column identifiers to survive mixed case and awkward names like `"2B"`, `"3B"`, and `"rank"`.
5. Preserve text as text, escape single quotes safely, emit `NULL` for actual SQLite nulls, and normalize empty strings to `NULL` for numeric/date-like fields.
6. For text-declared columns, detect numeric-text/date-text patterns before export so blank pseudo-numeric values become realistic Postgres nulls.
7. Validate coverage by checking file existence, non-zero size, and generated row counts against source table counts.

## Notes

- This pattern is especially useful for Lahman-style historical datasets where SQLite stores many missing numeric facts as empty strings.
- Prefer generating into a clearly named folder such as `database/postgres-inserts/` to keep migration artifacts isolated.
