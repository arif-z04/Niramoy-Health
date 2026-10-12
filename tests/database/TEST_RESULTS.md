# Niramoy Health - Database Verification Test Results

**Execution Timestamp:** 2026-10-11 04:14:24 UTC  
**Target Database:** `niramoy_health_test` on `127.0.0.1:5432`  
**Overall Status:** **15/15 Tests Passed** (100% Pass Rate)  

| Test # | Test Description | Status | Verification Details |
|:---:|:---|:---:|:---|
| 01 | PostgreSQL service availability | **PASS** | 127.0.0.1:5432 - accepting connections |
| 02 | Database creation and connection | **PASS** | niramoy_health_test|niramoy_user |
| 03 | Schema scripts execution in order | **PASS** |  |
| 04 | Table, constraint, and index existence | **PASS** | Tables: 54, Constraints: 655, Indexes: 151 |
| 05 | Foreign-key enforcement | **PASS** | Foreign key correctly blocked invalid reference. |
| 06 | Unique constraint enforcement | **PASS** | Unique constraint correctly blocked duplicate phone. |
| 07 | Invalid price & quantity rejection | **PASS** | Negative price, discount > price, and zero quantity blocked. |
| 08 | Order & order-item calculation constraints | **PASS** | Mismatched order math and line-item math blocked. |
| 09 | Transaction rollback on intentional failure | **PASS** | Uncommitted data rolled back cleanly. |
| 10 | Inventory constraints prevent overselling | **PASS** | Constraint reserved <= on_hand enforced. |
| 11 | Prescription & order status integrity | **PASS** | Invalid prescription and order statuses rejected. |
| 12 | Currency precision & exact arithmetic | **PASS** | Exact NUMERIC(12,2) arithmetic verified without float rounding. |
| 13 | Seed-data integrity check | **PASS** | Divisions, manufacturers, products, and demo orders validated. |
| 14 | Script rerun idempotency behavior | **PASS** | Reference and demo seed scripts successfully re-executed without conflict. |
| 15 | Database connectivity from .NET (Npgsql) | **PASS** | Verified Npgsql DataSource, UTF-8 query, and connection pooling. |


All 15 test criteria required by Phase 1 have been rigorously tested against an isolated PostgreSQL test database.
