# Niramoy Health (নিরাময় হেলথ) — Responsive & Cross-Device Testing Guide

## 1. Overview & Test Methodology

Responsive and cross-device testing for **Niramoy Health** validates that users on mobile phones, tablets, and desktop workstations experience an identical level of performance, readability, and functional convenience.

The verification process combines four distinct automated and manual validation strategies:

1. **Automated Viewport Dimension Matrix**: Systematic evaluation of `document.documentElement.scrollWidth` versus `window.innerWidth` across 10 distinct breakpoints, verifying zero horizontal overflow without relying on blanket `overflow-x: hidden` band-aids on `body`.
2. **Automated Playwright Cross-Device E2E Suite**: End-to-end user journey tests running against real headless Google Chrome (`google-chrome-stable`), testing responsive drawer interactions, touch tap targets, body scroll locks, and mobile bottom navigation bars.
3. **Automated Visual Regression Screenshot Artifacts**: Full visual captures saved in `frontend/tests/artifacts/screenshots/` across 4 core viewport classes (`320px`, `375px`, `768px`, and `1280px`).
4. **Form Ergonomics & iOS Safari Zoom Audit**: Automated scanning of all `<input>`, `<select>`, and `<textarea>` font sizes on mobile viewports to ensure font sizes are `>= 16px` (1rem), preventing disruptive automatic zoom on iOS devices.

---

## 2. Tested Device Breakpoints

The test matrix covers 10 representative viewport widths:

