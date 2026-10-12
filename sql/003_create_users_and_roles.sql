-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 003: USERS, ROLES, ADDRESSES & AUTHENTICATION
-- =============================================================================
-- Domain: Users & Access Control
-- Standards:
--   - Identity primary keys with UUID public identifiers
--   - Normalized phone numbers (+8801XXXXXXXXX) and citext for emails
--   - Clear account lifecycle states: active, pending, suspended, deactivated
--   - Soft deletes (deleted_at) for GDPR / data retention compliance
--   - Bangladesh administrative geography (Divisions -> Districts -> Upazilas)
-- =============================================================================

BEGIN;

-- 1. Administrative Divisions of Bangladesh (8 Divisions)
CREATE TABLE IF NOT EXISTS divisions (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    code VARCHAR(20) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL UNIQUE,
    name_bn VARCHAR(100) NOT NULL UNIQUE,
    display_order INT DEFAULT 0 NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 2. Districts of Bangladesh (64 Districts)
CREATE TABLE IF NOT EXISTS districts (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    division_id INT NOT NULL REFERENCES divisions(id) ON DELETE RESTRICT,
    code VARCHAR(20) NOT NULL UNIQUE,
    name VARCHAR(100) NOT NULL,
    name_bn VARCHAR(100) NOT NULL,
    display_order INT DEFAULT 0 NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_district_division_name UNIQUE (division_id, name)
);

-- 3. Upazilas / Thanas of Bangladesh
CREATE TABLE IF NOT EXISTS upazilas (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    district_id INT NOT NULL REFERENCES districts(id) ON DELETE RESTRICT,
    name VARCHAR(100) NOT NULL,
    name_bn VARCHAR(100) NOT NULL,
    post_code VARCHAR(10),
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    CONSTRAINT uq_upazilas_district_name UNIQUE (district_id, name)
);

-- 4. User Roles (RBAC)
CREATE TABLE IF NOT EXISTS roles (
    id INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    name VARCHAR(50) NOT NULL UNIQUE,
    display_name VARCHAR(100) NOT NULL,
    description TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 5. Primary Users Table
CREATE TABLE IF NOT EXISTS users (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id UUID DEFAULT gen_random_uuid() NOT NULL UNIQUE,
    phone_number VARCHAR(20) NOT NULL UNIQUE,
    email CITEXT UNIQUE,
    password_hash VARCHAR(255),
    full_name VARCHAR(150) NOT NULL,
    is_phone_verified BOOLEAN DEFAULT FALSE NOT NULL,
    is_email_verified BOOLEAN DEFAULT FALSE NOT NULL,
    status VARCHAR(30) DEFAULT 'active' NOT NULL
        CHECK (status IN ('active', 'inactive', 'suspended', 'deactivated', 'pending_verification')),
    failed_login_attempts INT DEFAULT 0 NOT NULL CHECK (failed_login_attempts >= 0),
    lockout_until TIMESTAMPTZ,
    last_login_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    deleted_at TIMESTAMPTZ
);

-- 6. User Role Assignments (M:N)
CREATE TABLE IF NOT EXISTS user_roles (
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    role_id INT NOT NULL REFERENCES roles(id) ON DELETE RESTRICT,
    assigned_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    assigned_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    PRIMARY KEY (user_id, role_id)
);

-- 7. User Profiles (1:1 with Users)
CREATE TABLE IF NOT EXISTS user_profiles (
    user_id BIGINT PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    date_of_birth DATE,
    gender VARCHAR(20) CHECK (gender IN ('male', 'female', 'other')),
    blood_group VARCHAR(10) CHECK (blood_group IN ('A+', 'A-', 'B+', 'B-', 'AB+', 'AB-', 'O+', 'O-')),
    emergency_contact_name VARCHAR(150),
    emergency_contact_phone VARCHAR(20),
    avatar_url TEXT,
    preferred_language VARCHAR(5) DEFAULT 'bn' NOT NULL CHECK (preferred_language IN ('bn', 'en')),
    marketing_opt_in BOOLEAN DEFAULT TRUE NOT NULL,
    medical_notes TEXT,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 8. Customer Delivery Addresses (1:N with Users)
CREATE TABLE IF NOT EXISTS customer_addresses (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    address_label VARCHAR(50) DEFAULT 'Home' NOT NULL CHECK (address_label IN ('Home', 'Office', 'Other')),
    recipient_name VARCHAR(150) NOT NULL,
    recipient_phone VARCHAR(20) NOT NULL,
    division_id INT NOT NULL REFERENCES divisions(id) ON DELETE RESTRICT,
    district_id INT NOT NULL REFERENCES districts(id) ON DELETE RESTRICT,
    upazila_id INT REFERENCES upazilas(id) ON DELETE SET NULL,
    area_details TEXT NOT NULL,
    postal_code VARCHAR(10),
    delivery_instructions TEXT,
    latitude NUMERIC(9,6),
    longitude NUMERIC(9,6),
    is_default BOOLEAN DEFAULT FALSE NOT NULL,
    is_active BOOLEAN DEFAULT TRUE NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 9. User Sessions and Refresh Tokens
CREATE TABLE IF NOT EXISTS user_sessions (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    token_hash VARCHAR(255) NOT NULL UNIQUE,
    device_info VARCHAR(255),
    ip_address VARCHAR(45),
    user_agent TEXT,
    is_revoked BOOLEAN DEFAULT FALSE NOT NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Attach updated_at triggers
CREATE TRIGGER trg_users_updated_at
    BEFORE UPDATE ON users
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_user_profiles_updated_at
    BEFORE UPDATE ON user_profiles
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_customer_addresses_updated_at
    BEFORE UPDATE ON customer_addresses
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
