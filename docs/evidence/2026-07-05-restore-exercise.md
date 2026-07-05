# Restore Exercise: Local SQLite Reference

- Date: 2026-07-05
- Decision: Continue; local restore path is repeatable
- Scope: reference database created by the current migrations
- Limitation: local single-process exercise, not a live traffic or remote storage
  recovery claim

## Procedure and result

1. Started the API and confirmed database-backed `/health` returned `Healthy`.
2. Backed up `src/Commerce.Api/commerce.db` with SQLite's online backup command.
3. Verified the backup with `PRAGMA integrity_check` and required at least one
   schema table.
4. Restored into a new target rather than overwriting the running database.
5. Verified foreign keys, integrity, and matching seeded product counts.

All checks passed. An initial exercise against the wrong relative database path
created an empty SQLite file; the scripts now reject databases without tables,
and the runbook names the application content-root path explicitly. This failure
is retained because it changed the recovery control.

## Remaining boundary

Release rollback against a deployed immutable artifact and recovery with
accepted orders still require a test environment. The current evidence proves
the local data mechanism and its empty-database guard, not an RTO or RPO.
