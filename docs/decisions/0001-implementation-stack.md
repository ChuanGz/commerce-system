# ADR 0001: Implementation Stack

- Status: Accepted
- Date: 2026-07-05
- Review trigger: the reference slice cannot demonstrate an invariant or failure
  mode without infrastructure the current stack cannot provide

## Context

The first release must prove one durable checkout outcome inside one deployment.
It needs real persistence, transactions, concurrency checks, health reporting,
automated tests, and a one-command local entry point. It must not require a
broker or separately managed database before those dependencies solve an
observed constraint.

## Decision

Use .NET 8, ASP.NET Core Minimal APIs, Entity Framework Core, and SQLite:

- one `Commerce.Api` deployable application;
- module-owned code and tables inside one database;
- explicit application contracts between modules;
- optimistic versions plus database transactions for concurrency;
- OpenTelemetry-compatible instrumentation at the application boundary;
- xUnit integration tests against isolated SQLite databases.

The local entry point is `dotnet run --project src/Commerce.Api`. The API owns
database migration at startup for this reference system so a new checkout can
start without manual database administration.

## Alternatives considered

### PostgreSQL from the first commit

PostgreSQL offers stronger production parity and richer locking behavior, but a
required database server would add adoption friction before the domain contract
exists. Reconsider it when measured concurrency or SQL behavior invalidates the
SQLite evidence.

### In-memory persistence

Rejected because restart, migration, transaction, and reconciliation behavior
would be fictional.

### Microservices with a message broker

Rejected because the repository has no measured ownership, latency, burst, or
reliability pressure requiring distributed deployment. It would make failure
handling harder before establishing the modular baseline.

## Consequences

- The reference system is portable and inexpensive to run.
- SQLite serializes writes and is not evidence of high-throughput production
  capacity; performance claims must remain bounded to the recorded environment.
- Module ownership is enforced by code structure and tests, not by separate
  database credentials.
- Moving to another relational database requires new migration, concurrency,
  restore, and performance evidence.
