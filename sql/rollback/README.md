# Database Rollback Scripts

This directory contains ordered rollback scripts designed to safely revert database migrations and schema objects.

## Safety Rules

1. **Never run rollback scripts in production without a verified backup.**
2. Rollback scripts drop schema objects and will result in data loss if executed against active production environments.
3. Rollbacks must be executed in reverse numerical order to satisfy foreign key dependencies.

## Script Mapping

| Rollback Script | Target Script Being Reverted |
|---|---|
| `012_rollback_indexes_and_constraints.sql` | `../012_create_indexes_and_constraints.sql` |
| `011_rollback_audit_and_reporting.sql` | `../011_create_audit_and_reporting.sql` |
| `010_rollback_notifications_and_support.sql` | `../010_create_notifications_and_support.sql` |
| `009_rollback_delivery_and_promotions.sql` | `../009_create_delivery_and_promotions.sql` |
| `008_rollback_orders_and_payments.sql` | `../008_create_orders_and_payments.sql` |
| `007_rollback_prescriptions.sql` | `../007_create_prescriptions.sql` |
| `006_rollback_carts_and_wishlists.sql` | `../006_create_carts_and_wishlists.sql` |
| `005_rollback_inventory.sql` | `../005_create_inventory.sql` |
| `004_rollback_catalog.sql` | `../004_create_catalog.sql` |
| `003_rollback_users_and_roles.sql` | `../003_create_users_and_roles.sql` |
| `002_rollback_extensions.sql` | `../002_create_extensions.sql` |
| `rollback_all.sql` | Complete schema teardown in single transactional run |

## Example Usage

To rollback a specific migration:
```bash
psql -U niramoy_user -d niramoy_health_dev -f sql/rollback/011_rollback_audit_and_reporting.sql
```

To tear down all test schema:
```bash
psql -U niramoy_user -d niramoy_health_test -f sql/rollback/rollback_all.sql
```
