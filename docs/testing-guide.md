# Niramoy Health (নিরাময় হেলথ) — Testing Guide

## 1. Overview & Test Strategy

To ensure clinical compliance, financial calculation accuracy, and a flawless user experience, the Niramoy Health platform employs a comprehensive multi-layered automated testing pyramid:

1. **Backend Unit Tests (.NET xUnit)**: Fast, isolated verification of domain entities, business calculation services (discounts, taxes, delivery fees), and security policies.
2. **Backend Integration Tests (.NET WebApplicationFactory & PostgreSQL)**: In-memory test server testing complete API controllers, database persistence, transaction integrity, and authentication middleware.
3. **Frontend End-to-End Tests (Playwright & Google Chrome)**: Real browser automation testing full user journeys across desktop, tablet, and mobile viewport sizes.

---

## 2. Test Execution Summary

| Test Layer | Framework | Scope / Location | Passed | Failed | Status |
|---|---|---|---|---|---|
| **Backend Unit Tests** | xUnit / FluentAssertions | `backend/tests/NiramoyHealth.UnitTests` | **40** | 0 | **100% PASSED** |
| **Backend Integration Tests** | xUnit / WebApplicationFactory | `backend/tests/NiramoyHealth.IntegrationTests` | **9** | 0 | **100% PASSED** |
| **Frontend Browser E2E Tests** | Playwright / Chrome | `frontend/tests/` | **6** | 0 | **100% PASSED** |
| **Total Test Suite** | Multi-Layer | Full Solution | **55** | **0** | **ALL GREEN** |

---

## 3. Running Backend Tests

### 3.1. Prerequisites
Ensure .NET 10 SDK is installed and PostgreSQL is running:
```bash
dotnet --version
# Should output 10.0.x
```

### 3.2. Run All Backend Tests (Unit + Integration)
From the repository root:
```bash
dotnet test backend/NiramoyHealth.sln
```

### 3.3. Run Unit Tests Only
```bash
dotnet test backend/tests/NiramoyHealth.UnitTests/NiramoyHealth.UnitTests.csproj
```
**Key Scenarios Covered**:
- Domain validations for Customer, Order, OrderItem, Prescription, and Product.
- Phone number format validation for Bangladeshi telecom operators (Grameenphone, Banglalink, Robi, Teletalk).
- Cart total calculation, discounts, and tier pricing.
- Stock availability and replenishment business rules.
- Password hashing and JWT generation claims.

### 3.4. Run Integration Tests Only
```bash
dotnet test backend/tests/NiramoyHealth.IntegrationTests/NiramoyHealth.IntegrationTests.csproj
```
**Key Scenarios Covered**:
- `POST /api/v1/auth/login` and token claims issuance.
- Product catalog querying and facet filtering.
- Guest cart item creation and retrieval via `X-Guest-Token`.
- Multi-step checkout with address selection and payment mode.
- Prescription upload and pharmacist approval state transitions.

---

## 4. Running Frontend End-to-End (E2E) Tests

Playwright automates real browser sessions against the running frontend and ASP.NET Core API.

### 4.1. Setup & Environment
Ensure dependencies and Playwright browser binaries are installed:
```bash
cd frontend
npm install
npx playwright install
```

### 4.2. Starting Backend & Frontend for Testing
In separate terminals (or background jobs):
1. **Start ASP.NET Core API**:
   ```bash
   dotnet run --project backend/src/NiramoyHealth.Api/NiramoyHealth.Api.csproj --urls "http://127.0.0.1:5131"
   ```
2. **Start Vite Dev Server**:
   ```bash
   cd frontend
   npm run dev -- --host 127.0.0.1 --port 5173
   ```

### 4.3. Run the Playwright Test Suite
In the `frontend/` directory:
```bash
npx playwright test
```

To run with visual browser inspection:
```bash
npx playwright test --headed
```

To inspect individual test traces and step-by-step screenshots:
```bash
npx playwright show-report
```

---

## 5. End-to-End Test Suite Details

### 5.1. `storefront.spec.js`
- **Homepage Verification**: Verifies the Niramoy Health brand logo, Bengali tagline, search input, emergency hotline banner, category cards, and deals carousel.
- **Live Catalog Search**: Types *"Napa"* into the search bar, verifies instant autocomplete suggestions from PostgreSQL, navigates to the search results page, and opens the product details page.
- **Responsive Layouts**: Validates mobile (375x667 - iPhone SE), tablet (768x1024 - iPad), and desktop (1280x800) layouts to ensure responsive grid adjustments, hamburger menu functionality, and sticky bottom navigation.

### 5.2. `cart-and-checkout.spec.js`
- **Guest-to-User Shopping Flow**:
  1. An unauthenticated visitor adds *Napa Extra* to the cart.
  2. Slide-over cart opens showing items, price in BDT (৳), and delivery fee.
  3. Clicks "Proceed to Checkout", triggering route guard redirect to login.
  4. Customer logs in (`01712345678` / `Password123!`).
  5. Guest cart is automatically merged into the user account.
  6. Customer selects delivery address in Dhanmondi, Dhaka, chooses Cash on Delivery (COD), and clicks "Place Order".
  7. Verifies navigation to the order confirmation page with a generated tracking ID (`ORD-...`).

### 5.3. `prescriptions-and-admin.spec.js`
- **Customer Prescription Workflow**:
  1. Customer logs in and navigates to `#/prescriptions`.
  2. Uploads sample medical prescription document with clinical notes.
  3. Verifies prescription enters `submitted` status.
- **Pharmacist / Admin Workflow**:
  1. Admin logs in (`admin@niramoyhealth.com` / `Admin123!`) and opens `#/admin`.
  2. Navigates to the pending prescription queue.
  3. Reviews patient prescription and clicks "Approve Prescription" with clinical dosage instructions.
  4. Verifies prescription status transitions from `submitted` to `approved`.
- **Security Role Guard**:
  1. Verifies that an unauthenticated visitor or non-staff user attempting to access `#/admin` is blocked and redirected to the storefront with an access denied alert.
