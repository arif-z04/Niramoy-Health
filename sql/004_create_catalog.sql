-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 004: PRODUCT CATALOG & PHARMACEUTICAL DOMAIN
-- =============================================================================
-- Domain: Healthcare & Pharmaceutical Catalog
-- Standards:
--   - Hierarchical categories (parent_id)
--   - Pharmaceutical precision: generics, dosage forms, strength, pack size
--   - Explicit prescription requirement flag (is_prescription_required)
--   - Exact monetary pricing with NUMERIC(12,2) and discount validations
--   - Bilingual representation (English and Bengali / বাংলা)
--   - Rating & review moderation
-- =============================================================================

BEGIN;

-- 1. Product Categories (Hierarchical: Parent -> Subcategories)
CREATE TABLE IF NOT EXISTS categories (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    parent_id INT REFERENCES categories(id) ON DELETE RESTRICT,
    slug VARCHAR(180) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    name_bn VARCHAR(150) NOT NULL,
    description TEXT,
    icon_url TEXT,
    banner_url TEXT,
    display_order INT DEFAULT 0 NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 2. Brands
CREATE TABLE IF NOT EXISTS brands (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    slug VARCHAR(180) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL UNIQUE,
    name_bn VARCHAR(150),
    logo_url TEXT,
    description TEXT,
    website_url TEXT,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 3. Pharmaceutical Manufacturers (e.g., Square, Beximco, Incepta, Renata)
CREATE TABLE IF NOT EXISTS manufacturers (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    slug VARCHAR(220) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL UNIQUE,
    name_bn VARCHAR(200),
    country VARCHAR(100) DEFAULT 'Bangladesh' NOT NULL,
    license_number VARCHAR(100),
    website_url TEXT,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 4. Generic Medicines / Active Pharmaceutical Ingredients (APIs)
CREATE TABLE IF NOT EXISTS generics (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    slug VARCHAR(220) NOT NULL UNIQUE,
    name VARCHAR(200) NOT NULL UNIQUE,
    name_bn VARCHAR(200),
    description TEXT,
    therapeutic_class VARCHAR(150),
    indication TEXT,
    dosage_guidelines TEXT,
    side_effects TEXT,
    precautions TEXT,
    pregnancy_category VARCHAR(10),
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 5. Dosage Forms (Tablet, Capsule, Syrup, Suspension, Injection, Ointment, etc.)
CREATE TABLE IF NOT EXISTS dosage_forms (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(100) NOT NULL UNIQUE,
    name_bn VARCHAR(100),
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 6. Master Products Table
CREATE TABLE IF NOT EXISTS products (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id UUID DEFAULT gen_random_uuid() NOT NULL UNIQUE,
    sku VARCHAR(100) NOT NULL UNIQUE,
    slug VARCHAR(255) NOT NULL UNIQUE,
    name VARCHAR(255) NOT NULL,
    name_bn VARCHAR(255) NOT NULL,
    category_id INT NOT NULL REFERENCES categories(id) ON DELETE RESTRICT,
    brand_id INT REFERENCES brands(id) ON DELETE SET NULL,
    manufacturer_id INT REFERENCES manufacturers(id) ON DELETE RESTRICT,
    generic_id INT REFERENCES generics(id) ON DELETE SET NULL,
    dosage_form_id INT REFERENCES dosage_forms(id) ON DELETE SET NULL,
    strength VARCHAR(100),
    pack_size VARCHAR(100),
    unit_of_measure VARCHAR(50) DEFAULT 'Piece' NOT NULL,
    units_per_pack INT DEFAULT 1 NOT NULL CHECK (units_per_pack > 0),
    base_price NUMERIC(12,2) NOT NULL CHECK (base_price >= 0),
    discount_price NUMERIC(12,2) CHECK (discount_price IS NULL OR (discount_price >= 0 AND discount_price <= base_price)),
    is_prescription_required BOOLEAN DEFAULT FALSE NOT NULL,
    is_otc BOOLEAN DEFAULT TRUE NOT NULL,
    description TEXT,
    description_bn TEXT,
    usage_instructions TEXT,
    storage_conditions TEXT,
    is_featured BOOLEAN DEFAULT FALSE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    badge VARCHAR(50),
    avg_rating NUMERIC(3,2) DEFAULT 0.00 NOT NULL CHECK (avg_rating >= 0 AND avg_rating <= 5.00),
    review_count INT DEFAULT 0 NOT NULL CHECK (review_count >= 0),
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    deleted_at TIMESTAMPTZ
);

-- 7. Product Variants (Pack sizes, volumes, shades, diaper sizes)
CREATE TABLE IF NOT EXISTS product_variants (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    sku VARCHAR(100) NOT NULL UNIQUE,
    name VARCHAR(150) NOT NULL,
    name_bn VARCHAR(150),
    variant_attributes JSONB NOT NULL DEFAULT '{}'::jsonb,
    base_price NUMERIC(12,2) NOT NULL CHECK (base_price >= 0),
    discount_price NUMERIC(12,2) CHECK (discount_price IS NULL OR (discount_price >= 0 AND discount_price <= base_price)),
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 8. Multi-Category Mapping (Product -> Categories M:N)
CREATE TABLE IF NOT EXISTS product_categories (
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    category_id INT NOT NULL REFERENCES categories(id) ON DELETE CASCADE,
    is_primary BOOLEAN DEFAULT FALSE NOT NULL,
    PRIMARY KEY (product_id, category_id)
);

-- 9. Product Images
CREATE TABLE IF NOT EXISTS product_images (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    image_url TEXT NOT NULL,
    alt_text VARCHAR(255),
    display_order INT DEFAULT 0 NOT NULL,
    is_primary BOOLEAN DEFAULT FALSE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 10. Product Dynamic Attributes / Specifications
CREATE TABLE IF NOT EXISTS product_attributes (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    attribute_key VARCHAR(100) NOT NULL,
    attribute_value TEXT NOT NULL,
    attribute_value_bn TEXT,
    display_order INT DEFAULT 0 NOT NULL,
    CONSTRAINT uq_product_attribute_key UNIQUE (product_id, attribute_key)
);

-- 11. Customer Reviews and Ratings
CREATE TABLE IF NOT EXISTS product_reviews (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    rating INT NOT NULL CHECK (rating >= 1 AND rating <= 5),
    title VARCHAR(200),
    comment TEXT,
    is_verified_purchase BOOLEAN DEFAULT FALSE NOT NULL,
    status VARCHAR(20) DEFAULT 'pending' NOT NULL CHECK (status IN ('pending', 'approved', 'rejected')),
    moderated_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    moderated_at TIMESTAMPTZ,
    moderation_note TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_product_user_review UNIQUE (product_id, user_id)
);

-- Attach updated_at triggers
CREATE TRIGGER trg_categories_updated_at
    BEFORE UPDATE ON categories
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_brands_updated_at
    BEFORE UPDATE ON brands
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_manufacturers_updated_at
    BEFORE UPDATE ON manufacturers
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_generics_updated_at
    BEFORE UPDATE ON generics
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_products_updated_at
    BEFORE UPDATE ON products
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_product_variants_updated_at
    BEFORE UPDATE ON product_variants
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_product_reviews_updated_at
    BEFORE UPDATE ON product_reviews
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
