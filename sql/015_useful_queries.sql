-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 015: USEFUL OPERATIONAL & REPORTING QUERIES
-- =============================================================================
-- Description:
-- Collection of optimized SQL queries commonly executed by the application backend,
-- administrative dashboard, inventory controllers, and pharmacists.
-- =============================================================================

-- -----------------------------------------------------------------------------
-- 1. Product Search & Catalog Listing with Filters, Pagination, and Bilingual Support
-- -----------------------------------------------------------------------------
-- Searches products by English or Bengali query with category, brand, and prescription filters
SELECT 
    p.public_id,
    p.sku,
    p.name,
    p.name_bn,
    p.strength,
    p.pack_size,
    p.base_price,
    p.discount_price,
    p.is_prescription_required,
    p.badge,
    p.avg_rating,
    p.review_count,
    c.name AS category_name,
    c.name_bn AS category_name_bn,
    m.name AS manufacturer_name,
    b.name AS brand_name,
    g.name AS generic_name,
    COALESCE(SUM(i.quantity_on_hand - i.quantity_reserved), 0) AS available_stock
FROM products p
JOIN categories c ON p.category_id = c.id
LEFT JOIN manufacturers m ON p.manufacturer_id = m.id
LEFT JOIN brands b ON p.brand_id = b.id
LEFT JOIN generics g ON p.generic_id = g.id
LEFT JOIN inventory i ON p.id = i.product_id
WHERE p.is_active = TRUE 
  AND p.deleted_at IS NULL
  -- Example search term matching name, name_bn, or generic
  AND (p.name ILIKE '%Napa%' OR p.name_bn ILIKE '%নাপা%' OR g.name ILIKE '%Paracetamol%')
GROUP BY p.id, p.public_id, p.sku, p.name, p.name_bn, p.strength, p.pack_size, 
         p.base_price, p.discount_price, p.is_prescription_required, p.badge, 
         p.avg_rating, p.review_count, c.name, c.name_bn, m.name, b.name, g.name
ORDER BY p.avg_rating DESC, p.id ASC
LIMIT 20 OFFSET 0;

-- -----------------------------------------------------------------------------
-- 2. Real-time Warehouse Stock Availability & Low Stock Alert
-- -----------------------------------------------------------------------------
-- Used by warehouse managers to restock before stockouts occur
SELECT 
    p.sku,
    p.name,
    p.name_bn,
    w.name AS warehouse_name,
    i.quantity_on_hand,
    i.quantity_reserved,
    (i.quantity_on_hand - i.quantity_reserved) AS available_quantity,
    i.reorder_level,
    i.reorder_quantity,
    CASE 
        WHEN (i.quantity_on_hand - i.quantity_reserved) <= 0 THEN 'OUT_OF_STOCK'
        WHEN (i.quantity_on_hand - i.quantity_reserved) <= i.reorder_level THEN 'REORDER_REQUIRED'
        ELSE 'SUFFICIENT'
    END AS replenishment_status
FROM inventory i
JOIN products p ON i.product_id = p.id
JOIN warehouses w ON i.warehouse_id = w.id
WHERE (i.quantity_on_hand - i.quantity_reserved) <= i.reorder_level
ORDER BY available_quantity ASC;

-- -----------------------------------------------------------------------------
-- 3. Customer Order History with Full Financial Snapshots & Status Timeline
-- -----------------------------------------------------------------------------
-- Used by the mobile app & web frontend to display order details
SELECT 
    o.order_number,
    o.placed_at,
    o.order_status,
    o.payment_status,
    o.subtotal_amount,
    o.discount_amount,
    o.delivery_fee,
    o.total_amount,
    o.currency,
    pm.name AS payment_method,
    dm.name AS delivery_method,
    oa.recipient_name,
    oa.recipient_phone,
    oa.area_details,
    oa.district,
    (
        SELECT jsonb_agg(jsonb_build_object(
            'sku', oi.product_sku,
            'name', oi.product_name,
            'unit_price', oi.unit_price_snapshot,
            'quantity', oi.quantity,
            'total_price', oi.total_price,
            'is_rx', oi.is_prescription_item
        ))
        FROM order_items oi
        WHERE oi.order_id = o.id
    ) AS line_items,
    (
        SELECT jsonb_agg(jsonb_build_object(
            'status', osh.new_status,
            'notes', osh.notes,
            'time', osh.created_at
        ) ORDER BY osh.created_at ASC)
        FROM order_status_history osh
        WHERE osh.order_id = o.id
    ) AS timeline
FROM orders o
JOIN payment_methods pm ON o.payment_method_id = pm.id
JOIN delivery_methods dm ON o.delivery_method_id = dm.id
JOIN order_addresses oa ON o.id = oa.order_id
WHERE o.order_number = 'NRM-202610-0001';

