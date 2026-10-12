-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 006: SHOPPING CARTS & WISHLISTS
-- =============================================================================
-- Domain: Shopping Cart & Customer Wishlists
-- Standards:
--   - Dual support for authenticated users and anonymous guest shoppers
--   - Exactly one owner per cart: user_id OR guest_token
--   - Strict positive quantities (quantity > 0)
--   - Prices are NEVER stored in carts (prices dynamically evaluated from catalog)
-- =============================================================================

BEGIN;

-- 1. Shopping Carts (Supports registered user or guest checkout sessions)
CREATE TABLE IF NOT EXISTS carts (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id UUID DEFAULT gen_random_uuid() NOT NULL UNIQUE,
    user_id BIGINT REFERENCES users(id) ON DELETE CASCADE,
    guest_token UUID UNIQUE,
    status VARCHAR(30) DEFAULT 'active' NOT NULL
        CHECK (status IN ('active', 'converted', 'abandoned')),
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT chk_cart_owner CHECK (
        (user_id IS NOT NULL AND guest_token IS NULL) OR
        (user_id IS NULL AND guest_token IS NOT NULL)
    )
);

-- 2. Cart Items
CREATE TABLE IF NOT EXISTS cart_items (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    cart_id BIGINT NOT NULL REFERENCES carts(id) ON DELETE CASCADE,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    quantity INT NOT NULL CHECK (quantity > 0),
    added_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_cart_item UNIQUE (cart_id, product_id)
);

-- 3. Customer Wishlists
CREATE TABLE IF NOT EXISTS wishlists (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    name VARCHAR(100) DEFAULT 'My Wishlist' NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_user_wishlist_name UNIQUE (user_id, name)
);

-- 4. Wishlist Items
CREATE TABLE IF NOT EXISTS wishlist_items (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    wishlist_id BIGINT NOT NULL REFERENCES wishlists(id) ON DELETE CASCADE,
    product_id BIGINT NOT NULL REFERENCES products(id) ON DELETE CASCADE,
    added_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_wishlist_item UNIQUE (wishlist_id, product_id)
);

-- Attach updated_at triggers
CREATE TRIGGER trg_carts_updated_at
    BEFORE UPDATE ON carts
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_cart_items_updated_at
    BEFORE UPDATE ON cart_items
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_wishlists_updated_at
    BEFORE UPDATE ON wishlists
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
