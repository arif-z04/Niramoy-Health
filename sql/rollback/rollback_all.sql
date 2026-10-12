-- =============================================================================
-- NIRAMOY HEALTH - COMPLETE ROLLBACK SCRIPT (REVERSE ORDER)
-- =============================================================================
-- Drops all tables, views, triggers, and extensions in reverse dependency order.
-- =============================================================================

BEGIN;

-- 1. Drop Views
DROP VIEW IF EXISTS vw_top_selling_products CASCADE;
DROP VIEW IF EXISTS vw_customer_order_summary CASCADE;
DROP VIEW IF EXISTS vw_daily_sales_summary CASCADE;
DROP VIEW IF EXISTS vw_prescription_review_queue CASCADE;
DROP VIEW IF EXISTS vw_low_stock_alerts CASCADE;
DROP VIEW IF EXISTS vw_product_stock_status CASCADE;

-- 2. Drop Administrative & Audit Tables
DROP TABLE IF EXISTS app_configurations CASCADE;
DROP TABLE IF EXISTS administrative_actions CASCADE;
DROP TABLE IF EXISTS audit_logs CASCADE;

-- 3. Drop Notifications & Support
DROP TABLE IF EXISTS support_messages CASCADE;
DROP TABLE IF EXISTS support_tickets CASCADE;
DROP TABLE IF EXISTS notification_preferences CASCADE;
DROP TABLE IF EXISTS notifications CASCADE;

-- 4. Drop Marketing & Promotions
DROP TABLE IF EXISTS newsletter_subscribers CASCADE;
DROP TABLE IF EXISTS homepage_sections CASCADE;
DROP TABLE IF EXISTS banners CASCADE;
DROP TABLE IF EXISTS promotional_campaigns CASCADE;
DROP TABLE IF EXISTS coupon_usages CASCADE;
DROP TABLE IF EXISTS coupons CASCADE;
DROP TABLE IF EXISTS delivery_zones CASCADE;

-- 5. Drop Orders & Payments
DROP TABLE IF EXISTS refunds CASCADE;
DROP TABLE IF EXISTS payment_transactions CASCADE;
DROP TABLE IF EXISTS payments CASCADE;
DROP TABLE IF EXISTS order_status_history CASCADE;
DROP TABLE IF EXISTS order_items CASCADE;
DROP TABLE IF EXISTS order_addresses CASCADE;
DROP TABLE IF EXISTS orders CASCADE;
DROP TABLE IF EXISTS payment_methods CASCADE;
DROP TABLE IF EXISTS delivery_methods CASCADE;

-- 6. Drop Prescriptions
DROP TABLE IF EXISTS prescription_review_records CASCADE;
DROP TABLE IF EXISTS prescription_files CASCADE;
DROP TABLE IF EXISTS prescriptions CASCADE;

-- 7. Drop Carts & Wishlists
DROP TABLE IF EXISTS wishlist_items CASCADE;
DROP TABLE IF EXISTS wishlists CASCADE;
DROP TABLE IF EXISTS cart_items CASCADE;
DROP TABLE IF EXISTS carts CASCADE;

-- 8. Drop Inventory
DROP TABLE IF EXISTS inventory_transactions CASCADE;
DROP TABLE IF EXISTS medicine_batches CASCADE;
DROP TABLE IF EXISTS inventory CASCADE;
DROP TABLE IF EXISTS warehouses CASCADE;

-- 9. Drop Product Catalog
DROP TABLE IF EXISTS product_reviews CASCADE;
DROP TABLE IF EXISTS product_attributes CASCADE;
DROP TABLE IF EXISTS product_images CASCADE;
DROP TABLE IF EXISTS product_categories CASCADE;
DROP TABLE IF EXISTS product_variants CASCADE;
DROP TABLE IF EXISTS products CASCADE;
DROP TABLE IF EXISTS dosage_forms CASCADE;
DROP TABLE IF EXISTS generics CASCADE;
DROP TABLE IF EXISTS manufacturers CASCADE;
DROP TABLE IF EXISTS brands CASCADE;
DROP TABLE IF EXISTS categories CASCADE;

-- 10. Drop Users & Auth
DROP TABLE IF EXISTS user_sessions CASCADE;
DROP TABLE IF EXISTS customer_addresses CASCADE;
DROP TABLE IF EXISTS user_profiles CASCADE;
DROP TABLE IF EXISTS user_roles CASCADE;
DROP TABLE IF EXISTS users CASCADE;
DROP TABLE IF EXISTS roles CASCADE;
DROP TABLE IF EXISTS upazilas CASCADE;
DROP TABLE IF EXISTS districts CASCADE;
DROP TABLE IF EXISTS divisions CASCADE;

-- 11. Drop Trigger Functions
DROP FUNCTION IF EXISTS trigger_set_updated_at CASCADE;

COMMIT;
