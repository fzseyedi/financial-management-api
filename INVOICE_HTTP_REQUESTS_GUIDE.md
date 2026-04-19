# Invoice Update HTTP Requests Guide

## 📋 Overview
The `Invoices.http` file contains 5 update request examples demonstrating different update scenarios using optimistic concurrency control.

---

## 🔧 Setup

### Prerequisites
1. API running on `http://localhost:5182`
2. Visual Studio REST Client extension (built-in for .http files)
3. Existing invoices in database

### Version Extraction
```
1. Send: GET {{host}}/api/invoices/1
2. Copy the "version" field from response
3. Base64 string like: "AAAAAAAA8ng="
4. Paste into PUT request version field
```

---

## 📝 HTTP Request Examples

### 1️⃣ Update Invoice with Multiple Items
**File**: `Invoices.http` - Line 48-75  
**Purpose**: Update both items and metadata

```http
PUT http://localhost:5182/api/invoices/8
Content-Type: application/json

{
  "customerId": 1,
  "invoiceDate": "2026-04-10T00:00:00",
  "notes": "Updated invoice - modified items and date",
  "version": "AAAAAAAA8ng=",
  "modifiedBy": "user@company.com",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "quantity": 3,
      "unitPrice": 160.00
    },
    {
      "id": 2,
      "productId": 2,
      "quantity": 2,
      "unitPrice": 50.00
    }
  ]
}
```

**Expected Response**: `204 No Content` ✅

**What This Tests**:
- Multiple item replacement
- Price increase on both items
- Quantity changes
- Date update
- Notes modification

---

### 2️⃣ Update Invoice with Single Item
**File**: `Invoices.http` - Line 77-99  
**Purpose**: Simplify to single item update

```http
PUT http://localhost:5182/api/invoices/8
Content-Type: application/json

{
  "customerId": 1,
  "invoiceDate": "2026-04-11T00:00:00",
  "notes": "Updated with single item",
  "version": "AAAAAAAA8ng=",
  "modifiedBy": "admin@company.com",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "quantity": 5,
      "unitPrice": 175.00
    }
  ]
}
```

**Expected Response**: `204 No Content` ✅

**What This Tests**:
- Removing items (was 2, now 1)
- Higher quantity
- ModifiedBy change (admin vs user)

---

### 3️⃣ Reassign Invoice to Different Customer
**File**: `Invoices.http` - Line 101-123  
**Purpose**: Change invoice customer

```http
PUT http://localhost:5182/api/invoices/8
Content-Type: application/json

{
  "customerId": 2,
  "invoiceDate": "2026-04-12T00:00:00",
  "notes": "Reassigned to different customer",
  "version": "AAAAAAAA8ng=",
  "modifiedBy": "user@company.com",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "quantity": 2,
      "unitPrice": 150.00
    }
  ]
}
```

**Expected Response**: `204 No Content` ✅

**What This Tests**:
- Customer ID change (1 → 2)
- Customer validation runs again
- Item restoration

---

### 4️⃣ Concurrency Conflict Demo (Expected Failure)
**File**: `Invoices.http` - Line 125-145  
**Purpose**: Demonstrate optimistic locking error

```http
PUT http://localhost:5182/api/invoices/8
Content-Type: application/json

{
  "customerId": 1,
  "invoiceDate": "2026-04-13T00:00:00",
  "notes": "This should fail due to version mismatch",
  "version": "InvalidVersionString==",
  "modifiedBy": "user@company.com",
  "items": [
    {
      "id": 1,
      "productId": 1,
      "quantity": 2,
      "unitPrice": 150.00
    }
  ]
}
```

**Expected Response**: `409 Conflict` ⚠️

**Response Body**:
```json
{
  "errors": [
    "Invoice has been modified by another user. Please refresh and try again."
  ]
}
```

**What This Tests**:
- Wrong version detection
- Optimistic concurrency mechanism
- Proper error response
- Client error handling

---

## 🎯 Common Workflows

### Workflow 1: Normal Update
```
1. GET /api/invoices/1
   ↓ Extract version
2. PUT /api/invoices/1 (with version)
   ↓ Success: 204
3. GET /api/invoices/1 (verify)
   ↓ Confirm changes
```

### Workflow 2: Concurrent Update Retry
```
1. GET /api/invoices/1 → version: A
2. PUT /api/invoices/1 (version: A) 
   ↓ Another user updates invoice!
3. PUT fails with 409 Conflict
   ↓ Client refreshes
4. GET /api/invoices/1 → version: B (new)
5. PUT /api/invoices/1 (version: B)
   ↓ Success: 204
```

