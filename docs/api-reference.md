# Niramoy Health (নিরাময় হেলথ) — REST API Reference

Base URL: `/api/v1`

All responses follow the standard JSON envelope:

```json
{
  "success": true,
  "message": "Operation completed successfully.",
  "data": { ... },
  "errors": null,
  "timestamp": "2026-10-11T12:00:00Z"
}
```

---

## 1. Authentication & User Profile (`/auth`)

### Register
- **Endpoint**: `POST /api/v1/auth/register`
- **Auth**: None
- **Body**:
  ```json
  {
    "fullName": "Tamim Iqbal",
    "phoneNumber": "01712345678",
    "email": "tamim@example.com",
    "password": "Password123!"
  }
  ```
- **Response**: `200 OK` with `AuthResponseDto` containing `accessToken`, `refreshToken`, and user summary.

### Login
- **Endpoint**: `POST /api/v1/auth/login`
- **Auth**: None
- **Body**:
  ```json
  {
    "phoneOrEmail": "01712345678",
    "password": "Password123!"
  }
  ```
- **Response**: `200 OK` with `AuthResponseDto`.

### Refresh Token
- **Endpoint**: `POST /api/v1/auth/refresh`
- **Auth**: None
- **Body**:
  ```json
  {
    "refreshToken": "4a7f8e..."
  }
  ```
- **Response**: `200 OK` with new `accessToken` and rotated `refreshToken`.

### Current User Profile
- **Endpoint**: `GET /api/v1/auth/me`
- **Auth**: Bearer Token
- **Response**: `200 OK` with user details including roles, verified flags, and default address.

### Update Profile
- **Endpoint**: `PUT /api/v1/auth/profile`
- **Auth**: Bearer Token
- **Body**:
  ```json
  {
    "fullName": "Tamim Iqbal",
    "email": "tamim.new@example.com",
    "dateOfBirth": "1992-05-15",
    "gender": "male"
  }
  ```

---

## 2. Customer Delivery Addresses (`/addresses`)

- `GET /api/v1/addresses`: List saved addresses for logged-in user.
- `GET /api/v1/addresses/{id}`: Get specific address.
- `POST /api/v1/addresses`: Create new address (`recipientName`, `recipientPhone`, `division`, `district`, `upazila`, `areaDetails`, `isDefaultShipping`).
- `PUT /api/v1/addresses/{id}`: Update address.
- `POST /api/v1/addresses/{id}/default`: Set as default shipping address.
- `DELETE /api/v1/addresses/{id}`: Soft delete or remove address.

---

## 3. Product Catalog & Categories (`/products`, `/categories`)

### Product Listing & Search
- **Endpoint**: `GET /api/v1/products`
- **Auth**: None
- **Query Parameters**:
  - `query`: Text search on medicine brand name, generic name, manufacturer.
  - `categoryId`: Filter by category ID.
  - `dosageFormId`: Filter by dosage form (tablet, syrup, capsule).
  - `manufacturerId`: Filter by pharmaceutical manufacturer (Square, Beximco, Incepta, etc.).
  - `minPrice`, `maxPrice`: Price range filters in BDT.
  - `isPrescriptionRequired`: Boolean.
  - `sortBy`: `popular`, `price_asc`, `price_desc`, `newest`, `discount`.
  - `page`: Page index (default: `1`).
  - `pageSize`: Items per page (default: `20`, max: `100`).
- **Response**: `200 OK` with `PagedResult<ProductListDto>`.

### Product Details
- **Endpoint**: `GET /api/v1/products/{id}` or `GET /api/v1/products/slug/{slug}`
- **Auth**: None
- **Response**: Detailed product specification including generics, strengths, pack sizes, stock availability, images, and warnings.

### Featured & Discounted Products
- `GET /api/v1/products/featured?limit=10`
- `GET /api/v1/products/discounted?limit=10`

### Categories
- `GET /api/v1/categories`: Hierarchical category tree (Prescription Medicine, OTC, Healthcare Devices, Personal Care).
- `GET /api/v1/categories/{slug}`: Category metadata with parent/children references.

---

## 4. Shopping Cart (`/cart`)

Supports both Guest users (via `X-Guest-Token` header) and authenticated customers (`Bearer` token).

- `GET /api/v1/cart`: Get current active cart with server-recalculated line items, discounts, and total.
- `POST /api/v1/cart/items`: Add item (`productId`, `quantity`).
- `PUT /api/v1/cart/items/{id}`: Update item quantity.
- `DELETE /api/v1/cart/items/{id}`: Remove item from cart.
- `DELETE /api/v1/cart`: Clear cart completely.
- `POST /api/v1/cart/merge`: Merge guest cart into user cart after login (`guestToken`).
- `POST /api/v1/cart/apply-coupon`: Apply promo code (`couponCode`).
- `DELETE /api/v1/cart/remove-coupon`: Remove applied coupon.

