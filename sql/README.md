# Niramoy Health (নিরাময় হেলথ) - PostgreSQL Database Architecture

## 1. Overview & Directory Structure

This directory contains the authoritative PostgreSQL database architecture scripts for **Niramoy Health**, an enterprise-grade Bangladeshi healthcare e-commerce and digital pharmacy platform.

```
sql/
├── README.md                           # Schema architecture and operational manual
├── 001_create_database.sql             # Database creation with UTF-8 & locale settings
├── 002_create_extensions.sql           # Extensions (uuid, pgcrypto, pg_trgm, citext) & triggers
├── 003_create_users_and_roles.sql      # Users, RBAC, Profiles, BD Geography, Addresses, Sessions
├── 004_create_catalog.sql              # Categories, Brands, Manufacturers, Generics, Products, Reviews
├── 005_create_inventory.sql            # Warehouses, Stock levels, Medicine Batches, Inventory Tx Ledger
├── 006_create_carts_and_wishlists.sql  # Shopping carts (User/Guest) and Wishlists
├── 007_create_prescriptions.sql        # Prescription uploads, Secure files, Pharmacist Review Log
├── 008_create_orders_and_payments.sql  # Orders, Snapshots, Payments (bKash/Nagad/COD), Refunds
├── 009_create_delivery_and_promotions.sql # Delivery zones, Coupons, Campaigns, Banners, Newsletters
├── 010_create_notifications_and_support.sql # In-app notifications, Helpdesk tickets & messages
├── 011_create_audit_and_reporting.sql  # Audit logs, Configurations, and 6 Operational Views
├── 012_create_indexes_and_constraints.sql # Foreign keys, Trigram GIN, composite & partial indexes
├── 013_seed_reference_data.sql         # BD Divisions/Districts/Thanas, Roles, Delivery, Payment, Categories
├── 014_seed_demo_data.sql              # Authentic BD Pharma data (Square, Beximco, demo accounts, orders)
├── 015_useful_queries.sql              # High-performance catalog, inventory, and analytics queries
├── 016_database_health_checks.sql      # Automated schema verification & integrity diagnostics
└── rollback/                           # Reverse-dependency rollback scripts
    ├── README.md
    ├── rollback_all.sql
    ├── 011_rollback_audit_and_reporting.sql
    ├── 010_rollback_notifications_and_support.sql
    ├── 009_rollback_delivery_and_promotions.sql
    ├── 008_rollback_orders_and_payments.sql
    ├── 007_rollback_prescriptions.sql
    ├── 006_rollback_carts_and_wishlists.sql
    ├── 005_rollback_inventory.sql
    ├── 004_rollback_catalog.sql
    ├── 003_rollback_users_and_roles.sql
    └── 002_rollback_extensions.sql
```

---

## 2. Execution Order

### Standard Provisioning Workflow

To provision and initialize the development database from scratch:

```bash
# Step 1: Connect as PostgreSQL administrative user to initialize the database
psql -U postgres -h 127.0.0.1 -f sql/001_create_database.sql

# Step 2: Apply schema scripts sequentially in transaction blocks
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/002_create_extensions.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/003_create_users_and_roles.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/004_create_catalog.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/005_create_inventory.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/006_create_carts_and_wishlists.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/007_create_prescriptions.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/008_create_orders_and_payments.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/009_create_delivery_and_promotions.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/010_create_notifications_and_support.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/011_create_audit_and_reporting.sql
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/012_create_indexes_and_constraints.sql

# Step 3: Seed Reference Catalogs (Required for system operation)
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/013_seed_reference_data.sql

# Step 4: (Optional for Development/Testing) Seed Demonstration Catalog
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/014_seed_demo_data.sql

# Step 5: Verify Schema Integrity
psql -U niramoy_user -h 127.0.0.1 -d niramoy_health_dev -f sql/016_database_health_checks.sql
```

---

## 3. Database Architecture & Design Standards

### A. Key Architectural Decisions

1. **Surrogate Primary Keys + Public UUIDs**:
   - `id BIGINT GENERATED ALWAYS AS IDENTITY PRIMARY KEY` is used for high-performance internal joins and index clustering.
   - `public_id UUID DEFAULT gen_random_uuid() NOT NULL UNIQUE` is used on external customer-facing entities (`users`, `products`, `orders`, `prescriptions`, `payments`) to prevent ID enumeration attacks.
