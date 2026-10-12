-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 005: INVENTORY, BATCHES & STOCK MOVEMENTS
-- =============================================================================
-- Domain: Warehousing, Inventory Management & Batch Tracking
-- Standards:
--   - Strict non-negative inventory constraints (quantity_on_hand >= 0)
--   - Two-phase reservation model (quantity_reserved <= quantity_on_hand)
--   - Pharmaceutical batch tracking with manufacturing and expiration dates
--   - Complete auditable ledger of stock transactions (inventory_transactions)
-- =============================================================================

BEGIN;

-- 1. Warehouses & Fulfillment Hubs
CREATE TABLE IF NOT EXISTS warehouses (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    division_id INT NOT NULL REFERENCES divisions(id) ON DELETE RESTRICT,
    district_id INT NOT NULL REFERENCES districts(id) ON DELETE RESTRICT,
    address TEXT NOT NULL,
    contact_phone VARCHAR(20),
    is_central_hub BOOLEAN DEFAULT FALSE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 2. Warehouse Stock Levels (Aggregated per product per warehouse)
CREATE TABLE IF NOT EXISTS inventory (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
    warehouse_id INT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    quantity_on_hand INT NOT NULL DEFAULT 0 CHECK (quantity_on_hand >= 0),
    quantity_reserved INT NOT NULL DEFAULT 0 CHECK (quantity_reserved >= 0),
    reorder_level INT NOT NULL DEFAULT 10 CHECK (reorder_level >= 0),
    reorder_quantity INT NOT NULL DEFAULT 50 CHECK (reorder_quantity > 0),
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_inventory_product_warehouse UNIQUE (product_id, warehouse_id),
    CONSTRAINT chk_inventory_reserved_lte_on_hand CHECK (quantity_reserved <= quantity_on_hand)
);

-- 3. Medicine Batches (Expiry and Batch Traceability for Drug Safety)
CREATE TABLE IF NOT EXISTS medicine_batches (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
    warehouse_id INT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    batch_number VARCHAR(100) NOT NULL,
    mfg_date DATE NOT NULL,
    expiry_date DATE NOT NULL,
    quantity_received INT NOT NULL CHECK (quantity_received > 0),
    quantity_remaining INT NOT NULL CHECK (quantity_remaining >= 0),
    unit_cost NUMERIC(12,2) NOT NULL CHECK (unit_cost >= 0),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_product_warehouse_batch UNIQUE (product_id, warehouse_id, batch_number),
    CONSTRAINT chk_batch_expiry_after_mfg CHECK (expiry_date > mfg_date),
    CONSTRAINT chk_batch_remaining_lte_received CHECK (quantity_remaining <= quantity_received)
);

-- 4. Inventory Transaction Ledger
CREATE TABLE IF NOT EXISTS inventory_transactions (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
    warehouse_id INT NOT NULL REFERENCES warehouses(id) ON DELETE RESTRICT,
    batch_id BIGINT REFERENCES medicine_batches(id) ON DELETE RESTRICT,
    transaction_type VARCHAR(50) NOT NULL
        CHECK (transaction_type IN (
            'purchase_receipt',      -- Inbound stock arrival
            'sale_deduction',        -- Final order fulfillment deduction
            'reservation',           -- Order placed, stock held
            'reservation_release',   -- Order cancelled, reservation released
            'customer_return',       -- Return accepted back into stock
            'damage_writeoff',       -- Damaged goods discarded
            'expired_writeoff',      -- Expired medicine safely disposed
            'stock_adjustment'       -- Physical count audit reconciliation
        )),
    quantity_change INT NOT NULL,
    balance_after INT NOT NULL CHECK (balance_after >= 0),
    reference_type VARCHAR(50) NOT NULL
        CHECK (reference_type IN ('order', 'purchase_order', 'return', 'prescription', 'manual_audit')),
    reference_id VARCHAR(100),
    notes TEXT,
    created_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Attach updated_at triggers
CREATE TRIGGER trg_warehouses_updated_at
    BEFORE UPDATE ON warehouses
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_inventory_updated_at
    BEFORE UPDATE ON inventory
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_medicine_batches_updated_at
    BEFORE UPDATE ON medicine_batches
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
