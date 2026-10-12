# Niramoy Health (নিরাময় হেলথ) — Environment Configuration & Feature Matrix

## 1. Overview

This document outlines all environment variables, configuration settings, connection strings, and credential templates required to run the **Niramoy Health** application across local development, staging, and production environments. It also includes the comprehensive **Feature Implementation Matrix**.

---

## 2. Frontend Configuration (`frontend/`)

The frontend reads configuration via Vite environment variables prefixed with `VITE_`.

### 2.1. Environment Variables Template (`.env.example`)
Create `.env` inside `frontend/`:
```bash
# Frontend Port and Host (Development)
PORT=5173
HOST=127.0.0.1

# API Base URL (leave empty or use /api to utilize Vite reverse proxy)
VITE_API_BASE_URL=/api

# Application Metadata
VITE_APP_NAME="Niramoy Health"
VITE_APP_NAME_BN="নিরাময় হেলথ"
VITE_SUPPORT_HOTLINE="09612-NIRAMOY (647266)"
VITE_ENABLE_MOCK_PAYMENT=true
```

---

## 3. Backend Configuration (`backend/src/NiramoyHealth.Api/`)

Configuration is managed hierarchically via `appsettings.json`, `appsettings.Development.json`, and OS environment variables.

### 3.1. Standard `appsettings.json` Schema
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=127.0.0.1;Port=5432;Database=niramoy_health_dev;Username=niramoy_user;Password=niramoy_dev_pass_2026;"
  },
  "Jwt": {
    "Issuer": "NiramoyHealth",
    "Audience": "NiramoyHealthApp",
    "SecretKey": "NiramoyHealth_Dev_SuperSecretJwtSigningKey_2026_SecureKeyToken!",
    "ExpiryMinutes": 120,
    "RefreshTokenExpiryDays": 30
  },
  "Cors": {
    "AllowedOrigins": [
      "http://localhost:5173",
      "http://127.0.0.1:5173",
      "http://localhost:3000"
    ]
  },
  "Storage": {
    "PrescriptionUploadPath": "storage/prescriptions",
    "MaxFileSizeBytes": 10485760
  },
  "PaymentGateways": {
    "Mode": "Sandbox",
    "Bkash": {
      "AppKey": "sandbox_bkash_key",
      "AppSecret": "sandbox_bkash_secret",
      "MerchantNumber": "01770000000"
    },
    "Nagad": {
      "MerchantId": "sandbox_nagad_id",
      "PublicKey": "sandbox_nagad_pubkey"
    },
    "SslCommerz": {
      "StoreId": "sandbox_sslcommerz_id",
      "StorePassword": "sandbox_sslcommerz_pass",
      "IsLive": false
    }
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore.Database.Command": "Information"
    }
  }
}
```

### 3.2. Environment Variable Overrides (Production)
In containerized or cloud environments, override hierarchical JSON keys using double underscores (`__`):
```bash
export ConnectionStrings__DefaultConnection="Host=db.internal;Port=5432;Database=niramoy_health_prod;Username=prod_admin;Password=STRONG_SECRET;"
export Jwt__SecretKey="A_VERY_LONG_CRYPTOGRAPHICALLY_SECURE_SECRET_STRING_FOR_PRODUCTION"
export Cors__AllowedOrigins__0="https://niramoyhealth.com"
export Cors__AllowedOrigins__1="https://www.niramoyhealth.com"
export PaymentGateways__Mode="Live"
```

---

## 4. Default Seeded Accounts & Credentials

The seed data loaded from `sql/03_seed_data.sql` provides verified test accounts:

| Role | Identifier / Phone / Email | Password | Pre-seeded Details |
|---|---|---|---|
| **Customer** | `01712345678` / `customer@niramoyhealth.com` | `Password123!` | Active customer with pre-configured delivery address in Dhanmondi, Dhaka. |
| **Pharmacist** | `pharmacist@niramoyhealth.com` | `Pharma123!` | Registered staff with clinical verification and prescription approval privileges. |
| **Administrator** | `admin@niramoyhealth.com` | `Admin123!` | System administrator with full catalog, orders, and user access. |

---

## 5. Comprehensive Feature Implementation Matrix

| Module | Feature Description | Status | Test Coverage |
|---|---|---|---|
| **Branding & UI** | Original Niramoy Health visual identity, emerald clinical theme | **Implemented** | Verified via Playwright E2E |
| **Localization** | Bilingual English and Bengali (`Hind Siliguri` font, ৳ symbols) | **Implemented** | Verified across all pages |
| **Catalog** | Category listing, manufacturer brands, dosage filters | **Implemented** | Backend Unit/Integration + E2E |
| **Live Search** | Debounced search bar with instant autocomplete suggestions | **Implemented** | Verified in `storefront.spec.js` |
| **Product Details**| High-res images, pack pricing, generic formulation, stock level | **Implemented** | Verified via Playwright E2E |
| **Cart System** | Guest cart (`X-Guest-Token`), persistent user cart, quantity stepper | **Implemented** | Backend Unit/Integration + E2E |
| **Cart Merge** | Automatic merging of guest items into account upon login | **Implemented** | Verified in `cart-and-checkout.spec.js` |
| **Prescriptions** | Secure multipart file upload (`.jpg`, `.pdf`) with customer notes | **Implemented** | Verified in `prescriptions-and-admin.spec.js`|
| **Clinical Review**| Staff prescription queue, image inspector, pharmacist approval | **Implemented** | Verified in `prescriptions-and-admin.spec.js`|
| **Checkout Flow**| Multi-step address selection, express/regular delivery, COD/bKash | **Implemented** | Verified in `cart-and-checkout.spec.js` |
| **Orders** | Atomic inventory deduction, tracking number generation (`ORD-...`) | **Implemented** | Backend Unit/Integration + E2E |
| **Order Tracking**| Timeline progress: Placed ➔ Verified ➔ Dispatched ➔ Delivered | **Implemented** | Verified in `OrdersPage.js` & E2E |
| **Admin Portal** | Store KPI metrics, order dispatch pipeline, prescription triage | **Implemented** | Verified in `prescriptions-and-admin.spec.js`|
| **Role Guards** | Route protection preventing unauthorized customer access to Admin | **Implemented** | Verified in `prescriptions-and-admin.spec.js`|
| **Mobile UX** | 375px mobile, 768px tablet, 1280px desktop responsive breakpoints | **Implemented** | Verified in `storefront.spec.js` |
| **MFS Payment** | bKash/Nagad instant verification (sandbox simulation) | **Implemented** | Supported via API & UI |
| **Live Gateways** | SSLCommerz / bKash merchant live gateway production keys | **Partially Implemented** | Sandbox simulated (Live keys pending client merchant account) |
