# Financial Management API

A practical **financial/business backend API** built with **ASP.NET Core**, **Clean Architecture**, **SQL Server**, **Dapper**, and **Dapper.Contrib**.

This project is designed as a portfolio-grade backend system focused on **real business workflows**, not just tutorial-style CRUD.

---

## Highlights

- Customer management
- Product management
- Invoice creation with header + items in a single transaction
- Invoice issuing workflow
- Payment recording
- Customer balance and unpaid invoice reporting
- Server-side pagination
- Structured logging with Serilog
- Clean Architecture separation of concerns
- Validation, business rules, and consistent error handling

---

## Core Features

### Customers
- Create, update, activate, and deactivate customers
- Delete customers only when no related invoices or payments exist
- Get customer by id
- Get active customers
- Optionally include inactive customers
- Paginated customer listing

### Products
- Create, update, activate, and deactivate products
- Get product by id
- Get active products
- Optionally include inactive products
- Paginated product listing

### Invoices
- Create invoice with header and items in one request
- Save invoice header and rows in one SQL transaction
- Get invoice by id with item details
- Issue invoice
- Delete draft invoices only

### Payments and Reports
- Record payment against an invoice
- Update invoice payment state
- Get customer balance
- Get unpaid invoices

---

## Architecture

The solution follows **Clean Architecture** with clear separation of responsibilities.

### Projects
- `FinancialManagementApi.Api`
- `FinancialManagementApi.Application`
- `FinancialManagementApi.Domain`
- `FinancialManagementApi.Infrastructure`
- `FinancialManagementApi.Tests`

### Layers
- **Domain**: entities, enums, business rules, domain exceptions
- **Application**: commands, queries, handlers, validators, DTOs, abstractions
- **Infrastructure**: SQL Server persistence, Dapper repositories, Unit of Work
- **Api**: controllers, middleware, OpenAPI setup, request/response contracts
- **Tests**: validator, domain, and handler tests

---

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- SQL Server
- Dapper
- Dapper.Contrib
- FluentValidation
- Serilog
- xUnit
- FluentAssertions
- Moq

---

## Design Notes

### Dapper Strategy
This solution uses:
- **Dapper.Contrib** for simple insert/update operations
- **Raw Dapper SQL** for joins, reports, and transactional document persistence

### Transactional Invoice Creation
Invoice header and items are created in a **single SQL transaction** to avoid:
- orphan invoice headers
- partial document persistence
- inconsistent totals

### Payment Safety
Payment recording uses the **Unit of Work pattern** and pessimistic locking to support:
- atomic invoice/payment updates
- prevention of inconsistent payment state
- safer concurrent payment handling

### Lifecycle Strategy
Customers and products primarily use:
- activation
- deactivation

instead of hard delete in normal business flow.

---

## API Overview

### Customers
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `PUT /api/customers/{id}/deactivate`
- `PUT /api/customers/{id}/activate`
- `DELETE /api/customers/{id}`
- `GET /api/customers/{id}`
- `GET /api/customers`

### Products
- `POST /api/products`
- `PUT /api/products/{id}`
- `PUT /api/products/{id}/deactivate`
- `PUT /api/products/{id}/activate`
- `GET /api/products/{id}`
- `GET /api/products`

### Invoices
- `POST /api/invoices`
- `POST /api/invoices/{invoiceId}/issue`
- `DELETE /api/invoices/{id}`
- `GET /api/invoices/{id}`
- `GET /api/invoices`

### Payments
- `POST /api/payments`

### Reports
- `GET /api/reports/customer-balance/{customerId}`
- `GET /api/reports/unpaid-invoices`

---

## Example Workflow

1. Create a customer
2. Create products
3. Create an invoice with header and items
4. Issue the invoice
5. Record payment
6. Check customer balance
7. Check unpaid invoices

---

## Example Invoice Request

```json
{
  "customerId": 1,
  "invoiceDate": "2026-04-09T00:00:00",
  "notes": "First invoice",
  "items": [
    {
      "productId": 1,
      "quantity": 2,
      "unitPrice": 150.00
    },
    {
      "productId": 2,
      "quantity": 1,
      "unitPrice": 45.00
    }
  ]
}
```

## Pagination

Paginated endpoints return metadata such as:

```json
{
  "items": [],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

---

## Logging

The application uses **Serilog** for structured logging with file persistence.

Logging covers:
- validation errors
- business rule violations
- repository operations
- transactional workflows
- unexpected exceptions

Log files are written to:

```text
logs/financial-api-.txt
```

---

## Current Status

This project is in a **working and testable state** and is intended as a portfolio-quality backend project for real business scenarios.

Implemented areas include:
- customer, product, invoice, and payment workflows
- transactional invoice persistence
- payment consistency handling
- pagination
- structured logging
- exception handling middleware
- validator and handler tests

---

## Future Improvements

Possible next improvements:

- richer reporting endpoints
- more advanced filtering/search
- authentication and authorization
- more financial workflows
- additional automated tests
- API versioning
- Docker support

---

## Author

**Fariborz Seyedi**  
Senior .NET Developer  
Istanbul, Turkey

- LinkedIn: [linkedin.com/in/fariborz-seyedi-49582934b](https://linkedin.com/in/fariborz-seyedi-49582934b)
- GitHub: [github.com/fzseyedi](https://github.com/fzseyedi)
