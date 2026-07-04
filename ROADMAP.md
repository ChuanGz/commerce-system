# Roadmap

## Version 1: Modular Monolith

- Define domain language, module contracts, and ownership.
- Select an implementation stack through an explicit decision record.
- Implement the order lifecycle and its critical invariants.
- Add contract, integration, security, and failure-path tests.
- Establish telemetry and operational runbooks for the single deployment.

## Version 2: Event Bus

- Identify measured constraints that justify asynchronous integration.
- Define event schemas, ownership, compatibility, and delivery semantics.
- Implement idempotency, retries, dead-letter handling, and replay controls.
- Validate eventual-consistency behavior and end-to-end traceability.
- Compare operational cost and reliability against the Version 1 baseline.

No roadmap item is a release commitment. A phase is complete only when its acceptance evidence is documented and reviewable.
