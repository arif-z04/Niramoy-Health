# Project Proposal: Niramoy Health (নিরাময় হেলথ)

## Document Details
- Project Name: Niramoy Health
- Nature of Project: Digital Healthcare & E-Pharmacy Commerce Platform
- Target Region: Bangladesh (Nationwide delivery with regional hubs)
- Primary Stack: PostgreSQL 16+, Vite & Tailwind CSS, ASP.NET Core (.NET 10)
- Project Status: Frontend (Completed), Database (Completed), Backend API (In Progress / Incomplete)

---

## 1. Executive Summary

Niramoy Health is a specialized digital pharmacy and healthcare e-commerce platform engineered specifically for the healthcare retail market in Bangladesh. Unlike conventional online retail stores, a digital pharmacy requires strict regulatory adherence, rigorous prescription validation, temperature-controlled batch tracking, and localized delivery workflows.

The project addresses core challenges in the Bangladeshi pharmaceutical supply chain: the circulation of unverified medicines, lack of digital prescription management, stock inaccuracies during high-demand periods, and difficulties in addressing and dispatching outside major metropolitan areas.

### Current Implementation State
To ensure transparency regarding project deliverables, the system components currently stand at the following stages:
- **Database Architecture: COMPLETED.** The complete relational database schema comprising 54 tables, 6 operational reporting views, 655 constraints, seed data for all 8 divisions and 64 districts of Bangladesh, genuine pharmaceutical catalogs, and automated health checks has been finalized and verified.
- **Frontend Storefront & Admin Portal: COMPLETED.** The client interface built with Vite, Tailwind CSS, and vanilla JavaScript is fully built and audited across mobile, tablet, and desktop viewports. All storefront pages, cart drawers, checkout flows, prescription upload queues, and staff administrative views are implemented and functional with client-side state management.
- **Backend Services & REST API: IN PROGRESS (INCOMPLETE).** The core architectural framework (Clean Architecture layers, Domain entities, DTOs, service interfaces, controller endpoints, and DbContext mappings) has been scaffolded. However, actual production integrations—including live bKash/Nagad merchant payment gateways, local SMS gateway dispatch, cloud file storage with signed access URLs, background jobs for inventory expiry, and real-time WebSocket notifications—are incomplete and remain under active development.

---

## 2. Problem Statement and Market Requirements

### 2.1 Market Challenges in Bangladesh
1. **Counterfeit & Unregulated Medicines:** Consumers often face uncertainty regarding the authenticity of over-the-counter and prescription medicines purchased from local unregulated dispensaries.
2. **Dispensing Without Prescriptions:** Schedule drugs and antibiotics are frequently sold without doctor verification, posing public health risks. A digital platform must enforce a clinical gatekeeping step before restricted orders are confirmed.
3. **Inventory Discrepancies & Overselling:** Conventional retail inventory counters do not account for cart holding times. When multiple buyers purchase life-saving medicines simultaneously, overselling occurs if stock reservation is not handled at the database transaction level.
4. **Localization Gaps in Standard E-Commerce Platforms:** Generic e-commerce systems fail to accommodate Bangladesh's geographic administrative layout (Division -> District -> Upazila/Thana), popular Mobile Financial Services (bKash and Nagad), and bilingual user expectations (Bengali and English).

### 2.2 Project Objectives
- Build a compliant, reliable digital pharmacy platform serving customers across Bangladesh.
- Provide a dual-workflow purchasing system: direct checkout for OTC healthcare products, and pharmacist-validated checkout for prescription-only pharmaceuticals.
- Implement strict database-level concurrency controls to guarantee that no medicine can be oversold.
- Deliver a lightweight, mobile-first web storefront that operates smoothly on varying mobile network conditions (3G/4G/broadband) without requiring a heavy application download.
- Provide pharmacy staff and administrators with dedicated triage portals for reviewing prescription images, adjusting statuses, and tracking orders.

---

## 3. System Architecture & Technical Specifications

The system is structured as a modular three-tier architecture:

```
[ Frontend Client: Vite + Tailwind CSS ]
               │
               ▼ REST API / JSON (JWT)
[ Backend Services: ASP.NET Core (.NET 10) ] ── (In Progress)
               │
               ▼ Npgsql / EF Core
[ Database Engine: PostgreSQL 16+ ] ── (Completed)
```

