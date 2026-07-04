# Architecture

## Goals

The system is intended to make commerce boundaries, consistency choices, and failure handling reviewable. Each implementation decision must preserve a traceable customer order lifecycle and state its operational cost.

## Version 1: Modular Monolith

Version 1 uses one deployable application with modules that own their behavior and data access.

| Module | Responsibility |
|---|---|
| Catalog | Product information and availability for discovery |
| Pricing | Price calculation and promotion rules |
| Inventory | Stock state and reservation decisions |
| Cart | Customer purchase intent before checkout |
| Ordering | Order lifecycle and customer-visible status |
| Payments | Payment-provider coordination and result recording |
| Fulfillment | Shipment preparation and delivery handoff |
| Notifications | Customer communication triggered by recorded outcomes |

Cross-module calls must use explicit contracts. A module must not modify another module's state directly. Transactions remain local to the boundary that owns the invariant.

## Version 2: Event Bus

Version 2 may add asynchronous events after Version 1 has measurable coupling or latency constraints. Adoption requires:

- named event owners and consumers;
- versioned schemas and compatibility rules;
- idempotent consumers and duplicate-delivery tests;
- retry, dead-letter, replay, and poison-message procedures;
- correlation from customer request to resulting events;
- explicit consistency expectations for every affected workflow.

Commands express an intended action and may be rejected. Events record facts that already occurred. Neither is a substitute for an owned business invariant.

## Required evidence

Architecture changes must include affected workflows, failure modes, consistency boundaries, test evidence, operational ownership, and a rollback or migration path.
