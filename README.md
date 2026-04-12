# FinancialManagementApi

A practical **financial/business backend API** built with **ASP.NET Core**, **Clean Architecture**, **SQL Server**, **Dapper**, and **Dapper.Contrib**.

This project is designed as a portfolio project for a senior .NET developer profile and focuses on **real business workflows** rather than tutorial-style CRUD only.

---

## Features

### Customers
- Create customer
- Update customer
- Activate / deactivate customer
- Get customer by id
- Get active customers
- Optionally include inactive customers

### Products
- Create product
- Update product
- Activate / deactivate product
- Get product by id
- Get active products
- Optionally include inactive products

### Invoices
- Create invoice with header and items in a single request
- Persist invoice header and rows in one SQL transaction
- Get invoice by id with item details
- Issue invoice

### Payments & Reports
- Record payment against an invoice
- Update invoice payment state
- Get customer balance
- Get unpaid invoices

---

## Architecture

The solution follows a **Clean Architecture** structure with clear separation of concerns.

### Projects
- `FinancialManagementApi.Api`
- `FinancialManagementApi.Application`
- `FinancialManagementApi.Domain`
- `FinancialManagementApi.Infrastructure`
- `FinancialManagementApi.Tests`

### Layer responsibilities
- **Domain**: entities, enums, domain exceptions, business rules
- **Application**: commands, queries, handlers, validators, DTOs, repository abstractions
- **Infrastructure**: SQL Server persistence, Dapper repositories, Dapper.Contrib usage, SQL queries
- **Api**: controllers, middleware, request contracts, OpenAPI setup, `.http` files
- **Tests**: validator tests, domain tests, handler tests

---

## Tech Stack

- .NET 10
- ASP.NET Core Web API
- OpenAPI (`AddOpenApi`)
- SQL Server
- Dapper
- Dapper.Contrib
- FluentValidation
- xUnit
- FluentAssertions
- Moq

---

## Design Notes

### Dapper strategy
This solution uses:
- **Dapper.Contrib** for simple `Insert` / `Update`
- **Dapper SQL** for joins, reports, and transactional document persistence

### Transactional invoice creation
Invoices are created with **header + items in one request** and saved in **one SQL transaction**.

This avoids:
- orphan invoice headers
- partial document persistence
- inconsistency between invoice totals and rows

### Error handling
The API uses:
- custom application exceptions
- custom domain exceptions
- exception handling middleware
- consistent JSON error responses

### Lifecycle design
Customers and products support:
- activation
- deactivation

instead of physical deletion in the main workflow.

---

## API Overview

### Customers
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `PUT /api/customers/{id}/deactivate`
- `PUT /api/customers/{id}/activate`
- `GET /api/customers/{id}`
- `GET /api/customers`
- `GET /api/customers?includeInactive=true`

### Products
- `POST /api/products`
- `PUT /api/products/{id}`
- `PUT /api/products/{id}/deactivate`
- `PUT /api/products/{id}/activate`
- `GET /api/products/{id}`
- `GET /api/products`
- `GET /api/products?includeInactive=true`

### Invoices
- `POST /api/invoices`
- `POST /api/invoices/{invoiceId}/issue`
- `GET /api/invoices/{id}`

### Payments
- `POST /api/payments`

### Reports
- `GET /api/reports/customer-balance/{customerId}`
- `GET /api/reports/unpaid-invoices`

---

## Example Workflow

1. Create a customer
2. Create products
3. Create an invoice with header and items in one transaction
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

---

## Author

Fariborz Seyedi
Senior .NET Developer
Istanbul, Turkey

LinkedIn: [linkedin.com/in/fariborz-seyedi-49582934b](https://linkedin.com/in/fariborz-seyedi-49582934b)
GitHub: [github.com/fzseyedi](https://github.com/fzseyedi)
