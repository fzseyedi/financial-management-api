using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Invoices;

public sealed class UpdateInvoiceEdgeCasesTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly UpdateInvoiceCommandValidator _validator = new();

    [Fact]
    public async Task Should_Allow_Updating_Invoice_Notes_To_Null()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var version = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        var invoice = new Invoice("INV-0001", customerId, DateTime.UtcNow, "Original notes");
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
            null,
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

        // Assert
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(
                It.Is<Invoice>(i => i.Notes == null),
                It.IsAny<IReadOnlyCollection<InvoiceItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Allow_Updating_Invoice_Notes_With_Whitespace_Only_As_Null()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var version = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        var invoice = new Invoice("INV-0001", customerId, DateTime.UtcNow, "Original notes");
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
            "   ",
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

        // Assert
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(
                It.Is<Invoice>(i => i.Notes == null),
                It.IsAny<IReadOnlyCollection<InvoiceItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Support_Large_Decimal_Values_For_Prices()
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

        var product = new Product("PROD001", "Enterprise Software License", 99999.99m);
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(product, 1);

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow,
            "Enterprise purchase",
            version,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 10m, 99999.99m)
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

        // Assert
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(
                It.Is<Invoice>(i => i.TotalAmount == 999999.90m),
                It.IsAny<IReadOnlyCollection<InvoiceItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Support_Many_Items_In_Single_Update()
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

        var items = new List<UpdateInvoiceCommandItem>();
        for (int i = 1; i <= 50; i++)
        {
            var product = new Product($"PROD{i:00}", $"Product {i}", 100m * i);
            typeof(FinancialManagementApi.Domain.Common.BaseEntity)
                .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
                .SetValue(product, i);

            _productRepositoryMock
                .Setup(x => x.GetByIdAsync(i, It.IsAny<CancellationToken>()))
                .ReturnsAsync(product);

            items.Add(new(i, i, 1m, 100m * i));
        }

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow,
            "Bulk order",
            version,
            "TestUser",
            items);

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

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
                It.Is<IReadOnlyCollection<InvoiceItem>>(items => items.Count == 50),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Preserve_Invoice_Number_When_Updating()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var version = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var originalInvoiceNumber = "INV-ABC12345";

        var invoice = new Invoice(originalInvoiceNumber, customerId, DateTime.UtcNow, "Notes");
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
            DateTime.UtcNow.AddDays(10),
            "Updated notes",
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

        // Assert - Invoice number should not change
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(
                It.Is<Invoice>(i => i.InvoiceNumber == originalInvoiceNumber),
                It.IsAny<IReadOnlyCollection<InvoiceItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Should_Throw_ValidationException_When_Validation_Fails()
    {
        // Arrange
        var command = new UpdateInvoiceCommand(
            0,  // Invalid
            10,
            DateTime.UtcNow,
            "Notes",
            new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 },
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        var handler = new UpdateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        // Act & Assert
        await handler.Invoking(h => h.HandleAsync(command, CancellationToken.None))
            .Should()
            .ThrowAsync<ValidationException>();
    }

    [Fact]
    public async Task Should_Update_ModifiedBy_Field()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var version = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var modifiedBy = "john.doe@company.com";

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
            modifiedBy,
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
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

        // Assert
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(
                It.Is<Invoice>(i => i.ModifiedBy == modifiedBy),
                It.IsAny<IReadOnlyCollection<InvoiceItem>>(),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
