using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.DTOs;
using FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using Moq;

namespace FinancialManagementApi.Tests.Application.Invoices;

public sealed class UpdateInvoiceIntegrationTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly UpdateInvoiceCommandValidator _validator = new();

    [Fact]
    public async Task Integration_Should_Successfully_Retrieve_Updated_Invoice_Details()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var currentVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        var invoice = new Invoice("INV-0001", customerId, new DateTime(2026, 4, 9), "Original notes");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(invoice, invoiceId);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Version))!
            .SetValue(invoice, currentVersion);

        var customer = new Customer("CUST001", "Test Customer", "test@example.com", "1234567890", "123 Main St");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, customerId);

        var product = new Product("PROD001", "Office Chair", 150m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        var updatedInvoice = new Invoice("INV-0001", customerId, new DateTime(2026, 4, 10), "Updated notes");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(updatedInvoice, invoiceId);
        updatedInvoice.UpdateTotalAmount(300m, "TestUser");

        var expectedDto = new InvoiceDto(
            invoiceId,
            "INV-0001",
            customerId,
            "Test Customer",
            new DateTime(2026, 4, 10),
            0,
            300m,
            0m,
            300m,
            "Updated notes",
            DateTime.UtcNow,
            "TestUser",
            Convert.ToBase64String(currentVersion),
            new List<InvoiceItemDto>
            {
                new(1, "Office Chair", 2m, 150m, 300m)
            });

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            new DateTime(2026, 4, 10),
            "Updated notes",
            currentVersion,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _invoiceRepositoryMock
            .Setup(x => x.GetDetailsByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedDto);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _invoiceRepositoryMock
            .Setup(x => x.UpdateWithItemsAsync(It.IsAny<Invoice>(), It.IsAny<IReadOnlyCollection<InvoiceItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);
        var result = await _invoiceRepositoryMock.Object.GetDetailsByIdAsync(invoiceId, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(invoiceId);
        result.Notes.Should().Be("Updated notes");
        result.InvoiceDate.Should().Be(new DateTime(2026, 4, 10));
        result.ModifiedBy.Should().Be("TestUser");
        result.Items.Should().HaveCount(1);
        result.Items.First().Quantity.Should().Be(2m);
        result.Items.First().UnitPrice.Should().Be(150m);
    }

    [Fact]
    public async Task Integration_Should_Fail_When_Updating_With_Incorrect_Version()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var currentVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var wrongVersion = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 };

        var invoice = new Invoice("INV-0001", customerId, DateTime.UtcNow, "Notes");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(invoice, invoiceId);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Version))!
            .SetValue(invoice, currentVersion);

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow,
            "Notes",
            wrongVersion,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        var handler = new UpdateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        // Act & Assert
        await handler.Invoking(h => h.HandleAsync(command, CancellationToken.None))
            .Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("*modified by another user*");

        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(It.IsAny<Invoice>(), It.IsAny<IReadOnlyCollection<InvoiceItem>>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task Integration_Should_Replace_All_Items_When_Updating()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var version = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        var invoice = new Invoice("INV-0001", customerId, DateTime.UtcNow, "Notes");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(invoice, invoiceId);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Version))!
            .SetValue(invoice, version);

        var customer = new Customer("CUST001", "Test Customer", "test@example.com", "1234567890", "123 Main St");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, customerId);

        var product1 = new Product("PROD001", "Office Chair", 150m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product1, 1);

        var product2 = new Product("PROD002", "Desk", 300m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product2, 2);

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow,
            "Updated with new items",
            version,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 1m, 150m),
                new(2, 2, 1m, 300m)
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);

        _invoiceRepositoryMock
            .Setup(x => x.UpdateWithItemsAsync(It.IsAny<Invoice>(), It.IsAny<IReadOnlyCollection<InvoiceItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(
                It.IsAny<Invoice>(),
                It.Is<IReadOnlyCollection<InvoiceItem>>(items =>
                    items.Count == 2 &&
                    items.Any(i => i.ProductId == 1 && i.Quantity == 1m && i.UnitPrice == 150m && i.LineTotal == 150m) &&
                    items.Any(i => i.ProductId == 2 && i.Quantity == 1m && i.UnitPrice == 300m && i.LineTotal == 300m)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Integration_Should_Update_Total_Amount_Correctly()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var version = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        var invoice = new Invoice("INV-0001", customerId, DateTime.UtcNow, "Notes");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(invoice, invoiceId);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Version))!
            .SetValue(invoice, version);

        var customer = new Customer("CUST001", "Test Customer", "test@example.com", "1234567890", "123 Main St");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(customer, customerId);

        var product = new Product("PROD001", "Office Chair", 150m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow,
            "Notes",
            version,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 5m, 200m)  // 5 * 200 = 1000
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _invoiceRepositoryMock
            .Setup(x => x.UpdateWithItemsAsync(It.IsAny<Invoice>(), It.IsAny<IReadOnlyCollection<InvoiceItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert - Verify that the update includes the correct total amount
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(
                It.Is<Invoice>(i => i.TotalAmount == 1000m && i.ModifiedBy == "TestUser"),
                It.IsAny<IReadOnlyCollection<InvoiceItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Integration_Should_Update_Customer_Reference()
    {
        // Arrange
        var invoiceId = 1;
        var originalCustomerId = 10;
        var newCustomerId = 20;
        var version = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        var invoice = new Invoice("INV-0001", originalCustomerId, DateTime.UtcNow, "Notes");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(invoice, invoiceId);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Version))!
            .SetValue(invoice, version);

        var originalCustomer = new Customer("CUST001", "Original Customer", "original@example.com", "1111111111", "Address 1");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(originalCustomer, originalCustomerId);

        var newCustomer = new Customer("CUST002", "New Customer", "new@example.com", "2222222222", "Address 2");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(newCustomer, newCustomerId);

        var product = new Product("PROD001", "Office Chair", 150m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        var command = new UpdateInvoiceCommand(
            invoiceId,
            newCustomerId,
            DateTime.UtcNow,
            "Reassigned to new customer",
            version,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(newCustomerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(newCustomer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        _invoiceRepositoryMock
            .Setup(x => x.UpdateWithItemsAsync(It.IsAny<Invoice>(), It.IsAny<IReadOnlyCollection<InvoiceItem>>(), It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var handler = new UpdateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        // Act
        await handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(
                It.Is<Invoice>(i => i.CustomerId == newCustomerId && i.Notes == "Reassigned to new customer"),
                It.IsAny<IReadOnlyCollection<InvoiceItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