### 3.1 Technology Stack

#### Database (Completed)
- **Engine:** PostgreSQL 16+
- **Extensions:** `uuid-ossp` (UUID generation), `pgcrypto` (cryptographic operations), `pg_trgm` (trigram fuzzy text search), `citext` (case-insensitive emails and handles)
- **Design Philosophy:** Surrogate identity primary keys (`BIGINT`) for internal relational performance, paired with public UUIDs (`UUID v4`) for external endpoints to prevent ID enumeration.
- **Data Integrity:** Strict numeric typing (`NUMERIC(12,2)`) for all financial columns, engine check constraints enforcing invoice mathematics, and double-entry inventory ledger records.

#### Frontend Client (Completed)
- **Tooling & Bundler:** Vite 8.3+
- **Styling & Layout:** Tailwind CSS 3.4+ with custom clinical theme tokens, fully responsive from 320px mobile displays to 1440px desktop screens.
- **Typography:** Dual font stack: `Hind Siliguri` for Bengali (বাংলা) and clean sans-serif typography for English.
- **Icons:** Lucide Icons.
- **Architecture:** Single Page Application (SPA) using a client-side hash router with role-based navigation guards (`Customer`, `Pharmacist`, `Admin`).
- **State Management:** Reactive pub/sub store modules for authentication session tokens, cart operations (including guest cart persistence), and toast feedback.

#### Backend API (In Progress / Incomplete)
- **Framework:** ASP.NET Core (.NET 10)
- **Architecture Pattern:** Clean Architecture (Domain, Application, Infrastructure, Presentation/Api)
- **Object-Relational Mapping:** Entity Framework Core (EF Core) with Npgsql PostgreSQL provider
- **Authentication & Security:** JWT Bearer authentication, BCrypt password hashing, role-based authorization policies, security headers middleware (CSP, HSTS, X-Frame-Options).

---

## 4. Current Implementation Status Breakdown

To provide an accurate technical assessment of where the project stands today, the following matrix outlines completed versus incomplete modules:

| Component | Sub-Module | Status | Details |
|---|---|---|---|
| **Database** | Core Schemas & Tables | **Completed** | 54 tables covering Auth, Catalog, Inventory, Cart, Rx, Orders, Delivery, Audit. |
| **Database** | Constraints & Health Checks | **Completed** | 655 constraints and indexes, 15 automated validation test suites. |
| **Database** | Operational Views | **Completed** | 6 reporting views for stock, Rx triage queue, sales summaries, and reorder alerts. |
| **Database** | Reference & Demo Data | **Completed** | 8 Divisions, 64 Districts, 495+ Thanas seeded; authentic Bangladeshi pharma catalog. |
| **Frontend** | Storefront & Search | **Completed** | Responsive homepage, category navigator, debounced live search with fuzzy matching. |
| **Frontend** | Catalog & Product Details | **Completed** | Faceted filtering (brand, dosage form, prescription requirement), alternative generics view. |
| **Frontend** | Prescription Upload UI | **Completed** | Multi-file drag-and-drop (.jpg, .png, .pdf), patient details modal, status history list. |
| **Frontend** | Shopping Cart & Checkout | **Completed** | Slide-out cart drawer, guest-to-user merge, Bangladeshi address selector, MFS options. |
| **Frontend** | Order Tracking & Invoices | **Completed** | Visual 5-stage status timeline, order detail breakdown, printable receipts. |
| **Frontend** | Staff / Admin Dashboard | **Completed** | Pharmacist prescription review lightbox with approve/reject actions, order dispatch controls. |
| **Backend** | Architectural Scaffolding | **Completed** | Domain entities, DTOs, service interfaces, controller routing, DbContext definitions. |
| **Backend** | Core Logic Services | **In Progress** | Basic catalog reading and cart services written; complex transaction handlers ongoing. |
| **Backend** | Live Payment Gateways | **Incomplete** | Live bKash/Nagad token renewal, webhook listeners, and cryptographic hash verification needed. |
| **Backend** | File Storage Security | **Incomplete** | Local storage active; needs migration to S3/Blob storage with private pre-signed URLs. |
| **Backend** | SMS & OTP Verification | **Incomplete** | Integration with local Bangladeshi telecom SMS gateways (e.g., SSL Wireless) pending. |
| **Backend** | Background Workers | **Incomplete** | Quartz/Hangfire jobs for releasing expired cart reservations and batch expiry tracking needed. |
| **Backend** | Real-time WebSockets | **Incomplete** | SignalR hub for live pharmacist queue updates and order tracking notifications pending. |

