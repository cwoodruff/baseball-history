---
name: "sqlite-postgres-schema-export"
description: "Generate PostgreSQL CREATE TABLE scripts directly from live SQLite metadata without hand-transcribing Lahman-style schemas."
domain: "database-migration"
confidence: "high"
source: "earned"
tools:
  - name: "sqlite3"
    description: "Reads live table, index, and foreign-key metadata from the source SQLite database."
    when: "Use when the checked-in SQL files may lag behind the actual database file."
---

## Context
Use this when a SQLite database is the source of truth, but the deliverable needs PostgreSQL-compatible DDL per table. This is especially useful for Lahman-style schemas with mixed-case identifiers, numeric-looking column names like `2B`, and composite keys that SQLite reports loosely.

## Patterns
1. Read table names from `sqlite_master`, then use `PRAGMA table_info`, `PRAGMA index_list` + `PRAGMA index_info`, and `PRAGMA foreign_key_list` to build the schema from live metadata.
2. Map types conservatively: `nvarchar(n)` → `varchar(n)`, `INT` → `integer`, `tinyint` → `smallint`, `float` → `double precision`.
3. Double-quote every table, column, and constraint identifier so PostgreSQL accepts mixed case, reserved words, and names like `"2B"`.
4. Drop SQLite-only clauses such as `COLLATE NOCASE`, but preserve primary keys, unique constraints, and foreign keys.
5. Force all primary-key columns to `NOT NULL` in the PostgreSQL output, even when SQLite metadata does not mark composite-key members as non-null.
6. Emit an ordered combined script from a foreign-key topological sort when replay order matters.
7. Validate coverage by comparing live SQLite table names to generated filenames and checking that every output file is non-empty.

## Examples
- Source database: `lahman.db`
- Per-table output: `database/postgres-schema/{TableName}.sql`
- Reusable generator: `scripts/generate_postgres_schema.py`
- Ordered replay artifact: `database/postgres-schema/all_tables.sql`

## Anti-Patterns
- Hand-copying schema from an older migration script instead of reading the live database.
- Leaving identifiers unquoted and then breaking on `rank`, `2B`, `3B`, or mixed-case names.
- Inventing foreign keys that are absent from the source metadata just because another migration format suggested them.
- Treating SQLite composite primary-key columns as nullable in PostgreSQL output.
