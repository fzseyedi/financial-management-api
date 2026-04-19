using FinancialManagementApi.Api.Contracts;
using FinancialManagementApi.Application.Invoices.Commands;
using FinancialManagementApi.Application.Invoices.Commands.CreateInvoice;
using FinancialManagementApi.Application.Invoices.Commands.UpdateInvoice;
using FinancialManagementApi.Application.Invoices.Queries;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class InvoicesController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromServices] GetAllInvoicesHandler handler,
        CancellationToken cancellationToken,
        [FromQuery] int? customerId = null,
        [FromQuery] bool includeIssued = false,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        var query = new GetAllInvoicesPagedQuery(customerId, includeIssued, dateFrom, dateTo, pageNumber, pageSize);
        var result = await handler.HandlePagedAsync(query, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateInvoiceRequest request,
        [FromServices] CreateInvoiceHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new CreateInvoiceCommand(
            request.CustomerId,
            request.InvoiceDate,
            request.Notes,
            request.Items.Select(x => new CreateInvoiceCommandItem(
                x.ProductId,
                x.Quantity,
                x.UnitPrice))
            .ToList());

        var newId = await handler.HandleAsync(command, cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = newId },
            new { id = newId });
    }

    [HttpPost("{invoiceId:int}/issue")]
    public async Task<IActionResult> Issue(
        int invoiceId,
        [FromServices] IssueInvoiceHandler handler,
        CancellationToken cancellationToken)
    {
        await handler.HandleAsync(new IssueInvoiceCommand(invoiceId), cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        [FromServices] GetInvoiceByIdHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(new GetInvoiceByIdQuery(id), cancellationToken);

        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPut("{invoiceId:int}")]
    public async Task<IActionResult> Update(
        int invoiceId,
        [FromBody] UpdateInvoiceRequest request,
        [FromServices] UpdateInvoiceHandler handler,
        CancellationToken cancellationToken)
    {
        var versionBytes = string.IsNullOrEmpty(request.Version) ? [] : Convert.FromBase64String(request.Version);

        var command = new UpdateInvoiceCommand(
            invoiceId,
            request.CustomerId,
            request.InvoiceDate,
            request.Notes,
            versionBytes,
            request.ModifiedBy,
            request.Items.Select(x => new UpdateInvoiceCommandItem(
                x.Id,
                x.ProductId,
                x.Quantity,
                x.UnitPrice))
            .ToList());

        await handler.HandleAsync(command, cancellationToken);

        return NoContent();
    }
}