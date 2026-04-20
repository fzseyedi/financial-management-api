using FinancialManagementApi.Application.Reports.Queries;
using Microsoft.AspNetCore.Mvc;

namespace FinancialManagementApi.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class ReportsController : ControllerBase
{
    [HttpGet("customer-balance/{customerId:int}")]
    public async Task<IActionResult> GetCustomerBalance(
        int customerId,
        [FromServices] GetCustomerBalanceHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetCustomerBalanceQuery(customerId),
            cancellationToken);

        return Ok(result);
    }

    [HttpGet("unpaid-invoices")]
    public async Task<IActionResult> GetUnpaidInvoices(
        [FromServices] GetUnpaidInvoicesHandler handler,
        CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            new GetUnpaidInvoicesQuery(),
            cancellationToken);

        return Ok(result);
    }
}