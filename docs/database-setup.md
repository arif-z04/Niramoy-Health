# Niramoy Health (নিরাময় হেলথ) - Database Setup Guide

## 1. Prerequisites

Before setting up the Niramoy Health database, verify that your development environment meets the following requirements:

- **PostgreSQL**: Version 14 or higher (PostgreSQL 18.x supported and tested).
- **.NET SDK**: .NET 10 SDK or higher (`dotnet --version`).
- **Python**: Python 3.8+ (used for the automated test suite).
- **CLI Tools**: `psql`, `pg_isready`.

Verify installed tools:
```bash
psql --version
pg_isready
dotnet --version
python3 --version
```

---

## 2. Environment Configuration

### A. Secret Management Rules

1. **Never commit `.env` or sensitive credentials to Git.**
2. A template file `.env.example` is committed with placeholder values.
3. Copy `.env.example` to create your local `.env`:

```bash
cp .env.example .env
chmod 600 .env
```

### B. Configuration Variables

| Key | Description | Development Default |
|---|---|---|
| `DB_HOST` | PostgreSQL host | `127.0.0.1` |
| `DB_PORT` | PostgreSQL port | `5432` |
| `DB_NAME` | Development Database | `niramoy_health_dev` |
| `DB_USER` | Application Database User | `niramoy_user` |
| `DB_PASSWORD` | Database User Password | `niramoy_dev_pass_2026` |
| `TEST_DB_NAME`| Testing Database | `niramoy_health_test` |
| `ConnectionStrings__DefaultConnection` | .NET Dev Connection String | Npgsql connection string |
| `ConnectionStrings__TestConnection` | .NET Test Connection String | Npgsql connection string |

---

## 3. Database Initialization Procedure

### Step 1: Create PostgreSQL Role & Databases

Connect as superuser (`postgres`) to initialize the application role and create the databases:

```bash
# 1. Create dedicated user if not already present
psql -U postgres -h 127.0.0.1 -c "DO \$\$
BEGIN
  IF NOT EXISTS (SELECT FROM pg_catalog.pg_roles WHERE rolname = 'niramoy_user') THEN
    CREATE ROLE niramoy_user WITH LOGIN PASSWORD 'niramoy_dev_pass_2026' CREATEDB;
  END IF;
END
\$\$;"

# 2. Run database creation script
psql -U postgres -h 127.0.0.1 -f sql/001_create_database.sql
```

This creates:
- `niramoy_health_dev` (Development Database with UTF-8 encoding)
- `niramoy_health_test` (Isolated Test Database with UTF-8 encoding)

---

### Step 2: Apply Schema Scripts Sequentially

Apply the modular schema scripts to the development database in dependency order:

```bash
export PGPASSWORD='niramoy_dev_pass_2026'

for script in \
  sql/002_create_extensions.sql \
  sql/003_create_users_and_roles.sql \
  sql/004_create_catalog.sql \
  sql/005_create_inventory.sql \
  sql/006_create_carts_and_wishlists.sql \
  sql/007_create_prescriptions.sql \
  sql/008_create_orders_and_payments.sql \
  sql/009_create_delivery_and_promotions.sql \
  sql/010_create_notifications_and_support.sql \
  sql/011_create_audit_and_reporting.sql \
  sql/012_create_indexes_and_constraints.sql
do
  echo "Applying $script..."
  psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -v ON_ERROR_STOP=1 -f "$script"
done
```

---

### Step 3: Seed Reference & Demo Data

```bash
# 1. Seed Mandatory Reference Catalogs (Divisions, Districts, Roles, Categories, Payment Methods)
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/013_seed_reference_data.sql

# 2. (Optional for local development) Seed Demonstration Products, Inventory & Demo Accounts
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/014_seed_demo_data.sql
```

---

## 4. Verification & Diagnostics

### A. Run Database Health Checks
Execute the diagnostic health check script:
```bash
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/016_database_health_checks.sql
```
Expected output:
- 54 registered base tables
- 0 orphaned order items
- 0 invalid inventory records
- 0 order calculation inconsistencies
- Correct bilingual UTF-8 rendering

### B. Verify .NET Connectivity
Run the minimal .NET database verification console tool:
```bash
dotnet run --project backend/NiramoyHealth.DbVerification
```
Expected output:
```
=================================================================
NIRAMOY HEALTH (.NET 10 / NPGSQL) DATABASE CONNECTIVITY CHECK
=================================================================
...
[SUCCESS] All .NET Npgsql database connectivity checks passed!
```

### C. Run Full Automated Test Suite
Run the 15-point automated database test suite:
```bash
python3 tests/database/run_tests.py
```
Expected output:
```
TEST SUMMARY: 15/15 Tests Passed (100.0%)
Detailed Markdown report generated: tests/database/TEST_RESULTS.md
```

---

## 5. Rollback Procedures

To tear down or revert database migrations, execute the scripts in `sql/rollback/`.

### Complete Teardown
```bash
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/rollback/rollback_all.sql
```

### Modular Rollback
To revert specific domains without affecting base tables:
```bash
# Revert reporting views and audit logs
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/rollback/011_rollback_audit_and_reporting.sql

# Revert notifications and customer support
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/rollback/010_rollback_notifications_and_support.sql
```
