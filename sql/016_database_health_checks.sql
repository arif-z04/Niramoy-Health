-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 016: DATABASE INTEGRITY & HEALTH CHECKS
-- =============================================================================
-- Description:
-- Diagnostic script to verify the health, structural completeness, constraint
-- consistency, and referential integrity of the Niramoy Health PostgreSQL database.
-- =============================================================================

\echo '-----------------------------------------------------------------------------'
\echo 'NIRAMOY HEALTH - DATABASE SYSTEM HEALTH REPORT'
\echo '-----------------------------------------------------------------------------'

-- 1. PostgreSQL Version and Connection Verification
SELECT 
    current_database() AS database_name,
    current_user AS session_user,
    version() AS postgres_version,
    pg_database_size(current_database()) / 1024 / 1024 AS size_mb;

-- 2. Schema Table Inventory
\echo '\n--- Table Inventory & Row Counts ---'
SELECT 
    schemaname,
    relname AS table_name,
    n_live_tup AS estimated_rows
FROM pg_stat_user_tables
ORDER BY relname ASC;

-- 3. Constraint Inventory
\echo '\n--- Constraint Counts by Type ---'
SELECT 
    contype,
    CASE contype
        WHEN 'p' THEN 'Primary Key'
        WHEN 'f' THEN 'Foreign Key'
        WHEN 'u' THEN 'Unique Constraint'
        WHEN 'c' THEN 'Check Constraint'
        ELSE 'Other'
    END AS constraint_type,
    COUNT(*) AS count
FROM pg_constraint
WHERE connamespace = 'public'::regnamespace
GROUP BY contype
ORDER BY count DESC;

-- 4. Index Inventory
\echo '\n--- Custom Indexes (Excluding Primary Keys) ---'
SELECT 
    tablename,
    indexname,
    indexdef
FROM pg_indexes
WHERE schemaname = 'public' 
  AND indexname NOT LIKE '%_pkey'
ORDER BY tablename, indexname;

-- 5. Business Invariant & Data Integrity Verifications
\echo '\n--- Running Data Integrity Audits ---'

-- Check A: Orphaned order items
SELECT COUNT(*) AS orphaned_order_items
FROM order_items oi
LEFT JOIN orders o ON oi.order_id = o.id
WHERE o.id IS NULL;

-- Check B: Inventory sanity (reserved > on hand or negative stock)
SELECT COUNT(*) AS invalid_inventory_rows
FROM inventory
WHERE quantity_on_hand < 0 
   OR quantity_reserved < 0 
   OR quantity_reserved > quantity_on_hand;

-- Check C: Order calculation consistency
SELECT COUNT(*) AS order_math_violations
FROM orders
WHERE total_amount != (subtotal_amount - discount_amount + delivery_fee);

-- Check D: Order item calculation consistency
SELECT COUNT(*) AS order_item_math_violations
FROM order_items
WHERE total_price != (unit_price_snapshot * quantity);

-- Check E: Medicine batch expiration consistency (expired before manufactured)
SELECT COUNT(*) AS invalid_batch_dates
FROM medicine_batches
WHERE expiry_date <= mfg_date;

-- Check F: Bengali Unicode String verification
SELECT 
    name AS english_name, 
    name_bn AS bengali_name,
    octet_length(name_bn) AS byte_length,
    char_length(name_bn) AS char_length
FROM products
WHERE name_bn IS NOT NULL
LIMIT 3;

\echo '-----------------------------------------------------------------------------'
\echo 'HEALTH CHECK COMPLETED SUCCESSFULLY'
\echo '-----------------------------------------------------------------------------'
