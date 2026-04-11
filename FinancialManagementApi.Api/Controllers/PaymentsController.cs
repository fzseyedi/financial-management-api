using FinancialManagementApi.Api.Contracts;
using FinancialManagementApi.Application.Payments.Commands.RecordPayment;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class PaymentsController : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] RecordPaymentRequest request,
        [FromServices] RecordPaymentHandler handler,
        CancellationToken cancellationToken)
    {
        var command = new RecordPaymentCommand(
            request.CustomerId,
            request.InvoiceId,
            request.PaymentDate,
            request.Amount,
            request.ReferenceNumber,
            request.Notes);

        var newId = await handler.HandleAsync(command, cancellationToken);

        return Ok(new { id = newId });
    }
}