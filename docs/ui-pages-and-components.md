# Niramoy Health (নিরাময় হেলথ) — UI Pages & Components

## 1. Component Hierarchy & Layout

The user interface follows a predictable layout hierarchy designed for mobile, tablet, and desktop viewports:

```text
App Root (#app)
├── Global Navigation (Navbar)
│   ├── Top Bar (Hotline: 09612-NIRAMOY, Express Delivery Notice)
│   ├── Main Header (Brand Logo, Live Search Autocomplete, Prescription CTA, Cart Drawer Trigger, User Profile)
│   └── Category Ribbon (Quick category shortcuts & flash deals)
├── Main Content Outlet (<main id="main-content">)
│   └── [Dynamic Page Controller Rendered Based on Hash Route]
├── Reusable Floating Modals & Drawers
│   ├── Cart Slide-Over Drawer (CartDrawer)
│   ├── Prescription Upload Dialog (PrescriptionModal)
│   └── Notification Banner Stack (Toast)
└── Global Footer (Footer)
    ├── Brand mission, DGDA License verification, 24/7 pharmacist support
    ├── Category links, Terms of Service, Privacy Policy, Return Policy
    ├── Payment channel badges (bKash, Nagad, Rocket, Visa, Mastercard, COD)
    └── Copyright notice & emergency medical disclaimer
```

---

## 2. Page Specifications

### 2.1. Homepage (`pages/HomePage.js`)
- **Route**: `#/` or `#`
- **Features**:
  - **Hero Promotional Banner**: Bold banner with CTA to "Upload Prescription" (প্রেসক্রিপশন আপলোড করুন) and "Browse Medicines" with quick delivery guarantee (Dhaka 2-4 hours).
  - **Quick Action Cards**: 1-click access to *Prescription Upload*, *Doctor Consultations*, *Healthcare Devices*, and *Baby Care*.
  - **Top Health Categories Grid**: Visual category pills with icons (OTC Medicines, Chronic Care, Diabetes, Personal Care, Baby Care, Vitamins).
  - **Deals & Discount Carousel**: Dynamic cards loaded from `GET /api/v1/products/deals` with live calculation of savings in BDT (৳).
  - **Featured Medicines**: Popular authentic pharmaceuticals loaded from `GET /api/v1/products/featured`.
  - **Value Proposition Highlights**: *100% Genuine Medicines*, *Registered Pharmacist Supervision*, *Cold-Chain Storage Guaranteed*, *Cash on Delivery & Mobile Banking*.

### 2.2. Product Catalog & Search (`pages/CatalogPage.js`)
- **Route**: `#/catalog` (supports query params `?category=`, `?brand=`, `?q=`, `?page=`)
- **Features**:
  - **Faceted Sidebar Filter**:
    - Categories list with active item highlighting.
    - Brand filter (Square, Beximco, Incepta, Renata, Opsonin, Healthcare).
    - Dosage form filter (Tablet, Capsule, Syrup, Drop, Suspension).
    - Prescription requirement filter (OTC Only vs Prescription Drugs).
    - In-stock toggle.
  - **Live Search Bar & Sort Dropdown**: Sort by *Popularity*, *Price: Low to High*, *Price: High to Low*, *Discount Percentage*.
  - **Product Grid**: Responsive 1/2/3/4-column card layout displaying pricing, generic chemical formula, dosage, stock status, and add-to-cart buttons.
  - **Pagination Controls**: Server-side page navigation with dynamic total result counts.

### 2.3. Product Details Page (`pages/ProductDetailPage.js`)
- **Route**: `#/product/:slug`
- **Features**:
  - **Visual Presentation**: High-resolution medicine imagery with stock status badges and prescription warnings.
  - **Bilingual Medicine Identity**: English brand name, Bengali translation (`nameBn`), strength (e.g. `500 mg + 65 mg`), and pack size.
  - **Pricing Breakdown**: Standard unit price vs discounted price, pack/strip price calculations.
  - **Generic & Manufacturer Details**: Clickable generic formula linking to alternative brands with identical formulation.
  - **Prescription Notice**: Explicit warning banner for Schedule medicines (*"A valid doctor prescription is required to order this medicine"*).
  - **Quantity Selector & Direct Cart Addition**: Stepper input with instant visual feedback and slide-over cart trigger.

