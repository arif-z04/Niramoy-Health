-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 012: INDEXES, PERFORMANCE OPTIMIZATIONS & FULL-TEXT SEARCH
-- =============================================================================
-- Description:
-- Establishes critical indexing for high-throughput e-commerce lookups:
--   - Foreign key indexing to prevent full table locks on cascading actions
--   - GIN Trigram indexes on English & Bengali product names for fuzzy auto-complete
--   - Full-text search tsvector indexes
--   - Partial indexes for high-frequency filtered queries (active products, pending prescriptions)
--   - Composite indexes for pagination, sorting and order history
-- =============================================================================

BEGIN;

-- -----------------------------------------------------------------------------
-- 1. Users, Addresses & Authentication Indexes
-- -----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS idx_users_phone ON users(phone_number);
CREATE INDEX IF NOT EXISTS idx_users_email ON users(email);
CREATE INDEX IF NOT EXISTS idx_users_status ON users(status);
CREATE INDEX IF NOT EXISTS idx_customer_addresses_user ON customer_addresses(user_id);
CREATE INDEX IF NOT EXISTS idx_customer_addresses_district ON customer_addresses(district_id);
CREATE INDEX IF NOT EXISTS idx_user_sessions_user ON user_sessions(user_id);
CREATE INDEX IF NOT EXISTS idx_user_sessions_token ON user_sessions(token_hash) WHERE is_revoked = FALSE;

-- -----------------------------------------------------------------------------
-- 2. Product Catalog Indexes & Trigram Search
-- -----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS idx_categories_parent ON categories(parent_id);
CREATE INDEX IF NOT EXISTS idx_categories_slug ON categories(slug);

CREATE INDEX IF NOT EXISTS idx_products_category ON products(category_id);
CREATE INDEX IF NOT EXISTS idx_products_brand ON products(brand_id);
CREATE INDEX IF NOT EXISTS idx_products_manufacturer ON products(manufacturer_id);
CREATE INDEX IF NOT EXISTS idx_products_generic ON products(generic_id);
CREATE INDEX IF NOT EXISTS idx_products_dosage_form ON products(dosage_form_id);

-- Partial index for active catalog products
CREATE INDEX IF NOT EXISTS idx_products_active_catalog 
    ON products(id, category_id, base_price) 
    WHERE is_active = TRUE AND deleted_at IS NULL;

-- Featured products index
CREATE INDEX IF NOT EXISTS idx_products_featured 
    ON products(avg_rating DESC, review_count DESC) 
    WHERE is_featured = TRUE AND is_active = TRUE AND deleted_at IS NULL;

-- Trigram GIN indexes for fast fuzzy search in English and Bengali
CREATE INDEX IF NOT EXISTS idx_products_name_trgm 
    ON products USING gin (name gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_products_name_bn_trgm 
    ON products USING gin (name_bn gin_trgm_ops);

CREATE INDEX IF NOT EXISTS idx_generics_name_trgm 
    ON generics USING gin (name gin_trgm_ops);

-- Product images display order
CREATE INDEX IF NOT EXISTS idx_product_images_product 
    ON product_images(product_id, display_order);

-- Product reviews
CREATE INDEX IF NOT EXISTS idx_product_reviews_product 
    ON product_reviews(product_id, rating) 
    WHERE status = 'approved';

CREATE INDEX IF NOT EXISTS idx_product_reviews_pending 
    ON product_reviews(created_at) 
    WHERE status = 'pending';

-- -----------------------------------------------------------------------------
-- 3. Inventory & Expiry Tracking Indexes
-- -----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS idx_inventory_product_warehouse 
    ON inventory(product_id, warehouse_id);

CREATE INDEX IF NOT EXISTS idx_medicine_batches_product_expiry 
    ON medicine_batches(product_id, expiry_date);

CREATE INDEX IF NOT EXISTS idx_inventory_tx_product_date 
    ON inventory_transactions(product_id, created_at DESC);

-- -----------------------------------------------------------------------------
-- 4. Shopping Carts & Wishlists
-- -----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS idx_carts_user ON carts(user_id) WHERE user_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_carts_guest ON carts(guest_token) WHERE guest_token IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_cart_items_cart ON cart_items(cart_id);
CREATE INDEX IF NOT EXISTS idx_wishlists_user ON wishlists(user_id);
CREATE INDEX IF NOT EXISTS idx_wishlist_items_wishlist ON wishlist_items(wishlist_id);

-- -----------------------------------------------------------------------------
-- 5. Prescriptions Indexes
-- -----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS idx_prescriptions_user ON prescriptions(user_id);
CREATE INDEX IF NOT EXISTS idx_prescriptions_queue 
    ON prescriptions(created_at ASC) 
    WHERE status IN ('submitted', 'under_review');
CREATE INDEX IF NOT EXISTS idx_prescription_files_rx ON prescription_files(prescription_id);

-- -----------------------------------------------------------------------------
-- 6. Orders, Payments & Financial Indexes
-- -----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS idx_orders_user_history 
    ON orders(user_id, placed_at DESC);

CREATE INDEX IF NOT EXISTS idx_orders_status 
    ON orders(order_status, payment_status);

CREATE INDEX IF NOT EXISTS idx_order_items_order ON order_items(order_id);
CREATE INDEX IF NOT EXISTS idx_order_items_product ON order_items(product_id);
CREATE INDEX IF NOT EXISTS idx_order_status_history_order ON order_status_history(order_id, created_at ASC);

CREATE INDEX IF NOT EXISTS idx_payments_order ON payments(order_id);
CREATE INDEX IF NOT EXISTS idx_payments_trx_id ON payments(transaction_id) WHERE transaction_id IS NOT NULL;
CREATE INDEX IF NOT EXISTS idx_refunds_order ON refunds(order_id);

-- -----------------------------------------------------------------------------
-- 7. Promotions, Notifications & Audit Indexes
-- -----------------------------------------------------------------------------
CREATE INDEX IF NOT EXISTS idx_coupons_active_window 
    ON coupons(code, valid_from, valid_until) 
    WHERE is_active = TRUE;

CREATE INDEX IF NOT EXISTS idx_notifications_user_unread 
    ON notifications(user_id, created_at DESC) 
    WHERE is_read = FALSE;

CREATE INDEX IF NOT EXISTS idx_support_tickets_user ON support_tickets(user_id, status);
CREATE INDEX IF NOT EXISTS idx_support_messages_ticket ON support_messages(ticket_id, created_at ASC);

CREATE INDEX IF NOT EXISTS idx_audit_logs_entity ON audit_logs(entity_type, entity_id);
CREATE INDEX IF NOT EXISTS idx_audit_logs_created ON audit_logs(created_at DESC);

COMMIT;
