# Niramoy Health (নিরাময় হেলথ) — Automated Backend Testing

## 1. Testing Philosophy & Strategy

The Niramoy Health backend includes comprehensive automated test suites split into two distinct testing tiers:

1. **Unit Tests (`NiramoyHealth.UnitTests`)**:
   - Fast, isolated tests targeting pure business logic, calculations, pricing math, coupon evaluation, and input validators.
   - Run in-memory with zero external infrastructure dependencies.
   - Execute in under 1 second.

2. **Integration Tests (`NiramoyHealth.IntegrationTests`)**:
   - End-to-end API tests utilizing ASP.NET Core `WebApplicationFactory<Program>` executing against a real PostgreSQL database (`niramoy_health_test`).
   - Verifies real entity persistence, foreign key cascades, unique constraints, check constraints, authentication token issuance, and HTTP pipelines.

---

## 2. Test Suites Overview

### 2.1. Unit Test Suite (`NiramoyHealth.UnitTests`) — 40 Tests
- **`PricingAndDiscountsTests.cs`**:
  - Verification of tiered product pricing and MRP calculations.
  - Percentage and fixed-amount coupon calculations.
  - Rounding accuracy for Bangladeshi Taka (2 decimal places).
  - Minimum purchase thresholds and maximum discount ceilings.
- **`ValidationRulesTests.cs`**:
  - Bangladeshi mobile phone format (`+8801[3-9]\d{8}` or `01[3-9]\d{8}`).
  - Password complexity rules (upper, lower, digits, symbols).
  - Positive quantities and positive prices.
  - Prescription patient details validation.
- **`BusinessLogicTests.cs`**:
  - Prescription status flow: submitted -> under_review -> approved/rejected.
  - Ensuring unapproved prescriptions cannot be checked out with prescription-only medicines.
  - Order status state transitions.

### 2.2. Integration Test Suite (`NiramoyHealth.IntegrationTests`) — 9 Tests
- **`AuthAndProfileTests.cs`**:
  - `Register_WithValidCredentials_ReturnsSuccessAndJwtTokens`: Customer onboarding and token issuance.
  - `Login_WithCorrectCredentials_ReturnsValidJwtAndUser`: Authentication verification.
  - `GetProfile_WithValidToken_ReturnsUserProfileAndAddress`: Profile query with Bearer claims.
- **`CatalogAndCartTests.cs`**:
  - `GetProducts_WithFilteringAndPagination_ReturnsPaginatedResult`: Product search and filter pipeline.
  - `GetCategories_ReturnsCategoryHierarchy`: Tree-structured category query.
  - `CartLifecycle_AddUpdateRemove_CalculatesTotalsCorrectly`: Multi-item cart manipulations, line totals, and recalculations.
- **`PrescriptionAndCheckoutTests.cs`**:
  - `PrescriptionUpload_InitializesStatusToSubmitted_NeverAutoApproved`: Verifies prescription creation enforces `submitted` status.
  - `PharmacistReview_UpdatesPrescriptionStatus_ToApproved`: Role-based clinical approval by authorized pharmacist.
  - `CheckoutWorkflow_CompletesFullOrderCycle_AndPayment`: Complete end-to-end customer checkout, order creation, inventory reservation, payment initiation, and payment execution advancing order status to `confirmed` and `paid`.

---

## 3. Running Automated Tests

### 3.1. Running the Entire Test Solution
```bash
dotnet test backend/NiramoyHealth.slnx
```

Expected output:
```
Passed!  - Failed: 0, Passed: 40, Skipped: 0, Total: 40 (NiramoyHealth.UnitTests.dll)
Passed!  - Failed: 0, Passed:  9, Skipped: 0, Total:  9 (NiramoyHealth.IntegrationTests.dll)
```

### 3.2. Running Unit Tests Only
```bash
dotnet test backend/tests/NiramoyHealth.UnitTests
```

### 3.3. Running Integration Tests Only
Ensure PostgreSQL is running and `niramoy_health_test` is initialized:
```bash
dotnet test backend/tests/NiramoyHealth.IntegrationTests
```

### 3.4. Test Configuration
Integration test configuration is located in `backend/tests/NiramoyHealth.IntegrationTests/NiramoyTestWebApplicationFactory.cs`:
- Connects to: `Host=127.0.0.1;Port=5432;Database=niramoy_health_test;Username=niramoy_user;Password=niramoy_dev_pass_2026;`
- Configures isolated in-memory test file storage.
- Overrides external payment gateway calls with sandbox simulators.