---

## 5. Prescriptions (`/prescriptions`)

### Upload Prescription
- **Endpoint**: `POST /api/v1/prescriptions/upload`
- **Auth**: Bearer Token
- **Format**: `multipart/form-data`
- **Fields**:
  - `file`: Binary image/PDF file (max 10MB, allowed types: jpg, png, pdf).
  - `patientName`: Patient's name.
  - `patientAge`: Numeric age in years.
  - `patientGender`: `male`, `female`, or `other`.
  - `patientPhone`: BD phone number.
  - `doctorName`: Optional doctor or hospital name.
  - `notes`: Optional patient comments or special instructions.
- **Initial Status**: Always set to `submitted`. Never auto-approved.

### Review Prescription (Pharmacist / Admin)
- **Endpoint**: `POST /api/v1/prescriptions/{id}/review`
- **Auth**: Roles: `Pharmacist`, `Admin`, `SuperAdmin`
- **Body**:
  ```json
  {
    "newStatus": "approved", // approved, partially_approved, rejected
    "clinicalNotes": "Verified against registered BMDC doctor prescription."
  }
  ```

---

## 6. Orders & Checkout (`/orders`)

### Checkout Order
- **Endpoint**: `POST /api/v1/orders/checkout`
- **Auth**: Bearer Token
- **Body**:
  ```json
  {
    "addressId": 1,
    "deliveryMethodId": 1,
    "paymentMethodId": 2,
    "prescriptionId": 5,
    "customerNotes": "Please ring the bell on delivery.",
    "idempotencyKey": "a93bf81d-e593-4b02-a726-5b4cf5d13411"
  }
  ```
- **Validation**:
  - Validates stock for all items.
  - If any item requires prescription, verifies that `prescriptionId` belongs to user and status is `approved`.
  - Atomically reserves inventory and writes `inventory_transactions` record.
  - Creates order with status `pending` and payment status `unpaid`.

### Customer Order History
- `GET /api/v1/orders?page=1&pageSize=10`: User's order list.
- `GET /api/v1/orders/{id}`: Complete order details including status timeline, tracking number, and items.
- `POST /api/v1/orders/{id}/cancel`: Cancel pending order and release reserved inventory.

---

## 7. Payments (`/payments`)

### Initiate Payment
- **Endpoint**: `POST /api/v1/payments/initiate`
- **Auth**: Bearer Token
- **Body**:
  ```json
  {
    "orderId": 12
  }
  ```
- **Response**: Returns `paymentPublicId`, `gatewayProvider`, `redirectUrl`, or MFS payment intent tokens.

### Execute Payment
- **Endpoint**: `POST /api/v1/payments/execute`
- **Auth**: None (or customer session)
- **Body**:
  ```json
  {
    "paymentPublicId": "c39b1a20-4107-4251-bd12-f02781498112",
    "gatewayPaymentId": "BKSH_GW_0918237",
    "providerTrxId": "TRX9988123"
  }
  ```
- **Effect**:
  - Updates payment status to `captured`.
  - Advances order status to `confirmed` and order payment status to `paid`.
  - Creates audit entries in `payment_transactions` and `order_status_history`.

---

## 8. Administrative Operations (`/admin`)

Requires `Admin` or `SuperAdmin` role.

- `GET /api/v1/admin/dashboard`: Platform KPIs (total orders, total revenue, low-stock alerts, pending prescriptions, support ticket counts).
- `GET /api/v1/admin/orders`: Filterable order list with customer details.
- `PUT /api/v1/admin/orders/{id}/status`: Advance order lifecycle (`confirmed` -> `processing` -> `packed` -> `shipped` -> `delivered`).
- `GET /api/v1/admin/prescriptions`: Review queue for registered pharmacists.
- `GET /api/v1/admin/inventory`: Low stock monitor and batch expiry tracker.
- `POST /api/v1/admin/inventory/adjust`: Manual inventory adjustments with audit trail.
- `POST /api/v1/admin/products`: Create medicine/product catalog items.
- `PUT /api/v1/admin/products/{id}`: Update medicine details, prices, or regulatory attributes.

---

## 9. Customer Support (`/support`)

- `POST /api/v1/support/tickets`: Create support request (`subject`, `orderId`, `priority`, `message`).
- `GET /api/v1/support/tickets`: List user's tickets.
- `GET /api/v1/support/tickets/{id}`: View ticket conversation thread.
- `POST /api/v1/support/tickets/{id}/messages`: Post reply message.
- `PUT /api/v1/support/tickets/{id}/status`: Close or resolve ticket (Agent/Admin).
