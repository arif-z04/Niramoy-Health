# Niramoy Health (নিরাময় হেলথ) - Database Testing & Verification Report

## 1. Testing Methodology & Isolation Strategy

To ensure zero risk of data corruption, schema regression, or unintended state mutations:
1. **Isolated Test Database**: All automated schema teardown, stress tests, and destructive negative-path assertions run strictly against `niramoy_health_test`.
2. **Dedicated User Permissions**: Tests run under the unprivileged application role `niramoy_user`.
3. **Repeatable Automation**: The entire test suite is codified in `tests/database/run_tests.py` and can be executed unattended in local development or CI/CD pipelines.
4. **Dual Provider Verification**: Validations run via both direct PostgreSQL client execution (`psql`) and .NET 10 ADO.NET Npgsql driver.

---

## 2. Test Suite Execution & Results Matrix

**Test Execution Date**: 2026-10-11 04:14:24 UTC  
**Environment**: PostgreSQL 18.6, .NET SDK 10.0.111, Linux x86_64  
**Target Database**: `niramoy_health_test` (UTF-8)  
**Overall Result**: **15 / 15 Passed (100.0%)**

| Test ID | Test Category | Specification & Verification Criteria | Result | Details |
|:---:|:---|:---|:---:|:---|
| **01** | PostgreSQL Service Availability | Server daemon is responsive on TCP 127.0.0.1:5432 | **PASS** | `pg_isready` returned exit code 0 |
| **02** | Database Creation & Connection | Can connect to `niramoy_health_test` as `niramoy_user` | **PASS** | Verified active session on `niramoy_health_test` |
| **03** | Sequential Schema Execution | Clean rollout of scripts `002` through `014` in order | **PASS** | All DDL, DML and triggers applied in transaction |
| **04** | Schema Object Existence | Confirms presence of all required tables, constraints, indexes | **PASS** | 54 tables, 655 constraints, 151 indexes verified |
| **05** | Foreign-Key Enforcement | Blocks insertion of records with non-existent parent foreign keys | **PASS** | Invalid category ID rejected with FK violation |
| **06** | Unique Constraint Enforcement | Blocks insertion of duplicate natural keys (phone, SKU, email) | **PASS** | Duplicate phone number rejected |
| **07** | Price & Quantity Validation | Enforces `base_price >= 0`, `discount <= price`, `qty > 0` | **PASS** | Negative price, excess discount & 0 qty rejected |
| **08** | Order Calculation Integrity | Enforces order total math & order item total multiplication | **PASS** | Mismatched totals blocked by database engine |
| **09** | Transaction Rollback Integrity | Confirms complete rollback on intentional failure / exception | **PASS** | Uncommitted data rolled back cleanly via savepoint |
| **10** | Inventory Concurrency & Safety | Enforces `quantity_reserved <= quantity_on_hand` constraint | **PASS** | Overselling attempt blocked by check constraint |
| **11** | Status Domain Integrity | Blocks unauthorized prescription and order status strings | **PASS** | Non-enumerated status strings rejected |
| **12** | Currency Precision & Math | NUMERIC(12,2) exact representation without floating-point error | **PASS** | Exact fractional BDT math verified |
| **13** | Seed-Data Integrity | Validates mandatory reference divisions, brands, and categories | **PASS** | 8 divisions, 8 manufacturers, 10 products verified |
| **14** | Script Rerun Idempotency | Confirms seed scripts can be re-run safely without errors | **PASS** | Re-execution of 013 and 014 succeeded cleanly |
| **15** | .NET Npgsql Connectivity | Validates .NET 10 Npgsql DataSource connection & UTF-8 queries | **PASS** | DbVerification app passed with exit code 0 |

---

## 3. Deep Dive: Key Test Validations

### A. Overselling Prevention & Concurrency (Test 10)
To prevent overselling life-saving prescription medicines when multiple shoppers check out concurrently:
- **Constraint**: `chk_inventory_reserved_lte_on_hand CHECK (quantity_reserved <= quantity_on_hand)`
- **Verification**: An attempt to set `quantity_reserved = quantity_on_hand + 500` failed immediately with:
  ```
  ERROR: new row for relation "inventory" violates check constraint "chk_inventory_reserved_lte_on_hand"
  ```
- In Phase 2, order checkout transactions will issue `SELECT ... FOR UPDATE` on inventory rows before updating reservations, ensuring strict serialization.

### B. Prescription Safety & Clinical Governance (Test 11)
- Prescriptions must strictly adhere to the approved status lifecycle (`submitted`, `under_review`, `approved`, `partially_approved`, `rejected`, `expired`).
- An attempt to bypass pharmacist review by inserting status `'unauthorized_approved'` failed with:
  ```
  ERROR: new row for relation "prescriptions" violates check constraint "prescriptions_status_check"
  ```

### C. Financial Snapshot Integrity (Test 08)
- Orders enforce mathematical consistency: `total_amount = subtotal_amount - discount_amount + delivery_fee`.
- Order items enforce: `total_price = unit_price_snapshot * quantity`.
- Both constraints operate at the database engine level, guaranteeing that buggy API logic or client-side tampering cannot corrupt financial ledgers.

---

## 4. How to Run Tests

### Run Full Python Automated Suite:
```bash
python3 tests/database/run_tests.py
```

### Run Direct Health Check Diagnostic:
```bash
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_test -f sql/016_database_health_checks.sql
```

### Run .NET Connectivity Verification:
```bash
dotnet run --project backend/NiramoyHealth.DbVerification
```
