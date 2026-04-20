# FinancialManagementApi

A practical **financial/business backend API** built with **ASP.NET Core**, **Clean Architecture**, **SQL Server**, **Dapper**, and **Dapper.Contrib**.

This project is designed as a portfolio project for a senior .NET developer profile and focuses on **real business workflows** rather than tutorial-style CRUD only.

---

## Features

### Customers
- Create customer
- Update customer
- Activate / deactivate customer
- **Delete customer** (requires no associated invoices or payments)
- Get customer by id
- Get active customers
- Optionally include inactive customers
- **Get customers with pagination support**

### Products
- Create product
- Update product
- Activate / deactivate product
- Get product by id
- Get active products
- Optionally include inactive products
- **Get products with pagination support**

### Invoices
- Create invoice with header and items in a single request
- Persist invoice header and rows in one SQL transaction
- Get invoice by id with item details
- Issue invoice
- **Delete invoice** (only draft invoices can be deleted; items are removed in a transaction)

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
- **Application**: commands, queries, handlers, validators, DTOs, repository abstractions, Unit of Work pattern
- **Infrastructure**: SQL Server persistence, Dapper repositories, Dapper.Contrib usage, SQL queries, UnitOfWork implementation
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
- Serilog (structured logging with file persistence)
- xUnit
- FluentAssertions
- Moq

---

## Logging

The application uses **Serilog** for structured logging with persistent file storage to track application events, errors, and debugging information.

### Configuration

Serilog is configured in `appsettings.json` with the following settings:

**Production (appsettings.json)**:
- **Log Level**: Information (default) with Warning level for Microsoft and System namespaces
- **Output**: File-based with daily rolling intervals
- **Location**: `logs/financial-api-.txt`
- **Retention**: 30 days of log files
- **File Size Limit**: 100 MB per file
- **Enrichers**: Context, Machine Name, Environment User Name, Thread ID

**Development (appsettings.Development.json)**:
- **Log Level**: Debug (for detailed debugging)
- **Output**: Both console and file
- **Console Format**: Compact timestamps with color levels
- **File Format**: Full timestamps with milliseconds

### Log File Format

Logs include the following structure:
```
2024-01-15 10:30:45.123 +00:00 [INF] [FinancialManagementApi.Infrastructure.Repositories.CustomerRepository] Customer created successfully. CustomerId: 5, Code: CUST001
2024-01-15 10:30:46.456 +00:00 [ERR] [FinancialManagementApi.Api.Middleware.ExceptionHandlingMiddleware] An unexpected error occurred for request /api/invoices POST
   System.Exception: Database connection failed
   at FinancialManagementApi.Infrastructure.Persistence.SqlConnectionFactory...
```

### Logging Coverage

Logging is implemented across:

#### **Exception Handling Middleware**
- Validation errors (Warning)
- Resource not found (Information)
- Business rule violations (Warning)
- Unexpected errors (Error) with full exception details

#### **Repositories** (all CRUD operations)

**CustomerRepository**:
- Create, update, delete operations with customer codes
- Error tracking with exception details

**ProductRepository**:
- Create and update operations with product codes and names
- Pagination retrieval with result counts

**InvoiceRepository**:
- Multi-item invoice creation with transaction tracking
- Update operations (standard and transactional)
- Delete operations with transactional item cleanup
- Detail retrieval with item counts
- Pessimistic locking operations
- Paginated queries with filtering

**PaymentRepository**:
- Payment creation (standalone and transactional) with amounts
- Customer balance retrieval
- Unpaid invoices reporting
- Payment existence verification

### Accessing Logs

#### **Production**
Logs are stored in the `logs/` directory:
```
logs/
├── financial-api-20240115.txt
├── financial-api-20240114.txt
└── financial-api-20240113.txt
```

#### **Development**
- **Console**: Immediate real-time feedback in terminal
- **File**: Same location as production for offline analysis

### Configuration Examples

#### Changing Log Level
Edit `appsettings.json`:
```json
{
  "Serilog": {
    "MinimumLevel": {
      "Default": "Debug"  // Changed from Information
    }
  }
}
```

#### Changing Log File Location
Edit `appsettings.json`:
```json
{
  "Serilog": {
    "WriteTo": [
      {
        "Name": "File",
        "Args": {
          "path": "C:/MyLogs/financial-api-.txt"  // Custom path
        }
      }
    ]
  }
}
```

#### Adding Custom Logging
Inject `ILogger<T>` in any service:
```csharp
public class MyService
{
    private readonly ILogger<MyService> _logger;

    public MyService(ILogger<MyService> logger)
    {
        _logger = logger;
    }

    public void DoWork()
    {
        _logger.LogInformation("Work started");
        try 
        {
            // work here
            _logger.LogInformation("Work completed successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Work failed");
            throw;
        }
    }
}
```

---

## Design Notes

### Design Patterns

**Unit of Work Pattern**: Implemented via `IUnitOfWork` to manage atomic transactions across multiple repositories. Used for complex operations requiring ACID guarantees (e.g., payment recording).

**Repository Pattern**: Abstractions separate data access logic from business logic. Custom SQL queries and Dapper.Contrib are used strategically.

**Command/Query Handler Pattern**: Business logic is organized into handler classes following CQRS principles for clarity and testability.

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

### ACID-compliant payment recording
Payment recording against invoices uses the **Unit of Work pattern** to ensure ACID compliance:

**Atomicity**: Invoice state update and payment creation happen within a single database transaction. Both succeed or both fail.