| # | Breakpoint Name | Dimensions (WxH) | Target Category | Example Device |
|---|---|---|---|---|
| 1 | `small-mobile-320` | 320 x 568 px | Small smartphone | iPhone SE (1st gen) |
| 2 | `standard-mobile-360` | 360 x 640 px | Compact Android phone | Samsung Galaxy S8 / A10 |
| 3 | `mobile-375` | 375 x 667 px | Standard smartphone | iPhone SE (2nd/3rd gen), iPhone 8 |
| 4 | `large-mobile-430` | 430 x 932 px | Large flagship smartphone | iPhone 14/15/16 Pro Max |
| 5 | `small-tablet-600` | 600 x 960 px | Mini tablet / Foldable | Google Nexus 7, Galaxy Z Fold |
| 6 | `tablet-768` | 768 x 1024 px | Portrait tablet | Apple iPad (10.2"), iPad Mini |
| 7 | `small-laptop-1024` | 1024 x 768 px | Landscape tablet / Netbook | iPad Pro (landscape), Surface Go |
| 8 | `laptop-1280` | 1280 x 800 px | Compact laptop display | MacBook Air (13"), Chromebook |
| 9 | `desktop-1440` | 1440 x 900 px | Standard workstation monitor | MacBook Pro (15"/16"), 24" Monitor |
| 10 | `large-desktop-1920` | 1920 x 1080 px | Full HD widescreen monitor | 1080p Desktop Display |

---

## 3. Test Suites & Commands

### 3.1. Running the Responsive Test Suite
To run the automated responsive Playwright suite (`16 tests`):
```bash
cd frontend
npx playwright test tests/e2e/responsive.spec.js
```
**Coverage**:
- Tests zero horizontal overflow across all 10 viewports on Home, Catalog, and Product Details.
- Tests mobile drawer opening, closing via button/backdrop/Escape, and background body scroll locking.
- Tests mobile bottom navigation bar visibility on phone viewports and hiding on desktop.
- Tests catalog mobile filter drawer modal behavior.
- Tests sticky mobile bottom purchase bar on product details.
- Verifies input font sizes are `>= 16px` on mobile viewports.
- Generates 18 screenshot artifacts in `tests/artifacts/screenshots/`.

### 3.2. Running the Multi-Route Audit Script
To run the standalone multi-route matrix audit script across all 10 viewports:
```bash
cd frontend
node tests/audit-runner.mjs
```
**Coverage**:
- Tests 7 routes across 10 viewports (70 combinations total).
- Verifies document scroll bounds and identifies any overflowing elements in the DOM.
- Flags any small input font sizes that risk triggering iOS auto-zoom.

### 3.3. Running All E2E Integration & Responsive Tests
To run the entire frontend E2E test suite (`22 tests`):
```bash
cd frontend
npx playwright test
```
**Summary**:
- `tests/e2e/storefront.spec.js` (3 tests)
- `tests/e2e/cart-and-checkout.spec.js` (1 test)
- `tests/e2e/prescriptions-and-admin.spec.js` (2 tests)
- `tests/e2e/responsive.spec.js` (16 tests)
- **Status**: **22 passed (100% PASS)**

### 3.4. Running the Backend Test Suite
To verify that backend unit and integration tests remain 100% green:
```bash
dotnet test backend/NiramoyHealth.slnx
```
**Summary**:
- `NiramoyHealth.UnitTests`: 40 passed
- `NiramoyHealth.IntegrationTests`: 9 passed
- **Status**: **49 passed (100% PASS)**

---

## 4. Manual Verification Checklist

In addition to automated headless browser testing, the following checklist was validated:

| Verification Area | Check Performed | Verification Criteria | Result |
|---|---|---|---|
| **Touch Targets** | All interactive elements (buttons, quantity steppers, cart icons, filter tags) tested for tap convenience. | Minimum dimensions meet 44px x 44px or contain comfortable padding to prevent mis-clicks. | **PASS** |
| **Mobile Drawer** | Click hamburger icon, tap backdrop, press `Escape`, and tap internal links. | Drawer smoothly slides from left (`-translate-x-full` to `translate-x-0`), backdrop fades in, body scroll locks, and closes reliably. | **PASS** |
| **Mobile Bottom Nav** | Scroll down on Home, Catalog, Details, and Checkout on 375px viewport. | `#mobile-bottom-nav` remains anchored at bottom of screen with active route indicators and live cart badge; does not overlap content due to `#app-main` `pb-16`. | **PASS** |
| **Catalog Filter Drawer** | Click "Filter Medicines" button on mobile catalog page. | Aside drawer slides in from right, backdrop locks body scroll, selects allow filtering by category/dosage/manufacturer, and close button restores scroll. | **PASS** |
| **Sticky Purchase Bar** | Navigate to `#/products/napa-extra-tablet` on 375px mobile. | Bottom bar `#mobile-sticky-purchase-bar` displays current price, stock badge, and direct "Add to Cart" button; disappears on desktop screens (`md:hidden`). | **PASS** |
| **Form Inputs & iOS Zoom** | Focus inputs in Login, Register, Checkout, and Coupon code fields on mobile. | Font size is computed as `16px`, preventing mobile Safari from forcing a zoom viewport change. | **PASS** |
| **Cart Quantity Stepper** | Tap `+` and `-` buttons in the cart drawer on a small smartphone. | Step buttons have `tap-target-44` sizing with immediate live count feedback in the header and mobile bottom bar. | **PASS** |

---

## 5. Visual Test Artifacts & Screenshots

Visual artifacts are automatically generated during test runs and stored in the following directory:
```
frontend/tests/artifacts/screenshots/
```

### Generated Screenshot Artifacts

1. **Mobile Drawer Navigation**:
   - `01-mobile-drawer-375px.png` — Off-canvas navigation menu on 375px mobile showing quick categories, order tracking, prescription upload, and support hotline.
2. **Mobile Catalog Filter Drawer**:
   - `02-catalog-mobile-filters-375px.png` — Slide-over filter modal drawer on 375px mobile showing category dropdowns, dosage form chips, and prescription requirement toggles.
3. **Homepage Across Viewports**:
   - `home-320px-small-phone.png` — Compact hero layout, dual-row header, and stacked CTA on 320px screen.
   - `home-375px-standard-phone.png` — Standard phone presentation with 3-column category icons and bottom navigation.
   - `home-768px-tablet.png` — Tablet presentation with bounded category ribbon and 3-column feature badges.
   - `home-1280px-desktop.png` — Desktop widescreen presentation with trust bar and expanded menus.
4. **Catalog Page Across Viewports**:
   - `catalog-320px-small-phone.png` — Compact medicine card layout with mobile filter toolbar.
   - `catalog-375px-standard-phone.png` — Standard mobile catalog with 2-column product grid and quick add-to-cart.
   - `catalog-768px-tablet.png` — Tablet catalog with 3-column product cards.
   - `catalog-1280px-desktop.png` — Full desktop catalog with persistent sidebar filters and sorting dropdown.
5. **Product Details Page Across Viewports**:
   - `product-details-320px-small-phone.png` — Compact dosage details with sticky mobile purchase bar.
   - `product-details-375px-standard-phone.png` — Full mobile details with clear clinical warnings and clearance padding.
   - `product-details-768px-tablet.png` — Tablet presentation with side-by-side specs and generic alternatives.
   - `product-details-1280px-desktop.png` — Full desktop presentation with inline purchase card.
6. **Prescription Upload Page Across Viewports**:
   - `rx-upload-320px-small-phone.png` — Touch-friendly upload dropzone, clinical guide, and patient note input.
   - `rx-upload-375px-standard-phone.png` — Standard phone prescription upload workflow.
   - `rx-upload-768px-tablet.png` — Tablet prescription submission layout.
   - `rx-upload-1280px-desktop.png` — Desktop widescreen prescription upload workflow.
