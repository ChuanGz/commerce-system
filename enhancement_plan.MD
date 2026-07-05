# Enhancement Plan

## Purpose

Turn the documented architecture direction into one runnable, failure-tested
commerce outcome before considering event-driven expansion.

## Current evidence and gap

The repository has a restrained evolution roadmap and sound ownership rules, but
no stack decision, executable application, domain state model, or operational
evidence. The immediate goal is a thin modular-monolith checkout slice.

## Enhancement sequence

### E1 — Complete the decision baseline

- Record the implementation stack and rejected alternatives in an ADR.
- Define the order state machine and ownership of price, stock, payment,
  cancellation, refund, and fulfillment decisions.
- Specify intent identity, concurrency policy, trust boundaries, service
  objectives, and local developer entry point.

**Exit evidence:** domain states, invariants, module contracts, risks, and
acceptance tests can be reviewed without implementation code.

### E2 — Build the walking skeleton

- Create one deployable modular-monolith application and repeatable migration.
- Implement product query, stock reservation, idempotent checkout intent,
  durable order creation, simulated payment result, and operator timeline.
- Add health, structured errors, correlation, telemetry, and CI.

**Exit evidence:** one documented command starts the system; a test-environment
deployment is traceable end to end; rollback is exercised.

### E3 — Prove correctness under failure

- Test duplicate submission, concurrent reservation, stale version, payment
  timeout, restart during checkout, and partial-failure reconciliation.
- Enforce module ownership and prohibit cross-module table writes.
- Add API, contract, persistence, migration, authorization, and security tests.

**Exit evidence:** all negative paths converge to an explainable durable state
with no oversell, duplicate order, or hidden payment uncertainty.

### E4 — Establish the operational baseline

- Add dashboards, actionable alerts, support queries, and runbooks.
- Exercise restore, rollback, and dependency degradation.
- Record latency, throughput, errors, saturation, recovery, and cost per order.

**Exit evidence:** an operator can detect, diagnose, contain, reconcile, and
explain a failed order from correlated evidence.

### E5 — Review architecture pressure

- Measure whether module coupling, latency, bursts, ownership, or reliability
  justify asynchronous integration.
- Compare monolith improvement, an internal queue, and one bounded event pilot.

**Exit evidence:** any event-bus work has a measured constraint, alternatives,
coexistence plan, rollback, and stop condition. Otherwise it remains deferred.

## Measures

| Measure | Target |
|---|---|
| Critical checkout failure scenarios passing | 6 of 6 |
| Cross-module table writes | 0 |
| Customer intent to durable outcome trace coverage | 100% |
| Restore and rollback exercises | 1 each |

## Non-goals

- Treating microservices or an event bus as the destination.
- Expanding catalog or fulfillment breadth before checkout correctness.
- Using architecture diagrams as runtime evidence.

## Review trigger

Review after each milestone exit, invariant failure, restore exercise, or
measured architecture-pressure change.