---

## 5. Detailed Feature List

### 5.1 Customer Storefront & Navigation
- **Homepage:**
  - Hero promotional banner with direct call-to-actions for uploading prescriptions and browsing essential medicines.
  - Quick action shortcut cards (Prescription Upload, Healthcare Devices, Chronic Care, Baby Care).
  - Deals and discount showcase calculating exact savings in Bangladeshi Taka (৳).
  - Featured medicines carousel loaded with genuine Bangladeshi pharmaceuticals.
  - Value proposition indicators highlighting cold-chain assurance, licensed pharmacists, and genuine sourcing.
- **Catalog & Smart Search:**
  - Debounced search input querying both English brand names and Bengali script.
  - Trigram fuzzy text matching allowing minor spelling variations to still return accurate products.
  - Faceted sidebar filters:
    - Therapeutic category hierarchy.
    - Pharmaceutical manufacturer (Square, Beximco, Incepta, Renata, Opsonin, Healthcare).
    - Dosage form (Tablet, Capsule, Syrup, Suspension, Ointment, Injection).
    - Prescription requirement flag (OTC vs Prescription Required).
    - In-stock availability toggle.
  - Sorting options by popularity, price ascending, price descending, and highest discount percentage.
- **Product Details:**
  - High-resolution packaging images and stock availability indicators.
  - Bilingual drug identity showing English name, Bengali name, chemical strength, and pack size.
  - Transparent pricing breakdown showing pack price versus unit strip price.
  - Active generic chemical formula with clickable links to alternative brand medicines containing the same formulation.
  - Prominent clinical warning banner on Schedule medicines indicating that a valid doctor's prescription is legally required before delivery.

### 5.2 Prescription Upload & Verification Workflow
- **Multi-File Upload:**
  - Secure client-side file upload supporting image formats (.jpg, .jpeg, .png) and document formats (.pdf) up to 10MB per file.
  - Drag-and-drop or device camera file attachment.
  - Capture of patient information: patient name, contact phone number, delivery address, and optional clinical notes for the dispensing pharmacist.
- **Prescription Lifecycle Tracking:**
  - Real-time status indicators visible to the customer:
    - `Submitted`: Received and waiting in the triage queue.
    - `Under Review`: Assigned to a licensed pharmacist.
    - `Approved`: Pharmacist verified dosage and authenticity; converted to an executable order.
    - `Rejected`: Clarification needed, image illegible, or medicine restricted. Pharmacist rejection reason displayed clearly to the customer.

### 5.3 Shopping Cart & Checkout
- **Cart Management:**
  - Slide-over Cart Drawer accessible from any screen without interrupting navigation.
  - Immediate line item controls: increment, decrement, and item removal with live subtotal calculation.
  - Guest Cart support: Unauthenticated visitors can add items using a persistent client guest identifier. Upon account login or registration, the guest cart merges automatically into the user's permanent account.
  - Prescription Advisory Banner: Notifies the buyer if any item in their cart requires prescription verification prior to order placement.
- **Localized Checkout:**
  - Integrated Bangladeshi administrative hierarchy selector (Division -> District -> Upazila/Thana) ensuring clean delivery addressing.
  - Delivery speed options:
    - Express Delivery (2–4 hours within Dhaka metropolitan area).
    - Standard Delivery (Next day inside Dhaka, 48–72 hours nationwide).
  - Payment method selection:
    - Cash on Delivery (COD) with inspection upon delivery.
    - bKash Mobile Wallet.
    - Nagad Mobile Wallet.
    - Credit/Debit Cards (Visa/Mastercard).

