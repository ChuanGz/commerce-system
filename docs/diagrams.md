# Architecture Diagrams

These diagrams express responsibility and evolution. They do not prescribe a
programming language, broker, database, or deployment platform.

## Version 1: Modular Monolith

```mermaid
flowchart LR
    Customer["Customer or operator"] --> Edge["Web/API boundary"]
    Edge --> Catalog["Catalog"]
    Edge --> Cart["Cart"]
    Edge --> Ordering["Ordering"]
    Ordering --> Pricing["Pricing"]
    Ordering --> Inventory["Inventory"]
    Ordering --> Payments["Payments"]
    Ordering --> Fulfillment["Fulfillment"]
    Ordering --> Notifications["Notifications"]

    subgraph App["One deployable application"]
        Catalog
        Cart
        Ordering
        Pricing
        Inventory
        Payments
        Fulfillment
        Notifications
    end

    Catalog --> CatalogData[("Catalog-owned data")]
    Ordering --> OrderData[("Order-owned data")]
    Inventory --> InventoryData[("Inventory-owned data")]
    Payments --> PaymentData[("Payment coordination data")]
```

The single deployment simplifies transactions and operations. Module-owned data
and explicit contracts preserve a migration option without paying distributed
systems cost prematurely.

## Version 2: One event-driven pilot

```mermaid
sequenceDiagram
    actor Customer
    participant API as Commerce API
    participant Ordering
    participant DB as Order DB and Outbox
    participant Relay as Outbox Relay
    participant Bus as Event Bus
    participant Notify as Notification Consumer

    Customer->>API: Submit checkout with intent ID
    API->>Ordering: Create order
    Ordering->>DB: Commit order and OrderConfirmed fact
    Ordering-->>Customer: Authoritative order result
    Relay->>DB: Read unpublished facts
    Relay->>Bus: Publish OrderConfirmed
    Bus-->>Notify: Deliver one or more times
    Notify->>Notify: Deduplicate by event ID
    Notify-->>Bus: Acknowledge after durable outcome
```

## Evolution decision loop

```mermaid
flowchart TD
    Observe["Observe delivery or runtime pressure"] --> Measure["Record baseline and consequence"]
    Measure --> Options["Compare local improvement, queue, and event bus"]
    Options --> Pilot["Move one reversible flow"]
    Pilot --> Test["Test duplicates, delay, outage, replay, and recovery"]
    Test --> Review{"Expected pressure improved?"}
    Review -->|Yes, within cost| Expand["Consider the next bounded flow"]
    Review -->|No| Revert["Rollback or adapt"]
    Expand --> Observe
    Revert --> Observe
```
