#!/usr/bin/env python3
"""Generate PostgreSQL CREATE TABLE scripts from the live Lahman SQLite database."""

from __future__ import annotations

import sqlite3
from collections import defaultdict
from graphlib import TopologicalSorter
from pathlib import Path
from typing import Iterable


REPO_ROOT = Path(__file__).resolve().parents[1]
SQLITE_DB = REPO_ROOT / "lahman.db"
OUTPUT_DIR = REPO_ROOT / "database" / "postgres-schema"
COMBINED_FILE = OUTPUT_DIR / "all_tables.sql"


def quote_ident(identifier: str) -> str:
    return '"' + identifier.replace('"', '""') + '"'


def map_type(sqlite_type: str) -> str:
    normalized = sqlite_type.strip().lower()
    if normalized.startswith("nvarchar("):
        return "varchar" + normalized[len("nvarchar") :]
    if normalized.startswith("varchar("):
        return normalized
    if normalized == "int":
        return "integer"
    if normalized == "smallint":
        return "smallint"
    if normalized == "tinyint":
        return "smallint"
    if normalized == "bigint":
        return "bigint"
    if normalized == "float":
        return "double precision"
    if not normalized:
        return "text"
    return normalized


def make_constraint_name(table: str, suffix: str) -> str:
    return f"{table}_{suffix}"


def ordered_tables(conn: sqlite3.Connection, tables: list[str], fks_by_table: dict[str, list[dict]]) -> list[str]:
    sorter = TopologicalSorter()
    for table in tables:
        deps = {fk["table"] for fk in fks_by_table[table] if fk["table"] != table}
        sorter.add(table, *deps)
    return list(sorter.static_order())


def grouped_foreign_keys(conn: sqlite3.Connection, table: str) -> list[dict]:
    rows = conn.execute(f'PRAGMA foreign_key_list({quote_ident(table)})').fetchall()
    grouped: dict[int, dict] = {}
    for row in rows:
        bucket = grouped.setdefault(
            row["id"],
            {
                "table": row["table"],
                "from_cols": [],
                "to_cols": [],
                "on_update": row["on_update"],
                "on_delete": row["on_delete"],
                "match": row["match"],
            },
        )
        bucket["from_cols"].append(row["from"])
        bucket["to_cols"].append(row["to"])
    return [grouped[key] for key in sorted(grouped)]


def unique_constraints(conn: sqlite3.Connection, table: str) -> list[list[str]]:
    uniques: list[list[str]] = []
    for row in conn.execute(f'PRAGMA index_list({quote_ident(table)})'):
        if row["unique"] != 1 or row["origin"] != "u":
            continue
        cols = [part["name"] for part in conn.execute(f'PRAGMA index_info({quote_ident(row["name"])})')]
        if cols:
            uniques.append(cols)
    return uniques


def column_definition(column: sqlite3.Row, pk_columns: set[str]) -> str:
    pieces = [f'{quote_ident(column["name"])} {map_type(column["type"])}']
    if column["notnull"] or column["name"] in pk_columns:
        pieces.append("NOT NULL")
    if column["dflt_value"] is not None:
        pieces.append(f'DEFAULT {column["dflt_value"]}')
    return " ".join(pieces)


def render_table(
    conn: sqlite3.Connection,
    table: str,
    columns: list[sqlite3.Row],
    pk_columns: list[str],
    uniques: list[list[str]],
    fks: list[dict],
) -> str:
    lines: list[str] = [
        f"-- Generated from {SQLITE_DB.name} ({table})",
        f"CREATE TABLE {quote_ident(table)} (",
    ]

    pk_set = set(pk_columns)
    entries = [f"    {column_definition(column, pk_set)}" for column in columns]

    if pk_columns:
        pk_name = make_constraint_name(table, "pkey")
        pk_cols = ", ".join(quote_ident(column) for column in pk_columns)
        entries.append(f"    CONSTRAINT {quote_ident(pk_name)} PRIMARY KEY ({pk_cols})")

    for unique_cols in uniques:
        uq_name = make_constraint_name(table, f'{"_".join(unique_cols)}_key')
        uq_cols = ", ".join(quote_ident(column) for column in unique_cols)
        entries.append(f"    CONSTRAINT {quote_ident(uq_name)} UNIQUE ({uq_cols})")

    for fk in fks:
        fk_suffix = f'{"_".join(fk["from_cols"])}_fkey'
        fk_name = make_constraint_name(table, fk_suffix)
        from_cols = ", ".join(quote_ident(column) for column in fk["from_cols"])
        to_cols = ", ".join(quote_ident(column) for column in fk["to_cols"])
        entries.append(
            "    CONSTRAINT "
            f'{quote_ident(fk_name)} FOREIGN KEY ({from_cols}) '
            f'REFERENCES {quote_ident(fk["table"])} ({to_cols})'
        )

    lines.append(",\n".join(entries))
    lines.append(");")
    return "\n".join(lines) + "\n"


def main() -> None:
    OUTPUT_DIR.mkdir(parents=True, exist_ok=True)

    conn = sqlite3.connect(SQLITE_DB)
    conn.row_factory = sqlite3.Row

    tables = [
        row["name"]
        for row in conn.execute(
            "SELECT name FROM sqlite_master WHERE type='table' AND name NOT LIKE 'sqlite_%' ORDER BY name"
        )
    ]

    fks_by_table = {table: grouped_foreign_keys(conn, table) for table in tables}
    ordered = ordered_tables(conn, tables, fks_by_table)

    combined_chunks: list[str] = [
        f"-- Generated from {SQLITE_DB.name}",
        "-- Replay tables in dependency-safe order.",
        "",
    ]

    for table in ordered:
        columns = list(conn.execute(f'PRAGMA table_info({quote_ident(table)})'))
        pk_columns = [row["name"] for row in sorted(columns, key=lambda row: row["pk"]) if row["pk"]]
        sql = render_table(conn, table, columns, pk_columns, unique_constraints(conn, table), fks_by_table[table])
        (OUTPUT_DIR / f"{table}.sql").write_text(sql, encoding="utf-8")
        combined_chunks.append(sql.rstrip())
        combined_chunks.append("")

    COMBINED_FILE.write_text("\n".join(combined_chunks).rstrip() + "\n", encoding="utf-8")

    print(f"Generated {len(ordered)} table scripts in {OUTPUT_DIR.relative_to(REPO_ROOT)}")
    print(f"Combined script: {COMBINED_FILE.relative_to(REPO_ROOT)}")


if __name__ == "__main__":
    main()
