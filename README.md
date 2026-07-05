# Commerce System

A reference e-commerce system for studying architecture evolution from a modular monolith to event-driven integration.

## Status

Milestone 0 is complete: the implementation stack, domain ownership, invariants,
trust boundary, service objectives, and acceptance scenarios are recorded. The
runnable walking skeleton is the next evidence gate.

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
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)

## Setup and usage

Runtime setup will become available with the walking skeleton. Until then,
review the domain baseline and stack decision before the broader roadmap.

## License

Licensed under the [MIT License](LICENSE).
