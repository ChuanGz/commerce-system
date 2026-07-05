# Checkout Recovery Runbook

## Trigger and authority

Use this runbook when checkout returns elevated conflicts or errors, health is
unhealthy, an order remains `PaymentUncertain`, or the database must be restored.
The operator must have access to the runtime, database copy, application logs,
and API key. Stop mutations before replacing the database.

## Diagnose

1. Call `GET /health`; record status, time, revision, and correlation ID.
2. Use `GET /api/orders/{orderId}/timeline` with `X-API-Key` to distinguish an
   accepted order, payment uncertainty, and a missing intent.
3. Correlate logs by `X-Correlation-ID`. Never resubmit with a new idempotency
   key merely because the client timed out.
4. If the database is reachable, create and verify a backup:

   ```bash
   ./ops/backup.sh src/Commerce.Api/commerce.db backups/pre-recovery.db
   ```

## Contain and reconcile

- For `PaymentUncertain`, preserve the order and reservation. Compare the
  simulated provider outcome before changing state; this slice intentionally
  has no automated settlement authority.
- For stale version or insufficient stock, return the current product state to
  the caller. Do not override inventory.
- For repeated 5xx responses, stop mutation traffic while keeping evidence and
  the health endpoint available.

## Restore

1. Stop the application so no writer is active.
2. Verify and restore a known backup to a new target first:

   ```bash
   ./ops/restore.sh backups/known-good.db commerce-restored.db
   ```

3. Start the application against the restored target, call `/health`, query a
   known product and order timeline, and compare order and stock counts.
4. Switch the configured database only after behavior and data checks pass.

## Release rollback

Redeploy the previous immutable revision while retaining the current database
when migrations are backward compatible. A code rollback must pass `/health`
and the checkout integration suite. If a migration is incompatible, stop and
restore the verified pre-release backup; do not improvise a down migration.

## Escalate or stop

Stop when integrity checks fail, the accepted payment outcome is unknown,
inventory and order records disagree, rollback cannot read the current schema,
or correlation evidence is missing. Escalation must name the affected intent,
last known durable state, backup identity, revision, and decision owner.
