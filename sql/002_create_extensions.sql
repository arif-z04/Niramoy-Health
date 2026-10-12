-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 002: EXTENSIONS & COMMON FUNCTIONS
-- =============================================================================
-- Description:
-- Installs core PostgreSQL extensions required for UUID generation,
-- trigram fuzzy text search (Bengali and English), and case-insensitive text.
-- Creates utility functions used across the schema.
-- =============================================================================

-- Enable UUID functions (PostgreSQL 13+ includes gen_random_uuid() natively)
CREATE EXTENSION IF NOT EXISTS "uuid-ossp";
CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- Enable trigram matching for fuzzy search in product catalog & search suggestions
CREATE EXTENSION IF NOT EXISTS "pg_trgm";

-- Enable case-insensitive text support (used for emails, slugs)
CREATE EXTENSION IF NOT EXISTS "citext";

-- -----------------------------------------------------------------------------
-- Generic Trigger Function: Automatically update updated_at timestamp
-- -----------------------------------------------------------------------------
CREATE OR REPLACE FUNCTION trigger_set_updated_at()
RETURNS TRIGGER AS $$
BEGIN
  NEW.updated_at = CURRENT_TIMESTAMP;
  RETURN NEW;
END;
$$ LANGUAGE plpgsql;