-- -----------------------------------------------------------------------------
-- 4. Pharmacist Prescription Verification Queue
-- -----------------------------------------------------------------------------
-- High-priority queue showing prescriptions awaiting review by licensed pharmacists
SELECT 
    pr.id AS rx_id,
    pr.public_id AS rx_public_id,
    pr.patient_name,
    pr.patient_age,
    pr.patient_gender,
    pr.doctor_name,
    pr.hospital_clinic,
    pr.status,
    pr.created_at AS submitted_at,
    ROUND(EXTRACT(EPOCH FROM (CURRENT_TIMESTAMP - pr.created_at)) / 3600, 1) AS hours_waiting,
    u.full_name AS customer_name,
    u.phone_number AS customer_phone,
    COALESCE(
        jsonb_agg(jsonb_build_object(
            'file_id', pf.id,
            'storage_key', pf.file_storage_key,
            'mime_type', pf.mime_type,
            'file_size_kb', ROUND(pf.file_size_bytes / 1024.0, 1)
        )) FILTER (WHERE pf.id IS NOT NULL), '[]'::jsonb
    ) AS attached_files
FROM prescriptions pr
JOIN users u ON pr.user_id = u.id
LEFT JOIN prescription_files pf ON pr.id = pf.prescription_id
WHERE pr.status IN ('submitted', 'under_review')
GROUP BY pr.id, pr.public_id, pr.patient_name, pr.patient_age, pr.patient_gender, 
         pr.doctor_name, pr.hospital_clinic, pr.status, pr.created_at, u.full_name, u.phone_number
ORDER BY pr.created_at ASC;

-- -----------------------------------------------------------------------------
-- 5. Executive Sales Summary & Revenue by Channel
-- -----------------------------------------------------------------------------
-- High-level financial reporting query
SELECT 
    DATE_TRUNC('month', o.placed_at) AS sales_month,
    COUNT(o.id) AS total_orders,
    COUNT(CASE WHEN o.order_status = 'delivered' THEN 1 END) AS delivered_orders,
    COUNT(CASE WHEN o.order_status = 'cancelled' THEN 1 END) AS cancelled_orders,
    SUM(o.total_amount) AS gross_sales_bdt,
    SUM(o.discount_amount) AS total_discounts_bdt,
    SUM(o.delivery_fee) AS total_delivery_fees_bdt,
    SUM(CASE WHEN pm.code = 'cod' THEN o.total_amount ELSE 0 END) AS cod_sales_bdt,
    SUM(CASE WHEN pm.code = 'bkash' THEN o.total_amount ELSE 0 END) AS bkash_sales_bdt,
    SUM(CASE WHEN pm.code = 'nagad' THEN o.total_amount ELSE 0 END) AS nagad_sales_bdt
FROM orders o
JOIN payment_methods pm ON o.payment_method_id = pm.id
WHERE o.order_status != 'cancelled'
GROUP BY DATE_TRUNC('month', o.placed_at)
ORDER BY sales_month DESC;

-- -----------------------------------------------------------------------------
-- 6. Medicine Batch Expiry Risk Report (Next 180 Days)
-- -----------------------------------------------------------------------------
-- Critical for preventing expired drug sales and drug regulatory compliance
SELECT 
    p.sku,
    p.name,
    mb.batch_number,
    w.name AS warehouse,
    mb.mfg_date,
    mb.expiry_date,
    mb.quantity_remaining,
    mb.unit_cost,
    (mb.quantity_remaining * mb.unit_cost) AS total_inventory_value,
    (mb.expiry_date - CURRENT_DATE) AS days_until_expiry,
    CASE 
        WHEN mb.expiry_date <= CURRENT_DATE THEN 'EXPIRED'
        WHEN mb.expiry_date <= CURRENT_DATE + INTERVAL '60 days' THEN 'CRITICAL_UNDER_60_DAYS'
        WHEN mb.expiry_date <= CURRENT_DATE + INTERVAL '180 days' THEN 'WARNING_UNDER_180_DAYS'
        ELSE 'SAFE'
    END AS expiry_risk_tier
FROM medicine_batches mb
JOIN products p ON mb.product_id = p.id
JOIN warehouses w ON mb.warehouse_id = w.id
WHERE mb.quantity_remaining > 0 
  AND mb.expiry_date <= CURRENT_DATE + INTERVAL '180 days'
ORDER BY mb.expiry_date ASC;

-- -----------------------------------------------------------------------------
-- 7. Live Shopping Cart Recalculation (Server-Side Price Integrity)
-- -----------------------------------------------------------------------------
-- Dynamically pulls latest verified catalog pricing for items in a user's active cart
SELECT 
    ci.id AS cart_item_id,
    c.id AS cart_id,
    p.id AS product_id,
    p.name AS product_name,
    p.sku,
    ci.quantity,
    COALESCE(p.discount_price, p.base_price) AS current_unit_price,
    p.base_price AS regular_unit_price,
    (ci.quantity * COALESCE(p.discount_price, p.base_price)) AS item_subtotal,
    COALESCE(SUM(i.quantity_on_hand - i.quantity_reserved), 0) AS stock_available,
    CASE 
        WHEN COALESCE(SUM(i.quantity_on_hand - i.quantity_reserved), 0) >= ci.quantity THEN TRUE 
        ELSE FALSE 
    END AS is_in_stock
FROM carts c
JOIN cart_items ci ON c.id = ci.cart_id
JOIN products p ON ci.product_id = p.id
LEFT JOIN inventory i ON p.id = i.product_id
WHERE c.user_id = 3 AND c.status = 'active'
GROUP BY ci.id, c.id, p.id, p.name, p.sku, ci.quantity, p.discount_price, p.base_price;
