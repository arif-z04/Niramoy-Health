-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 009: DELIVERY ZONES, PROMOTIONS & MARKETING
-- =============================================================================
-- Domain: Delivery Geography, Promotional Campaigns & Dynamic Merchandising
-- Standards:
--   - Accurate division and district zone mapping
--   - Coupon usage quotas, expiry limits, and order eligibility rules
--   - Dynamic homepage sections and banners for marketing campaigns
--   - Newsletter subscription management
-- =============================================================================

BEGIN;

-- 1. Delivery Zones & Regional SLA Configuration
CREATE TABLE IF NOT EXISTS delivery_zones (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    division_id INT NOT NULL REFERENCES divisions(id) ON DELETE RESTRICT,
    district_id INT REFERENCES districts(id) ON DELETE RESTRICT,
    zone_name VARCHAR(150) NOT NULL,
    estimated_delivery_hours INT NOT NULL CHECK (estimated_delivery_hours > 0),
    base_delivery_fee NUMERIC(12,2) NOT NULL CHECK (base_delivery_fee >= 0),
    is_express_available BOOLEAN DEFAULT FALSE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_delivery_zone_name UNIQUE (zone_name)
);

-- 2. Promotional Coupons & Discount Vouchers
CREATE TABLE IF NOT EXISTS coupons (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code VARCHAR(50) NOT NULL UNIQUE,
    title VARCHAR(150) NOT NULL,
    title_bn VARCHAR(150),
    description TEXT,
    discount_type VARCHAR(20) NOT NULL CHECK (discount_type IN ('percentage', 'fixed_amount')),
    discount_value NUMERIC(12,2) NOT NULL CHECK (discount_value > 0),
    max_discount_amount NUMERIC(12,2) CHECK (max_discount_amount >= 0),
    min_order_amount NUMERIC(12,2) DEFAULT 0.00 NOT NULL CHECK (min_order_amount >= 0),
    usage_limit_per_user INT DEFAULT 1 NOT NULL CHECK (usage_limit_per_user > 0),
    total_usage_limit INT CHECK (total_usage_limit > 0),
    times_used INT DEFAULT 0 NOT NULL CHECK (times_used >= 0),
    valid_from TIMESTAMPTZ NOT NULL,
    valid_until TIMESTAMPTZ NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT chk_coupon_dates CHECK (valid_until > valid_from)
);

-- 3. Coupon Usage Audit Ledger
CREATE TABLE IF NOT EXISTS coupon_usages (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    coupon_id INT NOT NULL REFERENCES coupons(id) ON DELETE RESTRICT,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    order_id BIGINT NOT NULL REFERENCES orders(id) ON DELETE CASCADE,
    discount_applied NUMERIC(12,2) NOT NULL CHECK (discount_applied > 0),
    used_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_coupon_order UNIQUE (coupon_id, order_id)
);

-- 4. Promotional Marketing Campaigns
CREATE TABLE IF NOT EXISTS promotional_campaigns (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    slug VARCHAR(100) NOT NULL UNIQUE,
    title VARCHAR(150) NOT NULL,
    title_bn VARCHAR(150),
    description TEXT,
    banner_url TEXT,
    start_date TIMESTAMPTZ NOT NULL,
    end_date TIMESTAMPTZ NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT chk_campaign_dates CHECK (end_date > start_date)
);

-- 5. Promotional Banners & Carousel Slides
CREATE TABLE IF NOT EXISTS banners (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    campaign_id INT REFERENCES promotional_campaigns(id) ON DELETE SET NULL,
    title VARCHAR(150) NOT NULL,
    subtitle VARCHAR(255),
    image_url TEXT NOT NULL,
    target_url TEXT,
    display_order INT DEFAULT 0 NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 6. Dynamic Homepage Merchandising Sections
CREATE TABLE IF NOT EXISTS homepage_sections (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    section_type VARCHAR(50) NOT NULL
        CHECK (section_type IN (
            'featured_categories',
            'best_sellers',
            'flash_deals',
            'top_brands',
            'seasonal_care',
            'prescription_banner',
            'baby_care_special'
        )),
    title VARCHAR(150) NOT NULL,
    title_bn VARCHAR(150) NOT NULL,
    display_order INT DEFAULT 0 NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    configuration JSONB DEFAULT '{}'::jsonb,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 7. Newsletter Subscriptions
CREATE TABLE IF NOT EXISTS newsletter_subscribers (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    email CITEXT NOT NULL UNIQUE,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    subscribed_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Attach updated_at triggers
CREATE TRIGGER trg_coupons_updated_at
    BEFORE UPDATE ON coupons
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_promotional_campaigns_updated_at
    BEFORE UPDATE ON promotional_campaigns
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
