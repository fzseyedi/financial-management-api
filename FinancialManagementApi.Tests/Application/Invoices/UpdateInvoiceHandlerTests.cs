using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Invoices;

public sealed class UpdateInvoiceHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly Mock<IValidator<UpdateInvoiceCommand>> _validatorMock = new();

    private readonly UpdateInvoiceHandler _handler;

    public UpdateInvoiceHandlerTests()
    {
        _handler = new UpdateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validatorMock.Object);
    }

    [Fact]
    public async Task HandleAsync_Should_Update_Invoice_When_Valid_Command_Provided()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var currentVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        
        var invoice = new Invoice("INV-0001", customerId, DateTime.UtcNow, "Original notes");
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

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow.AddDays(1),
            "Updated notes",
            currentVersion,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        _validatorMock
            .Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

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

        // Act
        await _handler.HandleAsync(command, CancellationToken.None);

        // Assert
        _invoiceRepositoryMock.Verify(
            x => x.UpdateWithItemsAsync(It.IsAny<Invoice>(), It.IsAny<IReadOnlyCollection<InvoiceItem>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Invoice_Not_Found()
    {
        // Arrange
        var invoiceId = 999;
        var version = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };

        var command = new UpdateInvoiceCommand(
            invoiceId,
            10,
            DateTime.UtcNow,
            "Notes",
            version,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        _validatorMock
            .Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Invoice?)null);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("*999*");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ConflictException_When_Version_Mismatch()
    {
        // Arrange
        var invoiceId = 1;
        var customerId = 10;
        var currentVersion = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 };
        var providedVersion = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 };

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
            providedVersion,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        _validatorMock
            .Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command, CancellationToken.None))
            .Should()
            .ThrowAsync<ConflictException>()
            .WithMessage("*modified by another user*");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Customer_Not_Found()
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

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow,
            "Notes",
            version,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        _validatorMock
            .Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("*Customer*");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_BadRequestException_When_Customer_Inactive()
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

        var inactiveCustomer = new Customer("CUST001", "Test Customer", "test@example.com", "1234567890", "123 Main St");
        typeof(FinancialManagementApi.Domain.Common.BaseEntity)
            .GetProperty(nameof(FinancialManagementApi.Domain.Common.BaseEntity.Id))!
            .SetValue(inactiveCustomer, customerId);
        inactiveCustomer.Deactivate();

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow,
            "Notes",
            version,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 1, 2m, 150m)
            });

        _validatorMock
            .Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(inactiveCustomer);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command, CancellationToken.None))
            .Should()
            .ThrowAsync<BadRequestException>()
            .WithMessage("*inactive*");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Product_Not_Found()
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

        var command = new UpdateInvoiceCommand(
            invoiceId,
            customerId,
            DateTime.UtcNow,
            "Notes",
            version,
            "TestUser",
            new List<UpdateInvoiceCommandItem>
            {
                new(1, 999, 2m, 150m)
            });

        _validatorMock
            .Setup(x => x.ValidateAsync(command, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new FluentValidation.Results.ValidationResult());

        _invoiceRepositoryMock
            .Setup(x => x.GetByIdAsync(invoiceId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(invoice);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(customerId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(999, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        // Act & Assert
        await _handler.Invoking(h => h.HandleAsync(command, CancellationToken.None))
            .Should()
            .ThrowAsync<NotFoundException>()
            .WithMessage("*Product*");
    }
}
