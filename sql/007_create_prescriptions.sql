-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 007: PRESCRIPTION UPLOAD & PHARMACIST REVIEW
-- =============================================================================
-- Domain: Prescription Verification (Rx Compliance)
-- Standards:
--   - Default status 'submitted' (Prescriptions NEVER auto-approved)
--   - Secure file storage references (keys/hashes, never unauthenticated public URLs)
--   - Pharmacist review audit records with decision notes and approved items
-- =============================================================================

BEGIN;

-- 1. Prescription Submissions
CREATE TABLE IF NOT EXISTS prescriptions (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    public_id UUID DEFAULT gen_random_uuid() NOT NULL UNIQUE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    patient_name VARCHAR(150) NOT NULL,
    patient_age INT CHECK (patient_age > 0 AND patient_age < 150),
    patient_gender VARCHAR(20) CHECK (patient_gender IN ('male', 'female', 'other')),
    doctor_name VARCHAR(150),
    hospital_clinic VARCHAR(200),
    prescription_date DATE,
    status VARCHAR(30) DEFAULT 'submitted' NOT NULL
        CHECK (status IN ('submitted', 'under_review', 'approved', 'partially_approved', 'rejected', 'expired')),
    rejection_reason TEXT,
    customer_notes TEXT,
    reviewed_by BIGINT REFERENCES users(id) ON DELETE SET NULL,
    reviewed_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 2. Prescription Files & Storage Metadata
CREATE TABLE IF NOT EXISTS prescription_files (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    prescription_id BIGINT NOT NULL REFERENCES prescriptions(id) ON DELETE CASCADE,
    storage_provider VARCHAR(50) DEFAULT 'local' NOT NULL
        CHECK (storage_provider IN ('local', 's3', 'azure_blob', 'gcs')),
    file_storage_key VARCHAR(500) NOT NULL,
    original_filename VARCHAR(255) NOT NULL,
    file_size_bytes BIGINT NOT NULL CHECK (file_size_bytes > 0),
    mime_type VARCHAR(100) NOT NULL
        CHECK (mime_type IN ('image/jpeg', 'image/png', 'image/webp', 'application/pdf')),
    is_secure_storage BOOLEAN DEFAULT TRUE NOT NULL,
    uploaded_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 3. Pharmacist Prescription Review Audit Log
CREATE TABLE IF NOT EXISTS prescription_review_records (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    prescription_id BIGINT NOT NULL REFERENCES prescriptions(id) ON DELETE CASCADE,
    pharmacist_id BIGINT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    previous_status VARCHAR(30) NOT NULL,
    new_status VARCHAR(30) NOT NULL,
    decision_notes TEXT NOT NULL,
    verified_medicines JSONB NOT NULL DEFAULT '[]'::jsonb,
    reviewed_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Attach updated_at triggers
CREATE TRIGGER trg_prescriptions_updated_at
    BEFORE UPDATE ON prescriptions
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
