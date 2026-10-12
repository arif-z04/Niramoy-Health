-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 008: ORDERS, PAYMENTS, SNAPSHOTS & TRANSACTIONS
-- =============================================================================
-- Domain: E-Commerce Checkout, Orders & Financial Payments
-- Standards:
--   - BDT currency explicit modeling (NUMERIC(12,2))
--   - Immutable order item pricing snapshots to guard historical financial records
--   - Immutable address snapshots at moment of purchase
--   - Separate order status from payment status
--   - Idempotency keys on orders and payment transactions to prevent duplicate debits
--   - Strict non-storage of sensitive MFS PIN/OTP credentials
-- =============================================================================

BEGIN;

-- 1. Supported Delivery Shipping Methods
CREATE TABLE IF NOT EXISTS delivery_methods (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    name_bn VARCHAR(100) NOT NULL,
    base_fee NUMERIC(12,2) NOT NULL CHECK (base_fee >= 0),
    free_delivery_threshold NUMERIC(12,2) CHECK (free_delivery_threshold >= 0),
    estimated_days_min INT NOT NULL CHECK (estimated_days_min >= 0),
    estimated_days_max INT NOT NULL CHECK (estimated_days_max >= estimated_days_min),
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 2. Payment Methods (COD, bKash, Nagad, Rocket, Cards)
CREATE TABLE IF NOT EXISTS payment_methods (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    name_bn VARCHAR(100) NOT NULL,
    is_mfs BOOLEAN DEFAULT FALSE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 3. Master Orders Table
CREATE TABLE IF NOT EXISTS orders (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id UUID DEFAULT gen_random_uuid() NOT NULL UNIQUE,
    order_number VARCHAR(50) NOT NULL UNIQUE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    prescription_id BIGINT REFERENCES prescriptions(id) ON DELETE SET NULL,
    order_status VARCHAR(30) DEFAULT 'pending' NOT NULL
        CHECK (order_status IN (
            'pending',
            'confirmed',
            'prescription_check',
            'processing',
            'packed',
            'shipped',
            'out_for_delivery',
            'delivered',
            'cancelled',
            'returned'
        )),
    payment_status VARCHAR(30) DEFAULT 'unpaid' NOT NULL
        CHECK (payment_status IN ('unpaid', 'partially_paid', 'paid', 'refunded', 'failed')),
    subtotal_amount NUMERIC(12,2) NOT NULL CHECK (subtotal_amount >= 0),
    discount_amount NUMERIC(12,2) DEFAULT 0.00 NOT NULL CHECK (discount_amount >= 0),
    delivery_fee NUMERIC(12,2) DEFAULT 0.00 NOT NULL CHECK (delivery_fee >= 0),
    total_amount NUMERIC(12,2) NOT NULL CHECK (total_amount >= 0),
    currency VARCHAR(3) DEFAULT 'BDT' NOT NULL CHECK (currency = 'BDT'),
    coupon_code VARCHAR(50),
    delivery_method_id INT NOT NULL REFERENCES delivery_methods(id) ON DELETE RESTRICT,
    payment_method_id INT NOT NULL REFERENCES payment_methods(id) ON DELETE RESTRICT,
    customer_notes TEXT,
    cancel_reason TEXT,
    idempotency_key VARCHAR(128) UNIQUE,
    placed_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT chk_order_math CHECK (total_amount = (subtotal_amount - discount_amount + delivery_fee))
);

-- 4. Immutable Delivery Address Snapshot (Persisted at time of order)
CREATE TABLE IF NOT EXISTS order_addresses (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE UNIQUE,
    recipient_name VARCHAR(150) NOT NULL,
    recipient_phone VARCHAR(20) NOT NULL,
    division VARCHAR(100) NOT NULL,
    district VARCHAR(100) NOT NULL,
    upazila VARCHAR(100),
    area_details TEXT NOT NULL,
    postal_code VARCHAR(10),
    delivery_instructions TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 5. Order Line Items with Price Snapshots
CREATE TABLE IF NOT EXISTS order_items (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE RESTRICT,
    product_variant_id BIGINT REFERENCES product_variants(id) ON DELETE RESTRICT,
    product_name VARCHAR(255) NOT NULL,
    product_name_bn VARCHAR(255) NOT NULL,
    product_sku VARCHAR(100) NOT NULL,
    unit_price_snapshot NUMERIC(12,2) NOT NULL CHECK (unit_price_snapshot >= 0),
    quantity INT NOT NULL CHECK (quantity > 0),
    total_price NUMERIC(12,2) NOT NULL CHECK (total_price >= 0),
    is_prescription_item BOOLEAN DEFAULT FALSE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT chk_order_item_calc CHECK (total_price = (unit_price_snapshot * quantity))
);

-- 6. Order Status Audit History
CREATE TABLE IF NOT EXISTS order_status_history (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    previous_status VARCHAR(30),
    new_status VARCHAR(30) NOT NULL,
    changed_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    notes TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 7. Payment Records (bKash, Nagad, COD, SSLCommerz)
CREATE TABLE IF NOT EXISTS payments (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id UUID DEFAULT gen_random_uuid() NOT NULL UNIQUE,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE RESTRICT,
    payment_method_id INT NOT NULL REFERENCES payment_methods(id) ON DELETE RESTRICT,
    payment_status VARCHAR(30) DEFAULT 'pending' NOT NULL
        CHECK (payment_status IN ('pending', 'authorized', 'captured', 'failed', 'refunded', 'cancelled')),
    amount NUMERIC(12,2) NOT NULL CHECK (amount > 0),
    currency VARCHAR(3) DEFAULT 'BDT' NOT NULL CHECK (currency = 'BDT'),
    transaction_id VARCHAR(100) UNIQUE,
    idempotency_key VARCHAR(128) UNIQUE,
    gateway_provider VARCHAR(50),
    gateway_response JSONB DEFAULT '{}'::jsonb,
    initiated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    completed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 8. Payment Gateway Transaction Log / Attempts
CREATE TABLE IF NOT EXISTS payment_transactions (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    payment_id BIGINT NOT NULL REFERENCES payments(id) ON DELETE CASCADE,
    transaction_type VARCHAR(50) NOT NULL
        CHECK (transaction_type IN ('initiate', 'query', 'execute', 'refund', 'ipn_callback', 'status_check')),
    status VARCHAR(30) NOT NULL,
    provider_reference VARCHAR(150),
    raw_response_sanitized JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 9. Refunds
CREATE TABLE IF NOT EXISTS refunds (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE RESTRICT,
    payment_id BIGINT NOT NULL REFERENCES payments(id) ON DELETE RESTRICT,
    refund_amount NUMERIC(12,2) NOT NULL CHECK (refund_amount > 0),
    reason TEXT NOT NULL,
    status VARCHAR(30) DEFAULT 'requested' NOT NULL
        CHECK (status IN ('requested', 'approved', 'processed', 'rejected')),
    processed_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    processed_at TIMESTAMPTZ,
    mfs_refund_trx_id VARCHAR(100),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Attach updated_at triggers
CREATE TRIGGER trg_orders_updated_at
    BEFORE UPDATE ON orders
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
