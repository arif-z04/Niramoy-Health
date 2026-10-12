-- =============================================================================
-- NIRAMOY HEALTH (নিরাময় হেলথ) - SCRIPT 001: CREATE DATABASE & ENCODING
-- =============================================================================
-- Description:
-- Creates the production, development, and test PostgreSQL databases for Niramoy Health.
-- Ensures UTF-8 encoding for full bilingual support (English and Bengali / বাংলা).
--
-- Execution:
-- Run using psql as superuser:
--   psql -U postgres -h 127.0.0.1 -f sql/001_create_database.sql
-- =============================================================================

-- 1. Create development database if not exists
SELECT 'CREATE DATABASE niramoy_health_dev WITH ENCODING = ''UTF8'' LC_COLLATE = ''en_US.UTF-8'' LC_CTYPE = ''en_US.UTF-8'' TEMPLATE = template1'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'niramoy_health_dev')\gexec

-- 2. Create testing database if not exists
SELECT 'CREATE DATABASE niramoy_health_test WITH ENCODING = ''UTF8'' LC_COLLATE = ''en_US.UTF-8'' LC_CTYPE = ''en_US.UTF-8'' TEMPLATE = template1'
WHERE NOT EXISTS (SELECT FROM pg_database WHERE datname = 'niramoy_health_test')\gexec

-- Grant privileges to application user if role exists
DO $$
BEGIN
  IF EXISTS (SELECT FROM pg_roles WHERE rolname = 'niramoy_user') THEN
    GRANT ALL PRIVILEGES ON DATABASE niramoy_health_dev TO niramoy_user;
    GRANT ALL PRIVILEGES ON DATABASE niramoy_health_test TO niramoy_user;
  END IF;
END
$$;
