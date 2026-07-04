# Roadmap

This roadmap evolves one observable commerce flow before expanding architecture.
Dates are intentionally absent until an implementation stack and delivery capacity
are selected. Progress is evidence-gated, not calendar-gated.

## Outcome

Build a reference system that demonstrates how to preserve business ownership,
consistency, traceability, and recovery while moving from a modular monolith to
event-driven integration.

## Milestone 0: Decision baseline

**Decision:** establish the smallest implementable system shape.

- Define the customer journey from product discovery to fulfilled order.
- Record domain language, module owners, trust boundaries, and critical data.
- Select the implementation stack through an architecture decision record.
- Define service objectives, test strategy, and local developer entry point.

**Exit evidence:** the order lifecycle, module contracts, stack decision, risks,
and acceptance checks can be reviewed without reading implementation code.

## Milestone 1: Walking skeleton

**Decision:** prove the delivery path before adding commerce breadth.

- Create one deployable application and one repeatable data migration path.
- Implement health checks, structured errors, correlation IDs, and telemetry.
- Deliver a thin catalog-to-order flow through real module boundaries.
- Establish CI for build, tests, security checks, and documentation.

**Exit evidence:** a change can move from commit to a test environment, the
critical request can be traced end to end, and rollback has been exercised.

## Milestone 2: Modular Monolith core

**Decision:** make the order lifecycle correct inside one deployment.

- Implement Catalog, Pricing, Inventory, Cart, Ordering, and Payments.
- Keep business rules and data mutations inside their owning modules.
- Use explicit in-process contracts; prohibit cross-module table writes.
- Protect mutation retries with stable intent identity and idempotency.
- Cover validation, authorization, concurrency, failure, and recovery paths.

**Exit evidence:** checkout preserves price, stock, payment, and order invariants
under duplicate requests and concurrent updates; module contract tests pass.

## Milestone 3: Operational commerce loop

**Decision:** complete the customer-visible outcome and operate it safely.

- Add Fulfillment and Notifications without weakening order ownership.
- Define reconciliation for payment, inventory, and fulfillment disagreement.
- Add dashboards, actionable alerts, support queries, and runbooks.
- Run a restore exercise and a controlled dependency-failure exercise.
- Baseline latency, throughput, error rate, saturation, and cost per order.

**Exit evidence:** an operator can detect, diagnose, contain, reconcile, and
explain a failed order using documented procedures and correlated evidence.

## Milestone 4: Architecture pressure review

**Decision:** improve the monolith or introduce asynchronous integration.

Evidence must show at least one consequential pressure: recurring cross-module
change, unacceptable synchronous latency, burst absorption need, independent
ownership, or a reliability boundary the monolith cannot satisfy economically.

Compare three options:

1. improve the current module and query path;
2. add an internal queue while retaining one deployment;
3. introduce an event bus for one bounded flow.

**Exit evidence:** an accepted decision records the measured constraint,
alternatives, expected improvement, coexistence, rollback, and stop condition.

## Milestone 5: Event Bus pilot

**Decision:** migrate one non-authoritative side effect first.

- Publish a versioned fact from the transaction owner through an outbox.
- Process it with an idempotent inbox or equivalent deduplication record.
- Implement bounded retry, dead-letter handling, replay, and reconciliation.
- Preserve correlation from customer intent through publication and consumption.
- Run duplicate, delayed, reordered, unavailable-broker, and poison-message tests.

Notification after an order state change is the preferred pilot because the
Ordering module remains authoritative and delayed delivery does not redefine the
order transaction.

**Exit evidence:** the pilot improves the pressure named in Milestone 4 without
losing facts, hiding failures, or exceeding the accepted operational budget.

## Milestone 6: Selective event-driven expansion

**Decision:** expand only where the pilot evidence transfers.

- Evaluate inventory reservation, payment outcomes, and fulfillment handoff
  separately; do not migrate them as one program.
- Maintain a schema compatibility policy and consumer inventory.
- Build read models only when query needs justify replicated state.
- Compare reliability, recovery time, delivery coupling, and cost against the
  Modular Monolith baseline after every migrated flow.

**Exit evidence:** every asynchronous flow has one fact owner, named consumers,
observable lag, tested recovery, compatibility rules, and a manual repair path.

## Explicit non-goals

- Microservices are not the automatic destination.
- Event count, service count, and request volume are not success measures.
- Exactly-once delivery is not assumed.
- A target architecture diagram is not accepted as a migration plan.

See the [delivery plan](PLAN.md) for workstreams and
[architecture diagrams](docs/diagrams.md) for system views.