2. **Strict Currency Modeling**:
   - All financial amounts use `NUMERIC(12,2)` (e.g. `base_price`, `subtotal_amount`, `discount_amount`, `delivery_fee`, `total_amount`).
   - Floating-point representations (`FLOAT`, `REAL`, `DOUBLE PRECISION`) are prohibited.
   - Check constraints enforce non-negative prices and exact invoice mathematics (`total_amount = subtotal_amount - discount_amount + delivery_fee`).
3. **Bangladeshi Administrative Structure**:
   - Built-in hierarchical administrative geography: `divisions` (8 Divisions) -> `districts` (64 Districts) -> `upazilas` (Thanas/Upazilas).
   - Delivery addresses reference these validated geographic entities while supporting freeform street details.
4. **Pharmaceutical Compliance & Safety**:
   - Prescription medicines explicitly require doctor's prescriptions (`is_prescription_required = TRUE`).
   - Prescription upload status defaults to `submitted` and can NEVER be auto-approved by the system.
   - Traceable medicine batches (`medicine_batches`) track `batch_number`, `mfg_date`, and `expiry_date` with constraint `expiry_date > mfg_date`.
5. **Inventory Concurrency & Overselling Prevention**:
   - Two-phase reservation model (`quantity_on_hand` and `quantity_reserved`).
   - Database constraint: `CHECK (quantity_reserved <= quantity_on_hand)` prevents overselling at the database engine level.
   - Auditable double-entry stock movement ledger (`inventory_transactions`).
6. **Immutable Financial Snapshots**:
   - Catalog changes must never alter historical invoices. `order_items` stores an immutable snapshot of `product_name`, `product_name_bn`, `product_sku`, and `unit_price_snapshot` at the time of purchase.
   - `order_addresses` stores an immutable snapshot of recipient name, phone, and delivery address.
7. **Bilingual Support (English and Bengali / বাংলা)**:
   - All catalog and geographic entities provide paired `name` and `name_bn` columns.
   - `pg_trgm` GIN indexes enable instant fuzzy auto-complete and search in both languages.

---

## 4. Entity Relationship Summary

```
[Divisions] ──< [Districts] ──< [Upazilas]
     │               │
     └───< [Customer Addresses] >─── [Users] ──< [User Roles] >── [Roles]
                                       │    ├─── [User Profiles]
                                       │    ├─── [User Sessions]
                                       │    └─── [Notification Preferences]
                                       │
     ┌─────────────────────────────────┴─────────────────────────────────┐
     │                                                                   │
     ▼                                                                   ▼
[Prescriptions]                                                      [Orders]
     │                                                                   │
     ├──< [Prescription Files]                                           ├─── [Order Addresses] (Snapshot)
     ├──< [Prescription Review Records] (Pharmacist)                     ├──< [Order Items] (Price Snapshot)
     │                                                                   ├──< [Order Status History]
     └───────────────────────────────┐                                   ├──< [Payments] ──< [Payment Tx]
                                     ▼                                   └──< [Refunds]
                                  [Orders]                                       │
                                     ▲                                           ▼
                                     │                                  [Payment Methods]
                                     │                                  (bKash, Nagad, COD)
                                     │
 [Categories] ──< [Categories] (Hierarchy)
      ▲
      │
  [Products] ──< [Product Variants]
      ├──< [Product Images]
      ├──< [Product Attributes]
      ├──< [Product Reviews] >── [Users]
      ├──> [Generics]
      ├──> [Manufacturers]
      ├──> [Brands]
      ├──> [Dosage Forms]
      └──< [Inventory] >── [Warehouses]
             └──< [Medicine Batches]
```

---

## 5. Summary of Domain Tables