### 5.4 Order Management & Invoicing
- **Customer Order History:**
  - List of past and active orders with dates, item counts, total spent, and current state.
  - Visual 5-stage progress tracker:
    1. Placed
    2. Pharmacist Verified (for prescription items)
    3. Packed & Quality Checked
    4. Out for Delivery
    5. Delivered
  - Printable invoice receipt displaying official Order Number (e.g., `ORD-20261011-XXXX`), itemized unit prices, discount deductions, delivery fee, and payment method.

### 5.5 Staff & Pharmacist Administration
- **Pharmacist Triage Queue:**
  - Dedicated portal displaying pending prescription submissions sorted by submission time.
  - High-resolution image lightbox to examine doctor's signature, BMDC registration number, and prescribed dosage.
  - Pharmacist decision controls: Approve (with instructions or pack adjustments) or Reject (with structured customer-facing explanation).
- **Order Dispatch Pipeline:**
  - Pharmacy staff view to update order fulfillment states (Confirmed, Processing, Dispatched, Delivered, Cancelled).
- **Inventory Threshold Alerts:**
  - Live warning view highlighting products that have fallen below safety reorder stock levels.

---

## 6. Detailed Database Architecture (Completed Component)

The database layer serves as the single source of truth for the entire platform. Designed specifically for PostgreSQL 16+, it is fully implemented with complete rollback capabilities.

### 6.1 Schema Structure
The 54 relational tables are organized into distinct functional domains:
1. **Users, Roles & Security:** `users`, `roles`, `user_roles`, `user_profiles`, `user_sessions`.
2. **Bangladeshi Geography:** `divisions`, `districts`, `upazilas`, `customer_addresses`.
3. **Pharmaceutical Catalog:** `categories`, `brands`, `manufacturers`, `generics`, `dosage_forms`, `products`, `product_variants`, `product_categories`, `product_images`, `product_attributes`, `product_reviews`.
4. **Inventory & Batch Traceability:** `warehouses`, `inventory`, `medicine_batches`, `inventory_transactions`.
5. **Carts & Wishlists:** `carts`, `cart_items`, `wishlists`, `wishlist_items`.
6. **Prescriptions:** `prescriptions`, `prescription_files`, `prescription_review_records`.
7. **Orders & Invoicing:** `orders`, `order_addresses`, `order_items`, `order_status_history`, `payments`, `payment_transactions`, `refunds`.
8. **Delivery & Promotions:** `delivery_methods`, `payment_methods`, `delivery_zones`, `coupons`, `coupon_usages`, `promotional_campaigns`, `banners`, `homepage_sections`, `newsletter_subscribers`.
9. **Support & Notifications:** `notifications`, `notification_preferences`, `support_tickets`, `support_messages`.
10. **Audit & Governance:** `audit_logs`, `administrative_actions`, `app_configurations`.

### 6.2 Key Architectural Decisions in Database
- **Two-Phase Inventory Reservation:** To eliminate overselling during traffic peaks, the database maintains both `quantity_on_hand` and `quantity_reserved`. An engine-level check constraint `CHECK (quantity_reserved <= quantity_on_hand)` prevents any transaction from reserving more inventory than physically exists.
- **Immutable Financial Records:** Changes to product prices in the catalog never alter existing invoices. When an order is created, `order_items` stores a permanent snapshot of the unit price, SKU, product name, and Bengali name at the exact moment of purchase.
- **Batch Expiry Enforcement:** Medicine batches track batch numbers, manufacturing dates, and expiration dates. A check constraint `CHECK (expiry_date > mfg_date)` prevents invalid data entry.
- **Six Operational Analytics Views:**
  - `vw_product_stock_status`: Real-time stock status computation (IN_STOCK, LOW_STOCK, OUT_OF_STOCK).
  - `vw_low_stock_alerts`: Identifies items below safe warehouse reorder points.
  - `vw_prescription_review_queue`: Prioritized queue of prescriptions pending pharmacist triage.
  - `vw_daily_sales_summary`: Daily gross merchandise volume, net revenue, discounts, and payment method breakdown.
  - `vw_customer_order_summary`: Customer lifetime order counts and total expenditure.
  - `vw_top_selling_products`: Revenue and volume rankings for pharmaceuticals and healthcare items.