### Workflow 3: Customer Reassignment
```
1. GET /api/invoices/8 → customerId: 1
2. PUT /api/invoices/8 (customerId: 2)
   ↓ Validates customer 2 exists & active
3. Success: 204
4. GET /api/invoices/8 → customerId: 2 (updated)
```

---

## 📊 Request/Response Reference

### Request Format
```json
{
  "customerId": 1,              // Required: Valid customer ID
  "invoiceDate": "ISO 8601",    // Required: Invoice date
  "notes": "string",            // Optional: Up to 500 chars
  "version": "base64 string",   // Required: Current version from GET
  "modifiedBy": "string",       // Optional: User identifier
  "items": [                    // Required: At least 1 item
    {
      "id": 1,                  // Required: Item ID
      "productId": 1,           // Required: Valid product ID
      "quantity": 2.0,          // Required: > 0
      "unitPrice": 150.00       // Required: > 0
    }
  ]
}
```

### Success Response
```
HTTP/1.1 204 No Content
```

### Conflict Response
```json
HTTP/1.1 409 Conflict
Content-Type: application/json

{
  "errors": [
    "Invoice has been modified by another user. Please refresh and try again."
  ]
}
```

### Validation Error Response
```json
HTTP/1.1 400 Bad Request
Content-Type: application/json

{
  "errors": [
    "'CustomerId' must be greater than 0.",
    "'Version' cannot be empty."
  ]
}
```

### Not Found Response
```json
HTTP/1.1 404 Not Found
Content-Type: application/json

{
  "errors": [
    "Invoice with id 999 was not found."
  ]
}
```

---

## 🔄 Testing Tips

### Tip 1: Using Variables
```http
@invoiceId = 8
@version = AAAAAAAA8ng=

PUT http://localhost:5182/api/invoices/{{invoiceId}}
...
"version": "{{version}}"
```

### Tip 2: Copy-Paste Version
```
1. Run: GET /api/invoices/8
2. Click version value → Copy (Ctrl+C)
3. Paste into PUT request
```

### Tip 3: Test Sequence
```
GET /api/invoices/8        (verify before)
PUT /api/invoices/8 (V1)   (update)
GET /api/invoices/8        (verify after)
```

### Tip 4: Batch Testing
Run requests in order:
1. Create invoice (POST)
2. Get invoice (GET)
3. Update invoice (PUT) ← Extract version here
4. Update with wrong version (PUT) ← Expect 409

---

## 🚫 Common Errors & Solutions

### Error 409: Conflict
```
"Invoice has been modified by another user"

Solution:
1. Refresh invoice: GET /api/invoices/8
2. Copy new version
3. Retry PUT with new version
```

### Error 400: Validation
```
"'CustomerId' must be greater than 0"

Solution:
1. Check customerId is valid (> 0)
2. Check customer exists and is active
3. Verify version is not empty
```

### Error 404: Not Found
```
"Invoice with id 999 was not found"

Solution:
1. Verify invoice ID exists
2. Use GET /api/invoices to list invoices
3. Pick an existing ID
```

### Error 500: Server Error
```
Check server logs for:
1. Database connectivity
2. Transaction rollback (check items)
3. Concurrency violation
```

---

## 📍 File Location
```
Project Root
└── FinancialManagementApi.Api
    └── HTTP
        └── Invoices.http  ← This file
```

## 🖱️ Running in Visual Studio
```
1. Open Invoices.http
2. Hover over request line
3. Click "Send Request" link
4. View response in "Response" panel
```

## 📡 Alternative: cURL Commands

If using command line:

```bash
# Get version
curl http://localhost:5182/api/invoices/8 | grep version

# Update invoice
curl -X PUT http://localhost:5182/api/invoices/8 \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "invoiceDate": "2026-04-10T00:00:00",
    "notes": "Updated",
    "version": "AAAAAAAA8ng=",
    "modifiedBy": "user@company.com",
    "items": [{"id": 1, "productId": 1, "quantity": 2, "unitPrice": 150}]
  }'

# Test conflict
curl -X PUT http://localhost:5182/api/invoices/8 \
  -H "Content-Type: application/json" \
  -d '{
    "customerId": 1,
    "invoiceDate": "2026-04-10T00:00:00",
    "notes": "Updated",
    "version": "InvalidVersion==",
    "modifiedBy": "user@company.com",
    "items": [{"id": 1, "productId": 1, "quantity": 2, "unitPrice": 150}]
  }'
```

---

## ✅ Checklist for Manual Testing

- [ ] Create or select test invoice
- [ ] GET invoice and copy version
- [ ] Update single field (notes) ✓
- [ ] Update items ✓
- [ ] Update customer ✓
- [ ] Test with wrong version (expect 409) ✓
- [ ] Refresh after conflict ✓
- [ ] Verify all changes persisted ✓

