-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 011: AUDITING, APP CONFIG & OPERATIONAL REPORTING VIEWS
-- =============================================================================
-- Domain: Administrative Auditing & Business Intelligence
-- Standards:
--   - Structured JSONB audit trail (sanitized, no sensitive secrets/credentials)
--   - Centralized system settings table
--   - Real-time operational reporting views for pharmacy operations and executive dashboard
-- =============================================================================

BEGIN;

-- 1. System Audit Logs
CREATE TABLE IF NOT EXISTS audit_logs (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT REFERENCES users(id) ON DELETE SET NULL,
    action VARCHAR(100) NOT NULL,
    entity_type VARCHAR(100) NOT NULL,
    entity_id VARCHAR(100) NOT NULL,
    old_values JSONB,
    new_values JSONB,
    ip_address VARCHAR(45),
    user_agent TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 2. Staff / Administrative Action Log
CREATE TABLE IF NOT EXISTS administrative_actions (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    admin_user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    action_type VARCHAR(100) NOT NULL
        CHECK (action_type IN (
            'user_suspended',
            'user_activated',
            'prescription_reviewed',
            'refund_approved',
            'refund_rejected',
            'price_adjusted',
            'role_assigned',
            'settings_changed',
            'inventory_adjusted'
        )),
    notes TEXT NOT NULL,
    target_user_id BIGINT REFERENCES users(id) ON DELETE SET NULL,
    target_order_id BIGINT REFERENCES orders(id) ON DELETE SET NULL,
    target_product_id BIGINT REFERENCES products(id) ON DELETE SET NULL,
    metadata JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 3. Application System Configurations
CREATE TABLE IF NOT EXISTS app_configurations (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    config_key VARCHAR(100) NOT NULL UNIQUE,
    config_value TEXT NOT NULL,
    description TEXT,
    is_encrypted BOOLEAN DEFAULT FALSE NOT NULL,
    updated_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- -----------------------------------------------------------------------------
-- Operational Reporting Views
-- -----------------------------------------------------------------------------

-- View 1: Real-time Product Stock Status with Calculated Availability
CREATE OR REPLACE VIEW vw_product_stock_status AS
SELECT 
    p.id AS product_id,
    p.sku,
    p.name,
    p.name_bn,
    c.name AS category_name,
    m.name AS manufacturer_name,
    p.is_prescription_required,
    p.base_price,
    p.discount_price,
    COALESCE(SUM(i.quantity_on_hand), 0) AS total_on_hand,
    COALESCE(SUM(i.quantity_reserved), 0) AS total_reserved,
    COALESCE(SUM(i.quantity_on_hand - i.quantity_reserved), 0) AS total_available,
    MIN(COALESCE(i.reorder_level, 10)) AS reorder_level,
    CASE 
        WHEN COALESCE(SUM(i.quantity_on_hand - i.quantity_reserved), 0) <= 0 THEN 'OUT_OF_STOCK'
        WHEN COALESCE(SUM(i.quantity_on_hand - i.quantity_reserved), 0) <= MIN(COALESCE(i.reorder_level, 10)) THEN 'LOW_STOCK'
        ELSE 'IN_STOCK'
    END AS stock_status
FROM products p
JOIN categories c ON p.category_id = c.id
LEFT JOIN manufacturers m ON p.manufacturer_id = m.id
LEFT JOIN inventory i ON p.id = i.product_id
WHERE p.is_active = TRUE AND p.deleted_at IS NULL
GROUP BY p.id, p.sku, p.name, p.name_bn, c.name, m.name, p.is_prescription_required, p.base_price, p.discount_price;

-- View 2: Urgent Low Stock Alerts
CREATE OR REPLACE VIEW vw_low_stock_alerts AS
SELECT 
    product_id,
    sku,
    name,
    name_bn,
    category_name,
    manufacturer_name,
    total_on_hand,
    total_reserved,
    total_available,
    reorder_level,
    stock_status
FROM vw_product_stock_status
WHERE stock_status IN ('OUT_OF_STOCK', 'LOW_STOCK');

-- View 3: Pharmacist Prescription Review Queue
CREATE OR REPLACE VIEW vw_prescription_review_queue AS
SELECT 
    pr.id AS prescription_id,
    pr.public_id,
    pr.user_id,
    u.full_name AS customer_name,
    u.phone_number AS customer_phone,
    pr.patient_name,
    pr.patient_age,
    pr.doctor_name,
    pr.status,
    pr.created_at AS submitted_at,
    ROUND(EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - pr.created_at)) / 3600, 2) AS waiting_hours,
    COUNT(pf.id) AS file_count
FROM prescriptions pr
JOIN users u ON pr.user_id = u.id
LEFT JOIN prescription_files pf ON pr.id = pf.prescription_id
WHERE pr.status IN ('submitted', 'under_review')
GROUP BY pr.id, pr.public_id, pr.user_id, u.full_name, u.phone_number, pr.patient_name, pr.patient_age, pr.doctor_name, pr.status, pr.created_at
ORDER BY pr.created_at ASC;

-- View 4: Daily Sales & Revenue Breakdown
CREATE OR REPLACE VIEW vw_daily_sales_summary AS
SELECT 
    CAST(o.placed_at AS DATE) AS sale_date,
    COUNT(o.id) AS total_orders,
    COUNT(CASE WHEN o.order_status = 'delivered' THEN 1 END) AS delivered_orders,
    COUNT(CASE WHEN o.order_status = 'cancelled' THEN 1 END) AS cancelled_orders,
    SUM(CASE WHEN o.order_status != 'cancelled' THEN o.subtotal_amount ELSE 0 END) AS gross_merchandise_value,
    SUM(CASE WHEN o.order_status != 'cancelled' THEN o.discount_amount ELSE 0 END) AS total_discounts,
    SUM(CASE WHEN o.order_status != 'cancelled' THEN o.delivery_fee ELSE 0 END) AS total_delivery_fees,
    SUM(CASE WHEN o.order_status != 'cancelled' THEN o.total_amount ELSE 0 END) AS net_revenue_bdt,
    SUM(CASE WHEN o.order_status != 'cancelled' AND pm.code = 'cod' THEN o.total_amount ELSE 0 END) AS cod_revenue,
    SUM(CASE WHEN o.order_status != 'cancelled' AND pm.is_mfs = TRUE THEN o.total_amount ELSE 0 END) AS mfs_revenue
FROM orders o
JOIN payment_methods pm ON o.payment_method_id = pm.id
GROUP BY CAST(o.placed_at AS DATE)
ORDER BY sale_date DESC;

-- View 5: Customer Order Lifetime Summary
CREATE OR REPLACE VIEW vw_customer_order_summary AS
SELECT 
    u.id AS user_id,
    u.public_id,
    u.full_name,
    u.phone_number,
    u.email,
    u.status AS account_status,
    COUNT(o.id) AS total_orders,
    COALESCE(SUM(CASE WHEN o.order_status != 'cancelled' THEN o.total_amount ELSE 0 END), 0.00) AS lifetime_spend_bdt,
    MAX(o.placed_at) AS last_order_at
FROM users u
LEFT JOIN orders o ON u.id = o.user_id
GROUP BY u.id, u.public_id, u.full_name, u.phone_number, u.email, u.status;

-- View 6: Top-Selling Healthcare Products
CREATE OR REPLACE VIEW vw_top_selling_products AS
SELECT 
    oi.product_id,
    oi.product_sku,
    oi.product_name,
    oi.product_name_bn,
    c.name AS category_name,
    SUM(oi.quantity) AS total_quantity_sold,
    SUM(oi.total_price) AS total_revenue_bdt,
    COUNT(DISTINCT oi.order_id) AS distinct_orders_count
FROM order_items oi
JOIN orders o ON oi.order_id = o.id
JOIN products p ON oi.product_id = p.id
JOIN categories c ON p.category_id = c.id
WHERE o.order_status NOT IN ('cancelled', 'returned')
GROUP BY oi.product_id, oi.product_sku, oi.product_name, oi.product_name_bn, c.name
ORDER BY total_revenue_bdt DESC;

-- Attach updated_at triggers
CREATE TRIGGER trg_app_configurations_updated_at
    BEFORE UPDATE ON app_configurations
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