| Domain | Table Name | Purpose | Primary Key Strategy |
|---|---|---|---|
| **Users & Auth** | `users` | Customer & staff credentials | `BIGINT IDENTITY` + `UUID` |
| | `roles` | RBAC roles (`customer`, `pharmacist`, `admin`) | `INT IDENTITY` |
| | `user_roles` | Role assignments | Composite `(user_id, role_id)` |
| | `user_profiles` | Personal details & medical preferences | `BIGINT` (1:1 with `users`) |
| | `divisions` | Bangladesh 8 divisions | `INT IDENTITY` |
| | `districts` | Bangladesh 64 districts | `INT IDENTITY` |
| | `upazilas` | Upazilas / Thanas | `INT IDENTITY` |
| | `customer_addresses` | User delivery addresses | `BIGINT IDENTITY` |
| | `user_sessions` | JWT refresh tokens & sessions | `BIGINT IDENTITY` |
| **Catalog** | `categories` | Hierarchical product taxonomy | `INT IDENTITY` |
| | `brands` | Product brands | `INT IDENTITY` |
| | `manufacturers` | Pharma manufacturers (Square, Beximco) | `INT IDENTITY` |
| | `generics` | Active ingredients (Paracetamol, etc.) | `INT IDENTITY` |
| | `dosage_forms` | Forms (Tablet, Capsule, Syrup) | `INT IDENTITY` |
| | `products` | Master healthcare catalog | `BIGINT IDENTITY` + `UUID` |
| | `product_variants` | Product size / flavor variations | `BIGINT IDENTITY` |
| | `product_categories` | M:N secondary categories | Composite `(product_id, category_id)` |
| | `product_images` | Product gallery & primary photos | `BIGINT IDENTITY` |
| | `product_attributes` | Key-value specifications | `BIGINT IDENTITY` |
| | `product_reviews` | Customer ratings & moderation status | `BIGINT IDENTITY` |
| **Inventory** | `warehouses` | Distribution centers & hubs | `INT IDENTITY` |
| | `inventory` | Stock levels per warehouse | `BIGINT IDENTITY` |
| | `medicine_batches` | Expiry & batch traceability | `BIGINT IDENTITY` |
| | `inventory_transactions` | Audit ledger of all stock movement | `BIGINT IDENTITY` |
| **Carts & Wishlists** | `carts` | Registered & guest shopping carts | `BIGINT IDENTITY` + `UUID` |
| | `cart_items` | Products in cart (dynamic pricing) | `BIGINT IDENTITY` |
| | `wishlists` | Customer saved lists | `BIGINT IDENTITY` |
| | `wishlist_items` | Items in wishlist | `BIGINT IDENTITY` |
| **Prescriptions** | `prescriptions` | Rx uploads & clinical approval queue | `BIGINT IDENTITY` + `UUID` |
| | `prescription_files` | Secure file metadata | `BIGINT IDENTITY` |
| | `prescription_review_records`| Pharmacist audit logs | `BIGINT IDENTITY` |
| **Orders & Delivery** | `delivery_methods` | Standard, express, urgent shipping | `INT IDENTITY` |
| | `payment_methods` | COD, bKash, Nagad, Cards | `INT IDENTITY` |
| | `orders` | Master orders table | `BIGINT IDENTITY` + `UUID` |
| | `order_addresses` | Immutable delivery address snapshot | `BIGINT IDENTITY` |
| | `order_items` | Immutable item price snapshot | `BIGINT IDENTITY` |
| | `order_status_history`| Order status timeline | `BIGINT IDENTITY` |
| | `payments` | MFS / gateway payment records | `BIGINT IDENTITY` + `UUID` |
| | `payment_transactions`| Gateway attempt / callback logs | `BIGINT IDENTITY` |
| | `refunds` | Payment refund records | `BIGINT IDENTITY` |
| **Promotions** | `delivery_zones` | Regional delivery SLA & fees | `INT IDENTITY` |
| | `coupons` | Discount voucher codes | `INT IDENTITY` |
| | `coupon_usages` | Per-user usage logs | `BIGINT IDENTITY` |
| | `promotional_campaigns`| Marketing campaigns | `INT IDENTITY` |
| | `banners` | Homepage carousel banners | `INT IDENTITY` |
| | `homepage_sections`| Dynamic CMS sections | `INT IDENTITY` |
| | `newsletter_subscribers`| Email newsletter subscribers | `BIGINT IDENTITY` |
| **Support & Alerts** | `notifications` | In-app alerts | `BIGINT IDENTITY` |
| | `notification_preferences`| User channel settings | `BIGINT` (1:1 with `users`) |
| | `support_tickets` | Customer service tickets | `BIGINT IDENTITY` |
| | `support_messages` | Threaded customer support messages | `BIGINT IDENTITY` |
| **Audit & Config** | `audit_logs` | System security audit trail | `BIGINT IDENTITY` |
| | `administrative_actions`| Staff action log | `BIGINT IDENTITY` |
| | `app_configurations`| Key-value system settings | `INT IDENTITY` |

---

## 6. Operational Views Summary

- `vw_product_stock_status`: Live inventory levels with calculated `stock_status` (`IN_STOCK`, `LOW_STOCK`, `OUT_OF_STOCK`).
- `vw_low_stock_alerts`: Products that have breached warehouse reorder thresholds.
- `vw_prescription_review_queue`: Real-time queue for pharmacists sorted by wait duration.
- `vw_daily_sales_summary`: Daily gross merchandise value, net revenue, discounts, COD vs MFS share.
- `vw_customer_order_summary`: Customer lifetime order counts and total spend in BDT.
- `vw_top_selling_products`: Best-selling medicines and healthcare devices by revenue and quantity.
