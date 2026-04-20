using FinancialManagementApi.Application.Abstractions;
using FinancialManagementApi.Application.Common.Exceptions;
using FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;
using FinancialManagementApi.Domain.Entities;
using FluentAssertions;
using FluentValidation;
using Moq;

namespace FinancialManagementApi.Tests.Application.Invoices;

public sealed class CreateInvoiceHandlerTests
{
    private readonly Mock<IInvoiceRepository> _invoiceRepositoryMock = new();
    private readonly Mock<ICustomerRepository> _customerRepositoryMock = new();
    private readonly Mock<IProductRepository> _productRepositoryMock = new();
    private readonly IValidator<CreateInvoiceCommand> _validator = new CreateInvoiceCommandValidator();

    [Fact]
    public async Task HandleAsync_Should_Create_Invoice_With_Items_When_Command_Is_Valid()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        var product1 = new Product("PRD-001", "Office Chair", 150m);
        var product2 = new Product("PRD-002", "Desk Lamp", 45m);

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product1);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(2, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product2);

        _invoiceRepositoryMock
            .Setup(x => x.CreateWithItemsAsync(
                It.IsAny<Invoice>(),
                It.IsAny<IReadOnlyCollection<InvoiceItem>>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(10);

        var handler = new CreateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            "First invoice",
            new List<CreateInvoiceCommandItem>
            {
                new(1, 2m, 150m),
                new(2, 1m, 45m)
            });

        var result = await handler.HandleAsync(command, CancellationToken.None);

        result.Should().Be(10);

        _invoiceRepositoryMock.Verify(
            x => x.CreateWithItemsAsync(
                It.Is<Invoice>(i =>
                    i.CustomerId == 1 &&
                    i.InvoiceDate == new DateTime(2026, 4, 9) &&
                    i.Notes == "First invoice" &&
                    i.InvoiceNumber.StartsWith("INV-")),
                It.Is<IReadOnlyCollection<InvoiceItem>>(items =>
                    items.Count == 2 &&
                    items.Any(it => it.ProductId == 1 && it.Quantity == 2m && it.UnitPrice == 150m && it.LineTotal == 300m) &&
                    items.Any(it => it.ProductId == 2 && it.Quantity == 1m && it.UnitPrice == 45m && it.LineTotal == 45m)),
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Customer_Does_Not_Exist()
    {
        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Customer?)null);

        var handler = new CreateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        var command = new CreateInvoiceCommand(
            99,
            new DateTime(2026, 4, 9),
            null,
            new List<CreateInvoiceCommandItem>
            {
                new(1, 1m, 100m)
            });

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Customer with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_BadRequestException_When_Customer_Is_Inactive()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        customer.Deactivate();

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        var handler = new CreateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            null,
            new List<CreateInvoiceCommandItem>
            {
                new(1, 1m, 100m)
            });

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Customer with id 1 is inactive.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_NotFoundException_When_Product_Does_Not_Exist()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(99, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product?)null);

        var handler = new CreateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            null,
            new List<CreateInvoiceCommandItem>
            {
                new(99, 1m, 100m)
            });

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("Product with id 99 was not found.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_BadRequestException_When_Product_Is_Inactive()
    {
        var customer = new Customer("CUST-001", "Acme Trading", "info@acme.com", "111111", "Istanbul");
        var product = new Product("PRD-001", "Office Chair", 150m);
        product.Deactivate();

        _customerRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(customer);

        _productRepositoryMock
            .Setup(x => x.GetByIdAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        var handler = new CreateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        var command = new CreateInvoiceCommand(
            1,
            new DateTime(2026, 4, 9),
            null,
            new List<CreateInvoiceCommandItem>
            {
                new(1, 1m, 100m)
            });

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<BadRequestException>()
            .WithMessage("Product with id 1 is inactive.");
    }

    [Fact]
    public async Task HandleAsync_Should_Throw_ValidationException_When_Command_Is_Invalid()
    {
        var handler = new CreateInvoiceHandler(
            _invoiceRepositoryMock.Object,
            _customerRepositoryMock.Object,
            _productRepositoryMock.Object,
            _validator);

        var command = new CreateInvoiceCommand(
            0,
            default,
            new string('A', 501),
            new List<CreateInvoiceCommandItem>());

        var act = async () => await handler.HandleAsync(command, CancellationToken.None);

        await act.Should().ThrowAsync<ValidationException>();

        _customerRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);

        _productRepositoryMock.Verify(
            x => x.GetByIdAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }
}