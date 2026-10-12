-- =============================================================================
-- NIRAMOY HEALTH - SCRIPT 010: NOTIFICATIONS & CUSTOMER SUPPORT
-- =============================================================================
-- Domain: Customer Communication, Alerts & Helpdesk Ticketing
-- Standards:
--   - Bilingual notification messages (English and Bengali)
--   - Notification channels preferences per customer
--   - Helpdesk tickets with multi-tiered priority and order linkages
--   - Threaded customer-support agent conversation messages
-- =============================================================================

BEGIN;

-- 1. In-App Notifications
CREATE TABLE IF NOT EXISTS notifications (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE CASCADE,
    title VARCHAR(200) NOT NULL,
    title_bn VARCHAR(200),
    message TEXT NOT NULL,
    message_bn TEXT,
    notification_type VARCHAR(50) NOT NULL
        CHECK (notification_type IN (
            'order_update',
            'prescription_review',
            'payment_status',
            'promotion',
            'security',
            'system'
        )),
    reference_id VARCHAR(100),
    is_read BOOLEAN DEFAULT FALSE NOT NULL,
    read_at TIMESTAMPTZ,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 2. Customer Notification Channel Preferences
CREATE TABLE IF NOT EXISTS notification_preferences (
    user_id BIGINT PRIMARY KEY REFERENCES users(id) ON DELETE CASCADE,
    sms_order_updates BOOLEAN DEFAULT TRUE NOT NULL,
    sms_promotions BOOLEAN DEFAULT FALSE NOT NULL,
    email_order_updates BOOLEAN DEFAULT TRUE NOT NULL,
    email_promotions BOOLEAN DEFAULT TRUE NOT NULL,
    push_notifications BOOLEAN DEFAULT TRUE NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 3. Customer Support Tickets
CREATE TABLE IF NOT EXISTS support_tickets (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ticket_number VARCHAR(50) NOT NULL UNIQUE,
    user_id BIGINT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    order_id BIGINT REFERENCES orders(id) ON DELETE SET NULL,
    subject VARCHAR(200) NOT NULL,
    priority VARCHAR(20) DEFAULT 'medium' NOT NULL
        CHECK (priority IN ('low', 'medium', 'high', 'urgent')),
    status VARCHAR(30) DEFAULT 'open' NOT NULL
        CHECK (status IN ('open', 'in_progress', 'resolved', 'closed')),
    assigned_to BIGINT REFERENCES users(id) ON DELETE SET NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL,
    updated_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- 4. Customer Support Ticket Messages
CREATE TABLE IF NOT EXISTS support_messages (
    id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    ticket_id BIGINT NOT NULL REFERENCES support_tickets(id) ON DELETE CASCADE,
    sender_id BIGINT NOT NULL REFERENCES users(id) ON DELETE RESTRICT,
    is_staff_reply BOOLEAN DEFAULT FALSE NOT NULL,
    message TEXT NOT NULL,
    created_at TIMESTAMPTZ DEFAULT CURRENT_TIMESTAMP NOT NULL
);

-- Attach updated_at triggers
CREATE TRIGGER trg_notification_preferences_updated_at
    BEFORE UPDATE ON notification_preferences
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

CREATE TRIGGER trg_support_tickets_updated_at
    BEFORE UPDATE ON support_tickets
    FOR EACH ROW EXECUTE FUNCTION trigger_set_updated_at();

COMMIT;