---

## 7. Detailed Frontend Storefront (Completed Component)

The frontend client is implemented as a lightweight, framework-independent Single Page Application (SPA) designed to load rapidly across mobile data connections.

### 7.1 Architecture & Performance
- **Zero Heavy Framework Overhead:** Built using modern vanilla JavaScript with ES modules and Vite for bundling. This avoids massive framework download footprints, resulting in sub-second load times on mobile devices.
- **Responsive Layout Hierarchy:**
  - Tested across 320px (compact smartphones), 375px (standard mobile), 768px (tablets), and 1280px+ (desktop monitors).
  - Desktop view presents a full search bar, utility links, and multi-column catalog grids.
  - Mobile view features an app-like sticky header, hamburger menu, and persistent bottom navigation bar (`BottomNav.js`) for rapid one-thumb access to Cart, Prescription Upload, and Account.
- **Modular Component Organization:**
  - Reusable layout components: `Header.js`, `Footer.js`, `BottomNav.js`.
  - Reusable display widgets: `ProductCard.js`, `CartDrawer.js`, `ToastContainer.js`.
  - Dedicated pages in `src/pages/` mapped through `router.js`.
- **Client State Management:**
  - `authStore.js`: Handles session tokens, login states, and user role claims.
  - `cartStore.js`: Tracks item counts, handles quantities, computes pricing totals, and syncs guest storage.
  - `toastStore.js`: Dispatches animated notification feedback for user actions.

---

## 8. Backend Services & Integration Gap Analysis (In Progress / Incomplete)

While the database and frontend interfaces are complete, the backend application layer remains under active development. The current status and technical gaps are detailed below.

### 8.1 Completed Scaffolding in Backend
- .NET 10 Solution structure created following Clean Architecture guidelines (`NiramoyHealth.Domain`, `NiramoyHealth.Application`, `NiramoyHealth.Infrastructure`, `NiramoyHealth.Api`).
- 54 domain entity classes mapped to the PostgreSQL relational tables.
- Standard DTO definitions for authentication, catalog querying, cart mutations, prescription submission, and checkout.
- Basic controller endpoints defined with OpenAPI/Swagger metadata.
- Initial Entity Framework Core `NiramoyDbContext` configured with Npgsql.

### 8.2 In-Progress and Incomplete Backend Work
1. **Mobile Financial Services (MFS) Gateway Integration:**
   - *Current State:* Simulated/mock payment endpoints exist for development flow testing.
   - *Work Remaining:* Complete official integration with bKash and Nagad merchant APIs. This requires implementing merchant credential handshakes, payment URL generation, instant payment notification (IPN) webhooks, transaction status query APIs, and cryptographic callback verification.
2. **Production File Storage & Prescription Security:**
   - *Current State:* Files uploaded via the API are saved to local server disk storage.
   - *Work Remaining:* Move file storage to private cloud object storage (AWS S3 or Azure Blob Storage). Implement short-lived, pre-signed URLs (expiring in 5–10 minutes) so that prescription documents are never accessible via static public URLs.
3. **SMS Gateway & Phone Authentication:**
   - *Current State:* Users register with email and password.
   - *Work Remaining:* Integrate local Bangladeshi telecom SMS gateways (e.g., SSL Wireless, Greenweb) to support OTP-based phone login, password resets, and automated SMS alerts when an order is dispatched or a prescription is approved.
4. **Automated Background Workers:**
   - *Current State:* No background scheduler is active.
   - *Work Remaining:* Implement background workers using Quartz.NET or Hangfire to:
     - Automatically release reserved inventory from carts that have been abandoned past a 30-minute threshold.
     - Scan medicine batches daily and generate warehouse alerts for batches nearing expiration within 90 days.
     - Aggregate operational tables into daily sales summary records.
5. **Real-time Notifications:**
   - *Current State:* State changes require page refresh or polling.
   - *Work Remaining:* Implement ASP.NET Core SignalR hubs to push live notifications to the pharmacist triage queue when a new prescription is uploaded, and push status changes to active customer order tracking screens.
