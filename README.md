# Commerce System

A reference e-commerce system for studying architecture evolution from a modular monolith to event-driven integration.

## Status

The repository is in its initial design stage. It currently defines scope, architecture boundaries, and delivery gates; it does not yet contain a runnable application.

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
- [Contributing](CONTRIBUTING.md)
- [Security policy](SECURITY.md)

## Setup and usage

No runtime setup is available yet. Read the architecture and roadmap documents to review the intended system boundaries and implementation sequence.

## License

Licensed under the [MIT License](LICENSE).