### 2.4. Prescription Upload & Management (`pages/PrescriptionPage.js` & `PrescriptionModal.js`)
- **Route**: `#/prescriptions`
- **Workflow**:
  - **File Attachment**: Supports Drag-and-drop or file browser for image (`.jpg`, `.jpeg`, `.png`) and document (`.pdf`) formats up to 10MB.
  - **Patient Details**: Name, contact phone number, delivery address, and optional clinical notes for the dispensing pharmacist.
  - **Multipart Upload**: Dispatches directly via `POST /api/v1/prescriptions/upload`.
  - **Prescription History Queue**: Displays previous submissions with real-time status badges:
    - `submitted` (Pending Pharmacist Verification)
    - `approved` (Order Created / Ready for Checkout)
    - `rejected` (Clarification Needed / Illegible Image)

### 2.5. Shopping Cart (`CartDrawer.js` & `pages/CartPage.js`)
- **Drawer Trigger**: Top navigation cart icon showing badge of total line items.
- **Features**:
  - **Line Item List**: Medicine thumbnail, pack size, unit price, quantity increment/decrement controls, item removal button.
  - **Prescription Advisory**: Highlights if any line item in the cart requires a doctor's prescription before final delivery.
  - **Cost Breakdown**:
    - Item Subtotal (৳)
    - Applied Savings / Promotional Discounts (৳)
    - Estimated Delivery Fee (Free over ৳500 in Dhaka)
    - Total Payable Amount (৳)
  - **Checkout Call-to-Action**: Direct button linking to `#/checkout`.

### 2.6. Checkout & Payment (`pages/CheckoutPage.js`)
- **Route**: `#/checkout` (Guarded: Authenticated Customers only)
- **Workflow**:
  1. **Delivery Address Selection**:
     - Pre-populated user addresses in Dhaka, Chittagong, Sylhet, etc.
     - Add new address modal with area, district, street address, and contact number.
  2. **Delivery Speed Options**:
     - Express Delivery (2–4 hours inside Dhaka city) — ৳80.
     - Regular Delivery (Next day inside Dhaka, 48 hours nationwide) — ৳40 (or Free for orders ≥ ৳500).
  3. **Payment Methods**:
     - Cash on Delivery (COD) — *Pay cash upon receiving and inspecting medicine*.
     - bKash / Nagad / Rocket Mobile Banking (Sandbox / simulated instant approval).
     - Visa / Mastercard / Debit Card.
  4. **Prescription Attachment**: Allows selecting an approved prescription for prescription-restricted items.
  5. **Order Review & Submission**: Atomic transaction creating the order and deducting inventory reserves.

### 2.7. Order Success & Tracking (`pages/OrderSuccessPage.js` & `pages/OrdersPage.js`)
- **Routes**: `#/order-success/:orderNumber` and `#/orders`
- **Features**:
  - **Order Confirmation**: Clean receipt with official Order Number (e.g. `ORD-20261011-XXXX`), payment method, delivery estimate, and breakdown.
  - **Timeline Progress Tracker**: Visual progress step indicator:
    - *Placed* ➔ *Pharmacist Verified* ➔ *Packed* ➔ *Out for Delivery* ➔ *Delivered*.
  - **Order History & Invoices**: List of past orders with re-order shortcuts and printable invoice receipts.

### 2.8. Staff Administrative Dashboard (`pages/AdminDashboardPage.js`)
- **Route**: `#/admin` (Guarded: Admin, Pharmacist, and Staff roles only)
- **Features**:
  - **Overview Metrics**: Total Revenue (৳), Pending Prescription Count, Active Orders, Low Stock Alerts.
  - **Prescription Queue Management**:
    - Image lightbox to inspect doctor's signature and medicine dosage.
    - Action buttons: **Approve** (with clinical dosage notes) or **Reject** (with reason).
  - **Order Processing Pipeline**: Change order statuses (`confirmed`, `processing`, `dispatched`, `delivered`).
  - **Inventory Insights**: Real-time stock counts reflecting customer purchases.

---

## 3. Reusable UI Components

### 3.1. `Navbar.js`
- **Responsive Behavior**:
  - Desktop: Full search bar, hotlines, category navigation, user dropdown menu, cart icon with item badge.
  - Mobile: Hamburger drawer, compact search icon, sticky bottom navigation for Cart, Upload Prescription, and Account.
- **Search Autocomplete**: 300ms debounced input listener querying `GET /api/v1/catalog/search?q=` with instant thumbnail previews.

### 3.2. `ProductCard.js`
- Displays medicine generic formula, brand name, Bengali name, pack size, price comparison (original vs discounted), stock indicator, and "+ Add" button.
- Handles instant addition to `cartStore` and dispatches success toast.

### 3.3. `Toast.js`
- Floating notification toaster rendering auto-dismissing notifications:
  - `Toast.success(message, duration)`
  - `Toast.error(message, duration)`
  - `Toast.info(message, duration)`
  - `Toast.warning(message, duration)`
