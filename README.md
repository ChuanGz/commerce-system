# Commerce System

A reference e-commerce system for studying architecture evolution from a modular monolith to event-driven integration.

## Status

Milestone 0 is complete. The walking skeleton now provides a migrated SQLite
database, database-backed health check, correlation IDs, OpenAPI, and a versioned
product query. Checkout mutation and failure evidence remain the next gate.

## Architecture direction

- **Version 1 — Modular Monolith:** establish explicit commerce modules, in-process contracts, one deployable unit, and transactional boundaries.
- **Version 2 — Event Bus:** introduce asynchronous integration only where measured coupling, throughput, or reliability needs justify it.

The event bus is an evolution step, not a default requirement. Version 1 must expose module ownership and observable behavior before distributed messaging is introduced.

## Scope

The reference system covers the core commerce flow: catalog, pricing, inventory, cart, ordering, payment coordination, fulfillment, and customer notifications. It documents module responsibilities and cross-module contracts without selecting a programming language, framework, database, broker, or cloud provider before those decisions are evidenced.

## Start here

- [Architecture](docs/architecture.md)
- [Delivery roadmap](ROADMAP.md)
- [Delivery plan](PLAN.md)
- [Architecture diagrams](docs/diagrams.md)
- [Domain baseline](docs/domain-baseline.md)
- [Implementation stack decision](docs/decisions/0001-implementation-stack.md)
- [Checkout recovery runbook](docs/runbooks/checkout-recovery.md)
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)

## Run locally

Requires the .NET 8 SDK.

```bash
dotnet tool restore
Commerce__ApiKey='replace-with-a-local-secret' \
  dotnet run --project src/Commerce.Api
```

The application applies its migration, seeds one reference product, and exposes:

- `GET /health`
- `GET /api/products/9d67986b-9500-4527-858a-3118d1ac6a90`
- `POST /api/checkouts` with `Idempotency-Key` and `X-API-Key` headers
- `GET /api/orders/{orderId}/timeline` with an `X-API-Key` header
- `/swagger`

Run the evidence suite with:

```bash
dotnet test Commerce.sln
```

The local `commerce.db` file is disposable reference data and is excluded from
version control.

## License

Licensed under the [MIT License](LICENSE).
