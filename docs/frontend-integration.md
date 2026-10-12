# Niramoy Health (নিরাময় হেলথ) — Frontend Integration Guide

## 1. Client Architecture & API Contract

This guide defines the integration contract between the **ASP.NET Core Web API (Phase 2)** and the upcoming **Phase 3 Responsive Frontend**.

---

## 2. Standard HTTP Request Headers

| Header | Description | Required When |
|---|---|---|
| `Content-Type: application/json` | Request payload format. | All `POST`, `PUT`, `PATCH` requests (except multipart uploads). |
| `Authorization: Bearer <token>` | JWT Access Token. | Accessing protected routes (profile, checkout, orders, admin). |
| `X-Guest-Token: <uuid>` | Guest cart identifier. | Anonymous browsing, cart additions before login. |
| `Accept-Language: bn-BD, en-US` | Preferred response localization. | Optional. |

---

## 3. Standard Response Formats

### 3.1. Successful Response Envelope
```json
{
  "success": true,
  "message": "Cart updated successfully.",
  "data": {
    "items": [...],
    "subtotal": 520.00,
    "discount": 52.00,
    "total": 468.00
  },
  "errors": null,
  "timestamp": "2026-10-11T12:00:00Z"
}
```

### 3.2. Validation Failure (HTTP 400 Bad Request)
```json
{
  "success": false,
  "message": "Validation failed for one or more fields.",
  "data": null,
  "errors": {
    "phoneNumber": ["Please provide a valid Bangladeshi phone number (e.g. 017XXXXXXXX)."],
    "password": ["Password must contain at least one uppercase letter and one special character."]
  },
  "timestamp": "2026-10-11T12:00:00Z"
}
```

### 3.3. Business Rule Error (HTTP 422 Unprocessable Entity)
```json
{
  "success": false,
  "message": "Insufficient stock for Napa Extra 500mg. Requested: 10, Available: 4.",
  "data": null,
  "errors": null,
  "timestamp": "2026-10-11T12:00:00Z"
}
```

### 3.4. Resource Not Found (HTTP 404 Not Found)
```json
{
  "success": false,
  "message": "Product with ID 9999 was not found.",
  "data": null,
  "errors": null,
  "timestamp": "2026-10-11T12:00:00Z"
}
```

---

## 4. Status Constants & Enums Mapping

Frontend components must use these exact string literals when matching status states:

### 4.1. Order Status (`orderStatus`)
- `pending`: Placed by customer, awaiting payment or packing.
- `confirmed`: Payment received / COD confirmed.
- `prescription_check`: Prescription attached, awaiting pharmacist review.
- `processing`: Warehouse gathering items.
- `packed`: Package sealed, awaiting courier dispatch.
- `shipped`: In transit with delivery partner.
- `out_for_delivery`: Delivery agent out with parcel.
- `delivered`: Handed over to customer.
- `cancelled`: Cancelled by user or administrative action.
- `returned`: Item returned by customer.

### 4.2. Payment Status (`paymentStatus`)
- `unpaid`: No payment collected.
- `partially_paid`: Partial deposit received.
- `paid`: Full amount successfully collected.
- `refunded`: Amount refunded back to customer.
- `failed`: Payment gateway attempt failed.

### 4.3. Prescription Status (`prescriptionStatus`)
- `submitted`: Prescription uploaded, pending pharmacist review.
- `under_review`: Pharmacist currently evaluating image.
- `approved`: Validated by pharmacist; ready for order fulfillment.
- `partially_approved`: Certain medications approved, others omitted.
- `rejected`: Invalid or expired prescription.

---

## 5. Recommended Frontend State Management

1. **Token Persistence**:
   - Store `accessToken` in memory or secure storage.
   - Store `refreshToken` in `httpOnly` secure cookies or safe local storage.
   - Intercept HTTP 401 responses to automatically call `POST /api/v1/auth/refresh` before retrying failed requests.

2. **Guest Cart to User Cart Handover**:
   - When a guest visitor logs in:
   - Extract `guestToken` from browser storage.
   - Call `POST /api/v1/cart/merge` with `{ "guestToken": "<uuid>" }`.
   - Remove `guestToken` from storage upon successful merge.

3. **Prescription Upload Component**:
   - Use `FormData` to append `file` and patient demographic fields.
   - Show patient age, gender, and doctor notes fields.
   - Do not display an order completion UI until prescription is approved if order contains Rx-only drugs.