**Consistency**: Invoice status and paid amount remain consistent with payment records. Domain validation prevents overpayments.

**Isolation**: Pessimistic locking (`WITH UPDLOCK`) prevents concurrent modifications during payment recording, avoiding race conditions.

**Durability**: SQL Server transaction commits guarantee persistence.

This design ensures:
- No partial payment states
- No duplicate payments
- No inconsistent invoice/payment data
- Safe concurrent payment handling

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

### Pagination strategy
The `GET /api/customers` endpoint supports **server-side pagination** using:
- **OFFSET/FETCH** SQL syntax for efficient database queries
- **Metadata response** containing `totalCount`, `pageNumber`, `pageSize`, and `totalPages`
- **Configurable page size** with sensible defaults (page size: 10)

Pagination uses `GetAllCustomersPagedQuery` query object and `PaginatedResponse<T>` generic DTO for consistency across paginated endpoints.

Benefits:
- Reduced memory footprint for large datasets
- Better performance with database-level pagination
- Client can calculate total pages for UI rendering
- Enables consistent API design for future paginated endpoints

### Hard delete strategy
While the main lifecycle uses soft delete (activation/deactivation), customers can be **permanently deleted** via `DELETE /api/customers/{id}` with the following constraints:
- Customer must have **no associated invoices**
- Customer must have **no associated payments**

This ensures **data integrity** by preventing orphaned financial records and maintaining audit trail consistency.

---

## API Overview

### Customers
- `POST /api/customers`
- `PUT /api/customers/{id}`
- `PUT /api/customers/{id}/deactivate`
- `PUT /api/customers/{id}/activate`
- `DELETE /api/customers/{id}`
- `GET /api/customers/{id}`
- `GET /api/customers` - Get active customers (with pagination)
- `GET /api/customers?includeInactive=true` - Get all customers including inactive (with pagination)

#### Pagination Parameters
The `GET /api/customers` endpoint supports the following query parameters:
- `pageNumber` (default: `1`) - The page number to retrieve
- `pageSize` (default: `10`) - Number of items per page
- `includeInactive` (default: `false`) - Include inactive customers in results

**Response format** includes pagination metadata:
```json
{
  "items": [...],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

**Example requests:**
- `GET /api/customers?pageNumber=1&pageSize=10` - First page with 10 items
- `GET /api/customers?pageNumber=2&pageSize=20` - Second page with 20 items
- `GET /api/customers?includeInactive=true&pageNumber=1&pageSize=15` - All customers (including inactive), page 1, 15 items per page

### Products
- `POST /api/products`
- `PUT /api/products/{id}`
- `PUT /api/products/{id}/deactivate`
- `PUT /api/products/{id}/activate`
- `GET /api/products/{id}`
- `GET /api/products` - Get active products (with pagination)
- `GET /api/products?includeInactive=true` - Get all products including inactive (with pagination)

#### Pagination Parameters
The `GET /api/products` endpoint supports the following query parameters:
- `pageNumber` (default: `1`) - The page number to retrieve
- `pageSize` (default: `10`) - Number of items per page
- `includeInactive` (default: `false`) - Include inactive products in results

**Response format** includes pagination metadata:
```json
{
  "items": [...],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

**Example requests:**
- `GET /api/products?pageNumber=1&pageSize=10` - First page with 10 items
- `GET /api/products?pageNumber=2&pageSize=20` - Second page with 20 items
- `GET /api/products?includeInactive=true&pageNumber=1&pageSize=15` - All products (including inactive), page 1, 15 items per page

### Invoices
- `POST /api/invoices`
- `POST /api/invoices/{invoiceId}/issue`
- `GET /api/invoices/{id}`
- `GET /api/invoices` - Get all invoices (with pagination and optional filtering)

#### Pagination and Filtering Parameters
The `GET /api/invoices` endpoint supports the following query parameters:
- `pageNumber` (default: `1`) - The page number to retrieve
- `pageSize` (default: `10`) - Number of items per page
- `customerId` (optional) - Filter invoices by customer ID
- `includeIssued` (default: `false`) - If `true`, includes draft invoices; if `false`, shows only issued and later statuses
- `dateFrom` (optional) - Filter invoices from this date (inclusive)
- `dateTo` (optional) - Filter invoices up to this date (inclusive)

**Response format** includes pagination metadata:
```json
{
  "items": [...],
  "totalCount": 50,
  "pageNumber": 1,
  "pageSize": 10,
  "totalPages": 5
}
```

**Example requests:**
- `GET /api/invoices?pageNumber=1&pageSize=10` - First page with 10 items
- `GET /api/invoices?customerId=1&pageNumber=1&pageSize=10` - Invoices for customer 1
- `GET /api/invoices?includeIssued=true&pageNumber=1&pageSize=10` - All invoices including drafts
- `GET /api/invoices?dateFrom=2024-01-01&dateTo=2024-12-31&pageNumber=1&pageSize=10` - Invoices within date range
- `GET /api/invoices?customerId=1&includeIssued=true&dateFrom=2024-01-01&dateTo=2024-12-31&pageNumber=1&pageSize=10` - Combined filters

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
```
---

## Author

Fariborz Seyedi
Senior .NET Developer
Istanbul, Turkey

LinkedIn: [linkedin.com/in/fariborz-seyedi-49582934b](https://linkedin.com/in/fariborz-seyedi-49582934b)
GitHub: [github.com/fzseyedi](https://github.com/fzseyedi)
