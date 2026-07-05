# Domain Baseline

## Critical outcome

A customer selects a product at a known version and price, reserves available
stock, submits one checkout intent, and receives one durable order whose payment
state can be inspected and reconciled by an operator.

## State and ownership

| Decision | Owner | Initial rule |
|---|---|---|
| Product description and active state | Catalog | Only active products can be quoted |
| Current unit price and currency | Pricing | Checkout records the quoted price; later changes do not rewrite an order |
| Available and reserved quantity | Inventory | A reservation cannot make available stock negative |
| Checkout intent and order lifecycle | Ordering | One customer and idempotency key identify one immutable intent |
| Payment outcome | Payments | Payment data is simulated; no card or credential enters this system |

Order states are `PendingPayment`, `Confirmed`, `PaymentUncertain`, and
`Cancelled`. The first slice creates `PendingPayment`, then records a simulated
`Confirmed` or `PaymentUncertain` outcome. Uncertainty is retained for operator
reconciliation; it is never silently treated as failure or success.

## Invariants

1. One checkout intent creates at most one order.
2. Reusing an idempotency key with different input is rejected.
3. Product, price, and inventory versions must match the client's expectation.
4. Reserved stock never exceeds stock available at the accepted version.
5. An order stores its accepted unit price and currency permanently.
6. Every state change appends a correlated timeline entry.
7. No module writes another module's tables except through its declared
   application contract.

## Trust and authorization boundary

The first slice is a local reference API, not an identity provider. Mutation
endpoints require an operator-configured API key; health and product reads are
anonymous. The key is supplied through configuration and is never stored in the
repository or logs. Real customer identity, payment credentials, refund, and
fulfillment authorization remain outside this slice.

## Service objectives for the reference slice

- Successful checkout requests preserve all invariants: 100% in the automated
  acceptance suite.
- Duplicate identical intent returns the original durable outcome: 100%.
- Every API response contains a correlation ID: 100% in contract tests.
- Health reports database reachability, not only process liveness.

These are correctness gates, not production availability claims.

## Acceptance scenarios

- product and current price can be queried;
- valid stock can be reserved and converted into one durable order;
- identical duplicate submission returns the same order;
- conflicting duplicate, stale version, and insufficient stock are rejected;
- payment timeout leaves an explainable `PaymentUncertain` state;
- restart preserves the order and timeline;
- rollback and restore procedures can be executed against the reference data.