6. **End-to-End API Integration & Concurrency Testing:**
   - *Current State:* Unit tests cover pricing calculations and validation rules.
   - *Work Remaining:* Conduct load and concurrency testing on the checkout pipeline under high transaction volumes to stress-test PostgreSQL row-level locks and verify reservation consistency.

---

## 9. Security, Compliance, and Data Protection

Healthcare platforms handle sensitive personal and medical data. The design incorporates strict safeguards:
- **Prescription Privacy:** Prescription uploads are stored in restricted directories and categorized as confidential medical records. Access is restricted to authenticated owners and verified pharmacy personnel.
- **Protection Against ID Harvesting:** Database tables utilize private auto-incrementing identity keys internally, while exposing randomized UUID v4 identifiers to the frontend. This prevents unauthorized users from guessing or scraping sequential order, user, or prescription records.
- **Strict Role-Based Access Control (RBAC):** Access to admin endpoints and prescription triage queues is guarded by JWT role claims (`Admin`, `Pharmacist`). Unauthorized requests are blocked with HTTP 403 Forbidden responses.
- **Cryptographic Credential Handling:** Customer passwords are encrypted using BCrypt with salt factor 12. JWT access tokens use HMAC-SHA256 signing with strict expiration lifetimes, paired with secure 64-byte refresh tokens.
- **Security Headers:** The API middleware enforces HTTP Strict Transport Security (HSTS), Content Security Policy (CSP), `X-Content-Type-Options: nosniff`, and `X-Frame-Options: DENY` to defend against clickjacking and cross-site scripting attacks.

---

## 10. Development Roadmap & Next Steps

To bring Niramoy Health from its current state to a fully operational production release, the following sequential phases are planned:

```
[ Phase 1: Completed ] ── Database Architecture (54 Tables, Views, Health Checks)
[ Phase 2: Completed ] ── Frontend Storefront & Admin Portal (Vite, Tailwind, Responsive SPA)
[ Phase 3: Active ]    ── Backend Core Completion (Transaction Logic, Auth & Cart Finalization)
[ Phase 4: Upcoming ]  ── External Integrations (bKash/Nagad MFS, SMS Gateway, Cloud Object Storage)
[ Phase 5: Upcoming ]  ── Background Workers & SignalR (Abandoned Cart Release, Expiry Scans, Push Alerts)
[ Phase 6: Final ]     ── End-to-End System Testing, Load Testing & Production Deployment
```

### Immediate Milestones (Backend Completion Focus)
1. **Milestone 1: Service Logic Finalization**
   - Finalize database transactions in `OrderService` and `CartService` using atomic PostgreSQL savepoints.
   - Verify guest-to-user cart merging logic across all edge cases.
2. **Milestone 2: External Service Connectors**
   - Implement bKash Checkout URL and IPN callback listeners.
   - Implement Nagad payment verification services.
   - Integrate SMS gateway provider for OTP verification and dispatch notifications.
3. **Milestone 3: Cloud Storage Migration**
   - Replace local file storage with S3-compatible cloud storage.
   - Enforce pre-signed temporary URLs for prescription view actions in the admin portal.
4. **Milestone 4: Background Workers & SignalR**
   - Configure Hangfire or Quartz.NET background job queues.
   - Implement automated release of expired cart inventory reservations.
   - Add SignalR hub for real-time prescription queue notifications.
5. **Milestone 5: Verification & Deployment**
   - Run full end-to-end integration test suites connecting the completed frontend to the backend.
   - Perform security audits and stress testing.
   - Deploy database to managed PostgreSQL instance and containerize backend via Docker.

---

## 11. Conclusion

Niramoy Health provides a well-engineered foundation tailored specifically to the digital healthcare and e-pharmacy requirements of Bangladesh. 

By completing the database schema and frontend storefront first, the project has established a clear, validated user experience and an exceptionally robust data foundation with 54 relational tables and strict pharmaceutical integrity constraints. 

With the frontend and database fully delivered, project execution is now concentrated on completing the backend API—specifically external payment gateways, SMS delivery, cloud storage security, and background workers. Upon finishing these backend services, Niramoy Health will be fully ready for deployment as an enterprise-grade digital pharmacy platform.
